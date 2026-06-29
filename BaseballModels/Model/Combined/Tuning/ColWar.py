import optuna
import numpy as np

from Combined.Model.Model_Train import TrainAndGraph
from Pro.Model.Player_Model import RNN_Model as Pro_Model, LayerArch
from College.Model.College_Model import RNN_Model as Col_Model
from College.Model.College_Model import DEFAULT_LEARNING_RATE, DEFAULT_LEARNING_RATE_P
from College.Model.College_Model import DEFAULT_WEIGHT_DECAY, DEFAULT_WEIGHT_DECAY_P
from College.DataPrep.Data_Prep import College_Data_Prep
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Utilities.GetVariableLossIndex import GetVariableLossIndex
from Constants import device

def objective(
            trial : optuna.trial.Trial, 
            pro_network : Pro_Model,
            train_dataset : Combined_Player_Dataset,
            test_dataset : Combined_Player_Dataset,
            data_prep : College_Data_Prep,
            is_hitter : bool,
            num_repeats : int = 3) -> float:
    
    # Hyperparameters
    num_layers = trial.suggest_int('num_layers', 1, 8)
    hidden_size = trial.suggest_int('hidden_size', 4, 64)
    noise = trial.suggest_float('noise', 0, 0.5)
    dropout = trial.suggest_float('dropout', 0, 0.5)
    
    wararch_numlayers = trial.suggest_int('layerarch_layers', 1, 4)
    wararch_layersize = trial.suggest_int('layerarch_size', 4, 128)
    
    lr_shared = trial.suggest_float("lr_shared", 1e-5, 1e-1, log=True)
    lr_war = trial.suggest_float("lr_war", 1e-5, 1e-1, log=True)
    lr_hidden = trial.suggest_float("lr_hidden", 1e-5, 1e-1, log=True)
    
    lr_list = DEFAULT_LEARNING_RATE if is_hitter else DEFAULT_LEARNING_RATE_P
    lr_list[0] = lr_shared
    lr_list[2] = lr_war
    lr_list[-1] = lr_hidden
    
    wd_shared = trial.suggest_float("wd_shared", 1e-7, 1e-2, log=True)
    wd_war = trial.suggest_float("wd_war", 1e-7, 1e-2, log=True)
    wd_hidden = trial.suggest_float("wd_hidden", 1e-7, 1e-2, log=True)
    
    wd_list = DEFAULT_WEIGHT_DECAY if is_hitter else DEFAULT_WEIGHT_DECAY_P
    wd_list[0] = wd_shared
    wd_list[2] = wd_war
    wd_list[-1] = wd_hidden
    
    loss_index = GetVariableLossIndex(name="WAR", is_pro=False, is_hitter=is_hitter)
    
    metrics = []
    for _ in range(num_repeats):
        # Create variant to test
        col_network = Col_Model(
            input_size=train_dataset.GetColInputSize(),
            data_prep=data_prep,
            is_hitter=is_hitter,
            output_hidden_size=pro_network.GetHiddenSize(),
            output_num_layers=pro_network.GetNumLayers(),
            
            num_layers=num_layers,
            hidden_size=hidden_size,
            noise=noise,
            dropout=dropout,
            
            lr=lr_list,
            lr_p=lr_list,
            weight_decay=wd_list,
            weight_decay_p=wd_list,
            
            war_arch=LayerArch(num_layers=wararch_numlayers, layer_size=wararch_layersize)
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
        
    return min(np.mean(metrics), 0.2)