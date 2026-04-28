import torch.nn as nn
from Buckets import *
from Stuff.Model.ResnetBlock import ResnetBlock

class PitcherPredLayers(nn.Module):
    def __init__(self, 
                input_size : int,
                block_size_value : int = 50,
                num_layers_value : int = 2,
                
                block_size_runs : int = 30,
                num_layers_runs : int = 2,
                
                block_size_outs : int = 20,
                num_layers_outs : int = 2,
                
                block_size_swung : int = 20,
                num_layers_swung : int = 2,
                
                block_size_contact : int = 20,
                num_layers_contact : int = 2,
                
                block_size_inplay : int = 20,
                num_layers_inplay : int = 2,
                
                ):
        super().__init__()
        
        self.value_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_value)] +
            [ResnetBlock(dim=block_size_value) for _ in range(num_layers_value)] +
            [nn.Linear(block_size_value, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
        self.runs_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_runs)] +
            [ResnetBlock(dim=block_size_runs) for _ in range(num_layers_runs)] +
            [nn.Linear(block_size_runs, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
        self.outs_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_outs)] +
            [ResnetBlock(dim=block_size_outs) for _ in range(num_layers_outs)] +
            [nn.Linear(block_size_outs, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
        self.swung_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_swung)] +
            [ResnetBlock(dim=block_size_swung) for _ in range(num_layers_swung)] +
            [nn.Linear(block_size_swung, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
        self.contact_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_contact)] +
            [ResnetBlock(dim=block_size_contact) for _ in range(num_layers_contact)] +
            [nn.Linear(block_size_contact, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
        self.inplay_modules = nn.ModuleList(
            [nn.Linear(input_size, block_size_inplay)] +
            [ResnetBlock(dim=block_size_inplay) for _ in range(num_layers_inplay)] +
            [nn.Linear(block_size_inplay, BUCKET_PITCHVALUE.size(0) + 1)]
        )
        
    def forward(self, x : torch.Tensor) -> tuple[torch.Tensor, ...]:
        value = x
        for module in self.value_modules:
            value = module(value)
            
        runs = x
        for module in self.runs_modules:
            runs = module(runs)
            
        outs = x
        for module in self.outs_modules:
            outs = module(outs)
            
        swung = x
        for module in self.swung_modules:
            swung = module(swung)
            
        contact = x
        for module in self.contact_modules:
            contact = module(contact)
            
        inplay = x
        for module in self.inplay_modules:
            inplay = module(inplay)
            
        return value, runs, outs, swung, contact, inplay