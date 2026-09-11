import optuna
from optuna.study import Study
import gc

from Model.Combined.Model.Model_Train import TrainAndGraph, DEFAULT_BATCH_SIZE, DEFAULT_NUM_EPOCHS, DEFAULT_BATCH_SIZE_P, DEFAULT_NUM_EPOCHS_P
from Model.Pro.Model.Player_Model import Recurrent_Model as Pro_Model, LayerArch
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Pro.Model.Player_Model import *
from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Constants import device
from Model.Combined.Tuning.ProTuningShared import *
from tqdm import tqdm
    
class ProModelHeadTuningRecipe(Enum):
    LEVEL = auto()
    PA = auto()
    STATS = auto()
    POS = auto()
    PT = auto()
    MLBSTAT = auto()
    MLBVALUE = auto()
    
SEARCH_SPACE: dict[ProModelHeadTuningRecipe, list[ParamSpec]] = {
    ProModelHeadTuningRecipe.LEVEL : [
        ParamSpec("level_layers", 2, 5, is_int=True),
        ParamSpec("level_size", 4, 128, is_int=True),
        ParamSpec("level_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_level", 1e-5, 1e-2, log=True),
        ParamSpec("wd_level", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.PA : [
        ParamSpec("pa_layers", 2, 5, is_int=True),
        ParamSpec("pa_size", 4, 128, is_int=True),
        ParamSpec("pa_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_pa", 1e-5, 1e-2, log=True),
        ParamSpec("wd_pa", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.STATS : [
        ParamSpec("stats_layers", 2, 5, is_int=True),
        ParamSpec("stats_size", 4, 128, is_int=True),
        ParamSpec("stats_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_stats", 1e-5, 1e-2, log=True),
        ParamSpec("wd_stats", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.POS : [
        ParamSpec("pos_layers", 2, 5, is_int=True),
        ParamSpec("pos_size", 4, 128, is_int=True),
        ParamSpec("pos_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_pos", 1e-5, 1e-2, log=True),
        ParamSpec("wd_pos", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.PT : [
        ParamSpec("pt_layers", 2, 5, is_int=True),
        ParamSpec("pt_size", 4, 128, is_int=True),
        ParamSpec("pt_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_pt", 1e-5, 1e-2, log=True),
        ParamSpec("wd_pt", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.MLBSTAT : [
        ParamSpec("mlbstat_layers", 2, 5, is_int=True),
        ParamSpec("mlbstat_size", 4, 128, is_int=True),
        ParamSpec("mlbstat_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_mlbstat", 1e-5, 1e-2, log=True),
        ParamSpec("wd_mlbstat", 1e-7, 1e-2, log=True)
    ],
    ProModelHeadTuningRecipe.MLBVALUE : [
        ParamSpec("mlbvalue_layers", 2, 5, is_int=True),
        ParamSpec("mlbvalue_size", 4, 128, is_int=True),
        ParamSpec("mlbvalue_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_mlbvalue", 1e-5, 1e-2, log=True),
        ParamSpec("wd_mlbvalue", 1e-7, 1e-2, log=True)
    ],
}

_ACTIVATION_NAME = {v: k for k, v in ACTIVATION_MAP.items()}
HITTER_DEFAULTS = {
    "level_layers": DEFAULT_LVL_ARCH.num_layers,
    "level_size": DEFAULT_LVL_ARCH.layer_size,
    "level_activation": _ACTIVATION_NAME[DEFAULT_LVL_ARCH.nonlin],
    "wd_level": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_LEVEL + 1],
    "lr_level": DEFAULT_LEARNING_RATES[LOSS_IDX_LEVEL + 1],

    "pa_layers": DEFAULT_PA_ARCH.num_layers,
    "pa_size": DEFAULT_PA_ARCH.layer_size,
    "pa_activation": _ACTIVATION_NAME[DEFAULT_PA_ARCH.nonlin],
    "wd_pa": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_PA + 1],
    "lr_pa": DEFAULT_LEARNING_RATES[LOSS_IDX_PA + 1],

    "stats_layers": DEFAULT_STATS_ARCH.num_layers,
    "stats_size": DEFAULT_STATS_ARCH.layer_size,
    "stats_activation": _ACTIVATION_NAME[DEFAULT_STATS_ARCH.nonlin],
    "wd_stats": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_STATS + 1],
    "lr_stats": DEFAULT_LEARNING_RATES[LOSS_IDX_STATS + 1],

    "pos_layers": DEFAULT_POS_ARCH.num_layers,
    "pos_size": DEFAULT_POS_ARCH.layer_size,
    "pos_activation": _ACTIVATION_NAME[DEFAULT_POS_ARCH.nonlin],
    "wd_pos": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_POS + 1],
    "lr_pos": DEFAULT_LEARNING_RATES[LOSS_IDX_POS + 1],

    "pt_layers": DEFAULT_PT_ARCH.num_layers,
    "pt_size": DEFAULT_PT_ARCH.layer_size,
    "pt_activation": _ACTIVATION_NAME[DEFAULT_PT_ARCH.nonlin],
    "wd_pt": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_PT + 1],
    "lr_pt": DEFAULT_LEARNING_RATES[LOSS_IDX_PT + 1],

    "mlbstat_layers": DEFAULT_MLBSTAT_ARCH.num_layers,
    "mlbstat_size": DEFAULT_MLBSTAT_ARCH.layer_size,
    "mlbstat_activation": _ACTIVATION_NAME[DEFAULT_MLBSTAT_ARCH.nonlin],
    "wd_mlbstat": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_MLBSTAT + 1],
    "lr_mlbstat": DEFAULT_LEARNING_RATES[LOSS_IDX_MLBSTAT + 1],

    "mlbvalue_layers": DEFAULT_VALUE_ARCH.num_layers,
    "mlbvalue_size": DEFAULT_VALUE_ARCH.layer_size,
    "mlbvalue_activation": _ACTIVATION_NAME[DEFAULT_VALUE_ARCH.nonlin],
    "wd_mlbvalue": DEFAULT_PRO_WEIGHT_DECAY[LOSS_IDX_MLBVALUE + 1],
    "lr_mlbvalue": DEFAULT_LEARNING_RATES[LOSS_IDX_MLBVALUE + 1],
}

PITCHER_DEFAULTS = {
    "level_layers": DEFAULT_LVL_ARCH_P.num_layers,
    "level_size": DEFAULT_LVL_ARCH_P.layer_size,
    "level_activation": _ACTIVATION_NAME[DEFAULT_LVL_ARCH_P.nonlin],
    "wd_level": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_LEVEL + 1],
    "lr_level": DEFAULT_LEARNING_RATES_P[LOSS_IDX_LEVEL + 1],

    "pa_layers": DEFAULT_PA_ARCH_P.num_layers,
    "pa_size": DEFAULT_PA_ARCH_P.layer_size,
    "pa_activation": _ACTIVATION_NAME[DEFAULT_PA_ARCH_P.nonlin],
    "wd_pa": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_PA + 1],
    "lr_pa": DEFAULT_LEARNING_RATES_P[LOSS_IDX_PA + 1],

    "stats_layers": DEFAULT_STATS_ARCH_P.num_layers,
    "stats_size": DEFAULT_STATS_ARCH_P.layer_size,
    "stats_activation": _ACTIVATION_NAME[DEFAULT_STATS_ARCH_P.nonlin],
    "wd_stats": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_STATS + 1],
    "lr_stats": DEFAULT_LEARNING_RATES_P[LOSS_IDX_STATS + 1],

    "pos_layers": DEFAULT_POS_ARCH_P.num_layers,
    "pos_size": DEFAULT_POS_ARCH_P.layer_size,
    "pos_activation": _ACTIVATION_NAME[DEFAULT_POS_ARCH_P.nonlin],
    "wd_pos": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_POS + 1],
    "lr_pos": DEFAULT_LEARNING_RATES_P[LOSS_IDX_POS + 1],

    "pt_layers": DEFAULT_PT_ARCH_P.num_layers,
    "pt_size": DEFAULT_PT_ARCH_P.layer_size,
    "pt_activation": _ACTIVATION_NAME[DEFAULT_PT_ARCH_P.nonlin],
    "wd_pt": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_PT + 1],
    "lr_pt": DEFAULT_LEARNING_RATES_P[LOSS_IDX_PT + 1],

    "mlbstat_layers": DEFAULT_MLBSTAT_ARCH_P.num_layers,
    "mlbstat_size": DEFAULT_MLBSTAT_ARCH_P.layer_size,
    "mlbstat_activation": _ACTIVATION_NAME[DEFAULT_MLBSTAT_ARCH_P.nonlin],
    "wd_mlbstat": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_MLBSTAT + 1],
    "lr_mlbstat": DEFAULT_LEARNING_RATES_P[LOSS_IDX_MLBSTAT + 1],

    "mlbvalue_layers": DEFAULT_VALUE_ARCH_P.num_layers,
    "mlbvalue_size": DEFAULT_VALUE_ARCH_P.layer_size,
    "mlbvalue_activation": _ACTIVATION_NAME[DEFAULT_VALUE_ARCH_P.nonlin],
    "wd_mlbvalue": DEFAULT_PRO_WEIGHT_DECAY_P[LOSS_IDX_MLBVALUE + 1],
    "lr_mlbvalue": DEFAULT_LEARNING_RATES_P[LOSS_IDX_MLBVALUE + 1],
}

