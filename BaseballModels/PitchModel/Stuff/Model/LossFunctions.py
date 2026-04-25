import torch
import torch.nn as nn

from Constants import profiler

@profiler
def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor) -> torch.Tensor:
    loss = nn.CrossEntropyLoss(reduction='none')
    return loss(pred, actual).sum()