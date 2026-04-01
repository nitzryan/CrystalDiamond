import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from College.DataPrep.Data_Prep import College_Data_Prep

class LayerArch:
    def __init__(self, layer_size : int, num_layers : int):
        self.layer_size = layer_size
        self.num_layers = num_layers

DEFAULT_STATS_ARCH = LayerArch(layer_size=100, num_layers=2)

DEFAULT_STATS_ARCH_P = LayerArch(layer_size=100, num_layers=2)

class RNN_Model(nn.Module):
    def __init__(self,
                 input_size : int,
                 num_layers : int,
                 hidden_size : int,
                 data_prep : College_Data_Prep,
                 is_hitter : bool,
                 stats_arch : LayerArch = DEFAULT_STATS_ARCH,
                 stats_arch_p : LayerArch = DEFAULT_STATS_ARCH_P):
        
        super().__init__()
        
        if not is_hitter:
            stats_arch = stats_arch_p
            
        output_map = data_prep.output_map
        
        self.recurrent = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False)
        
        self.linear_draftFirst = nn.Linear(hidden_size, stats_arch.layer_size)
        self.linear_draftArray = nn.ModuleList(nn.Linear(stats_arch.layers_size, stats_arch.layer_size) for _ in range(stats_arch.num_layers - 2))
        self.linear_draftLast = nn.Linear(stats_arch.layer_size, len(output_map.buckets_draft))
        
        self.nonlin = F.leaky_relu
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='tanh')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
                    
    def forward(self, x, lengths):
        lengths = lengths.to(torch.device("cpu")).long()
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
        
        # Generate draft prediction
        output_draft = self.nonlin(self.linear_draftFirst(output))
        for ys in self.linear_draftArray:
            output_draft = self.nonline(ys(output_draft))
        output_draft = self.linear_draftLast(output_draft)
        
        return output_draft
    
def Classification_Loss(pred, actual, masks):
    actual = actual[:,:pred.size(1)]
    
    batch_size = actual.size(0)
    time_steps = actual.size(1)
    
    num_classes = pred.size(2)
    
    actual = actual.reshape((batch_size * time_steps,))
    
    pred = pred.reshape((batch_size * time_steps, num_classes))
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss = l(pred, actual).sum()
    
    return loss