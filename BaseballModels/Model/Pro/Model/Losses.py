import torch
import torch.nn as nn
import torch.nn.functional as F
from Model.Constants import *

def MLB_War_Loss(pred : torch.Tensor, actual : torch.Tensor, masks : torch.Tensor, classes : list[int]) -> torch.Tensor:
    # pred: [B, T, M * sum(classes)]; actual: [B, T_full, M, K]; masks: [B, T_full, M]
    time_steps = pred.size(1)
    actual = actual[:, :time_steps]
    masks = masks[:, :time_steps]

    batch_size = pred.size(0)
    num_offsets = actual.size(2)
    total_classes = sum(classes)

    pred = pred.reshape(batch_size, time_steps, num_offsets, total_classes)

    criterion = nn.CrossEntropyLoss(reduction='none')
    total_loss = pred.new_zeros(())
    class_offset = 0
    for k, num_classes in enumerate(classes):
        pred_slot = pred[..., class_offset:class_offset + num_classes]
        class_offset += num_classes

        pred_flat = pred_slot.reshape(batch_size * time_steps * num_offsets, num_classes)
        actual_flat = actual[..., k].reshape(batch_size * time_steps * num_offsets)

        loss_flat = criterion(pred_flat, actual_flat)
        loss = loss_flat.reshape(batch_size, time_steps, num_offsets)

        total_loss = total_loss + (loss * masks).sum()

    return total_loss

def Stats_Loss(pred_stats, actual_stats, masks):
    actual_stats = actual_stats[:, :pred_stats.size(1)]
    masks = masks[:,:pred_stats.size(1)]
    
    batch_size = actual_stats.size(0)
    time_steps = actual_stats.size(1)
    output_size = actual_stats.size(3)
    mask_size = masks.size(2)
    
    pred_stats = pred_stats.reshape((batch_size * time_steps, mask_size, output_size))
    actual_stats = actual_stats.reshape((batch_size * time_steps, mask_size, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    loss = nn.MSELoss(reduction='none')
    l = loss(pred_stats, actual_stats) * masks.unsqueeze(-1)
    return (l * masks.unsqueeze(-1)).sum()
      
def Pt_Loss(pred_pt, actual_pt, lengths : torch.Tensor):
    actual_pt = actual_pt[:, :pred_pt.size(1)]
    batch_size, time_steps, num_levels, output_size = actual_pt.shape
    
    time_idx = torch.arange(time_steps, device=pred_pt.device).unsqueeze(0)   # (1, T)
    valid = (time_idx < lengths.unsqueeze(1)).to(pred_pt.dtype) 
    
    pred_pt = pred_pt.reshape((batch_size * time_steps, num_levels, output_size))
    actual_pt = actual_pt.reshape((batch_size * time_steps, num_levels, output_size))
    valid = valid.reshape(batch_size * time_steps, 1, 1)
    
    per_elem = F.mse_loss(pred_pt, actual_pt, reduction='none')
    return (per_elem * valid).sum()
        
def Position_Classification_Loss(pred_positions, actual_positions, masks):
    actual_positions  = actual_positions[:, :pred_positions.size(1)]
    masks = masks[:, :pred_positions.size(1)]
    
    batch_size = actual_positions.size(0)
    time_steps = actual_positions.size(1)
    output_size = actual_positions.size(3)
    mask_size = masks.size(2)
    
    pred_positions = pred_positions.reshape((batch_size * time_steps, mask_size, output_size))
    actual_positions = actual_positions.reshape((batch_size * time_steps, mask_size, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    loss = nn.CrossEntropyLoss(reduction='none')
    l = 0
    for x in range(mask_size):
        l += (loss(pred_positions[:,x,:], actual_positions[:,x,:]) * masks[:,x]).sum()
    return l
    
def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor, masks : torch.Tensor):
    # Clip masks to match prediction time dimension
    time_steps = pred.size(1)
    masks = masks[:, :time_steps]
    
    batch_size = pred.size(0)
    num_classes = pred.size(2)
    
    pred_flat = pred.reshape(batch_size * time_steps, num_classes)
    actual_flat = actual.repeat_interleave(time_steps)
    
    criterion = nn.CrossEntropyLoss(reduction='none')
    loss_flat = criterion(pred_flat, actual_flat)
    
    loss = loss_flat.reshape(batch_size, time_steps)
    
    masked_loss = loss * masks
    total_loss = masked_loss.sum()
    
    return total_loss