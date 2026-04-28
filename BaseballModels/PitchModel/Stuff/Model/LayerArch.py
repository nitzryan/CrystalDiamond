import torch.nn as nn
import torch
from Stuff.Model.ResnetBlock import ResnetBlock

class LayerArch(nn.Module):
    def __init__(self, input_size : int, num_blocks : int, block_dim : int, output_size : int, dropout : float = 0.0):
        super().__init__()
        
        self.input_linear = nn.Linear(input_size, block_dim)
        self.blocks = nn.ModuleList([
            ResnetBlock(dim=block_dim, dropout=dropout) for _ in range(num_blocks)
        ])
        self.output_linear = nn.Linear(block_dim, output_size)
        
    def forward(self, x : torch.Tensor) -> torch.Tensor:
        x = self.input_linear(x)
        for block in self.blocks:
            x = block(x)
        x = self.output_linear(x)
        return x