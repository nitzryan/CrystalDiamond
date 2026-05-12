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
                
                loss_backprop_weights : list[float] = [1, 0.1, 1, 0.1, 1, 0.1],
                
                ########## PRED BLOCKS ##########
                ## Combined ##
                combined_pred_size_result : int = 256,
                combined_pred_blocks_result : int = 2,
                combined_pred_dropout_result : float = 0.4,
                
                combined_pred_size_inplay : int = 128,
                combined_pred_blocks_inplay : int = 2,
                combined_pred_dropout_inplay : float = 0.4,
                
                ## Location ##
                location_pred_size_result : int = 256,
                location_pred_blocks_result : int = 4,
                location_pred_dropout_result = 0.2,
                
                location_pred_size_inplay : int = 128,
                location_pred_blocks_inplay : int = 2,
                location_pred_dropout_inplay : float = 0.4,
                
                ## Stuff ##
                stuff_pred_size_result : int = 128,
                stuff_pred_blocks_result : int = 2,
                stuff_pred_dropout_result : float = 0.4,
                
                stuff_pred_size_inplay : int = 128,
                stuff_pred_blocks_inplay : int = 2,
                stuff_pred_dropout_inplay : float = 0.4,
                
                ########## INIT BLOCKS ##########
                ## Location ##
                location_init_size : int = 256,
                location_init_layers : int = 4,
                location_branch_size : int = 512,
                location_init_dropout : float = 0.1,
                
                ## Stuff ##
                stuff_init_size : int = 256,
                stuff_init_layers : int = 4,
                stuff_branch_size : int = 512,
                stuff_init_dropout : float = 0.1,
                
                ## Combined
                combined_init_size : int = 256,
                combined_init_layers : int = 4,
                combined_branch_size : int = 512,
                combined_init_dropout : float = 0.1
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        self.loss_backprop_weights = loss_backprop_weights
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
        
        # Data only for pitch model (combines movement and location)
        self.combined_block = LayerArch(
            input_size=prep_map.pitch_combined_size,
            num_blocks=combined_init_layers,
            block_dim=combined_init_size,
            output_size=combined_branch_size,
            dropout=combined_init_dropout,
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
            input_size=location_branch_size + stuff_branch_size + combined_branch_size,
            
            block_size_result=combined_pred_size_result,
            num_layers_result=combined_pred_blocks_result,
            dropout_result=combined_pred_dropout_result,
            
            block_size_inplay=combined_pred_size_inplay,
            num_layers_inplay=combined_pred_blocks_inplay,
            dropout_inplay=combined_pred_dropout_inplay,
        )
        
        # Set parameter groups to allow for sub-parts of network to set learning rates independently
        stuff_result_parameters = GetParameters([self.stuff_block, self.stuff_pred.result_modules])
        location_result_parameters = GetParameters([self.location_block, self.location_pred.result_modules])
        combined_result_parameters = GetParameters([self.combined_block, self.combined_pred.result_modules])
        
        stuff_inplay_parameters = GetParameters(self.stuff_pred.inplay_modules)
        location_inplay_parameters = GetParameters(self.location_pred.inplay_modules)
        combined_inplay_parameters = GetParameters(self.combined_pred.inplay_modules)
        
        self.optimizer = torch.optim.AdamW([
            {'params': location_result_parameters, 'lr': 0.005, 'weight_decay': 1e-4},
            {'params': location_inplay_parameters, 'lr': 0.001, 'weight_decay': 3e-4},
            
            {'params' : stuff_result_parameters, 'lr': 0.005, 'weight_decay': 1e-4},
            {'params' : stuff_inplay_parameters, 'lr': 0.001, 'weight_decay': 3e-4},
            
            {'params': combined_result_parameters, 'lr': 0.005, 'weight_decay': 1e-4},
            {'params': combined_inplay_parameters, 'lr': 0.001, 'weight_decay': 3e-4}
        ])
        
    def forward(self, data : tuple[torch.Tensor, ...]) -> tuple[torch.Tensor, ...]:
        overview, location, stuff, combined, game, league_avg = data
        # Location
        data_location = torch.cat((overview, location), dim=-1)
        inter_location = self.location_block(data_location)
        
        # Stuff
        data_stuff = torch.cat((overview, stuff, game, league_avg), dim=-1)
        inter_stuff = self.stuff_block(data_stuff)
        
        # Combined
        inter_combined = self.combined_block(combined)
        inter_comb = torch.cat((inter_combined, inter_location, inter_stuff), dim=-1)
        
        # Location only ouput layers
        output_location = self.location_pred(inter_location)
        output_stuff = self.stuff_pred(inter_stuff)
        output_combined = self.combined_pred(inter_comb)
        
        return output_location + output_stuff + output_combined
            
