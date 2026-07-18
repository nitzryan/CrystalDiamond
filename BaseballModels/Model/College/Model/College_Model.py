import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
import json

from Model.College.DataPrep.Data_Prep import College_Data_Prep
from Model.Constants import DRAFT_BUCKETS, TOTAL_WAR_BUCKETS, HITTER_PA_BUCKETS, OFF_RATE_BUCKETS, DEF_RATE_BUCKETS
from Model.Pro.Model.Player_Model import LayerArch

DEFAULT_DRAFT_ARCH = LayerArch(layer_size=64, num_layers=2)
DEFAULT_POS_ARCH = LayerArch(layer_size=64, num_layers=1)

DEFAULT_WAR_ARCH = LayerArch(layer_size=19, num_layers=3)
DEFAULT_WAR_ARCH_P = LayerArch(layer_size=50, num_layers=2)

DEFAULT_DRAFT_ARCH_P = LayerArch(layer_size=100, num_layers=2)
DEFAULT_POS_ARCH_P = LayerArch(layer_size=50, num_layers=2)

DEFAULT_OFF_ARCH = LayerArch(layer_size=32, num_layers=2)
DEFAULT_DEF_ARCH = LayerArch(layer_size=32, num_layers=2)
DEFAULT_PA_ARCH = LayerArch(layer_size=32, num_layers=2)

DEFAULT_HITTER_HIDDEN_ARCH = LayerArch(layer_size=100, num_layers=2)
DEFAULT_PITCHER_HIDDEN_ARCH = LayerArch(layer_size=100, num_layers=2)

DEFAULT_LEARNING_RATE = [0.0137, 0.01, 0.00697, 0.01, 0.01, 0.01, 0.01, 2.1e-5]
DEFAULT_LEARNING_RATE_P = [0.01, 0.01, 0.01, 0.01, 0.01]

DEFAULT_WEIGHT_DECAY = [1.6e-6,1e-7,6.4e-3,1e-7,1e-7,1e-7,1e-7,2.9e-7]
DEFAULT_WEIGHT_DECAY_P = [1e-7,1e-7,1e-7,1e-7,1e-7]

