from tqdm import tqdm

from Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model

from College.Model.Model_Train import GetLossesHitter, GetLossesPitcher
from Pro.Model.Model_Train import GetLosses
from Pro.Model.Model_Scheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

import torch
import matplotlib.pyplot as plt
from Utilities import profiler

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
    pro_element_loss_scales : list[int] = DEFAULT_PRO_ELEMENT_LOSS_SCALES
) -> tuple[float, float, int]:
    if SHOULD_PROFILE:
        profiler.enable()
    
    num_pro_elements = NUM_ELEMENTS
    if is_hitter:
        num_col_elements =  + NUM_ELEMENTS_HITTER
        element_list = ELEMENT_LIST + HITTER_ELEMENT_LIST
    else:
        num_col_elements = NUM_ELEMENTS_PITCHER
        element_list = ELEMENT_LIST + PITCHER_ELEMENT_LIST
    num_elements = num_pro_elements + num_col_elements
        
    test_loss_history : list[list[float]] = [[] for _ in range(num_elements)]
    train_loss_history : list[list[float]] = [[] for _ in range(num_elements)]
    epoch_counter : list[int] = []
    
    # Per-WAR-class count histories (lazily sized on first epoch)
    num_war_classes = None
    train_pred_pct_history : list[list[float]] = None
    train_actual_pct_history : list[list[float]] = None
    test_pred_pct_history : list[list[float]] = None
    test_actual_pct_history : list[list[float]] = None
    
    best_loss = 999999
    best_loss_college = 999999
    best_epoch = -1
    epochs_since_improve = 0
    
    col_optimizer = torch.optim.Adam(col_network.parameters(), lr=0.0025)
    col_scheduler = torch.optim.lr_scheduler.ReduceLROnPlateau(col_optimizer, factor=0.5, patience=5, cooldown=5)
    pro_scheduler = Scheduler(pro_network.optimizer, [[0,1], [2], [3], [4], [5], [6], [7], [8]], verbose=False,
                        factor=0.5, patience=5, cooldown=5)
    
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
        train_loss, train_pred_pct, train_actual_pct = Train(
            pro_network=pro_network, 
            col_network=col_network, 
            dataset=train_dataset, 
            pro_size=train_dataset.GetProLength(), 
            col_size=train_dataset.GetColLength(), 
            pro_optimizer=pro_network.optimizer, 
            col_optimizer=col_optimizer,
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,
            batch_size=batch_size,
            pro_element_loss_scales=pro_element_loss_scales)
        test_loss, test_pred_pct, test_actual_pct = Test(
            pro_network=pro_network,
            col_network=col_network,
            dataset=test_dataset,
            pro_size=test_dataset.GetProLength(),
            col_size=test_dataset.GetColLength(),
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,
            batch_size=batch_size)
        
        for n in range(num_elements):
            train_loss_history[n].append(train_loss[n])
            test_loss_history[n].append(test_loss[n])
        epoch_counter.append(epoch)
        
        # Record per-WAR-class prediction/actual counts
        if num_war_classes is None:
            num_war_classes = len(train_pred_pct)
            train_pred_pct_history = [[] for _ in range(num_war_classes)]
            train_actual_pct_history = [[] for _ in range(num_war_classes)]
            test_pred_pct_history = [[] for _ in range(num_war_classes)]
            test_actual_pct_history = [[] for _ in range(num_war_classes)]
        for c in range(num_war_classes):
            train_pred_pct_history[c].append(train_pred_pct[c])
            train_actual_pct_history[c].append(train_actual_pct[c])
            test_pred_pct_history[c].append(test_pred_pct[c])
            test_actual_pct_history[c].append(test_actual_pct[c])
        
        col_scheduler.step(test_loss[num_pro_elements + 1])
        pro_scheduler.step(test_loss[:num_pro_elements])
        
        if should_output and (epoch % logging_interval == 0):  
            print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch + 1, num_epochs, train_loss[element_to_save], test_loss[element_to_save]))
        
        if test_loss[element_to_save] < best_loss:
            best_loss = test_loss[element_to_save]
            best_loss_college = test_loss[num_pro_elements + 1]
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
        for n in range(num_elements):
            GraphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=element_list[n])
        for c in range(num_war_classes):
            GraphClassCounts(
                epoch_counter,
                train_pred_pct_history[c], train_actual_pct_history[c],
                test_pred_pct_history[c], test_actual_pct_history[c],
                title=f"WAR Class {c} Distribution")
    
    if SHOULD_PROFILE:    
        profiler.disable()
        profiler.dump_stats("train_profile.lprof")
        
    if get_end_loss:
        return test_loss[element_to_save], epoch
    else:
        return best_loss, best_loss_college, epoch
  
