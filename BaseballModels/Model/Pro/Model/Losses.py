import torch
import torch.nn as nn
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