def resolve_params(
        trial: optuna.trial.Trial,
        recipe: ProModelHeadTuningRecipe,
        width: SearchWidth,
        is_hitter: bool) -> dict:
    
    defaults = HITTER_DEFAULTS if is_hitter else PITCHER_DEFAULTS
    params = dict(defaults)
    for flag, specs in SEARCH_SPACE.items():
        if recipe & flag:
            for spec in specs:
                params[spec.name] = spec.suggest(trial, defaults[spec.name], width)
    return params

@dataclass
class MultiHeadEvalResult:
    pa : float = 0
    level : float = 0
    pt : float = 0
    pos : float = 0
    stats : float = 0
    mlbvalue : float = 0
    mlbstat : float = 0

def GetHeadTypeResult(result: MultiHeadEvalResult, recipe: ProModelHeadTuningRecipe) -> float:
    match recipe.name:
        case "PA":
            return result.pa
        case "POS":
            return result.pos
        case "PT":
            return result.pt
        case "STATS":
            return result.stats
        case "LEVEL":
            return result.level
        case "MLBSTAT":
            return result.mlbstat
        case "MLBVALUE":
            return result.mlbvalue
        case _:
            raise ValueError(recipe)

def resolve_params(
        trials : list[optuna.trial.Trial],
        recipes: list[ProModelHeadTuningRecipe],
        width: SearchWidth,
        is_hitter: bool) -> dict:
    
    assert len(trials) == len(recipes)
    
    defaults = HITTER_DEFAULTS if is_hitter else PITCHER_DEFAULTS
    params = dict(defaults)
    
    for i in range(len(trials)):
        trial = trials[i]
        recipe = recipes[i]
        
        for flag, specs in SEARCH_SPACE.items():
            if recipe == flag:
                 for spec in specs:
                    params[spec.name] = spec.suggest(trial, defaults[spec.name], width)
                    
    return params

