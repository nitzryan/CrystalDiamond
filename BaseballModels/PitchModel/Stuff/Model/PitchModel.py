import torch
import torch.nn as nn
import torch.nn.functional as F
from Stuff.DataPrep.DataPrep import DataPrep
from Buckets import *
from Stuff.Model.LayerArch import LayerArch
from Stuff.Model.PitcherPredLayers import PitcherPredLayers
from Stuff.Model.Utilities import *
        
def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters
        
class PitchModel(nn.Module):
    def __init__(self,
                data_prep : DataPrep,
                location_branch_size : int = 55,
                stuff_branch_size : int = 55,
                
                combined_pred_size_result : int = 10,
                combined_pred_blocks_result : int = 8,
                
                location_pred_size_result : int = 10,
                location_pred_blocks_result : int = 8,
                
                stuff_pred_size_result : int = 10,
                stuff_pred_blocks_result : int = 8,
                
                location_init_size : int = 50,
                location_init_layers : int = 4,
                
                stuff_init_size : int = 50,
                stuff_init_layers : int = 4,
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        
        self.nonlin = F.leaky_relu
        
        # Location and pitch overview
        self.location_block = LayerArch(
            input_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size,
            num_blocks=location_init_layers,
            block_dim=location_init_size,
            output_size=location_branch_size
        )
        
        # Stuff and Pitch Overview
        self.stuff_block = LayerArch(
            input_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.pitcher_game_size + prep_map.league_baseline_size,
            num_blocks=stuff_init_layers,
            block_dim=stuff_init_size,
            output_size=location_branch_size
        )
        
        # Prediction layers for Location only
        self.location_pred = PitcherPredLayers(
            input_size=location_branch_size,
            
            block_size_result=location_pred_size_result,
            num_layers_result=location_pred_blocks_result,
        )
            
        # Prediction layers for stuff only
        self.stuff_pred = PitcherPredLayers(
            input_size=stuff_branch_size,
            
            block_size_result=stuff_pred_size_result,
            num_layers_result=stuff_pred_blocks_result,
        )
        
        # Prediction layers for location + stuff
        self.combined_pred = PitcherPredLayers(
            input_size=location_branch_size + stuff_branch_size,
            
            block_size_result=combined_pred_size_result,
            num_layers_result=combined_pred_blocks_result,
        )
        
        # Set parameter groups to allow for sub-parts of network to set learning rates independently
        stuff_parameters = GetParameters([self.stuff_block, self.stuff_pred])
        location_parameters = GetParameters([self.location_block, self.location_pred])
        combined_parameters = GetParameters([self.combined_pred])
        
        self.optimizer = torch.optim.Adam([
            {'params': location_parameters, 'lr': 0.005},
            {'params' : stuff_parameters, 'lr': 0.005},
            {'params': combined_parameters, 'lr': 0.005}
        ])
        
    def forward(self, data : tuple[torch.Tensor, ...]) -> tuple[torch.Tensor, ...]:
        overview, location, stuff, game, league_avg = data
        # Location
        data_location = torch.cat((overview, location), dim=-1)
        inter_location = self.location_block(data_location)
        
        # Stuff
        data_stuff = torch.cat((overview, stuff, game, league_avg), dim=-1)
        inter_stuff = self.stuff_block(data_stuff)
        
        inter_comb = torch.cat((inter_location, inter_stuff), dim=-1)
        
        # Location only ouput layers
        output_location = self.location_pred(inter_location)
        output_stuff = self.stuff_pred(inter_stuff)
        output_combined = self.combined_pred(inter_comb)
        
        return output_location + output_stuff + output_combined
            
