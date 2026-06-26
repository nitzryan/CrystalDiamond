from tqdm import tqdm

from Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model

from College.Model.Model_Train import GetLossesHitter, GetLossesPitcher
from Combined.Model.GetTimestepWarLoss import GetTimestepWarLoss
from Pro.Model.Model_Train import GetLosses
from Pro.Model.Model_Scheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Model.BrierStats import GetTimestepWarBrier
from Combined.Model.Graphing import *

import torch

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
    pro_element_loss_scales : list[int] = DEFAULT_PRO_ELEMENT_LOSS_SCALES,
    timestep_pct_cutoff : float = 1.0,
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
    
    # Brier score histories (total + per class; per-class lazily sized)
    train_brier_total_history : list[float] = []
    test_brier_total_history : list[float] = []
    train_brier_class_history : list[list[float]] = None
    test_brier_class_history : list[list[float]] = None
    
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
        train_loss, train_pred_pct, train_actual_pct, train_brier_class, train_brier_total = TestOrTrain(
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
            is_train=True,
            batch_size=batch_size,
            pro_element_loss_scales=pro_element_loss_scales)
        test_loss, test_pred_pct, test_actual_pct, test_brier_class, test_brier_total = TestOrTrain(
            pro_network=pro_network,
            col_network=col_network,
            dataset=test_dataset,
            pro_size=test_dataset.GetProLength(),
            col_size=test_dataset.GetColLength(),
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,
            is_train=False,
            batch_size=batch_size)
        
        # Record Losses
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
        
        # Record Brier scores
        train_brier_total_history.append(train_brier_total)
        test_brier_total_history.append(test_brier_total)
        if train_brier_class_history is None:
            train_brier_class_history = [[] for _ in range(num_war_classes)]
            test_brier_class_history = [[] for _ in range(num_war_classes)]
        for c in range(num_war_classes):
            train_brier_class_history[c].append(train_brier_class[c])
            test_brier_class_history[c].append(test_brier_class[c])
        
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
        
        # Per-timestep WAR loss
        tr_ts, tr_avg, tr_pct = GetTimestepWarLoss(
            pro_network, col_network, train_dataset, is_hitter, batch_size, timestep_pct_cutoff)
        te_ts, te_avg, te_pct = GetTimestepWarLoss(
            pro_network, col_network, test_dataset, is_hitter, batch_size, timestep_pct_cutoff)
        
        plots : list[tuple[str, "callable"]] = []

        for n in range(num_elements):
            plots.append((
                element_list[n],
                lambda n=n: GraphLoss(
                    epoch_counter, train_loss_history[n], test_loss_history[n],
                    title=element_list[n])
            ))

        plots.append((
            "WAR Brier Score (All Classes)",
            lambda: GraphLoss(
                epoch_counter, train_brier_total_history, test_brier_total_history,
                loss_name="Brier Score", title="WAR Brier Score (All Classes)")
        ))
        for c in range(num_war_classes):
            plots.append((
                f"WAR Class {c} Brier Score",
                lambda c=c: GraphLoss(
                    epoch_counter, train_brier_class_history[c], test_brier_class_history[c],
                    loss_name="Brier Score", title=f"WAR Class {c} Brier Score")
            ))

        for c in range(num_war_classes):
            plots.append((
                f"WAR Class {c} Distribution",
                lambda c=c: GraphClassCounts(
                    epoch_counter,
                    train_pred_pct_history[c], train_actual_pct_history[c],
                    test_pred_pct_history[c], test_actual_pct_history[c],
                    title=f"WAR Class {c} Distribution")
            ))
        
        plots.append((
            "WAR Loss per Timestep (Train)",
            lambda: GraphTimestepLoss(tr_ts, tr_avg, tr_pct,
                                      title="WAR Loss per Timestep (Train)")
        ))
        plots.append((
            "WAR Loss per Timestep (Test)",
            lambda: GraphTimestepLoss(te_ts, te_avg, te_pct,
                                      title="WAR Loss per Timestep (Test)")
        ))

        # Brier Skill Score and Murphy Decomposition
        train_brier_ts = GetTimestepWarBrier(
            pro_network, col_network, train_dataset, is_hitter, batch_size)
        test_brier_ts = GetTimestepWarBrier(
            pro_network, col_network, test_dataset, is_hitter, batch_size)
        plots.append((
            "WAR BSS per Timestep",
            lambda: GraphTimestepBSS(
                train_brier_ts['timesteps'], train_brier_ts['bss'],
                test_brier_ts['timesteps'],  test_brier_ts['bss'],
                title="WAR Brier Skill Score per Timestep", show=False)
        ))
        plots.append((
            "WAR Brier Decomposition per Timestep (Train)",
            lambda: GraphTimestepDecomposition(
                train_brier_ts, title="WAR Brier Decomposition per Timestep (Train)", show=False)
        ))
        plots.append((
            "WAR Brier Decomposition per Timestep (Test)",
            lambda: GraphTimestepDecomposition(
                test_brier_ts, title="WAR Brier Decomposition per Timestep (Test)", show=False)
        ))


        ShowPlotDropdown(plots)
    
    if SHOULD_PROFILE:    
        profiler.disable()
        profiler.dump_stats("train_profile.lprof")
        
    if get_end_loss:
        return test_loss[element_to_save], epoch
    else:
        return best_loss, best_loss_college, epoch
  
