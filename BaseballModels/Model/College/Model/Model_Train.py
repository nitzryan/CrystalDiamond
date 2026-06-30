from Constants import device
import torch
from College.Model.College_Model import Classification_Loss, Position_Loss
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.Model.GetWarClassCounts import *
from Combined.Utilities.Types import *
from Combined.Utilities.BrierScore import Brier_Score

HITTER_ELEMENT_LIST = ["DRAFT", "WAR", "OFF", "DEF", "PA", "POS"]
NUM_ELEMENTS_HITTER = len(HITTER_ELEMENT_LIST)

PITCHER_ELEMENT_LIST = ["DRAFT", "WAR", "POS"]
NUM_ELEMENTS_PITCHER = len(PITCHER_ELEMENT_LIST)

def GetLossesCollege(
        network : Col_Model,
        data : tuple[torch.Tensor, torch.Tensor],
        targets : tuple[torch.Tensor, ...],
        masks : tuple[torch.Tensor, ...],
        shouldBackprop : bool,
        is_hitter : bool,
        ) -> CollegeLossResult:
    # Get Output
    data, length = data
    data = data.to(device, non_blocking=True)
    length = length.to(device, non_blocking=True)
    
    if is_hitter:
        output_draft, output_war, output_off, output_def, output_pa, output_pos, output_hidden = network(data, length)
    else:
        output_draft, output_war, output_pos, output_hidden = network(data, length)
    
    
    max_len = output_draft.size(1)
    mask_length = (torch.arange(max_len, device=length.device)
                   .unsqueeze(0) < length.unsqueeze(1))
    
    mask_pos, = masks
    mask_pos = mask_pos.to(device, non_blocking=True)
    
    if is_hitter:
        target_draft, target_war, target_off, target_def, target_pa, target_pos = targets
    else:
        target_draft, target_war, target_pos = targets
    
    target_draft = target_draft.to(device, non_blocking=True)
    target_war = target_war.to(device, non_blocking=True)
    target_pos = target_pos.to(device, non_blocking=True)
    
    with torch.no_grad():
        war_predicted_counts, war_actual_counts = GetWarClassCounts(output_war, target_war, mask_length)
        brier_per_class_sum, brier_count = Brier_Score(output_war, target_war, mask_length)
    
    loss_draft = Classification_Loss(output_draft, target_draft, mask_length)
    loss_war = Classification_Loss(output_war, target_war, mask_length)
    loss_pos = Position_Loss(output_pos, target_pos, mask_length * mask_pos.unsqueeze(-1))
    
    losses : list[torch.Tensor] = [loss_draft, loss_war]
    
    if is_hitter:
        target_off = target_off.to(device, non_blocking=True)
        target_def = target_def.to(device, non_blocking=True)
        target_pa = target_pa.to(device, non_blocking=True)
        
        loss_off = Classification_Loss(output_off, target_off, mask_length)
        loss_def = Classification_Loss(output_def, target_def, mask_length)
        loss_pa = Classification_Loss(output_pa, target_pa, mask_length)
        
        losses.extend([loss_off, loss_def, loss_pa])
    
    losses.append(loss_pos)
    
    if shouldBackprop:
        torch.autograd.backward(losses, retain_graph=True)
    
    return CollegeLossResult(
        losses=tuple(losses),
        hidden=output_hidden,
        war_counts=WarClassCounts(predicted=war_predicted_counts, actual=war_actual_counts),
        brier=BrierAccumulator(per_class_sum=brier_per_class_sum, count=brier_count),
    )
