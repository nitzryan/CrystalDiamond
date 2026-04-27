import torch
import torch.nn as nn
import torch.nn.functional as F
from Stuff.DataPrep.DataPrep import DataPrep
from Buckets import *
from Stuff.Model.LayerArch import LayerArch
from Stuff.Model.PitcherPredLayers import PitcherPredLayers
from Stuff.Model.Utilities import *

DEFAULT_LOCATION_LA = LayerArch(layer_size=120, num_layers=4)
DEFAULT_STUFF_LA = LayerArch(layer_size=120, num_layers=4)
        
class PitchModel(nn.Module):
    def __init__(self,
                data_prep : DataPrep,
                location_branch_size : int = 55,
                stuff_branch_size : int = 55,
    ):
        super().__init__()
        
        prep_map = data_prep.prep_map
        
        self.nonlin = F.leaky_relu
        
        # Location and pitch overview
        self.layers_location = DEFAULT_LOCATION_LA.GetArchitecture(
            hidden_size=prep_map.pitch_loc_size + prep_map.pitch_overview_size,
            output_size=location_branch_size
        )
        
        # Stuff and Pitch Overview
        self.layers_stuff = DEFAULT_STUFF_LA.GetArchitecture(
            hidden_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.pitcher_game_size + prep_map.league_baseline_size,
            output_size=stuff_branch_size
        )
        
        # Prediction layers for Location only
        self.layers_loc_value, self.layers_loc_runs, self.layers_loc_outs, self.layers_loc_swung, self.layers_loc_contact, self.layers_loc_inplay = \
            PitcherPredLayers(location_branch_size).GetArchitecture()
            
        # Prediction layers for stuff only
        self.layers_stuff_value, self.layers_stuff_runs, self.layers_stuff_outs, self.layers_stuff_swung, self.layers_stuff_contact, self.layers_stuff_inplay = \
            PitcherPredLayers(stuff_branch_size).GetArchitecture()
        
        # Prediction layers for location + stuff
        self.layers_comb_value, self.layers_comb_runs, self.layers_comb_outs, self.layers_comb_swung, self.layers_comb_contact, self.layers_comb_inplay = \
            PitcherPredLayers(location_branch_size + stuff_branch_size).GetArchitecture()
        
    def forward(self, data : tuple[torch.Tensor, ...]) -> tuple[torch.Tensor, ...]:
        overview, location, stuff, game, league_avg = data
        # Location
        data_location = torch.cat((overview, location), dim=-1)
        inter_location = GetModuleOutput(data_location, self.layers_location, self.nonlin)
        
        # Stuff
        data_stuff = torch.cat((overview, stuff, game, league_avg), dim=-1)
        inter_stuff = GetModuleOutput(data_stuff, self.layers_stuff, self.nonlin)
        
        inter_comb = torch.cat((inter_location, inter_stuff), dim=-1)
        
        # Location only ouput layers
        output_location_value = GetModuleOutput(inter_location, self.layers_loc_value, self.nonlin)
        output_location_runs = GetModuleOutput(inter_location, self.layers_loc_runs, self.nonlin)
        output_location_outs = GetModuleOutput(inter_location, self.layers_loc_outs, self.nonlin)
        output_location_swung = GetModuleOutput(inter_location, self.layers_loc_swung, self.nonlin)
        output_location_contact = GetModuleOutput(inter_location, self.layers_loc_contact, self.nonlin)
        output_location_inplay = GetModuleOutput(inter_location, self.layers_loc_inplay, self.nonlin)
        
        # Stuff only ouput layers
        output_stuff_value = GetModuleOutput(inter_stuff, self.layers_stuff_value, self.nonlin)
        output_stuff_runs = GetModuleOutput(inter_stuff, self.layers_stuff_runs, self.nonlin)
        output_stuff_outs = GetModuleOutput(inter_stuff, self.layers_stuff_outs, self.nonlin)
        output_stuff_swung = GetModuleOutput(inter_stuff, self.layers_stuff_swung, self.nonlin)
        output_stuff_contact = GetModuleOutput(inter_stuff, self.layers_stuff_contact, self.nonlin)
        output_stuff_inplay = GetModuleOutput(inter_stuff, self.layers_stuff_inplay, self.nonlin)
        
        # Location + Stuff ouput layers
        output_comb_value = GetModuleOutput(inter_comb, self.layers_comb_value, self.nonlin)
        output_comb_runs = GetModuleOutput(inter_comb, self.layers_comb_runs, self.nonlin)
        output_comb_outs = GetModuleOutput(inter_comb, self.layers_comb_outs, self.nonlin)
        output_comb_swung = GetModuleOutput(inter_comb, self.layers_comb_swung, self.nonlin)
        output_comb_contact = GetModuleOutput(inter_comb, self.layers_comb_contact, self.nonlin)
        output_comb_inplay = GetModuleOutput(inter_comb, self.layers_comb_inplay, self.nonlin)
        
        return \
            (output_location_value, output_location_runs, output_location_outs, output_location_swung, output_location_contact, output_location_inplay, \
            output_stuff_value, output_stuff_runs, output_stuff_outs, output_stuff_swung, output_stuff_contact, output_stuff_inplay, \
            output_comb_value, output_comb_runs, output_comb_outs, output_comb_swung, output_comb_contact, output_comb_inplay)
            