def run_evaluation(
            io_list: list[Combined_IO],
            data_prep: Combined_Data_Prep,
            is_hitter: bool,
            level_arch : LayerArch,
            pa_arch : LayerArch,
            pt_arch : LayerArch,
            pos_arch : LayerArch,
            stats_arch : LayerArch,
            mlbvalue_arch : LayerArch,
            mlbstat_arch : LayerArch,
            lr_list: list[float],
            wd_list: list[float],
            repeats: int) -> MultiHeadEvalResult:
    
    result = MultiHeadEvalResult()
    
    for i in range(repeats):
        train_dataset, test_dataset = Create_Test_Train_Datasets(
                    player_list=io_list, 
                    is_hitter=is_hitter,
                    train_idx=i)
        
        pro_network = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            data_prep=data_prep.pro_data_prep,
            is_hitter=is_hitter,
            
            lvl_arch=level_arch,
            pa_arch=pa_arch,
            pt_arch=pt_arch,
            stats_arch=stats_arch,
            pos_arch=pos_arch,
            mlbstat_arch=mlbstat_arch,
            val_arch=mlbvalue_arch,
            
            weight_decay=wd_list,
            learning_rates=lr_list,
        ).to(device)
        col_network = Col_Model(
            input_size=train_dataset.GetColInputSize(),
            data_prep=data_prep.college_data_prep,
            is_hitter=is_hitter,
            output_init_state_size=pro_network.GetInitStateSize(),
        ).to(device)
        
        train_results = TrainAndGraph(
            pro_network=pro_network,
            col_network=col_network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            is_hitter=is_hitter,
            should_output=False,
            col_model_name="../../Models/no_name_col",
            pro_model_name="../../Models/no_name_pro",
        )
        
        del train_dataset
        del test_dataset
        del pro_network
        del col_network
        torch.cuda.empty_cache()
        gc.collect()
        
        def RangeBound(f : float) -> float:
            MAX_VALUE = 1000
            # NaN check
            if f != f: 
                return MAX_VALUE
            
            return min(f, MAX_VALUE)
            
        
        result.pa += RangeBound(train_results.best_loss_pa)
        result.pos += RangeBound(train_results.best_loss_pos)
        result.pt += RangeBound(train_results.best_loss_pt)
        result.stats += RangeBound(train_results.best_loss_stats)
        result.level += RangeBound(train_results.best_loss_level)
        result.mlbstat += RangeBound(train_results.best_loss_mlbstat)
        result.mlbvalue += RangeBound(train_results.best_loss_mlbvalue)
        
    return result

