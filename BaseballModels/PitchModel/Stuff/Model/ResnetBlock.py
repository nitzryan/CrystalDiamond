import torch.nn as nn
import torch
from typing import Callable

class ResnetBlock(nn.Module):
    def __init__(self, dim : int, dropout : float, activation_function : Callable[[torch.Tensor], torch.Tensor]):
        super().__init__()
        
        class _Activation(nn.Module):
            def __init__(self, fn: Callable[[torch.Tensor], torch.Tensor]):
                super().__init__()
                self.fn = fn

            def forward(self, x: torch.Tensor) -> torch.Tensor:
                return self.fn(x)
        
        self.net = nn.Sequential(
            nn.Linear(dim, dim),
            nn.LayerNorm(dim),
            _Activation(activation_function),
            nn.Dropout(dropout),
            nn.Linear(dim, dim),
            nn.LayerNorm(dim)
        )
        
    def forward(self, x : torch.Tensor) -> torch.Tensor:
        return x + self.net(x)