@profiler
def TestOrTrain(
    pro_network : Pro_Model,
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    pro_size : int,
    col_size : int,
    is_hitter : bool,
    pro_elements : int,
    col_elements : int,
    batch_size : int,
    is_train : bool,
    pro_optimizer : torch.optim.Optimizer | None = None,
    col_optimizer : torch.optim.Optimizer | None = None,
    pro_element_loss_scales : list[int] = DEFAULT_PRO_ELEMENT_LOSS_SCALES,
) -> tuple[list[float], list[float], list[float], list[float], float]:
    
    if is_train:
        pro_network.train()
        col_network.train()
    else:
        pro_network.eval()
        col_network.eval()
    
    avg_loss = [0] * (pro_elements + col_elements)
    
    war_pred_sum : torch.Tensor = None
    war_actual_counts : torch.Tensor = None
    
    # Brier score accumulators
    brier_per_class_sum : torch.Tensor = None
    brier_count : torch.Tensor = None
    
    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.randperm(n_samples) if is_train else torch.arange(n_samples)
    
    with torch.set_grad_enabled(is_train):
        for batch_i in range(num_batches):
            start = batch_i * batch_size
            end = min(start + batch_size, n_samples)
            batch_indices = indices[start:end]
            pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks = dataset.get_batch(batch_indices)
            
            if is_train:
                col_optimizer.zero_grad()
                pro_optimizer.zero_grad()
            
            if is_hitter:
                col_losses, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=is_train)
            else:
                col_losses, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=is_train)
            
            pro_losses, (batch_pred_sum, batch_actual_counts), (batch_brier_sum, batch_brier_count) = GetLosses(
                pro_network, 
                pro_data, 
                pro_targets, 
                pro_masks, 
                h0, 
                shouldBackprop=is_train, 
                is_hitter=is_hitter,
                pro_element_loss_scales=pro_element_loss_scales)
            
            if is_train:
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
            
            if brier_per_class_sum is None:
                brier_per_class_sum = torch.zeros_like(batch_brier_sum)
                brier_count = torch.zeros_like(batch_brier_count)
            brier_per_class_sum += batch_brier_sum
            brier_count += batch_brier_count
    
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
    
    total_valid = war_actual_counts.sum()
    war_pred_pct = (war_pred_sum / total_valid * 100).tolist()
    war_actual_pct = (war_actual_counts / total_valid * 100).tolist()
    
    brier_per_class = (brier_per_class_sum / brier_count).tolist()
    brier_total = (brier_per_class_sum.sum() / brier_count).item()
    
    return avg_loss, war_pred_pct, war_actual_pct, brier_per_class, brier_total



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

