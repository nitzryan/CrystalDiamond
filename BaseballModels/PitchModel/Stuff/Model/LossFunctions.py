import torch
import torch.nn as nn

def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor) -> torch.Tensor:
    if actual.min().item() < 0:
        print(f"Invalid Minimum for {pred.size(1)}")
    if actual.max().item() >= pred.size(1):
        print(f"Invalid Maximum for {pred.size(1)}")
    
    loss = nn.CrossEntropyLoss(reduction='none')
    return loss(pred, actual).sum()