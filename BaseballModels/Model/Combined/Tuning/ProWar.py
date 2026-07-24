import optuna
import gc
from enum import Flag, auto, Enum
import torch.nn.functional as F
import math
from dataclasses import dataclass

from Model.Combined.Model.Model_Train import TrainAndGraph, DEFAULT_BATCH_SIZE, DEFAULT_NUM_EPOCHS, DEFAULT_PRO_ELEMENT_LOSS_SCALES, DEFAULT_PRO_ELEMENT_LOSS_SCALES_P, DEFAULT_BATCH_SIZE_P, DEFAULT_NUM_EPOCHS_P
from Model.Pro.Model.Player_Model import Recurrent_Model as Pro_Model, LayerArch
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Pro.Model.Player_Model import *
from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Combined.Utilities.GetVariableLossIndex import GetVariableLossIndex
from Model.Constants import device

_ACTIVATION_FUNCTIONS = ["ReLU", "LeakyReLU", "GELU", "SiLU", "Tanh"]
_ACTIVATION_MAP = {
        "ReLU": F.relu,
        "LeakyReLU": F.leaky_relu,
        "GELU": F.gelu,
        "SiLU": F.silu,
        "Tanh": F.tanh,
    }

# Configurable Explore/Exploit balance
class SearchWidth(Enum):
    WIDE = auto()
    NARROW = auto()

# What hyperparameters to tune
class ProModelTuningRecipe(Flag):
    RECURRENT = auto()
    INIT_HIDDEN = auto()
    WAR_ARCH = auto()
    BATCH_PARAMS = auto()
    LOSS_SCALES = auto()

@dataclass(frozen=True)
class ParamSpec:
    name: str
    low: float = -1
    high: float = -1
    log: bool = False
    is_int: bool = False
    choices: list[str] | None = None
    narrow_frac: float = 0.2 # How much range (% of value) is varied in narrow test

    def __post_init__(self):
        if self.choices is None:
            assert self.low < self.high, f"ParamSpec '{self.name}': low must be < high"

    def suggest(self, trial: optuna.trial.Trial, default, width: SearchWidth):
        if self.choices is not None:
            if width is SearchWidth.NARROW:
                return default
            return trial.suggest_categorical(self.name, self.choices)

        low, high = self.low, self.high
        if width is SearchWidth.NARROW:
            span = self.narrow_frac * (self.high - self.low)
            low = max(low, default - span)
            high = min(high, default + span)
            
            # Low int should always round down, high int round up
            if self.is_int:
                low = round(low) if self.low == 0 else math.floor(low)
                high = math.ceil(high)

        if self.is_int:
            return trial.suggest_int(self.name, int(round(low)), int(round(high)), log=self.log)
        return trial.suggest_float(self.name, low, high, log=self.log)

SEARCH_SPACE: dict[ProModelTuningRecipe, list[ParamSpec]] = {
    ProModelTuningRecipe.RECURRENT: [
        ParamSpec("num_layers", 2, 4, is_int=True),
        ParamSpec("hidden_size", 16, 96, is_int=True),
        ParamSpec("dropout", 0.0, 0.5),
        ParamSpec("wd_shared", 1e-3, 1e-1, log=True),
        ParamSpec("lr_shared", 5e-4, 1e-2, log=True),
        ParamSpec("rnn_activation", choices=["relu", "tanh"]),
    ],
    ProModelTuningRecipe.WAR_ARCH: [
        ParamSpec("war_layers", 2, 6, is_int=True),
        ParamSpec("war_size", 4, 128, is_int=True),
        ParamSpec("war_activation", choices=_ACTIVATION_FUNCTIONS),
        ParamSpec("lr_war", 1e-4, 1e-1, log=True),
        ParamSpec("wd_war", 1e-7, 1e-2, log=True),
    ],
    ProModelTuningRecipe.INIT_HIDDEN: [
        ParamSpec("init_input_size", 4, 128, is_int=True),
        ParamSpec("init_layers", 2, 6, is_int=True),
        ParamSpec("init_size", 4, 128, is_int=True),
        ParamSpec("init_activation", choices=_ACTIVATION_FUNCTIONS),
    ],
    ProModelTuningRecipe.BATCH_PARAMS: [
        ParamSpec("batch_size", 400, 1600, is_int=True),
        ParamSpec("num_epochs", 30, 60, is_int=True),
    ],
    ProModelTuningRecipe.LOSS_SCALES: [
        ParamSpec(f"loss_scale_{i}", 1e-6, 10.0, log=True)
        for i in range(len(DEFAULT_PRO_ELEMENT_LOSS_SCALES))
    ],
}

