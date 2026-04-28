import torch.nn as nn
import torch

class ResnetBlock(nn.Module):
    def __init__(self, dim : int, dropout : float = 0):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(dim, dim),
            nn.BatchNorm1d(dim),
            nn.ReLU(),
            nn.Dropout(dropout),
            nn.Linear(dim, dim),
            nn.BatchNorm1d(dim)
        )
        
    def forward(self, x : torch.Tensor) -> torch.Tensor:
        return x + self.net(x)