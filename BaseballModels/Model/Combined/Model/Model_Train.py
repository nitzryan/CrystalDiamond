from tqdm import tqdm

from Model.Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from Model.College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Model.Pro.Model.Player_Model import RNN_Model as Pro_Model
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Model.Combined.Model.RunEpoch import RunEpoch
from Model.Combined.Model.BuildPlots import BuildPlots
from Model.Combined.Model.GetPlayerClassDistribution import GetPlayerClassDistribution
import torch

from Model.Combined.Utilities.Types import *

SHOULD_PROFILE = False
DEFAULT_PRO_ELEMENT_LOSS_SCALES = [1e-1, 1, 1e-1, 1e-3, 1, 1, 1, 1e-1]

def TrainAndGraph(
    pro_network : Pro_Model,
    col_network : Col_Model,
    train_dataset : Combined_Player_Dataset,
    test_dataset : Combined_Player_Dataset,
    is_hitter : bool,
    num_epochs : int = 41,
    batch_size : int = 1200,
    logging_interval : int = 10,
    should_output : bool = True,
    show_progress_bar : bool = False,
    pro_model_name : str = "no_name_pro",
    col_model_name : str = "no_name_col",
    element_to_save : int = 0,
    early_stopping_cutoff : int = 20,
    pro_element_loss_scales : list[int] = DEFAULT_PRO_ELEMENT_LOSS_SCALES,
    timestep_pct_cutoff : float = 1.0,
    save_last=True,
) -> TrainResults:
    
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
    best_epoch = -1
    epochs_since_improve = 0
    
    # Schedulers
    pro_scheduler = torch.optim.lr_scheduler.CosineAnnealingLR(pro_network.optimizer, T_max=num_epochs)
    col_scheduler = torch.optim.lr_scheduler.CosineAnnealingLR(col_network.optimizer, T_max=num_epochs)
    
    iterable = range(num_epochs)
    if show_progress_bar:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_result, test_result = RunEpoch(pro_network, col_network, train_dataset, test_dataset, is_hitter, num_pro_elements, num_col_elements, batch_size, pro_element_loss_scales)
        
        pro_scheduler.step()
        col_scheduler.step()
        
        train_history.append(train_result)
        test_history.append(test_result)
        epoch_counter.append(epoch)
        
        
        if should_output and (epoch % logging_interval == 0):  
            print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch + 1, num_epochs, train_result.avg_loss[element_to_save], test_result.avg_loss[element_to_save]))
        
        if (not save_last and test_result.avg_loss[element_to_save] < best_loss) \
            or (save_last and epoch == num_epochs - 1):
            best_loss = test_result.avg_loss[element_to_save]
            best_epoch = epoch
            epochs_since_improve = 0
            torch.save(col_network.state_dict(), col_model_name + ".pt")
            torch.save(pro_network.state_dict(), pro_model_name + ".pt")
        else:
            epochs_since_improve += 1
           
        if not save_last and epochs_since_improve >= early_stopping_cutoff:
            if should_output:
                print(f"Exited Early at epoch={epoch}")
            break
            
    if should_output:
        if save_last:
            print(f"End result at loss={test_result.avg_loss[element_to_save]}")
        else:
            print(f"Best result at epoch={best_epoch} loss={best_loss}")
        BuildPlots(epoch_counter=epoch_counter, train_history=train_history, test_history=test_history,
            element_list=element_list, pro_network=pro_network, col_network=col_network, train_dataset=train_dataset,
            test_dataset=test_dataset, is_hitter=is_hitter, batch_size=batch_size, timestep_pct_cutoff=timestep_pct_cutoff)
        
    test_losses = []
    for n in range(num_elements):
        test_losses.append([er.avg_loss[n] for er in test_history])
        
    return TrainResults(
        best_loss=best_loss,
        best_epoch=best_epoch,
        test_losses=test_losses
    )