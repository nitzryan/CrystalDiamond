import torch.nn as nn
from Buckets import *
from Stuff.Model.ResnetBlock import ResnetBlock

class PitcherPredLayers(nn.Module):
    def __init__(self, 
                input_size : int,
                block_size_result : int = 50,
                num_layers_result : int = 2,
                
                block_size_inplay : int = 5,
                num_layers_inplay : int = 2,
                
                ):
        super().__init__()
        
        self.result_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_result)] +
            [ResnetBlock(dim=block_size_result) for _ in range(num_layers_result)] +
            [nn.Linear(block_size_result, 6)]
        )
        
        self.inplay_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_inplay)] +
            [ResnetBlock(dim=block_size_inplay) for _ in range(num_layers_inplay)] +
            [nn.Linear(block_size_inplay, BUCKET_INPLAY_VALUE.size(0) + 1)]
        )
        
    def forward(self, x : torch.Tensor) -> tuple[torch.Tensor, ...]:
        result = x
        for module in self.result_modules:
            result = module(result)
            
        inplay = x
        for module in self.inplay_modules:
            inplay = module(inplay)
            
        return result, inplay