def objective(
            trials : list[optuna.trial.Trial],
            recipes: list[ProModelHeadTuningRecipe],
            io_list: list[Combined_IO],
            data_prep: Combined_Data_Prep,
            is_hitter: bool,
            width: SearchWidth,
            repeats : int) -> list[float]:
    
    p = resolve_params(trials=trials, recipes=recipes, width=width, is_hitter=is_hitter)
    
    level_arch = LayerArch(num_layers=p["level_layers"], layer_size=p["level_size"],
                            nonlin=ACTIVATION_MAP[p["level_activation"]])
    pa_arch = LayerArch(num_layers=p["pa_layers"], layer_size=p["pa_size"],
                            nonlin=ACTIVATION_MAP[p["pa_activation"]])
    stats_arch = LayerArch(num_layers=p["stats_layers"], layer_size=p["stats_size"],
                            nonlin=ACTIVATION_MAP[p["stats_activation"]])
    pos_arch = LayerArch(num_layers=p["pos_layers"], layer_size=p["pos_size"],
                            nonlin=ACTIVATION_MAP[p["pos_activation"]])
    pt_arch = LayerArch(num_layers=p["pt_layers"], layer_size=p["pt_size"],
                            nonlin=ACTIVATION_MAP[p["pt_activation"]])
    mlbstat_arch = LayerArch(num_layers=p["mlbstat_layers"], layer_size=p["mlbstat_size"],
                            nonlin=ACTIVATION_MAP[p["mlbstat_activation"]])
    mlbvalue_arch = LayerArch(num_layers=p["mlbvalue_layers"], layer_size=p["mlbvalue_size"],
                            nonlin=ACTIVATION_MAP[p["mlbvalue_activation"]])
    
    lr_list = list(DEFAULT_LEARNING_RATES if is_hitter else DEFAULT_LEARNING_RATES_P)
    wd_list = list(DEFAULT_PRO_WEIGHT_DECAY if is_hitter else DEFAULT_PRO_WEIGHT_DECAY_P)
    
    lr_list[LOSS_IDX_LEVEL], wd_list[LOSS_IDX_LEVEL] = p["lr_level"], p["wd_level"]
    lr_list[LOSS_IDX_PA], wd_list[LOSS_IDX_PA] = p["lr_pa"], p["wd_pa"]
    lr_list[LOSS_IDX_STATS], wd_list[LOSS_IDX_STATS] = p["lr_stats"], p["wd_stats"]
    lr_list[LOSS_IDX_POS], wd_list[LOSS_IDX_POS] = p["lr_pos"], p["wd_pos"]
    lr_list[LOSS_IDX_PT], wd_list[LOSS_IDX_PT] = p["lr_pt"], p["wd_pt"]
    lr_list[LOSS_IDX_MLBSTAT], wd_list[LOSS_IDX_MLBSTAT] = p["lr_mlbstat"], p["wd_mlbstat"]
    lr_list[LOSS_IDX_MLBVALUE], wd_list[LOSS_IDX_MLBVALUE] = p["lr_mlbvalue"], p["wd_mlbvalue"]

    result = run_evaluation(
        io_list=io_list,
        data_prep=data_prep,
        is_hitter=is_hitter,
        level_arch=level_arch,
        pa_arch=pa_arch,
        pt_arch=pt_arch,
        pos_arch=pos_arch,
        stats_arch=stats_arch,
        mlbvalue_arch=mlbvalue_arch,
        mlbstat_arch=mlbstat_arch,
        lr_list=lr_list,
        wd_list=wd_list,
        repeats=repeats
    )
    
    results = [GetHeadTypeResult(result, r) for r in recipes]
    return results
    
    
def trial_runner(
    recipes : list[ProModelHeadTuningRecipe],
    num_trials : int,
    is_hitter : bool,
    width : SearchWidth,
    data_prep: Combined_Data_Prep,
    io_list: list[Combined_IO],
    repeats : int = 3
) -> list[Study]:
    
    # Create studies
    studies : list[Study] = []
    for recipe in recipes:
        study_name = f"Pro_{"Hitter" if is_hitter else "Pitcher"}_{recipe.name}_Tuning_{width.name}"
        optuna.delete_study(study_name=study_name, storage="sqlite:///Pro_Tune.db")
        study = optuna.create_study(
        direction="minimize",
        load_if_exists=True,
        study_name=study_name,
        storage="sqlite:///Pro_Tune.db"
        )
        studies.append(study)
        
    for _ in tqdm(range(num_trials), desc="Trials"):
        trials = [study.ask() for study in studies]
        results = objective(
            trials=trials,
            recipes=recipes,
            io_list=io_list,
            data_prep=data_prep,
            is_hitter=is_hitter,
            width=width,
            repeats=repeats
        )
        for i in range(len(studies)):
            studies[i].tell(trials[i], results[i])
            
    return studies