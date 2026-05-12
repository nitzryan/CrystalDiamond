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
                
                ########## PRED BLOCKS ##########
                ## Combined ##
                combined_pred_size_result : int = 256,
                combined_pred_blocks_result : int = 2,
                combined_pred_dropout_result : float = 0.2,
                
                combined_pred_size_inplay : int = 128,
                combined_pred_blocks_inplay : int = 2,
                combined_pred_dropout_inplay : float = 0.5,
                
                ## Location ##
                location_pred_size_result : int = 256,
                location_pred_blocks_result : int = 2,
                location_pred_dropout_result = 0.2,
                
                location_pred_size_inplay : int = 128,
                location_pred_blocks_inplay : int = 2,
                location_pred_dropout_inplay : float = 0.5,
                
                ## Stuff ##
                stuff_pred_size_result : int = 256,
                stuff_pred_blocks_result : int = 2,
                stuff_pred_dropout_result : float = 0.2,
                
                stuff_pred_size_inplay : int = 128,
                stuff_pred_blocks_inplay : int = 2,
                stuff_pred_dropout_inplay : float = 0.5,
                
                ########## INIT BLOCKS ##########
                ## Location ##
                location_init_size : int = 256,
                location_init_layers : int = 4,
                location_init_dropout : float = 0.1,
                
                ## Stuff ##
                stuff_init_size : int = 256,
                stuff_init_layers : int = 4,
                stuff_init_dropout : float = 0.1,
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        
        self.nonlin = F.leaky_relu
        
        # Location and pitch overview
        self.location_block = LayerArch(
            input_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size,
            num_blocks=location_init_layers,
            block_dim=location_init_size,
            output_size=location_branch_size,
            dropout=location_init_dropout
        )
        
        # Stuff and Pitch Overview
        self.stuff_block = LayerArch(
            input_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.pitcher_game_size + prep_map.league_baseline_size,
            num_blocks=stuff_init_layers,
            block_dim=stuff_init_size,
            output_size=location_branch_size,
            dropout=stuff_init_dropout
        )
        
        # Prediction layers for Location only
        self.location_pred = PitcherPredLayers(
            input_size=location_branch_size,
            
            block_size_result=location_pred_size_result,
            num_layers_result=location_pred_blocks_result,
            dropout_result=location_pred_dropout_result,
            
            block_size_inplay=location_pred_size_inplay,
            num_layers_inplay=location_pred_blocks_inplay,
            dropout_inplay=location_pred_dropout_inplay,
        )
            
        # Prediction layers for stuff only
        self.stuff_pred = PitcherPredLayers(
            input_size=stuff_branch_size,
            
            block_size_result=stuff_pred_size_result,
            num_layers_result=stuff_pred_blocks_result,
            dropout_result=stuff_pred_dropout_result,
            
            block_size_inplay=stuff_pred_size_inplay,
            num_layers_inplay=stuff_pred_blocks_inplay,
            dropout_inplay=stuff_pred_dropout_inplay,
        )
        
        # Prediction layers for location + stuff
        self.combined_pred = PitcherPredLayers(
            input_size=location_branch_size + stuff_branch_size,
            
            block_size_result=combined_pred_size_result,
            num_layers_result=combined_pred_blocks_result,
            dropout_result=combined_pred_dropout_result,
            
            block_size_inplay=combined_pred_size_inplay,
            num_layers_inplay=combined_pred_blocks_inplay,
            dropout_inplay=combined_pred_dropout_inplay,
        )
        
        # Set parameter groups to allow for sub-parts of network to set learning rates independently
        stuff_parameters = GetParameters([self.stuff_block, self.stuff_pred])
        location_parameters = GetParameters([self.location_block, self.location_pred])
        combined_parameters = GetParameters([self.combined_pred])
        
        self.optimizer = torch.optim.AdamW([
            {'params': location_parameters, 'lr': 0.005, 'weight_decay': 1e-4},
            {'params' : stuff_parameters, 'lr': 0.005, 'weight_decay': 1e-4},
            {'params': combined_parameters, 'lr': 0.005, 'weight_decay': 1e-4}
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
            
