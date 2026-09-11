import optuna
import gc
from enum import Flag, auto

from Model.Combined.Model.Model_Train import TrainAndGraph, DEFAULT_BATCH_SIZE, DEFAULT_NUM_EPOCHS, DEFAULT_BATCH_SIZE_P, DEFAULT_NUM_EPOCHS_P
from Model.Pro.Model.Player_Model import Recurrent_Model as Pro_Model, LayerArch
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Pro.Model.Player_Model import *
from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Constants import device
from Model.Combined.Tuning.ProTuningShared import *

_NUM_GRAD_SCALES = len(DEFAULT_HITTER_GRAD_SCALES)
assert _NUM_GRAD_SCALES == len(DEFAULT_PITCHER_GRAD_SCALES), \
    "hitter/pitcher trunk-grad-scale vectors must be the same length"

# What hyperparameters to tune
class ProModelTuningRecipe(Flag):
    RECURRENT = auto()
    INIT_HIDDEN = auto()
    SHARED_OPTIM = auto()
    
    DATAINIT_ARCH = auto()
    WAR_ARCH = auto()
    
    BATCH_PARAMS = auto()
    TRUNK_GRAD_SCALES = auto()



SEARCH_SPACE: dict[ProModelTuningRecipe, list[ParamSpec]] = {
    ProModelTuningRecipe.RECURRENT: [
        ParamSpec("num_layers", 2, 4, is_int=True),
        ParamSpec("hidden_size", 16, 96, is_int=True),
        ParamSpec("dropout", 0.0, 0.5),
        ParamSpec("rnn_activation", choices=["relu", "tanh"]),
    ],
    ProModelTuningRecipe.SHARED_OPTIM: [
    ParamSpec("lr_shared", 5e-4, 1e-2, log=True),
    ParamSpec("wd_shared", 1e-3, 1e-1, log=True),
    ],
    ProModelTuningRecipe.DATAINIT_ARCH: [
        ParamSpec("datainit_layers", 2, 8, is_int=True),
        ParamSpec("datainit_size", 4, 128, is_int=True),
        ParamSpec("datainit_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_datainit", 1e-4, 1e-2, log=True),
        ParamSpec("wd_datainit", 1e-7, 1e-2, log=True),
    ],
    ProModelTuningRecipe.WAR_ARCH: [
        ParamSpec("war_layers", 2, 6, is_int=True),
        ParamSpec("war_size", 4, 128, is_int=True),
        ParamSpec("war_activation", choices=ACTIVATION_FUNCTIONS),
        ParamSpec("lr_war", 1e-4, 1e-2, log=True),
        ParamSpec("wd_war", 1e-7, 1e-2, log=True),
    ],
    ProModelTuningRecipe.INIT_HIDDEN: [
        ParamSpec("init_input_size", 4, 128, is_int=True),
        ParamSpec("init_layers", 2, 6, is_int=True),
        ParamSpec("init_size", 4, 128, is_int=True),
        ParamSpec("init_activation", choices=ACTIVATION_FUNCTIONS),
    ],
    ProModelTuningRecipe.BATCH_PARAMS: [
        ParamSpec("batch_size", 400, 1600, is_int=True),
        ParamSpec("num_epochs", 30, 60, is_int=True),
    ],
    ProModelTuningRecipe.TRUNK_GRAD_SCALES: [
        ParamSpec(f"grad_scale_{i}", 1e-3, 10, log=True)
        for i in range(_NUM_GRAD_SCALES)
        if i != 0 # WAR scale stays at 1.0
    ],
}

