import optuna
import gc
from enum import Flag, auto
import torch.nn.functional as F
from Model.Combined.Model.Model_Train import TrainAndGraph, DEFAULT_BATCH_SIZE, DEFAULT_NUM_EPOCHS
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
_RECURRENT_TYPES = ["RNN", "GRU", "LSTM"]

WAR_CUTOFF_FOLD_1 = 7.9 # Value above which folds 2-3 won't be used
WAR_EXIT_VALUES_23 = 7.4 + 7.7 # Value for folds 2-3 if not run

WAR_CUTOFF_FOLD_1_P = 9.5
WAR_EXIT_VALUES_23_P = 9.5 + 9.1

WAR_MAX = 30

class ProModelTuningRecipe(Flag):
    RECURRENT = auto()
    RECURRENT_TYPE = auto()
    INIT_HIDDEN = auto()
    WAR_ARCH = auto()
    BATCH_PARAMS = auto()

def objective(
            trial : optuna.trial.Trial, 
            io_list : list[Combined_IO],
            data_prep : Combined_Data_Prep,
            is_hitter : bool,
            recipe : ProModelTuningRecipe,
            max_repeats : int = 3) -> float:
    
    global MAX_LOSS, WAR_CUTOFF_FOLD_1, WAR_EXIT_VALUES_23
    if not is_hitter:
        WAR_CUTOFF_FOLD_1 = WAR_CUTOFF_FOLD_1_P
        WAR_EXIT_VALUES_23 = WAR_EXIT_VALUES_23_P
    
    lr_list = list(DEFAULT_LEARNING_RATES if is_hitter else DEFAULT_LEARNING_RATES_P)
    wd_list = list(DEFAULT_PRO_WEIGHT_DECAY if is_hitter else DEFAULT_PRO_WEIGHT_DECAY_P)
    
    # Recurrent Type
    if recipe & ProModelTuningRecipe.RECURRENT_TYPE:
        recurrent_type = trial.suggest_categorical('recurrent_type', _RECURRENT_TYPES)
    else:
        recurrent_type = DEFAULT_RECURRENT_TYPE if is_hitter else DEFAULT_RECURRENT_TYPE_P
    
    # RNN Hyperparameters
    if recipe & (ProModelTuningRecipe.RECURRENT | ProModelTuningRecipe.RECURRENT_TYPE):
        num_layers = trial.suggest_int('num_layers', 2, 4)
        hidden_size = trial.suggest_int('hidden_size', 16, 96)
        dropout = trial.suggest_float('dropout', 0, 0.5)
        wd_shared = trial.suggest_float("wd_shared", 1e-3, 1e-1, log=True)
        lr_shared = trial.suggest_float("lr_shared", 5e-4, 1e-2, log=True)
        
        if recurrent_type == "RNN":
            rnn_activation = trial.suggest_categorical('rnn_activation', ["relu", "tanh"])
        else:
            rnn_activation = DEFAULT_RNN_NONLINEARITY # Ignored by GRU/LSTM
        
        wd_list[0] = wd_shared
        lr_list[0] = lr_shared
    else:
        num_layers = DEFAULT_PRO_NUM_LAYERS if is_hitter else DEFAULT_PRO_NUM_LAYERS_P
        hidden_size = DEFAULT_PRO_HIDDEN_SIZE if is_hitter else DEFAULT_PRO_HIDDEN_SIZE_P
        dropout = DEFAULT_DROPOUT if is_hitter else DEFAULT_DROPOUT_P
        rnn_activation = DEFAULT_RNN_NONLINEARITY if is_hitter else DEFAULT_RNN_NONLINEARITY_P
    
    # War Output Hyperparameters
    if recipe & ProModelTuningRecipe.WAR_ARCH:
        war_numlayers = trial.suggest_int('war_layers', 2, 6)
        war_layersize = trial.suggest_int('war_size', 4, 128)
        war_activation = trial.suggest_categorical('war_activation', _ACTIVATION_FUNCTIONS)
        war_arch = LayerArch(num_layers=war_numlayers, layer_size=war_layersize, nonlin=_ACTIVATION_MAP[war_activation])
        lr_war = trial.suggest_float("lr_war", 1e-4, 1e-1, log=True)
        wd_war = trial.suggest_float("wd_war", 1e-7, 1e-2, log=True)
        
        lr_list[1] = lr_war
        wd_list[1] = wd_war
    else:
        war_arch = DEFAULT_WAR_ARCH if is_hitter else DEFAULT_WAR_ARCH_P
    
    # Initial State Hyperparameters
    if recipe & ProModelTuningRecipe.INIT_HIDDEN:
        init_size = trial.suggest_int('init_input_size', 4, 128)
        init_numlayers = trial.suggest_int('init_layers', 2, 6)
        init_layersize = trial.suggest_int('init_size', 4, 128)
        init_activation = trial.suggest_categorical('init_activation', _ACTIVATION_FUNCTIONS)
        init_arch = LayerArch(num_layers=init_numlayers, layer_size=init_layersize, nonlin=_ACTIVATION_MAP[init_activation])
    else:
        init_size = DEFAULT_INIT_STATE_SIZE if is_hitter else DEFAULT_INIT_STATE_SIZE_P
        init_arch = DEFAULT_INIT_STATE_ARCH if is_hitter else DEFAULT_INIT_STATE_ARCH_P
    
    # Batch size and num epochs
    if recipe & ProModelTuningRecipe.BATCH_PARAMS:
        batch_size = trial.suggest_int("batch_size", 400, 1600)
        num_epochs = trial.suggest_int("num_epochs", 30, 60)
    else:
        batch_size = DEFAULT_BATCH_SIZE
        num_epochs = DEFAULT_NUM_EPOCHS
    
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
            
            recurrent_dropout=dropout,
            num_layers=num_layers,
            hidden_size=hidden_size,
            
            war_arch=war_arch,
            
            weight_decay=wd_list,
            learning_rates=lr_list,
            
            recurrent_type=recurrent_type,
            rnn_nonlinearity=rnn_activation,
            
            init_state_arch=init_arch,
            init_state_size=init_size,
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
            batch_size=batch_size,
            num_epochs=num_epochs,
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
        if i == 0 and sum_war > WAR_CUTOFF_FOLD_1:
            sum_war += WAR_EXIT_VALUES_23
            break
        
    return min(sum_war, WAR_MAX)