import optuna
import numpy as np
import torch
import torch.nn.functional as F
from Combined.Model.Model_Train import TrainAndGraph
from Pro.Model.Player_Model import RNN_Model as Pro_Model, LayerArch
from College.Model.College_Model import RNN_Model as Col_Model
from Pro.Model.Player_Model import DEFAULT_LEARNING_RATES, DEFAULT_LEARNING_RATES_P
from Pro.Model.Player_Model import DEFAULT_PRO_WEIGHT_DECAY, DEFAULT_PRO_WEIGHT_DECAY_P
from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Utilities.GetVariableLossIndex import GetVariableLossIndex
from Constants import device

_ACTIVATION_FUNCTIONS = ["ReLU", "LeakyReLU", "GELU", "SiLU", "Tanh"]
_ACTIVATION_MAP = {
        "ReLU": F.relu,
        "LeakyReLU": F.leaky_relu,
        "GELU": F.gelu,
        "SiLU": F.silu,
        "Tanh": F.tanh,
    }

_MAX_LOSS = 9.0
_WAR_CUTOFF = 7.35

def objective(
            trial : optuna.trial.Trial, 
            train_dataset : Combined_Player_Dataset,
            test_dataset : Combined_Player_Dataset,
            data_prep : Combined_Data_Prep,
            is_hitter : bool,
            max_repeats : int = 3) -> float:
    
    # Hyperparameters
    num_layers = trial.suggest_int('num_layers', 1, 8)
    hidden_size = trial.suggest_int('hidden_size', 4, 128)
    dropout = trial.suggest_float('dropout', 0, 0.5)
    rnn_activation = trial.suggest_categorical('rnn_activation', ["relu", "tanh"])
    
    war_binary_numlayers = trial.suggest_int('war_binary_layers', 1, 6)
    war_binary_layersize = trial.suggest_int('war_binary_size', 4, 128)
    war_binary_activation = trial.suggest_categorical('war_binary_activation', _ACTIVATION_FUNCTIONS)
    
    war_ordinal_numlayers = trial.suggest_int('war_ordinal_layers', 1, 6)
    war_ordinal_layersize = trial.suggest_int('war_ordinal_size', 4, 128)
    war_ordinal_activation = trial.suggest_categorical('war_ordinal_activation', _ACTIVATION_FUNCTIONS)
    
    lr_shared = trial.suggest_float("lr_shared", 1e-5, 5e-2, log=True)
    lr_war_binary = trial.suggest_float("lr_war_binary", 1e-5, 5e-2, log=True)
    lr_war_ordinal = trial.suggest_float("lr_war_ordinal", 1e-5, 5e-2, log=True)
    
    lr_list = DEFAULT_LEARNING_RATES if is_hitter else DEFAULT_LEARNING_RATES_P
    lr_list[0] = lr_shared
    lr_list[1] = lr_war_binary
    lr_list[2] = lr_war_ordinal
    
    wd_shared = trial.suggest_float("wd_shared", 1e-7, 1e-2, log=True)
    wd_war_binary = trial.suggest_float("wd_war_binary", 1e-7, 1e-2, log=True)
    wd_war_ordinal = trial.suggest_float("wd_war_ordinal", 1e-7, 1e-2, log=True)
    
    wd_list = DEFAULT_PRO_WEIGHT_DECAY if is_hitter else DEFAULT_PRO_WEIGHT_DECAY_P
    wd_list[0] = wd_shared
    wd_list[1] = wd_war_binary
    wd_list[2] = wd_war_ordinal
    
    loss_index = GetVariableLossIndex(name="WAR", is_pro=True, is_hitter=is_hitter)
    
    metrics = []
    for i in range(max_repeats):
        war_binary_arch = LayerArch(num_layers=war_binary_numlayers, layer_size=war_binary_layersize, nonlin=_ACTIVATION_MAP[war_binary_activation])
        war_ordinal_arch = LayerArch(num_layers=war_ordinal_numlayers, layer_size=war_ordinal_layersize, nonlin=_ACTIVATION_MAP[war_ordinal_activation])
        
        pro_network = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            mutators=torch.empty(0),
            data_prep=data_prep.pro_data_prep,
            is_hitter=is_hitter,
            
            rnn_droupout=dropout,
            num_layers=num_layers,
            hidden_size=hidden_size,
            
            war_binary_arch=war_binary_arch,
            war_binary_arch_p=war_binary_arch,
            war_ordinal_arch=war_ordinal_arch,
            war_ordinal_arch_p=war_ordinal_arch,
            
            weight_decay=wd_list,
            weight_decay_p=wd_list,
            learning_rates=lr_list,
            learning_rates_p=lr_list,
            
            rnn_nonlinearity=rnn_activation
        ).to(device)
        # Create variant to test
        col_network = Col_Model(
            input_size=train_dataset.GetColInputSize(),
            data_prep=data_prep.college_data_prep,
            is_hitter=is_hitter,
            output_hidden_size=pro_network.GetHiddenSize(),
            output_num_layers=pro_network.GetNumLayers(),
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
        
        metrics.append(train_results.test_losses[loss_index][train_results.best_epoch])
        
        # Check if it should exit early
        current_mean = np.mean(metrics)
        if current_mean > _WAR_CUTOFF:
            break
        
    return min(np.mean(metrics), _MAX_LOSS)