import torch
import torch.nn as nn

from Constants import profiler

@profiler
def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor) -> tuple[torch.Tensor, int]:
    loss = nn.CrossEntropyLoss(reduction='none')
    l = loss(pred, actual)
    
    return l.sum() / pred.size(0), pred.size(0)