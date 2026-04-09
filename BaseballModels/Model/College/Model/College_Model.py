import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from College.DataPrep.Data_Prep import College_Data_Prep
from Constants import DRAFT_BUCKETS, TOTAL_WAR_BUCKETS, HITTER_PA_BUCKETS, OFF_RATE_BUCKETS, DEF_RATE_BUCKETS

class LayerArch:
    def __init__(self, layer_size : int, num_layers : int):
        self.layer_size = layer_size
        self.num_layers = num_layers

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
        
        self.linear_draftFirst = nn.Linear(hidden_size, stats_arch.layer_size)
        self.linear_draftArray = nn.ModuleList(nn.Linear(stats_arch.layer_size, stats_arch.layer_size) for _ in range(stats_arch.num_layers - 2))
        self.linear_draftLast = nn.Linear(stats_arch.layer_size, len(DRAFT_BUCKETS))
        
        self.linear_posFirst = nn.Linear(hidden_size, stats_arch_pos.layer_size)
        self.linear_posArray = nn.ModuleList(nn.Linear(stats_arch_pos.layer_size, stats_arch_pos.layer_size) for _ in range(stats_arch_pos.num_layers - 2))
        self.linear_posLast = nn.Linear(stats_arch_pos.layer_size, pos_output_len)
        
        self.linear_warFirst = nn.Linear(hidden_size, war_arch.layer_size)
        self.linear_warArray = nn.ModuleList(nn.Linear(war_arch.layer_size, war_arch.layer_size) for _ in range(war_arch.num_layers - 2))
        self.linear_warLast = nn.Linear(war_arch.layer_size, len(TOTAL_WAR_BUCKETS))
        
        if self.is_hitter:
            self.linear_offFirst = nn.Linear(hidden_size, off_arch.layer_size)
            self.linear_offArray = nn.ModuleList(nn.Linear(off_arch.layer_size, off_arch.layer_size) for _ in range(off_arch.num_layers - 2))
            self.linear_offLast = nn.Linear(off_arch.layer_size, len(OFF_RATE_BUCKETS) + 1)
            
            self.linear_defFirst = nn.Linear(hidden_size, def_arch.layer_size)
            self.linear_defArray = nn.ModuleList(nn.Linear(def_arch.layer_size, def_arch.layer_size) for _ in range(def_arch.num_layers - 2))
            self.linear_defLast = nn.Linear(def_arch.layer_size, len(DEF_RATE_BUCKETS) + 1)
            
            self.linear_paFirst = nn.Linear(hidden_size, pa_arch.layer_size)
            self.linear_paArray = nn.ModuleList(nn.Linear(pa_arch.layer_size, pa_arch.layer_size) for _ in range(pa_arch.num_layers - 2))
            self.linear_paLast = nn.Linear(pa_arch.layer_size, len(HITTER_PA_BUCKETS))
        
        self.nonlin = F.leaky_relu
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='tanh')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
                    
    def forward(self, x, lengths):
        if self.training:
            x = x + torch.rand_like(x) * self.noise
        
        lengths = lengths.to(torch.device("cpu")).long()
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
        
        # Generate draft prediction
        output_draft = self.nonlin(self.linear_draftFirst(output))
        for ys in self.linear_draftArray:
            output_draft = self.nonlin(ys(output_draft))
        output_draft = self.linear_draftLast(output_draft)
        
        # Evaluate WAR prediction
        output_war = self.nonlin(self.linear_warFirst(output))
        for ys in self.linear_warArray:
            output_war = self.nonlin(ys(output_war))
        output_war = self.linear_warLast(output_war)
        
        # Generate Pro pos prediction
        output_pos = self.nonlin(self.linear_posFirst(output))
        for ys in self.linear_posArray:
            output_pos = self.nonlin(ys(output_pos))
        output_pos = self.linear_posLast(output_pos)
        
        if self.is_hitter:
            # Evaluate OFF prediction
            output_off = self.nonlin(self.linear_offFirst(output))
            for ys in self.linear_offArray:
                output_off = self.nonlin(ys(output_off))
            output_off = self.linear_offLast(output_off)
            
            # Evaluate DEF prediction
            output_def = self.nonlin(self.linear_defFirst(output))
            for ys in self.linear_defArray:
                output_def = self.nonlin(ys(output_def))
            output_def = self.linear_defLast(output_def)
            
            # Evaluate OFF prediction
            output_pa = self.nonlin(self.linear_paFirst(output))
            for ys in self.linear_paArray:
                output_pa = self.nonlin(ys(output_pa))
            output_pa = self.linear_paLast(output_pa)
            
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