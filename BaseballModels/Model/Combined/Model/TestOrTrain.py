from College.Model.Model_Train import GetLossesCollege
from Pro.Model.Model_Train import GetLossesPro
from Combined.Utilities.Types import *
from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

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
    pro_element_loss_scales : list[int],
    pro_optimizer : torch.optim.Optimizer | None = None,
    col_optimizer : torch.optim.Optimizer | None = None,
) -> EpochResult:
    
    if is_train:
        pro_network.train()
        col_network.train()
    else:
        pro_network.eval()
        col_network.eval()
    
    avg_loss = [0] * (pro_elements + col_elements)
    
    pro_war_counts : WarClassCounts | None = None
    pro_brier : BrierAccumulator | None = None
    col_war_counts : WarClassCounts | None = None
    col_brier : BrierAccumulator | None = None
    
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
            
            col_result = GetLossesCollege(col_network, col_data, col_targets, col_masks, shouldBackprop=is_train, is_hitter=is_hitter)
            pro_result = GetLossesPro(pro_network, pro_data, pro_targets, pro_masks, col_result.hidden, shouldBackprop=is_train, is_hitter=is_hitter,pro_element_loss_scales=pro_element_loss_scales)
            
            if is_train:
                torch.nn.utils.clip_grad_norm_(col_network.parameters(), max_norm=0.05)
                col_optimizer.step()
                torch.nn.utils.clip_grad_norm_(pro_network.parameters(), max_norm=0.05)
                pro_optimizer.step()
            
            for i, loss in enumerate(pro_result.losses):
                avg_loss[i] += loss.item()
            for i, loss in enumerate(col_result.losses, start=len(pro_result.losses)):
                avg_loss[i] += loss.item()
            
            if pro_war_counts is None:
                pro_war_counts = WarClassCounts.zeros_like(pro_result.war_counts)
            pro_war_counts += pro_result.war_counts
            
            if pro_brier is None:
                pro_brier = BrierAccumulator.zeros_like(pro_result.brier)
            pro_brier += pro_result.brier
            
            if col_war_counts is None:
                col_war_counts = WarClassCounts.zeros_like(col_result.war_counts)
            col_war_counts += col_result.war_counts
            
            if col_brier is None:
                col_brier = BrierAccumulator.zeros_like(col_result.brier)
            col_brier += col_result.brier
    
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
    
    pro_total = pro_war_counts.actual.sum()
    pro_war_dist = WarDistribution(
        pred_pct=(pro_war_counts.predicted / pro_total * 100).tolist(),
        actual_pct=(pro_war_counts.actual / pro_total * 100).tolist())
    
    col_total = col_war_counts.actual.sum()
    col_war_dist = WarDistribution(
        pred_pct=(col_war_counts.predicted / col_total * 100).tolist(),
        actual_pct=(col_war_counts.actual / col_total * 100).tolist())
    
    pro_brier_total = (pro_brier.per_class_sum.sum() / pro_brier.count).item()
    col_brier_total = (col_brier.per_class_sum.sum() / col_brier.count).item()
    
    return EpochResult(
        avg_loss=avg_loss,
        pro_war_dist=pro_war_dist,
        pro_brier_total=pro_brier_total,
        col_war_dist=col_war_dist,
        col_brier_total=col_brier_total)