_ACTIVATION_NAME = {v: k for k, v in ACTIVATION_MAP.items()}
HITTER_DEFAULTS = {
    "num_layers": DEFAULT_PRO_NUM_LAYERS,
    "hidden_size": DEFAULT_PRO_HIDDEN_SIZE,
    "dropout": DEFAULT_DROPOUT,
    "wd_shared": DEFAULT_PRO_WEIGHT_DECAY[0],
    "lr_shared": DEFAULT_LEARNING_RATES[0],
    "rnn_activation": DEFAULT_RNN_NONLINEARITY,
    
    "war_layers": DEFAULT_WAR_ARCH.num_layers,
    "war_size": DEFAULT_WAR_ARCH.layer_size,
    "war_activation": _ACTIVATION_NAME[DEFAULT_WAR_ARCH.nonlin],
    "lr_war": DEFAULT_LEARNING_RATES[1],
    "wd_war": DEFAULT_PRO_WEIGHT_DECAY[1],
    
    "datainit_layers": DEFAULT_DATA_ARCH.num_layers,
    "datainit_size": DEFAULT_DATA_ARCH.layer_size,
    "datainit_activation": _ACTIVATION_NAME[DEFAULT_DATA_ARCH.nonlin],
    "lr_datainit": DEFAULT_LEARNING_RATES[9],
    "wd_datainit": DEFAULT_PRO_WEIGHT_DECAY[9],
    
    "init_input_size": DEFAULT_INIT_STATE_SIZE,
    "init_layers": DEFAULT_INIT_STATE_ARCH.num_layers,
    "init_size": DEFAULT_INIT_STATE_ARCH.layer_size,
    "init_activation": _ACTIVATION_NAME[DEFAULT_INIT_STATE_ARCH.nonlin],
    "batch_size": DEFAULT_BATCH_SIZE,
    "num_epochs": DEFAULT_NUM_EPOCHS,
    
    **{f"grad_scale_{i}": v for i, v in enumerate(DEFAULT_HITTER_GRAD_SCALES)},
}

PITCHER_DEFAULTS = {
    "num_layers": DEFAULT_PRO_NUM_LAYERS_P,
    "hidden_size": DEFAULT_PRO_HIDDEN_SIZE_P,
    "dropout": DEFAULT_DROPOUT_P,
    "wd_shared": DEFAULT_PRO_WEIGHT_DECAY_P[0],
    "lr_shared": DEFAULT_LEARNING_RATES_P[0],
    "rnn_activation": DEFAULT_RNN_NONLINEARITY_P,
    
    "war_layers": DEFAULT_WAR_ARCH_P.num_layers,
    "war_size": DEFAULT_WAR_ARCH_P.layer_size,
    "war_activation": _ACTIVATION_NAME[DEFAULT_WAR_ARCH_P.nonlin],
    "lr_war": DEFAULT_LEARNING_RATES_P[1],
    "wd_war": DEFAULT_PRO_WEIGHT_DECAY_P[1],
    
    "datainit_layers": DEFAULT_DATA_ARCH_P.num_layers,
    "datainit_size": DEFAULT_DATA_ARCH_P.layer_size,
    "datainit_activation": _ACTIVATION_NAME[DEFAULT_DATA_ARCH_P.nonlin],
    "lr_datainit": DEFAULT_LEARNING_RATES_P[9],
    "wd_datainit": DEFAULT_PRO_WEIGHT_DECAY_P[9],
    
    "init_input_size": DEFAULT_INIT_STATE_SIZE_P,
    "init_layers": DEFAULT_INIT_STATE_ARCH_P.num_layers,
    "init_size": DEFAULT_INIT_STATE_ARCH_P.layer_size,
    "init_activation": _ACTIVATION_NAME[DEFAULT_INIT_STATE_ARCH_P.nonlin],
    "batch_size": DEFAULT_BATCH_SIZE_P,
    "num_epochs": DEFAULT_NUM_EPOCHS_P,
    
    **{f"grad_scale_{i}": v for i, v in enumerate(DEFAULT_PITCHER_GRAD_SCALES)},
}

def AssertRecipeValid(recipe : ProModelTuningRecipe) -> None:
    if recipe & ProModelTuningRecipe.RECURRENT and not recipe & ProModelTuningRecipe.SHARED_OPTIM:
        assert(False)
    if recipe & ProModelTuningRecipe.TRUNK_GRAD_SCALES and not recipe & ProModelTuningRecipe.SHARED_OPTIM:
        assert(False)

def resolve_params(
        trial: optuna.trial.Trial,
        recipe: ProModelTuningRecipe,
        width: SearchWidth,
        is_hitter: bool) -> dict:
    
    AssertRecipeValid(recipe)
    
    defaults = HITTER_DEFAULTS if is_hitter else PITCHER_DEFAULTS
    params = dict(defaults)
    for flag, specs in SEARCH_SPACE.items():
        if recipe & flag:
            for spec in specs:
                params[spec.name] = spec.suggest(trial, defaults[spec.name], width)
    return params

