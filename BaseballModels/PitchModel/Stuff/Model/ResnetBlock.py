import math
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
        
    def ResetParameters(self, activation_gain : float, branch_scale : float) -> None:
        """
        Initializes for unit-scale signals (see Initialization.py).
        activation_gain : 1 / sqrt(E[f(z)^2]) for the block's activation f with z ~ N(0, 1)
        branch_scale    : initial gain of the final LayerNorm, i.e. how much this block adds to the residual stream
        """
        linear_in, norm_in, _, dropout, linear_out, norm_out = self.net
        dim = linear_in.in_features
        
        # Fed by the residual stream, whose scale is about 1
        nn.init.normal_(linear_in.weight, std=1.0 / math.sqrt(dim))
        # Fed by activation(LayerNorm output), after inverted dropout rescales what is kept
        nn.init.normal_(linear_out.weight, std=activation_gain * math.sqrt(1.0 - dropout.p) / math.sqrt(dim))
        nn.init.zeros_(linear_in.bias)
        nn.init.zeros_(linear_out.bias)
        
        for norm in (norm_in, norm_out):
            nn.init.ones_(norm.weight)
            nn.init.zeros_(norm.bias)
        nn.init.constant_(norm_out.weight, branch_scale)
        
    def forward(self, x : torch.Tensor) -> torch.Tensor:
        return x + self.net(x)