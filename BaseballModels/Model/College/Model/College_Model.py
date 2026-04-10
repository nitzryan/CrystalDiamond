import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from College.DataPrep.Data_Prep import College_Data_Prep
from Constants import DRAFT_BUCKETS, TOTAL_WAR_BUCKETS, HITTER_PA_BUCKETS, OFF_RATE_BUCKETS, DEF_RATE_BUCKETS
from Pro.Model.Player_Model import LayerArch

DEFAULT_STATS_ARCH = LayerArch(layer_size=50, num_layers=3)
DEFAULT_POS_ARCH = LayerArch(layer_size=50, num_layers=2)

DEFAULT_WAR_ARCH = LayerArch(layer_size=50, num_layers=2)
DEFAULT_WAR_ARCH_P = LayerArch(layer_size=50, num_layers=2)

DEFAULT_STATS_ARCH_P = LayerArch(layer_size=100, num_layers=2)
DEFAULT_POS_ARCH_P = LayerArch(layer_size=50, num_layers=2)

DEFAULT_OFF_ARCH = LayerArch(layer_size=100, num_layers=2)
DEFAULT_DEF_ARCH = LayerArch(layer_size=100, num_layers=2)
DEFAULT_PA_ARCH = LayerArch(layer_size=100, num_layers=2)

class RNN_Model(nn.Module):
    def __init__(self,
                 input_size : int,
                 data_prep : College_Data_Prep,
                 is_hitter : bool,
                 num_layers : int = 3,
                 hidden_size : int = 20,
                 noise : float = 0.125,
                 dropout : float = 0.075,
                 stats_arch : LayerArch = DEFAULT_STATS_ARCH,
                 stats_arch_p : LayerArch = DEFAULT_STATS_ARCH_P,
                 stats_arch_pos : LayerArch = DEFAULT_POS_ARCH,
                 stats_arch_pos_p : LayerArch = DEFAULT_POS_ARCH_P,
                 war_arch : LayerArch = DEFAULT_WAR_ARCH,
                 war_arch_p : LayerArch = DEFAULT_WAR_ARCH_P,
                 off_arch : LayerArch = DEFAULT_OFF_ARCH,
                 def_arch : LayerArch = DEFAULT_DEF_ARCH,
                 pa_arch : LayerArch = DEFAULT_PA_ARCH,):
        
        super().__init__()
        
        if is_hitter:
            pos_output_len = data_prep.output_map.len_pos_h
        else:
            stats_arch = stats_arch_p
            stats_arch_pos = stats_arch_pos_p
            war_arch = war_arch_p
            pos_output_len = data_prep.output_map.len_pos_p
        
        self.is_hitter = is_hitter
        # Solely so they can be extracted during training    
        self.num_layers = num_layers
        self.hidden_size = hidden_size
        
        self.noise = noise
        
        self.recurrent = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False, 
                                dropout=dropout,
                                )
        
        self.draft_layers = stats_arch.GetArchitecture(hidden_size, len(DRAFT_BUCKETS))
        self.pos_layers = stats_arch_pos.GetArchitecture(hidden_size, pos_output_len)
        self.war_layers = war_arch.GetArchitecture(hidden_size, len(TOTAL_WAR_BUCKETS))
        
        if self.is_hitter:
            self.off_layers = off_arch.GetArchitecture(hidden_size, len(OFF_RATE_BUCKETS) + 1)
            self.def_layers = def_arch.GetArchitecture(hidden_size, len(DEF_RATE_BUCKETS) + 1)
            self.pa_layers = pa_arch.GetArchitecture(hidden_size, len(HITTER_PA_BUCKETS))
        
        self.nonlin = F.leaky_relu
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='tanh')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
          
    def GetModuleOutput(self, output : torch.Tensor, moduleList : nn.ModuleList) -> torch.Tensor:
        for layer in moduleList:
            output = layer(output)
            if layer != moduleList[-1]:
                output = self.nonlin(output)
                
        return output
                    
    def forward(self, x, lengths):
        if self.training:
            x = x + torch.rand_like(x) * self.noise
        
        lengths = lengths.to(torch.device("cpu")).long()
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
        
        output_draft = self.GetModuleOutput(output, self.draft_layers)
        output_war = self.GetModuleOutput(output, self.war_layers)
        output_pos = self.GetModuleOutput(output, self.pos_layers)
        
        if self.is_hitter:
            output_off = self.GetModuleOutput(output, self.off_layers)
            output_def = self.GetModuleOutput(output, self.def_layers)
            output_pa = self.GetModuleOutput(output, self.pa_layers)
            
            return output_draft, output_war, output_off, output_def, output_pa, output_pos
        
        else:
            return output_draft, output_war, output_pos
    
def Classification_Loss(pred : torch.Tensor, 
                        actual : torch.Tensor, 
                        mask : torch.Tensor) -> torch.Tensor:
    # Pred is <Batch, Timestep, Classes>
    # Actual is <Batch, Classes>
    # Mask is <Batch>
    batch_size = pred.size(0)
    time_steps = pred.size(1)
    
    num_classes = pred.size(2)
    pred = pred.reshape((batch_size * time_steps, num_classes))
    actual = actual.repeat_interleave(time_steps)
    mask = mask.reshape((batch_size * time_steps,))
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss = l(pred, actual)
    loss = loss * mask
    loss = loss.sum()
    
    return loss

def Position_Loss(pred : torch.Tensor, 
                        actual : torch.Tensor, 
                        mask : torch.Tensor) -> torch.Tensor:
    # Pred is <Batch, Timestep, Classes>
    # Actual is <Batch, Classes>
    # Mask is <Batch, Timestep>
    batch_size = pred.size(0)
    time_steps = pred.size(1)
    
    num_classes = pred.size(2)
    pred = pred.reshape((batch_size * time_steps, num_classes))
    actual = actual.repeat_interleave(time_steps, dim=0).reshape((batch_size * time_steps, num_classes))
    mask = mask.reshape((batch_size * time_steps,))
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss = l(pred, actual)
    loss = loss * mask
    loss = loss.sum()
    
    return loss