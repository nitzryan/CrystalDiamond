import torch
import torch.nn as nn
import torch.nn.functional as F
from Stuff.DataPrep.DataPrep import DataPrep
from Buckets import *
from Stuff.Model.LayerArch import LayerArch
from Stuff.Model.PitcherPredLayers import PitcherPredLayers
from Stuff.Model.Utilities import *
        
class PitchModel(nn.Module):
    def __init__(self,
                data_prep : DataPrep,
                location_branch_size : int = 55,
                stuff_branch_size : int = 55,
                
                combined_pred_size_value : int = 70,
                combined_pred_blocks_value : int = 8,
                
                location_pred_size_value : int = 50,
                location_pred_blocks_value : int = 8,
                
                stuff_pred_size_value : int = 70,
                stuff_pred_blocks_value : int = 8,
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        
        self.nonlin = F.leaky_relu
        
        # Location and pitch overview
        self.location_block = LayerArch(
            input_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size,
            num_blocks=4,
            block_dim=100,
            output_size=location_branch_size
        )
        
        # Stuff and Pitch Overview
        self.stuff_block = LayerArch(
            input_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.pitcher_game_size + prep_map.league_baseline_size,
            num_blocks=4,
            block_dim=100,
            output_size=location_branch_size
        )
        
        # Prediction layers for Location only
        self.location_pred = PitcherPredLayers(
            input_size=location_branch_size,
            
            block_size_value=location_pred_size_value,
            num_layers_value=location_pred_blocks_value,
        )
            
        # Prediction layers for stuff only
        self.stuff_pred = PitcherPredLayers(
            input_size=stuff_branch_size,
            
            block_size_value=stuff_pred_size_value,
            num_layers_value=stuff_pred_blocks_value,
        )
        
        # Prediction layers for location + stuff
        self.combined_pred = PitcherPredLayers(
            input_size=location_branch_size + stuff_branch_size,
            
            block_size_value=combined_pred_size_value,
            num_layers_value=combined_pred_blocks_value,
        )
        
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
            