class RNN_Model(nn.Module):
    def __init__(self,
                input_size : int,
                data_prep : College_Data_Prep,
                is_hitter : bool,
                save_name : str | None = None,
                
                num_layers : int = 2,
                hidden_size : int = 8,
                noise : float = 0.0257,
                dropout : float = 0.117,
                draft_arch : LayerArch = DEFAULT_DRAFT_ARCH,
                draft_arch_p : LayerArch = DEFAULT_DRAFT_ARCH_P,
                pos_arch : LayerArch = DEFAULT_POS_ARCH,
                pos_arch_p : LayerArch = DEFAULT_POS_ARCH_P,
                war_arch : LayerArch = DEFAULT_WAR_ARCH,
                war_arch_p : LayerArch = DEFAULT_WAR_ARCH_P,
                off_arch : LayerArch = DEFAULT_OFF_ARCH,
                def_arch : LayerArch = DEFAULT_DEF_ARCH,
                pa_arch : LayerArch = DEFAULT_PA_ARCH,
                
                output_hidden_size : int = -1,
                output_num_layers : int = -1,
                output_hidden_arch : LayerArch = DEFAULT_HITTER_HIDDEN_ARCH,
                output_hidden_arch_p : LayerArch = DEFAULT_PITCHER_HIDDEN_ARCH,
                
                lr : list[float] = DEFAULT_LEARNING_RATE,
                lr_p : list[float] = DEFAULT_LEARNING_RATE_P,
                weight_decay : list[float] = DEFAULT_WEIGHT_DECAY,
                weight_decay_p : list[float] = DEFAULT_WEIGHT_DECAY_P):
        
        super().__init__()
        
        if save_name is not None:
            with open(save_name, "w") as f:
                config = {
                    "input_size": input_size,
                    "is_hitter": is_hitter,
                    
                    # RNN / core parameters
                    "num_layers": num_layers,
                    "hidden_size": hidden_size,
                    "noise": noise,
                    "dropout": dropout,
                    
                    # LayerArch definitions
                    "draft_arch": draft_arch.ToDict(),
                    "draft_arch_p": draft_arch_p.ToDict(),
                    "pos_arch": pos_arch.ToDict(),
                    "pos_arch_p": pos_arch_p.ToDict(),
                    "war_arch": war_arch.ToDict(),
                    "war_arch_p": war_arch_p.ToDict(),
                    "off_arch": off_arch.ToDict(),
                    "def_arch": def_arch.ToDict(),
                    "pa_arch": pa_arch.ToDict(),
                    
                    # Output head architecture
                    "output_hidden_size": output_hidden_size,
                    "output_num_layers": output_num_layers,
                    "output_hidden_arch": output_hidden_arch.ToDict(),
                    "output_hidden_arch_p": output_hidden_arch_p.ToDict(),
                    
                    # Training hyperparameters
                    "lr": lr,
                    "lr_p": lr_p,
                    "weight_decay": weight_decay,
                    "weight_decay_p": weight_decay_p,
                }
                json.dump(config, f, indent=2)
        
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        
        if num_layers == 1:
            dropout = 0 # Suppresses warning, dropout not used with 1 layesr
        
        if is_hitter:
            pos_output_len = data_prep.output_map.len_pos_h
        else:
            draft_arch = draft_arch_p
            pos_arch = pos_arch_p
            war_arch = war_arch_p
            output_hidden_arch = output_hidden_arch_p
            pos_output_len = data_prep.output_map.len_pos_p
            lr = lr_p
            weight_decay = weight_decay_p
        
        self.is_hitter = is_hitter
        # Solely so they can be extracted during training    
        self.num_layers = num_layers
        self.hidden_size = hidden_size
        
        # Use to reshape hidden output for Pro Model
        self.output_num_layers = output_num_layers
        self.output_hidden_size = output_hidden_size
        
        self.noise = noise
        
        self.recurrent = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False, 
                                dropout=dropout,
                                )
        
        self.draft = draft_arch.Build(hidden_size, len(DRAFT_BUCKETS))
        self.pos = pos_arch.Build(hidden_size, pos_output_len)
        
        self.war = war_arch.Build(hidden_size, len(TOTAL_WAR_BUCKETS))
        
        self.hidden = output_hidden_arch.Build(hidden_size, output_hidden_size * output_num_layers)
        
        if self.is_hitter:
            self.off = off_arch.Build(hidden_size, len(OFF_RATE_BUCKETS) + 1)
            self.deff = def_arch.Build(hidden_size, len(DEF_RATE_BUCKETS) + 1)
            self.pa = pa_arch.Build(hidden_size, len(HITTER_PA_BUCKETS))
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='tanh')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
    

        self.optimizer = torch.optim.AdamW([{'params': self.recurrent.parameters(), 'lr': lr[0], 'weight_decay': weight_decay[0]},
                                           {'params': self.draft.parameters(), 'lr': lr[1], 'weight_decay': weight_decay[1]},
                                           {'params': self.war.parameters(), 'lr': lr[2], 'weight_decay': weight_decay[2]},
                                           {'params': self.pa.parameters(), 'lr': lr[3], 'weight_decay': weight_decay[3]},
                                           {'params': self.off.parameters(), 'lr': lr[4], 'weight_decay': weight_decay[4]},
                                           {'params': self.pos.parameters(), 'lr': lr[5], 'weight_decay': weight_decay[5]},
                                           {'params': self.deff.parameters(), 'lr': lr[6], 'weight_decay': weight_decay[6]},
                                           {'params': self.hidden.parameters(), 'lr': lr[7], 'weight_decay': weight_decay[7]}]) \
                        if is_hitter else \
                        torch.optim.AdamW([{'params': self.recurrent.parameters(), 'lr': lr[0], 'weight_decay': weight_decay[0]},
                                           {'params': self.draft.parameters(), 'lr': lr[1], 'weight_decay': weight_decay[1]},
                                           {'params': self.war.parameters(), 'lr': lr[2], 'weight_decay': weight_decay[2]},
                                           {'params': self.pos.parameters(), 'lr': lr[3], 'weight_decay': weight_decay[3]},
                                           {'params': self.hidden.parameters(), 'lr': lr[4], 'weight_decay': weight_decay[4]}])
    
    def GetHiddenSize(self) -> int:
        return self.hidden_size
    
    def GetNumLayers(self) -> int:
        return self.num_layers      
          
    def GetModuleOutput(self, output : torch.Tensor, moduleList : nn.ModuleList) -> torch.Tensor:
        for layer in moduleList:
            output = layer(output)
            if layer != moduleList[-1]:
                output = self.nonlin(output)
                
        return output
                    
    def forward(self, x, lengths):
        if self.training and self.noise > 0:
            x = x + torch.rand_like(x) * self.noise
        
        lengths = lengths.to(torch.device("cpu")).long()
        lengths = torch.clamp(lengths, min=1)
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
        
        output_draft = self.draft(output)
        output_pos = self.pos(output)
        
        output_war = self.war(output)
        
        output_hidden = self.hidden(output)
        output_hidden = output_hidden[
            torch.arange(output_hidden.size(0), device=output_hidden.device),
            lengths - 1
        ]
        output_hidden = output_hidden.reshape((output_hidden.shape[0], self.output_num_layers, self.output_hidden_size))
        
        if self.is_hitter:
            output_off = self.off(output)
            output_def = self.deff(output)
            output_pa = self.pa(output)
            
            return output_draft, output_war, output_off, output_def, output_pa, output_pos, output_hidden
        
        else:
            return output_draft, output_war, output_pos, output_hidden
        
    @classmethod
    def LoadFromFile(cls, args_file : str, data_prep : College_Data_Prep):
        with open(args_file) as file:
            args_dict = json.load(file)
            return cls(
                input_size=args_dict["input_size"],
                data_prep=data_prep,
                is_hitter=args_dict["is_hitter"],
                
                # RNN / core parameters
                num_layers=args_dict["num_layers"],
                hidden_size=args_dict["hidden_size"],
                noise=args_dict["noise"],
                dropout=args_dict["dropout"],
                
                # LayerArch
                draft_arch=LayerArch.LoadFromDict(args_dict["draft_arch"]),
                draft_arch_p=LayerArch.LoadFromDict(args_dict["draft_arch_p"]),
                pos_arch=LayerArch.LoadFromDict(args_dict["pos_arch"]),
                pos_arch_p=LayerArch.LoadFromDict(args_dict["pos_arch_p"]),
                war_arch=LayerArch.LoadFromDict(args_dict["war_arch"]),
                war_arch_p=LayerArch.LoadFromDict(args_dict["war_arch_p"]),
                off_arch=LayerArch.LoadFromDict(args_dict["off_arch"]),
                def_arch=LayerArch.LoadFromDict(args_dict["def_arch"]),
                pa_arch=LayerArch.LoadFromDict(args_dict["pa_arch"]),
                
                # Output head
                output_hidden_size=args_dict["output_hidden_size"],
                output_num_layers=args_dict["output_num_layers"],
                output_hidden_arch=LayerArch.LoadFromDict(args_dict["output_hidden_arch"]),
                output_hidden_arch_p=LayerArch.LoadFromDict(args_dict["output_hidden_arch_p"]),
                
                # Training hyperparameters
                lr=args_dict["lr"],
                lr_p=args_dict["lr_p"],
                weight_decay=args_dict["weight_decay"],
                weight_decay_p=args_dict["weight_decay_p"],
            )
    
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