import optuna
import gc
import copy
import torch
import torch.nn.functional as F
from dataclasses import dataclass
from enum import Enum
from typing import Callable, Iterable

from PitchModel.Stuff.Model.PitchModel import PitchModelArgs, PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.DataPrep.PitchIO import PitchIOData
from PitchModel.Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.DataPrep.FoldGeneration import GetDisjointTrainIndices
from PitchModel.Stuff.Model.ModelTrain import TrainAndGraph
from PitchModel.Constants import device

_ACTIVATION_FUNCTIONS = ["ReLU", "LeakyReLU", "GELU", "SiLU", "Tanh"]
class TuneGroup(Enum):
    Optimization = 0
    Architecture = 1
    Regularization = 2

@dataclass(frozen=True)
class _FloatParameter:
    low : float
    high : float
    log : bool = False

@dataclass(frozen=True)
class _IntParameter:
    low : int
    high : int

@dataclass(frozen=True)
class _CategoricalParameter:
    choices : tuple[str, ...]

_ACTIVATION_MAP : dict[str, Callable[[torch.Tensor], torch.Tensor]] = {
        "ReLU": F.relu,
        "LeakyReLU": F.leaky_relu,
        "GELU": F.gelu,
        "SiLU": F.silu,
        "Tanh": F.tanh,
    }

_SEARCH_SPACE : dict[TuneGroup, dict[str, _FloatParameter | _IntParameter | _CategoricalParameter]] = {
    TuneGroup.Optimization: {
        "learning_rate": _FloatParameter(1e-4, 3e-3, log=True),
        "batch_size": _IntParameter(10000, 25000),
        "num_epochs": _IntParameter(50, 300),
    },
    TuneGroup.Architecture: {
        "block_size": _IntParameter(16, 300),
        "num_blocks": _IntParameter(6, 16),
        "activation": _CategoricalParameter(tuple(_ACTIVATION_MAP)),
    },
    TuneGroup.Regularization: {
        "dropout": _FloatParameter(0, 0.4),
        "weight_decay": _FloatParameter(1e-9, 1e-1, log=True),
    },
}

def GetTunedParameterNames(groups : list[TuneGroup]) -> list[str]:
    return [name for group in groups for name in _SEARCH_SPACE[group]]

def GetStudyName(
        model_variant_type : ModelVariantType,
        model_output_type : ModelOutputType,
        groups : list[TuneGroup],
        tag : str = "") -> str:
    group_names = "+".join(group.name for group in groups)
    name = f"{model_variant_type.name}_{model_output_type.name}_{group_names}"
    return f"{name}_{tag}" if tag != "" else name

def BuildTrialArgs(
        trial : optuna.trial.Trial,
        model_variant_type : ModelVariantType,
        model_output_type : ModelOutputType,
        groups : Iterable[TuneGroup],
        base_args : PitchModelArgs | None = None) -> PitchModelArgs:

    if base_args is None:
        base_args = DEFAULT_ARGS_MAP[(model_variant_type, model_output_type)]
    if base_args.model_variant_type != model_variant_type or base_args.model_output_type != model_output_type:
        raise ValueError(f"base_args is for {base_args.model_variant_type.name}/{base_args.model_output_type.name}, "
                         f"not {model_variant_type.name}/{model_output_type.name}")
    
    # Copied so that tuning never changes the shared defaults
    args = copy.copy(base_args)
    
    tuned_names : list[str] = []
    for group in groups:
        for name, spec in _SEARCH_SPACE[group].items():
            tuned_names.append(name)
            match spec:
                case _FloatParameter():
                    value = trial.suggest_float(name, spec.low, spec.high, log=spec.log)
                case _IntParameter():
                    value = trial.suggest_int(name, spec.low, spec.high)
                case _CategoricalParameter():
                    value = trial.suggest_categorical(name, list(spec.choices))
            
            if name == "activation":
                args.activation_function = _ACTIVATION_MAP[value]
            else:
                setattr(args, name, value)
    
    return args

def RunEvaluation(
            trial : optuna.trial.Trial,
            io_data : PitchIOData,
            data_prep : DataPrep,
            model_variant_type : ModelVariantType,
            model_output_type : ModelOutputType,
            max_repeats : int,
            groups : list[TuneGroup],
            base_args : PitchModelArgs | None = None) -> float:
    
    args = BuildTrialArgs(trial, model_variant_type, model_output_type, groups, base_args)
    
    sum_loss = 0
    # Repeats use folds whose test sets share no pitchers, so they are independent samples
    for train_idx in GetDisjointTrainIndices(max_repeats):
        datasets = CreateTestTrainDatasets(
            io_data=io_data,
            train_idx=train_idx
        )
        datasets.SetOutputType(model_output_type)
        
        network = PitchModel(args, data_prep).to(device)
        result = TrainAndGraph(
            network=network,
            datasets=datasets,
            model_name="tuning",
            should_output=False,
            show_progress=False,
            num_epochs=args.num_epochs,
            batch_size=args.batch_size,
            save_model=False
        )
        sum_loss += result.test_loss
        
        del datasets
        del network
        torch.cuda.empty_cache()
        gc.collect()
        
    return sum_loss