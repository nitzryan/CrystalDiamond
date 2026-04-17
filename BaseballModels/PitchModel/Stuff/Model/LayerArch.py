import torch.nn as nn

class LayerArch:
    def __init__(self, layer_size : int, num_layers : int):
        self.layer_size = layer_size
        self.num_layers = num_layers
        
    def GetArchitecture(self, hidden_size : int, output_size : int):
        return nn.ModuleList([nn.Linear(hidden_size, self.layer_size)] + \
                            [nn.Linear(self.layer_size, self.layer_size) for _ in range(self.num_layers - 2)] +\
                            [nn.Linear(self.layer_size, output_size)])