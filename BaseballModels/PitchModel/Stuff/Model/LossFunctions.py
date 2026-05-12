import torch
import torch.nn as nn

from Constants import profiler

@profiler
def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor, mask : torch.Tensor) -> tuple[torch.Tensor, int]:
    if mask.sum() == 0:
        return torch.tensor(0.0, device=pred.device), 0
    
    loss = nn.CrossEntropyLoss(reduction='none')
    l = loss(pred, actual)
    l *= mask
    return l.sum() / mask.sum(), mask.sum().item()