@profiler
def Train(
    pro_network : Pro_Model,
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    pro_size : int,
    col_size : int,
    pro_optimizer : torch.optim.Optimizer,
    col_optimizer : torch.optim.Optimizer,
    is_hitter : bool,
    pro_elements : int,
    col_elements : int,
    batch_size : int,
    pro_element_loss_scales : list[int]
) -> tuple[list[float], list[float], list[float]]:
    
    pro_network.train()
    col_network.train()
    avg_loss = [0] * (pro_elements + col_elements)
    
    war_pred_sum : torch.Tensor = None
    war_actual_counts : torch.Tensor = None
    
    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.randperm(n_samples)
    
    # for i, (pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks) in enumerate(generator):
    for batch_i in range(num_batches):
        start = batch_i * batch_size
        end = min(start + batch_size, n_samples)
        batch_indices = indices[start:end]
        pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks = dataset.get_batch(batch_indices)
        
        col_optimizer.zero_grad()
        pro_optimizer.zero_grad()
        if is_hitter:
            col_losses, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=True)
        else:
            col_losses, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=True)
        
        pro_losses, (batch_pred_sum, batch_actual_counts) = GetLosses(
            pro_network, 
            pro_data, 
            pro_targets, 
            pro_masks, 
            h0, 
            shouldBackprop=True, 
            is_hitter=is_hitter,
            pro_element_loss_scales=pro_element_loss_scales)
        
        torch.nn.utils.clip_grad_norm_(col_network.parameters(), max_norm=0.05)
        col_optimizer.step()
        torch.nn.utils.clip_grad_norm_(pro_network.parameters(), max_norm=0.05)
        pro_optimizer.step()
        
        for i, loss in enumerate(pro_losses):
            avg_loss[i] += loss.item()
        for i, loss in enumerate(col_losses, start=len(pro_losses)):
            avg_loss[i] += loss.item()
            
        if war_pred_sum is None:
            war_pred_sum = torch.zeros_like(batch_pred_sum)
            war_actual_counts = torch.zeros_like(batch_actual_counts)
        war_pred_sum += batch_pred_sum
        war_actual_counts += batch_actual_counts
            
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
        
    total_valid = war_actual_counts.sum()
    war_pred_pct = (war_pred_sum / total_valid * 100).tolist()
    war_actual_pct = (war_actual_counts / total_valid * 100).tolist()
        
    return avg_loss, war_pred_pct, war_actual_pct


@profiler
def Test(
    pro_network : Pro_Model,
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    pro_size : int,
    col_size : int,
    is_hitter : bool,
    pro_elements : int,
    col_elements : int,
    batch_size : int,
) -> tuple[list[float], list[float], list[float]]:
    
    pro_network.eval()
    col_network.eval()
    avg_loss = [0] * (pro_elements + col_elements)
    
    war_pred_sum : torch.Tensor = None
    war_actual_counts : torch.Tensor = None
    
    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.arange(n_samples)
    
    for batch_i in range(num_batches):
        start = batch_i * batch_size
        end = min(start + batch_size, n_samples)
        batch_indices = indices[start:end]
        pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks = dataset.get_batch(batch_indices)
        
        if is_hitter:
            col_losses, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=False)
        else:
            col_losses, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=False)
        
        pro_losses, (batch_pred_sum, batch_actual_counts) = GetLosses(pro_network, pro_data, pro_targets, pro_masks, h0, shouldBackprop=False, is_hitter=is_hitter)
        
        for i, loss in enumerate(pro_losses):
            avg_loss[i] += loss.item()
        for i, loss in enumerate(col_losses, start=len(pro_losses)):
            avg_loss[i] += loss.item()
            
        if war_pred_sum is None:
            war_pred_sum = torch.zeros_like(batch_pred_sum)
            war_actual_counts = torch.zeros_like(batch_actual_counts)
        war_pred_sum += batch_pred_sum
        war_actual_counts += batch_actual_counts
            
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
        
    total_valid = war_actual_counts.sum()
    war_pred_pct = (war_pred_sum / total_valid * 100).tolist()
    war_actual_pct = (war_actual_counts / total_valid * 100).tolist()
    
    return avg_loss, war_pred_pct, war_actual_pct


def GetPlayerClassDistribution(
    dataset : Combined_Player_Dataset,
    batch_size : int,
    num_war_classes : int = 0,
) -> list[float]:
    all_targets : list[torch.Tensor] = []

    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.arange(n_samples)

    for batch_i in range(num_batches):
        start = batch_i * batch_size
        end = min(start + batch_size, n_samples)
        batch_indices = indices[start:end]
        pro_data, pro_targets, _, _, _, _ = dataset.get_batch(batch_indices)

        _, length, _ = pro_data
        target_war = pro_targets[0]
        mask_valid = length > 0                      # same player set as GetLosses
        all_targets.append(target_war[mask_valid])   # one entry per valid player

    all_targets = torch.cat(all_targets)
    counts = torch.bincount(all_targets, minlength=num_war_classes).float()
    percentages = (counts / counts.sum() * 100).tolist()
    return percentages

def GraphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 1, graph_y_range=None, title=""):
    fig = plt.figure()
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)
    plt.show()
    plt.close(fig)
    
def GraphClassCounts(
    epoch_counter : list[int],
    train_pred : list[float],
    train_actual : list[float],
    test_pred : list[float],
    test_actual : list[float],
    start : int = 1,
    title : str = ""
) -> None:
    fig = plt.figure()
    plt.plot(epoch_counter[start:], train_pred[start:],   color='blue', linestyle='-')
    plt.plot(epoch_counter[start:], train_actual[start:], color='blue', linestyle='--')
    plt.plot(epoch_counter[start:], test_pred[start:],    color='red',  linestyle='-')
    plt.plot(epoch_counter[start+5:-5], test_actual[start+5:-5],  color='red',  linestyle='--') # Slightly shorter so train_actual is visual if this is too close
    plt.title(title)
    plt.legend(['Train Predicted', 'Train Actual', 'Test Predicted', 'Test Actual'],
               loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel('% of Dataset')
    plt.show()
    plt.close(fig)