_ACTIVATION_NAME = {v: k for k, v in _ACTIVATION_MAP.items()}
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
    "init_input_size": DEFAULT_INIT_STATE_SIZE,
    "init_layers": DEFAULT_INIT_STATE_ARCH.num_layers,
    "init_size": DEFAULT_INIT_STATE_ARCH.layer_size,
    "init_activation": _ACTIVATION_NAME[DEFAULT_INIT_STATE_ARCH.nonlin],
    "batch_size": DEFAULT_BATCH_SIZE,
    "num_epochs": DEFAULT_NUM_EPOCHS,
    
    # Loss Scale Factor
    **{f"loss_scale_{i}": v
       for i, v in enumerate(DEFAULT_PRO_ELEMENT_LOSS_SCALES)},
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
    "war_activation": _ACTIVATION_NAME[DEFAULT_WAR_ARCH.nonlin],
    "lr_war": DEFAULT_LEARNING_RATES_P[1],
    "wd_war": DEFAULT_PRO_WEIGHT_DECAY_P[1],
    "init_input_size": DEFAULT_INIT_STATE_SIZE_P,
    "init_layers": DEFAULT_INIT_STATE_ARCH_P.num_layers,
    "init_size": DEFAULT_INIT_STATE_ARCH_P.layer_size,
    "init_activation": _ACTIVATION_NAME[DEFAULT_INIT_STATE_ARCH.nonlin],
    "batch_size": DEFAULT_BATCH_SIZE_P,
    "num_epochs": DEFAULT_NUM_EPOCHS_P,
    
    # Loss Scale Factor
    **{f"loss_scale_{i}": v
       for i, v in enumerate(DEFAULT_PRO_ELEMENT_LOSS_SCALES_P)},
}

def resolve_params(
        trial: optuna.trial.Trial,
        recipe: ProModelTuningRecipe,
        width: SearchWidth,
        is_hitter: bool) -> dict:
    
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
            init_arch: LayerArch,
            lr_list: list[float],
            wd_list: list[float],
            loss_scales: list[float] | None = None,
            max_repeats: int = 3) -> float:
    
    WAR_MAX = 30
    cutoff_fold_1, exit_values_23 = (7.9, 7.4 + 7.7) if is_hitter else (9.5, 9.5 + 9.1)

    sum_war = 0
    for i in range(max_repeats):
        train_dataset, test_dataset = Create_Test_Train_Datasets(
            player_list=io_list, 
            is_hitter=is_hitter,
            train_idx=i)
        
        
        pro_network = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            data_prep=data_prep.pro_data_prep,
            is_hitter=is_hitter,
            
            recurrent_dropout=p["dropout"],
            num_layers=p["num_layers"],
            hidden_size=p["hidden_size"],
            
            war_arch=war_arch,
            
            weight_decay=wd_list,
            learning_rates=lr_list,
            
            rnn_nonlinearity=p["rnn_activation"],
            
            init_state_arch=init_arch,
            init_state_size=p["init_input_size"],
        ).to(device)
        # Create variant to test
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
            pro_element_loss_scales=loss_scales,
            col_model_name="../../Models/no_name_col",
            pro_model_name="../../Models/no_name_pro",
        )
        
        del train_dataset
        del test_dataset
        del pro_network
        del col_network
        torch.cuda.empty_cache()
        gc.collect()
        
        sum_war += train_results.best_loss
        
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
    loss_scales = [p[f"loss_scale_{i}"]
        for i in range(len(DEFAULT_PRO_ELEMENT_LOSS_SCALES))]

    war_arch = LayerArch(num_layers=p["war_layers"], layer_size=p["war_size"],
                         nonlin=_ACTIVATION_MAP[p["war_activation"]])
    init_arch = LayerArch(num_layers=p["init_layers"], layer_size=p["init_size"],
                          nonlin=_ACTIVATION_MAP[p["init_activation"]])

    lr_list = list(DEFAULT_LEARNING_RATES if is_hitter else DEFAULT_LEARNING_RATES_P)
    wd_list = list(DEFAULT_PRO_WEIGHT_DECAY if is_hitter else DEFAULT_PRO_WEIGHT_DECAY_P)
    lr_list[0], lr_list[1] = p["lr_shared"], p["lr_war"]
    wd_list[0], wd_list[1] = p["wd_shared"], p["wd_war"]

    return run_evaluation(
        io_list=io_list, data_prep=data_prep, is_hitter=is_hitter,
        p=p, war_arch=war_arch, init_arch=init_arch,
        loss_scales=loss_scales,
        lr_list=lr_list, wd_list=wd_list, max_repeats=max_repeats)