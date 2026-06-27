from tqdm import tqdm

from Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Model.RunEpoch import RunEpoch
from Combined.Model.BuildPlots import BuildPlots
from Combined.Model.GetPlayerClassDistribution import GetPlayerClassDistribution
import torch

from Combined.Utilities.Types import *

SHOULD_PROFILE = False
DEFAULT_PRO_ELEMENT_LOSS_SCALES = [1e-1, 1, 1e-1, 1e-3, 1, 1, 1, 1e-1]

def TrainAndGraph(
    pro_network : Pro_Model,
    col_network : Col_Model,
    train_dataset : Combined_Player_Dataset,
    test_dataset : Combined_Player_Dataset,
    is_hitter : bool,
    num_epochs : int = 201,
    batch_size : int = 1200,
    logging_interval : int = 10,
    should_output : bool = True,
    pro_model_name : str = "no_name_pro",
    col_model_name : str = "no_name_col",
    get_end_loss : bool = False,
    element_to_save : int = 0,
    early_stopping_cutoff : int = 20,
    pro_element_loss_scales : list[int] = DEFAULT_PRO_ELEMENT_LOSS_SCALES,
    timestep_pct_cutoff : float = 1.0,
) -> tuple[float, float, int]:
    
    num_pro_elements = NUM_ELEMENTS
    if is_hitter:
        num_col_elements = NUM_ELEMENTS_HITTER
        col_element_list = HITTER_ELEMENT_LIST
    else:
        num_col_elements = NUM_ELEMENTS_PITCHER
        col_element_list = PITCHER_ELEMENT_LIST
    num_elements = num_pro_elements + num_col_elements
    element_list = [f"PRO {e}" for e in ELEMENT_LIST] + [f"COL {e}" for e in col_element_list]
        
    train_history : list[EpochResult] = []
    test_history : list[EpochResult] = []
    epoch_counter : list[int] = []
    
    best_loss = 999999
    best_loss_college = 999999
    best_epoch = -1
    epochs_since_improve = 0
    
    col_optimizer = torch.optim.Adam(col_network.parameters(), lr=0.0025)
    
    # Per-player (not per-timestep) WAR class distribution, for verification vs SQL
    train_player_dist = GetPlayerClassDistribution(train_dataset, batch_size)
    test_player_dist = GetPlayerClassDistribution(test_dataset, batch_size)
    if should_output:
        print("Train per-player WAR class distribution (%):",
              [f"{p:.3f}" for p in train_player_dist])
        print("Test  per-player WAR class distribution (%):",
              [f"{p:.3f}" for p in test_player_dist])
    
    iterable = range(num_epochs)
    if not should_output:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_result, test_result = RunEpoch(pro_network, col_network, train_dataset, test_dataset, is_hitter, num_pro_elements, num_col_elements, batch_size, pro_element_loss_scales, col_optimizer)
        train_history.append(train_result)
        test_history.append(test_result)
        epoch_counter.append(epoch)
        
        
        if should_output and (epoch % logging_interval == 0):  
            print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch + 1, num_epochs, train_result.avg_loss[element_to_save], test_result.avg_loss[element_to_save]))
        
        if test_result.avg_loss[element_to_save] < best_loss:
            best_loss = test_result.avg_loss[element_to_save]
            best_loss_college = test_result.avg_loss[num_pro_elements + 1]
            best_epoch = epoch
            epochs_since_improve = 0
            torch.save(col_network.state_dict(), col_model_name + ".pt")
            torch.save(pro_network.state_dict(), pro_model_name + ".pt")
        else:
            epochs_since_improve += 1
           
        if epochs_since_improve >= early_stopping_cutoff:
            if should_output:
                print(f"Exited Early at epoch={epoch}")
            break
            
    if should_output:
        print(f"Best result at epoch={best_epoch} loss={best_loss}")
        BuildPlots(epoch_counter=epoch_counter, train_history=train_history, test_history=test_history,
            element_list=element_list, pro_network=pro_network, col_network=col_network, train_dataset=train_dataset,
            test_dataset=test_dataset, is_hitter=is_hitter, batch_size=batch_size, timestep_pct_cutoff=timestep_pct_cutoff)
        
    if get_end_loss:
        return test_result.avg_loss[element_to_save], epoch
    else:
        return best_loss, best_loss_college, epoch