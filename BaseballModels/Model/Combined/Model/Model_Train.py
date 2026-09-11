from tqdm import tqdm

from Model.Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from Model.College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Model.Pro.Model.Player_Model import Recurrent_Model as Pro_Model
from Model.Pro.Model.Player_Model import LOSS_IDX_WAR, LOSS_IDX_LEVEL, LOSS_IDX_PA, LOSS_IDX_STATS, LOSS_IDX_MLBSTAT, LOSS_IDX_MLBVALUE, LOSS_IDX_PT, LOSS_IDX_POS
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Model.Combined.Model.RunEpoch import RunEpoch
from Model.Combined.Model.BuildPlots import BuildPlots
from Model.Combined.Model.GetPlayerClassDistribution import GetPlayerClassDistribution
import torch

from Model.Combined.Utilities.Types import *
from Model.Utilities import GetPropertyValue

SHOULD_PROFILE = False

DEFAULT_BATCH_SIZE = 1038
DEFAULT_NUM_EPOCHS = 35
DEFAULT_BATCH_SIZE_P = 1276
DEFAULT_NUM_EPOCHS_P = 47

def TrainAndGraph(
    pro_network : Pro_Model,
    col_network : Col_Model,
    train_dataset : Combined_Player_Dataset,
    test_dataset : Combined_Player_Dataset,
    is_hitter : bool,
    num_epochs : int | None = None,
    batch_size : int | None = None,
    logging_interval : int = 10,
    should_output : bool = True,
    show_progress_bar : bool = False,
    pro_model_name : str = "no_name_pro",
    col_model_name : str = "no_name_col",
    timestep_pct_cutoff : float = 1.0,
) -> TrainResults:
    
    num_pro_elements = NUM_ELEMENTS
    num_epochs = GetPropertyValue(num_epochs, is_hitter, DEFAULT_NUM_EPOCHS, DEFAULT_NUM_EPOCHS_P)
    batch_size = GetPropertyValue(batch_size, is_hitter, DEFAULT_BATCH_SIZE, DEFAULT_BATCH_SIZE_P)
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
    
    # Schedulers
    pro_scheduler = torch.optim.lr_scheduler.CosineAnnealingLR(pro_network.optimizer, T_max=num_epochs)
    col_scheduler = torch.optim.lr_scheduler.CosineAnnealingLR(col_network.optimizer, T_max=num_epochs)
    
    iterable = range(num_epochs)
    if show_progress_bar:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_result, test_result = RunEpoch(pro_network, col_network, train_dataset, test_dataset, is_hitter, num_pro_elements, num_col_elements, batch_size)
        
        pro_scheduler.step()
        col_scheduler.step()
        
        train_history.append(train_result)
        test_history.append(test_result)
        epoch_counter.append(epoch)
        
        # Check if model is blowing up
        if epoch == 0:
            first_loss = test_result.avg_loss[LOSS_IDX_WAR]
        if epoch > 0:
            if test_result.avg_loss[LOSS_IDX_WAR] > first_loss * 2:
                break
            
        # If ever gets to NaN, report a really large number
        if test_result.avg_loss[LOSS_IDX_WAR] != test_result.avg_loss[LOSS_IDX_WAR]:
            test_result.avg_loss[LOSS_IDX_WAR] = 100
            best_loss = 100
            best_epoch = epoch
            break
        
        
        if should_output and (epoch % logging_interval == 0):  
            print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch + 1, num_epochs, train_result.avg_loss[LOSS_IDX_WAR], test_result.avg_loss[LOSS_IDX_WAR]))
        
        if epoch == num_epochs - 1:
            torch.save(col_network.state_dict(), col_model_name + ".pt")
            torch.save(pro_network.state_dict(), pro_model_name + ".pt")
            
    if should_output:
        print(f"End result loss={test_result.avg_loss[LOSS_IDX_WAR]}")
        BuildPlots(epoch_counter=epoch_counter, train_history=train_history, test_history=test_history,
            element_list=element_list, pro_network=pro_network, col_network=col_network, train_dataset=train_dataset,
            test_dataset=test_dataset, is_hitter=is_hitter, batch_size=batch_size, timestep_pct_cutoff=timestep_pct_cutoff)
        
    test_losses = []
    for n in range(num_elements):
        test_losses.append([er.avg_loss[n] for er in test_history])
        
    return TrainResults(
        best_loss_war=test_result.avg_loss[LOSS_IDX_WAR],
        best_loss_level=test_result.avg_loss[LOSS_IDX_LEVEL],
        best_loss_pa=test_result.avg_loss[LOSS_IDX_PA],
        best_loss_stats=test_result.avg_loss[LOSS_IDX_STATS],
        best_loss_pos=test_result.avg_loss[LOSS_IDX_POS],
        best_loss_pt=test_result.avg_loss[LOSS_IDX_PT],
        best_loss_mlbstat=test_result.avg_loss[LOSS_IDX_MLBSTAT],
        best_loss_mlbvalue=test_result.avg_loss[LOSS_IDX_MLBVALUE],
        test_losses=test_losses
    )