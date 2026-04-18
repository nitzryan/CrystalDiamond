import torch.nn as nn
from Buckets import *

class PitcherPredLayers:
    def __init__(self, 
                input_size : int,
                layer_size_value : int = 50,
                layer_size_runs : int = 30,
                layer_size_outs : int = 20,
                layer_size_swung : int = 20,
                layer_size_contact : int = 20,
                layer_size_inplay : int = 20):
        self.input_size = input_size
        
        self.layer_size_value = layer_size_value
        self.layer_size_runs = layer_size_runs
        self.layer_size_outs = layer_size_outs
        self.layer_size_swung = layer_size_swung
        self.layer_size_contact = layer_size_contact
        self.layer_size_inplay = layer_size_inplay
        
    def GetArchitecture(self) -> tuple[nn.ModuleList, nn.ModuleList, nn.ModuleList, nn.ModuleList, nn.ModuleList, nn.ModuleList]:
        return \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_value), nn.Linear(self.layer_size_value, BUCKET_PITCHVALUE.size(0) + 1)]), \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_runs), nn.Linear(self.layer_size_runs, 5)]), \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_outs), nn.Linear(self.layer_size_outs, 3)]), \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_swung), nn.Linear(self.layer_size_swung, 2)]), \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_contact), nn.Linear(self.layer_size_contact, 2)]), \
            nn.ModuleList([nn.Linear(self.input_size, self.layer_size_inplay), nn.Linear(self.layer_size_inplay, 2)])