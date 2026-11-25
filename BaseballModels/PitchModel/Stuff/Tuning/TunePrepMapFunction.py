import itertools
import math
import optuna
import pandas as pd

from PitchModel.Stuff.Model.PitchModel import PitchModelArgs
from PitchModel.Stuff.DataPrep.PrepMap import PrepMapConfig, STANDARD_PREP_MAP_CONFIG
from PitchModel.Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from PitchModel.Stuff.Tuning.TuningData import TuningData
from PitchModel.Stuff.Tuning.TuneFunction import (
    TuneGroup, PREP_MAP_GROUPS, GetTunedParameterNames, BuildTrialArgs, BuildTrialPrepMapConfig, TrainFolds,
    _SEARCH_SPACE, _IntParameter, _CategoricalParameter)

def _CheckPrepMapGroups(groups : list[TuneGroup]) -> None:
    not_prep_map = [group.name for group in groups if group not in PREP_MAP_GROUPS]
    if len(not_prep_map) > 0:
        raise ValueError(f"{not_prep_map} are model hyperparameter groups, only "
                         f"{[group.name for group in PREP_MAP_GROUPS]} can be grid searched")

def GetPrepMapGrid(groups : list[TuneGroup]) -> list[dict[str, int | str]]:
    """Every combination of the parameters in groups, as optuna params"""
    _CheckPrepMapGroups(groups)
    
    choices : dict[str, list[int | str]] = {}
    for group in groups:
        for name, spec in _SEARCH_SPACE[group].items():
            match spec:
                case _IntParameter():
                    choices[name] = list(range(spec.low, spec.high + 1))
                case _CategoricalParameter():
                    choices[name] = list(spec.choices)
    
    return [dict(zip(choices, combo)) for combo in itertools.product(*choices.values())]

def EnqueuePrepMapGrid(
        study : optuna.Study,
        groups : list[TuneGroup],
        evaluations_per_config : int = 1) -> int:
    
    # A RUNNING trial is left by a killed kernel and needs to be redone.
    counted_states = (optuna.trial.TrialState.COMPLETE, optuna.trial.TrialState.WAITING)
    existing : dict[tuple, int] = {}
    for t in study.get_trials(deepcopy=False, states=counted_states):
        # Queued trials that have not run yet only have their params in fixed_params (same lookup optuna uses)
        params = t.system_attrs.get("fixed_params", t.params)
        key = tuple(sorted(params.items()))
        existing[key] = existing.get(key, 0) + 1
    
    for params in GetPrepMapGrid(groups):
        missing = evaluations_per_config - existing.get(tuple(sorted(params.items())), 0)
        for _ in range(missing):
            study.enqueue_trial(params)

    return len(study.get_trials(deepcopy=False, states=(optuna.trial.TrialState.WAITING,)))

def RunPrepMapEvaluation(
            trial : optuna.trial.Trial,
            tuning_data : TuningData,
            model_variant_type : ModelVariantType,
            model_output_type : ModelOutputType,
            max_repeats : int,
            groups : list[TuneGroup],
            base_args : PitchModelArgs | None = None,
            base_prep_config : PrepMapConfig | None = None) -> float:
    
    _CheckPrepMapGroups(groups)
    args = BuildTrialArgs(trial, model_variant_type, model_output_type, [], base_args)
    prep_config = BuildTrialPrepMapConfig(trial, model_variant_type, groups, base_prep_config)
    data_prep, io_data = tuning_data.Get(prep_config)
    
    match model_variant_type:
        case ModelVariantType.Stuff:
            input_size = data_prep.GetStuffInputSize()
            variance_kept = data_prep.GetStuffVarianceKept()
        case ModelVariantType.Combined:
            input_size = data_prep.GetCombinedInputSize()
            variance_kept = data_prep.GetCombinedVarianceKept()
    
    fold_losses = TrainFolds(args, data_prep, io_data, model_output_type, max_repeats)
    
    # Folds are the same for every trial, so per-fold losses allow paired comparisons between configs
    trial.set_user_attr("prep_map", prep_config.Describe())
    trial.set_user_attr("input_size", input_size)
    trial.set_user_attr("pca_variance_kept", variance_kept)
    trial.set_user_attr("fold_losses", fold_losses)
    return sum(fold_losses)

def SummarizePrepMapStudy(
        study : optuna.Study,
        groups : list[TuneGroup],
        base_prep_config : PrepMapConfig | None = None) -> pd.DataFrame:
    """
    One row per combination, best first. Losses are per fold (trial value / number of folds).
    delta_vs_base is the paired per-fold difference from base_prep_config (the standard PrepMap by default,
    negative is better), folds_better is how many of those folds improved on it.
    """
    names = GetTunedParameterNames(groups)
    base_name = (base_prep_config if base_prep_config is not None else STANDARD_PREP_MAP_CONFIG).Describe()
    
    # Fold losses of every complete trial, grouped by combination
    folds_by_key : dict[tuple, list[list[float]]] = {}
    attrs_by_key : dict[tuple, dict] = {}
    base_trials : list[list[float]] = []
    for t in study.get_trials(deepcopy=False, states=(optuna.trial.TrialState.COMPLETE,)):
        fold_losses = t.user_attrs.get("fold_losses")
        if fold_losses is None or not all(math.isfinite(x) for x in fold_losses):
            continue
        key = tuple(t.params[n] for n in names)
        folds_by_key.setdefault(key, []).append(fold_losses)
        attrs_by_key[key] = t.user_attrs
        if t.user_attrs["prep_map"] == base_name:
            base_trials.append(fold_losses)
    
    def mean_folds(trials : list[list[float]]) -> list[float]:
        return [sum(fold) / len(fold) for fold in zip(*trials)]
    
    base_folds = mean_folds(base_trials) if len(base_trials) > 0 else None
    
    rows = []
    for key, trials in folds_by_key.items():
        trial_means = [sum(folds) / len(folds) for folds in trials]
        row = dict(zip(names, key))
        row["trials"] = len(trials)
        row["mean_loss"] = sum(trial_means) / len(trial_means)
        row["trial_std"] = pd.Series(trial_means).std()
        row["input_size"] = attrs_by_key[key]["input_size"]
        row["pca_variance_kept"] = attrs_by_key[key]["pca_variance_kept"]
        if base_folds is not None:
            deltas = [c - b for c, b in zip(mean_folds(trials), base_folds)]
            row["delta_vs_base"] = sum(deltas) / len(deltas)
            row["folds_better"] = f"{sum(d < 0 for d in deltas)}/{len(deltas)}"
        rows.append(row)
    
    if len(rows) == 0:
        return pd.DataFrame(columns=names)
    return pd.DataFrame(rows).sort_values("mean_loss").reset_index(drop=True)
