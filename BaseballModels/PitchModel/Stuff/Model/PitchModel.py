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
                
                loss_backprop_weights : list[float] = [1, 1, 0.1, 1, 1, 0.1, 1, 1, 0.1],
                
                
                ########## PRED BLOCKS ##########
                ## Combined ##
                combined_pred_size_result : int = 64,
                combined_pred_blocks_result : int = 8,
                combined_pred_dropout_result : float = 0.1,
                combined_result_weight_decay : float = 1e-4,
                
                combined_pred_size_swing : int = 64,
                combined_pred_blocks_swing : int = 2,
                combined_pred_dropout_swing : float = 0.1,
                combined_swing_weight_decay : float = 1e-4,
                
                combined_pred_size_inplay : int = 64,
                combined_pred_blocks_inplay : int = 8,
                combined_pred_dropout_inplay : float = 0.3,
                combined_inplay_weight_decay : float = 1e-4,
                
                ## Location ##
                location_pred_size_result : int = 128,
                location_pred_blocks_result : int = 8,
                location_pred_dropout_result = 0.1,
                location_result_weight_decay : float = 1e-4,
                
                location_pred_size_swing : int = 128,
                location_pred_blocks_swing : int = 4,
                location_pred_dropout_swing : float = 0.1,
                location_swing_weight_decay : float = 1e-4,
                
                location_pred_size_inplay : int = 64,
                location_pred_blocks_inplay : int = 8,
                location_pred_dropout_inplay : float = 0.3,
                location_inplay_weight_decay : float = 1e-4,
                
                ## Stuff ##
                stuff_pred_size_result : int = 64,
                stuff_pred_blocks_result : int = 2,
                stuff_pred_dropout_result : float = 0.3,
                stuff_result_weight_decay : float = 1e-4,
                
                stuff_pred_size_swing : int = 64,
                stuff_pred_blocks_swing : int = 2,
                stuff_pred_dropout_swing : float = 0.1,
                stuff_swing_weight_decay : float = 1e-4,
                
                stuff_pred_size_inplay : int = 64,
                stuff_pred_blocks_inplay : int = 8,
                stuff_pred_dropout_inplay : float = 0.3,
                stuff_inplay_weight_decay : float = 1e-4,
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        self.loss_backprop_weights = loss_backprop_weights
        self.nonlin = F.leaky_relu
        
        # Prediction layers for Location only
        self.location_pred = PitcherPredLayers(
            input_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size,
            
            block_size_result=location_pred_size_result,
            num_layers_result=location_pred_blocks_result,
            dropout_result=location_pred_dropout_result,
            
            block_size_swing=location_pred_size_swing,
            num_layers_swing=location_pred_blocks_swing,
            dropout_swing=location_pred_dropout_swing,
            
            block_size_inplay=location_pred_size_inplay,
            num_layers_inplay=location_pred_blocks_inplay,
            dropout_inplay=location_pred_dropout_inplay,
        )
            
        # Prediction layers for stuff only
        self.stuff_pred = PitcherPredLayers(
            input_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.league_baseline_size,
            
            block_size_result=stuff_pred_size_result,
            num_layers_result=stuff_pred_blocks_result,
            dropout_result=stuff_pred_dropout_result,
            
            block_size_swing=stuff_pred_size_swing,
            num_layers_swing=stuff_pred_blocks_swing,
            dropout_swing=stuff_pred_dropout_swing,
            
            block_size_inplay=stuff_pred_size_inplay,
            num_layers_inplay=stuff_pred_blocks_inplay,
            dropout_inplay=stuff_pred_dropout_inplay,
        )
        
        # Prediction layers for location + stuff
        self.combined_pred = PitcherPredLayers(
            input_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size + \
                prep_map.pitch_stuff_size + prep_map.league_baseline_size + \
                    prep_map.pitch_combined_size,
            
            block_size_result=combined_pred_size_result,
            num_layers_result=combined_pred_blocks_result,
            dropout_result=combined_pred_dropout_result,
            
            block_size_swing=combined_pred_size_swing,
            num_layers_swing=combined_pred_blocks_swing,
            dropout_swing=combined_pred_dropout_swing,
            
            block_size_inplay=combined_pred_size_inplay,
            num_layers_inplay=combined_pred_blocks_inplay,
            dropout_inplay=combined_pred_dropout_inplay,
        )
        
        # Set parameter groups to allow for sub-parts of network to set learning rates independently
        stuff_result_parameters = GetParameters(self.stuff_pred.result_modules)
        location_result_parameters = GetParameters(self.location_pred.result_modules)
        combined_result_parameters = GetParameters(self.combined_pred.result_modules)
        
        stuff_swing_parameters = GetParameters(self.stuff_pred.swing_modules)
        location_swing_parameters = GetParameters(self.location_pred.swing_modules)
        combined_swing_parameters = GetParameters(self.combined_pred.swing_modules)
        
        stuff_inplay_parameters = GetParameters(self.stuff_pred.inplay_modules)
        location_inplay_parameters = GetParameters(self.location_pred.inplay_modules)
        combined_inplay_parameters = GetParameters(self.combined_pred.inplay_modules)
        
        self.optimizer = torch.optim.AdamW([
            {'params': location_result_parameters, 'lr': 0.005, 'weight_decay': location_result_weight_decay},
            {'params': location_swing_parameters, 'lr': 0.005, 'weight_decay': location_swing_weight_decay},
            {'params': location_inplay_parameters, 'lr': 0.001, 'weight_decay': location_inplay_weight_decay},
            
            {'params' : stuff_result_parameters, 'lr': 0.005, 'weight_decay': stuff_result_weight_decay},
            {'params' : stuff_swing_parameters, 'lr': 0.001, 'weight_decay': stuff_swing_weight_decay},
            {'params' : stuff_inplay_parameters, 'lr': 0.001, 'weight_decay': stuff_inplay_weight_decay},
            
            {'params': combined_result_parameters, 'lr': 0.005, 'weight_decay': combined_result_weight_decay},
            {'params': combined_swing_parameters, 'lr': 0.001, 'weight_decay': combined_swing_weight_decay},
            {'params': combined_inplay_parameters, 'lr': 0.001, 'weight_decay': combined_inplay_weight_decay}
        ])
        
    def forward(self, data : tuple[torch.Tensor, ...]) -> tuple[torch.Tensor, ...]:
        overview, location, stuff, combined, game, league_avg = data
        # Location
        data_location = torch.cat((overview, location), dim=-1)
        output_location = self.location_pred(data_location)
        
        # Stuff
        data_stuff = torch.cat((overview, stuff, league_avg), dim=-1)
        output_stuff = self.stuff_pred(data_stuff)
        
        # Combined
        data_combined = torch.cat((overview, location, stuff, league_avg, combined), dim=-1)
        output_combined = self.combined_pred(data_combined)
        
        return output_location + output_stuff + output_combined
            