def run_evaluation(
            io_list: list[Combined_IO],
            data_prep: Combined_Data_Prep,
            is_hitter: bool,
            p: dict,
            war_arch: LayerArch,
            datainit_arch : LayerArch,
            init_arch: LayerArch,
            lr_list: list[float],
            wd_list: list[float],
            max_repeats: int,
            trunk_grad_scales: list[float] | None = None) -> float:
    
    WAR_MAX = 30
    cutoff_fold_1, exit_values_23 = (7.9, 7.4 + 7.7) if is_hitter else (9.5, 9.5 + 9.1)

    sum_war = 0
    for i in range(max_repeats):
        train_dataset, test_dataset = Create_Test_Train_Datasets(
            player_list=io_list, 
            is_hitter=is_hitter,
            train_idx=i)
        
        # Create variant to test
        pro_network = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            data_prep=data_prep.pro_data_prep,
            is_hitter=is_hitter,
            
            recurrent_dropout=p["dropout"],
            num_layers=p["num_layers"],
            hidden_size=p["hidden_size"],
            
            data_arch=datainit_arch,
            war_arch=war_arch,
            
            weight_decay=wd_list,
            learning_rates=lr_list,
            
            rnn_nonlinearity=p["rnn_activation"],
            
            init_state_arch=init_arch,
            init_state_size=p["init_input_size"],
            
            trunk_grad_scales=trunk_grad_scales,
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
            batch_size=p["batch_size"],
            num_epochs=p["num_epochs"],
            col_model_name="../../Models/no_name_col",
            pro_model_name="../../Models/no_name_pro",
        )
        
        del train_dataset
        del test_dataset
        del pro_network
        del col_network
        torch.cuda.empty_cache()
        gc.collect()
        
        sum_war += train_results.best_loss_war
        
        # Check if it should exit early
        if i == 0 and sum_war > cutoff_fold_1:
            sum_war += exit_values_23
            break
        
    return min(sum_war, WAR_MAX)

def objective(
            trial: optuna.trial.Trial,
            io_list: list[Combined_IO],
            data_prep: Combined_Data_Prep,
            is_hitter: bool,
            recipe: ProModelTuningRecipe,
            width: SearchWidth,
            max_repeats: int = 3) -> float:

    p = resolve_params(trial, recipe, width, is_hitter)
    
    num_grad_scales = len(DEFAULT_HITTER_GRAD_SCALES if is_hitter
                          else DEFAULT_PITCHER_GRAD_SCALES)
    trunk_grad_scales = [p[f"grad_scale_{i}"] for i in range(num_grad_scales)]

    datainit_arch = LayerArch(num_layers=p["datainit_layers"], layer_size=p["datainit_size"],
                        nonlin=ACTIVATION_MAP[p["datainit_activation"]])
    war_arch = LayerArch(num_layers=p["war_layers"], layer_size=p["war_size"],
                         nonlin=ACTIVATION_MAP[p["war_activation"]])
    init_arch = LayerArch(num_layers=p["init_layers"], layer_size=p["init_size"],
                          nonlin=ACTIVATION_MAP[p["init_activation"]])

    lr_list = list(DEFAULT_LEARNING_RATES if is_hitter else DEFAULT_LEARNING_RATES_P)
    wd_list = list(DEFAULT_PRO_WEIGHT_DECAY if is_hitter else DEFAULT_PRO_WEIGHT_DECAY_P)
    lr_list[0], lr_list[1] = p["lr_shared"], p["lr_war"]
    wd_list[0], wd_list[1] = p["wd_shared"], p["wd_war"]
    lr_list[9], wd_list[9] = p["lr_datainit"], p["wd_datainit"]

    return run_evaluation(
        io_list=io_list, data_prep=data_prep, is_hitter=is_hitter,
        p=p, war_arch=war_arch, init_arch=init_arch, datainit_arch=datainit_arch,
        trunk_grad_scales=trunk_grad_scales,
        lr_list=lr_list, wd_list=wd_list, max_repeats=max_repeats)