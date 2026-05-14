import torch.nn as nn
from Buckets import *
from Stuff.Model.ResnetBlock import ResnetBlock

class PitcherPredLayers(nn.Module):
    def __init__(self, 
                input_size : int,
                block_size_result : int,
                num_layers_result : int,
                dropout_result : float,
                
                block_size_swing : int,
                num_layers_swing : int,
                dropout_swing : float,
                
                block_size_inplay : int,
                num_layers_inplay : int,
                dropout_inplay : float,
                
                ):
        super().__init__()
        
        self.result_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_result)] +
            [ResnetBlock(dim=block_size_result, dropout=dropout_result) for _ in range(num_layers_result)] +
            [nn.Linear(block_size_result, 4)]
        )
        
        self.swing_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_swing)] +
            [ResnetBlock(dim=block_size_swing, dropout=dropout_swing) for _ in range(num_layers_swing)] +
            [nn.Linear(block_size_swing, 3)]
        )
        
        self.inplay_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_inplay)] +
            [ResnetBlock(dim=block_size_inplay, dropout=dropout_inplay) for _ in range(num_layers_inplay)] +
            [nn.Linear(block_size_inplay, BUCKET_INPLAY_VALUE.size(0) + 1)]
        )
        
    def forward(self, x : torch.Tensor) -> tuple[torch.Tensor, ...]:
        result = x
        for module in self.result_modules:
            result = module(result)
            
        swing = x
        for module in self.swing_modules:
            swing = module(swing)
            
        inplay = x
        for module in self.inplay_modules:
            inplay = module(inplay)
            
        return result, swing, inplay