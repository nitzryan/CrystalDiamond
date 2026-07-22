import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
import json

from Model.Pro.DataPrep.Data_Prep import Data_Prep
from Model.Pro.DataPrep.Output_StatAggregation import NUM_HITTER_STATS, NUM_HITTER_BUCKETS_PER_STAT, NUM_PITCHER_STATS, NUM_PITCHER_BUCKETS_PER_STAT
from Model.Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, NUM_LEVELS

class LayerArch(nn.Module):
    def __init__(self, layer_size: int, num_layers: int, nonlin=F.leaky_relu):
        super().__init__()
        self.layer_size = layer_size
        self.num_layers = num_layers
        self.nonlin = nonlin
        self.layers = None

    def Build(self, hidden_size: int, output_size: int):
        layers = [nn.Linear(hidden_size, self.layer_size)]
        for _ in range(self.num_layers - 2):
            layers.append(nn.Linear(self.layer_size, self.layer_size))
        layers.append(nn.Linear(self.layer_size, output_size))
        self.layers = nn.ModuleList(layers)
        return self

    def ToDict(self):
        nonlin_dict = {
            F.leaky_relu : 'leaky_relu',
            F.relu : 'relu',
            F.tanh : "tanh"
        }
        return {
            "layer_size" : self.layer_size,
            "num_layers" : self.num_layers,
            "nonlin" : nonlin_dict[self.nonlin],
        }
        
    @classmethod
    def LoadFromDict(cls, args_dict : dict):
        nonlin_dict = {
            'leaky_relu' : F.leaky_relu,
            'relu' : F.relu,
            "tanh" : F.tanh
        }
        return cls(
            args_dict["layer_size"], 
            args_dict["num_layers"],
            nonlin_dict[args_dict["nonlin"]]
        )

    def forward(self, x):
        for i, layer in enumerate(self.layers):
            x = layer(x)
            if i < len(self.layers) - 1:
                x = self.nonlin(x)
        return x

DEFAULT_WAR_ARCH = LayerArch(layer_size=92, num_layers=6, nonlin=F.leaky_relu)
DEFAULT_STATS_ARCH = LayerArch(layer_size=128, num_layers=2)
DEFAULT_PT_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_POS_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_LVL_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_PA_ARCH = LayerArch(layer_size=32, num_layers=4)
DEFAULT_VALUE_ARCH = LayerArch(layer_size=64, num_layers=2)
DEFAULT_MLBSTAT_ARCH = LayerArch(layer_size=30, num_layers=2)

DEFAULT_WAR_ARCH_P = LayerArch(layer_size=43, num_layers=5, nonlin=F.leaky_relu)
DEFAULT_STATS_ARCH_P = LayerArch(layer_size=90, num_layers=2)
DEFAULT_PT_ARCH_P = LayerArch(layer_size=110, num_layers=2)
DEFAULT_POS_ARCH_P = LayerArch(layer_size=55, num_layers=2)
DEFAULT_LVL_ARCH_P = LayerArch(layer_size=150, num_layers=2)
DEFAULT_PA_ARCH_P = LayerArch(layer_size=40, num_layers=2)
DEFAULT_VALUE_ARCH_P = LayerArch(layer_size=120, num_layers=2)
DEFAULT_MLBSTAT_ARCH_P = LayerArch(layer_size=100, num_layers=3)

DEFAULT_PRO_HIDDEN_SIZE = 71
DEFAULT_PRO_NUM_LAYERS = 2

DEFAULT_PRO_HIDDEN_SIZE_P = 51
DEFAULT_PRO_NUM_LAYERS_P = 3

DEFAULT_DROPOUT = 0.4696
DEFAULT_DROPOUT_P = 0.283

DEFAULT_INPUT_NOISE = 0
DEFAULT_INPUT_NOISE_P = 0

DEFAULT_PRO_WEIGHT_DECAY = [6.3e-2,1.8e-5,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7]
DEFAULT_PRO_WEIGHT_DECAY_P = [9.7e-4,8.4e-3,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7]

DEFAULT_LEARNING_RATES = [0.0029,0.030,0.003,0.003,0.003,0.003,0.003,0.003,0.003]
DEFAULT_LEARNING_RATES_P = [0.0076,0.018,0.003,0.003,0.003,0.003,0.003,0.003,0.003]

DEFAULT_INIT_STATE_SIZE = 40
DEFAULT_INIT_STATE_SIZE_P = 16
DEFAULT_INIT_STATE_ARCH = LayerArch(layer_size=64, num_layers=5, nonlin=F.relu)
DEFAULT_INIT_STATE_ARCH_P = LayerArch(layer_size=32, num_layers=4)

DEFAULT_RNN_NONLINEARITY = 'relu'
DEFAULT_RNN_NONLINEARITY_P = 'relu'

class RNN_Model(nn.Module):
    def __init__(self, 
                 
                input_size : int,  
                data_prep : Data_Prep, 
                is_hitter : bool, 
                save_name : str | None = None,
                
                rnn_droupout : float = DEFAULT_DROPOUT,
                num_layers : int = DEFAULT_PRO_NUM_LAYERS, 
                hidden_size : int = DEFAULT_PRO_HIDDEN_SIZE, 
                
                rnn_droupout_p : float = DEFAULT_DROPOUT_P,
                num_layers_p : int = DEFAULT_PRO_NUM_LAYERS_P, 
                hidden_size_p : int = DEFAULT_PRO_HIDDEN_SIZE_P, 
                
                rnn_nonlinearity : str = DEFAULT_RNN_NONLINEARITY,
                rnn_nonlinearity_p : str = DEFAULT_RNN_NONLINEARITY_P,
                
                input_noise = DEFAULT_INPUT_NOISE,
                input_noise_p = DEFAULT_INPUT_NOISE_P,
                
                stats_arch : LayerArch = DEFAULT_STATS_ARCH,
                war_arch : LayerArch = DEFAULT_WAR_ARCH,
                pt_arch : LayerArch = DEFAULT_PT_ARCH,
                pos_arch : LayerArch = DEFAULT_POS_ARCH,
                lvl_arch : LayerArch = DEFAULT_LVL_ARCH,
                pa_arch : LayerArch = DEFAULT_PA_ARCH,
                val_arch : LayerArch = DEFAULT_VALUE_ARCH,
                mlbstat_arch : LayerArch = DEFAULT_MLBSTAT_ARCH,
                
                stats_arch_p : LayerArch = DEFAULT_STATS_ARCH_P,
                war_arch_p : LayerArch = DEFAULT_WAR_ARCH_P,
                pt_arch_p : LayerArch = DEFAULT_PT_ARCH_P,
                pos_arch_p : LayerArch = DEFAULT_POS_ARCH_P,
                lvl_arch_p : LayerArch = DEFAULT_LVL_ARCH_P,
                pa_arch_p : LayerArch = DEFAULT_PA_ARCH_P,
                val_arch_p : LayerArch = DEFAULT_VALUE_ARCH_P,
                mlbstat_arch_p : LayerArch = DEFAULT_MLBSTAT_ARCH_P,
                
                weight_decay : list[float] = DEFAULT_PRO_WEIGHT_DECAY,
                weight_decay_p : list[float] = DEFAULT_PRO_WEIGHT_DECAY_P,
                
                learning_rates : list[float] = DEFAULT_LEARNING_RATES,
                learning_rates_p : list[float] = DEFAULT_LEARNING_RATES_P,
                
                init_state_size : float = DEFAULT_INIT_STATE_SIZE,
                init_state_size_p : float = DEFAULT_INIT_STATE_SIZE_P,
                init_state_arch : LayerArch = DEFAULT_INIT_STATE_ARCH,
                init_state_arch_p : LayerArch = DEFAULT_INIT_STATE_ARCH_P,
                ):
        super().__init__()
        if not is_hitter:
            rnn_droupout = rnn_droupout_p
            num_layers = num_layers_p
            hidden_size = hidden_size_p
            rnn_nonlinearity = rnn_nonlinearity_p
            input_noise = input_noise_p
            
            stats_arch = stats_arch_p
            war_arch = war_arch_p
            pt_arch = pt_arch_p
            pos_arch = pos_arch_p
            lvl_arch = lvl_arch_p
            pa_arch = pa_arch_p
            val_arch = val_arch_p
            mlbstat_arch = mlbstat_arch_p
            weight_decay = weight_decay_p
            learning_rates = learning_rates_p
            
            init_state_size = init_state_size_p
            init_state_arch = init_state_arch_p
        
        if save_name is not None:
            with open(save_name, "w") as f:
                config = {
                    "input_size": input_size,
                    "is_hitter": is_hitter,
                    
                    # RNN parameters
                    "rnn_droupout": rnn_droupout,
                    "num_layers": num_layers,
                    "hidden_size": hidden_size,
                    "rnn_nonlinearity": rnn_nonlinearity,
                    "input_noise": input_noise,
                    
                    # Initial Hidden State Calculation
                    "init_state_size" : init_state_size,
                    "init_state_arch" : init_state_arch.ToDict(),
                    
                    # LayerArch definitions
                    "stats_arch": stats_arch.ToDict(),
                    "war_arch": war_arch.ToDict(),
                    "pt_arch": pt_arch.ToDict(),
                    "pos_arch": pos_arch.ToDict(),
                    "lvl_arch": lvl_arch.ToDict(),
                    "pa_arch": pa_arch.ToDict(),
                    "val_arch": val_arch.ToDict(),
                    "mlbstat_arch": mlbstat_arch.ToDict(),
                    
                    # Training / other hyperparameters
                    "weight_decay": weight_decay,
                    "learning_rates": learning_rates,
                }
                json.dump(config, f, indent=2)
        
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        self.input_noise = input_noise
        self.init_state_size = init_state_size
        
        # Converting Initial State to RNN initial hidden state
        self.init_hidden = init_state_arch.Build(init_state_size + data_prep.prep_map.bio_size, hidden_size * num_layers)
        
        output_map = data_prep.output_map
        stats_size = output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size
        
        self.recurrent = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False, dropout=rnn_droupout, nonlinearity=rnn_nonlinearity)
        
        # Taking RNN output and making predictions
        num_war_classes = len(output_map.buckets_hitter_war)
        self.war = war_arch.Build(hidden_size, num_war_classes)
        self.level = lvl_arch.Build(hidden_size, len(HITTER_LEVEL_BUCKETS))
        self.pa = pa_arch.Build(hidden_size, len(HITTER_PA_BUCKETS))
        self.yearStats = stats_arch.Build(hidden_size, NUM_LEVELS * stats_size)
        self.pos = pos_arch.Build(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_positions_size if is_hitter else output_map.pitcher_positions_size))
        self.pt = pt_arch.Build(hidden_size + len(HITTER_LEVEL_BUCKETS), NUM_LEVELS * output_map.hitter_pt_size if is_hitter else NUM_LEVELS * output_map.pitcher_pt_size)
        self.value = val_arch.Build(hidden_size, (output_map.mlb_hitter_values_size if is_hitter else output_map.mlb_pitcher_values_size))
        
        if is_hitter:
            self.mlbstat = mlbstat_arch.Build(hidden_size, NUM_HITTER_STATS * NUM_HITTER_BUCKETS_PER_STAT)
        else:
            self.mlbstat = mlbstat_arch.Build(hidden_size, NUM_PITCHER_STATS * NUM_PITCHER_BUCKETS_PER_STAT)
        
        self.register_buffer('stat_offsets', data_prep.Get_HitStat_Offset() if is_hitter else data_prep.Get_PitStat_Offset())
        self.yearStats_output_transform = nn.Softplus(threshold=0.25)
        self.register_buffer('pt_offset', data_prep.Get_HitPt_Offset() if is_hitter else data_prep.Get_PitPt_Offset())
        
        # Range of stats to restrict quantile predictions
        self.war_min = data_prep.minHitWar if is_hitter else data_prep.minPitWar
        self.war_max = data_prep.maxHitWar if is_hitter else data_prep.maxPitWar
        
        # Initialize hidden state for players HS and international players
        self.init_hidden_hs = nn.Parameter(torch.zeros(init_state_size))
        self.init_hidden_intl = nn.Parameter(torch.zeros(init_state_size))
        
        self.softplus = nn.Softplus()
        
        self.pa_offset1, self.pa_offset2, self.pa_offset3 = data_prep.Get_Pa_Offsets()
        self.register_buffer('ip_offsets', data_prep.Get_Ip_Offsets())
        self.is_hitter = is_hitter
        #self.nonlin = F.leaky_relu
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='tanh')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
        
        # Set softmax-classification layers to Xavier for uniform initial predictions
        for layer in [self.war.layers[-1],
                      self.pa.layers[-1], self.level.layers[-1]]:
            init.xavier_uniform_(layer.weight, gain=1.0)
            if layer.bias is not None:
                init.zeros_(layer.bias)
                
        # Set softmax-regression layers
        pt_mean = -self.pt_offset.mean()
        for layer in [self.pt.layers[-1]]:
            if isinstance(layer, nn.Linear):
                init.xavier_uniform_(layer.weight, gain=init.calculate_gain('tanh'))
                
        init.constant_(self.yearStats.layers[-1].bias, 0)
        init.constant_(self.pt.layers[-1].bias, pt_mean * 0.75)
        
        # Set PA/IP to kaiming_uniform
        for vf in self.value.layers:
            if isinstance(vf, nn.Linear):
                init.kaiming_uniform_(vf.weight, mode='fan_in', nonlinearity='relu')
    
        # Create parameter groups for differentiating learning rates
        self.optimizer = torch.optim.AdamW([{'params': self.recurrent.parameters(), 'lr': learning_rates[0], 'weight_decay': weight_decay[0]},
                                           {'params': self.war.parameters(), 'lr': learning_rates[1], 'weight_decay': weight_decay[1]},
                                           {'params': self.level.parameters(), 'lr': learning_rates[2], 'weight_decay': weight_decay[2]},
                                           {'params': self.pa.parameters(), 'lr': learning_rates[3], 'weight_decay': weight_decay[3]},
                                           {'params': self.yearStats.parameters(), 'lr': learning_rates[4], 'weight_decay': weight_decay[4]},
                                           {'params': self.pos.parameters(), 'lr': learning_rates[5], 'weight_decay': weight_decay[5]},
                                           {'params': self.value.parameters(), 'lr': learning_rates[6], 'weight_decay': weight_decay[6]},
                                           {'params': self.pt.parameters(), 'lr': learning_rates[7], 'weight_decay': weight_decay[7]},
                                           {'params': self.mlbstat.parameters(), 'lr': learning_rates[8], 'weight_decay': weight_decay[8]}])

        
    def to(self, *args, **kwargs):
        return super(RNN_Model, self).to(*args, **kwargs)
    
    def GetInitStateSize(self) -> int:
        return self.init_state_size
    
    def forward(self, x, lengths, pt_levelYearGames, i0, player_demo, player_bios):
        if self.training and self.input_noise > 0:
            noise = torch.rand_like(x, requires_grad=False) * self.input_noise
            x += noise
        
        # Get entries for valid length
        lengths = lengths.to(torch.device("cpu")).long()
        
        # Need to get the correct initial state based on player type
        hs_mask = player_demo == 2
        num_hs = hs_mask.sum()
        if num_hs > 0:
            i0[hs_mask, :] = (
                self.init_hidden_hs
                .expand(num_hs, -1)
            )
            
        intl_mask = player_demo == 3
        num_intl = intl_mask.sum()
        if num_intl > 0:
            i0[intl_mask, :] = (
                self.init_hidden_intl
                .expand(num_intl, -1)
            )
        
        i0 = torch.cat((i0, player_bios), dim=-1)
        
        # Transform initial state to hidden state for RNN
        h0 = self.init_hidden(i0)
        h0 = h0.reshape(h0.shape[0], self.num_layers, self.hidden_size)
        h0 = h0.transpose(0, 1)
        
        # Compute
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput, h0)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
            
        output_war          = self.war(output)
        output_level        = self.level(output)
        output_pa           = self.pa(output)
        output_yearStats    = self.yearStats(output)
        output_yearPositions= self.pos(output)
        output_mlbValue     = self.value(output)
        output_mlbStat      = self.mlbstat(output)
        
        # Apply softplus to pa prediction to limit to positive values
        if (self.is_hitter):
            output_mlbValue = torch.cat([
                output_mlbValue[:,:,:-3],
                self.softplus(output_mlbValue[:,:,-3]).unsqueeze(-1) + self.pa_offset1,
                self.softplus(output_mlbValue[:,:,-2]).unsqueeze(-1) + self.pa_offset2,
                self.softplus(output_mlbValue[:,:,-1]).unsqueeze(-1) + self.pa_offset3,
                ],dim=-1
            )
        else:
            output_mlbValue = torch.cat([
                output_mlbValue[:,:,:-6],
                self.softplus(output_mlbValue[:,:,-6:]) + self.ip_offsets
            ], dim=-1)
        
        # Generate PT Predictions
        pt_levelYearGames = pt_levelYearGames[:, :output.size(1), :]
        output_pt = torch.cat((output, pt_levelYearGames), dim=-1)
        for layer in self.pt.layers[:-1]:
            output_pt = F.tanh(layer(output_pt))
        output_pt = self.softplus(self.pt.layers[-1](output_pt)) + self.pt_offset
        
        return output_war, output_level, output_pa, output_yearStats, output_yearPositions, output_mlbValue, output_pt, output_mlbStat
    
    @classmethod
    def LoadFromFile(cls, args_file : str, data_prep : Data_Prep):
        with open(args_file) as file:
            args_dict = json.load(file)
            return cls(
                input_size=args_dict["input_size"],
                data_prep=data_prep,
                is_hitter=args_dict["is_hitter"],
                
                # RNN parameters
                rnn_droupout=args_dict["rnn_droupout"],
                num_layers=args_dict["num_layers"],
                hidden_size=args_dict["hidden_size"],
                rnn_nonlinearity=args_dict["rnn_nonlinearity"],
                input_noise=args_dict["input_noise"],
                
                rnn_droupout_p=args_dict["rnn_droupout"],
                num_layers_p=args_dict["num_layers"],
                hidden_size_p=args_dict["hidden_size"],
                rnn_nonlinearity_p=args_dict["rnn_nonlinearity"],
                input_noise_p=args_dict["input_noise"],
                
                # Initial Hidden State Calculation
                init_state_size=args_dict["init_state_size"],
                init_state_size_p=args_dict["init_state_size"],
                init_state_arch=LayerArch.LoadFromDict(args_dict["init_state_arch"]),
                init_state_arch_p=LayerArch.LoadFromDict(args_dict["init_state_arch"]),
                
                # LayerArch (reconstructed)
                stats_arch=LayerArch.LoadFromDict(args_dict["stats_arch"]),
                war_arch=LayerArch.LoadFromDict(args_dict["war_arch"]),
                pt_arch=LayerArch.LoadFromDict(args_dict["pt_arch"]),
                pos_arch=LayerArch.LoadFromDict(args_dict["pos_arch"]),
                lvl_arch=LayerArch.LoadFromDict(args_dict["lvl_arch"]),
                pa_arch=LayerArch.LoadFromDict(args_dict["pa_arch"]),
                val_arch=LayerArch.LoadFromDict(args_dict["val_arch"]),
                mlbstat_arch=LayerArch.LoadFromDict(args_dict["mlbstat_arch"]),
                
                stats_arch_p=LayerArch.LoadFromDict(args_dict["stats_arch"]),
                war_arch_p=LayerArch.LoadFromDict(args_dict["war_arch"]),
                pt_arch_p=LayerArch.LoadFromDict(args_dict["pt_arch"]),
                pos_arch_p=LayerArch.LoadFromDict(args_dict["pos_arch"]),
                lvl_arch_p=LayerArch.LoadFromDict(args_dict["lvl_arch"]),
                pa_arch_p=LayerArch.LoadFromDict(args_dict["pa_arch"]),
                val_arch_p=LayerArch.LoadFromDict(args_dict["val_arch"]),
                mlbstat_arch_p=LayerArch.LoadFromDict(args_dict["mlbstat_arch"]),
                
                # Training hyperparameters
                weight_decay=args_dict["weight_decay"],
                weight_decay_p=args_dict["weight_decay"],
                learning_rates=args_dict["learning_rates"],
                learning_rates_p=args_dict["learning_rates"],
        )
    
    
def Stats_Loss(pred_stats, actual_stats, masks):
    actual_stats = actual_stats[:, :pred_stats.size(1)]
    masks = masks[:,:pred_stats.size(1)]
    
    batch_size = actual_stats.size(0)
    time_steps = actual_stats.size(1)
    output_size = actual_stats.size(3)
    mask_size = masks.size(2)
    
    pred_stats = pred_stats.reshape((batch_size * time_steps, mask_size, output_size))
    actual_stats = actual_stats.reshape((batch_size * time_steps, mask_size, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    #loss = nn.HuberLoss(reduction='none', delta=1)
    #loss = nn.L1Loss(reduction='none')
    loss = nn.MSELoss(reduction='none')
    l = loss(pred_stats, actual_stats) * masks.unsqueeze(-1)
    return (l * masks.unsqueeze(-1)).sum()
      
def Pt_Loss(pred_pt, actual_pt):
    actual_pt = actual_pt[:, :pred_pt.size(1)]
    
    batch_size = actual_pt.size(0)
    time_steps = actual_pt.size(1)
    num_levels = actual_pt.size(2)
    output_size = actual_pt.size(3)
    
    pred_pt = pred_pt.reshape((batch_size * time_steps, num_levels, output_size))
    actual_pt = actual_pt.reshape((batch_size * time_steps, num_levels, output_size))
    
    #loss = nn.MSELoss(reduction='none')
    loss = nn.HuberLoss(reduction='none', delta=0.5)
    #loss = nn.L1Loss(reduction='none')
    return loss(pred_pt, actual_pt).sum()
      
def Mlb_Value_Loss_Hitter(pred_value, actual_value, masks):
    actual_value = actual_value[:, :pred_value.size(1)]
    masks = masks[:,:pred_value.size(1)]
    
    batch_size = actual_value.size(0)
    time_steps = actual_value.size(1)
    mask_size_years = masks.size(2)
    mask_size_types = masks.size(3)
    output_size = actual_value.size(2) // mask_size_years # Group into years
    
    pred_value = pred_value.reshape((batch_size * time_steps, mask_size_years, output_size))
    actual_value = actual_value.reshape((batch_size * time_steps, mask_size_years, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size_years, mask_size_types))
    
    loss = nn.HuberLoss(reduction='none')
    l = 0
    for x in range(3):
        # Rate stats
        l += (loss(pred_value[:,x,:-3], actual_value[:,x,:-3]).sum(dim=1) * masks[:,x,1]).sum()
        # Value stats
        l += (loss(pred_value[:,x,-3:], actual_value[:,x,-3:]).sum(dim=1) * masks[:,x,0]).sum()
    
    return l
    
def Mlb_Value_Loss_Pitcher(pred_value, actual_value, masks):
    actual_value = actual_value[:, :pred_value.size(1)]
    masks = masks[:,:pred_value.size(1)]
    
    batch_size = actual_value.size(0)
    time_steps = actual_value.size(1)
    mask_size_years = masks.size(2)
    mask_size_types = masks.size(3)
    
    pred_war = pred_value[:,:,:-6].reshape((batch_size * time_steps, mask_size_years, 2))
    pred_ip = pred_value[:,:,-6:].reshape((batch_size * time_steps, mask_size_years, 2))
    actual_war = actual_value[:,:,:-6].reshape((batch_size * time_steps, mask_size_years, 2))
    actual_ip = actual_value[:,:,-6:].reshape((batch_size * time_steps, mask_size_years, 2))
    
    masks = masks.reshape((batch_size * time_steps, mask_size_years, mask_size_types))
    pa_masks = masks[:,:,0].reshape((batch_size * time_steps, mask_size_years))
    war_masks = masks[:,:,1:].reshape((batch_size * time_steps), mask_size_years, 2)
    
    #loss = nn.L1Loss(reduction='none')
    loss = nn.HuberLoss(reduction='none')
    
    # War
    l = (loss(pred_war, actual_war) * war_masks).sum()
    l += (loss(pred_ip, actual_ip).sum(dim=2) * pa_masks).sum()

    return l
        
def Position_Classification_Loss(pred_positions, actual_positions, masks):
    actual_positions  = actual_positions[:, :pred_positions.size(1)]
    masks = masks[:, :pred_positions.size(1)]
    
    batch_size = actual_positions.size(0)
    time_steps = actual_positions.size(1)
    output_size = actual_positions.size(3)
    mask_size = masks.size(2)
    
    pred_positions = pred_positions.reshape((batch_size * time_steps, mask_size, output_size))
    actual_positions = actual_positions.reshape((batch_size * time_steps, mask_size, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    loss = nn.CrossEntropyLoss(reduction='none')
    l = 0
    for x in range(8):
        l += (loss(pred_positions[:,x,:], actual_positions[:,x,:]) * masks[:,x]).sum()
    return l
    
def MLB_Stat_Classification_Loss(pred_stats, actual_stats, masks, is_hitter : bool):
    batch_size = pred_stats.size(0)
    time_steps = pred_stats.size(1)
    
    masks = masks[:,:time_steps]
    actual_stats = actual_stats[:, :time_steps]
    
    num_stats = NUM_HITTER_STATS if is_hitter else NUM_PITCHER_STATS
    num_buckets = NUM_HITTER_BUCKETS_PER_STAT if is_hitter else NUM_PITCHER_BUCKETS_PER_STAT
    
    pred_stats = pred_stats.reshape((batch_size * time_steps, num_stats, num_buckets))
    masks = masks.reshape((batch_size * time_steps,))
    actual_stats = actual_stats.reshape((batch_size * time_steps, num_stats))
    
    l = nn.CrossEntropyLoss(reduction='none', label_smoothing=0.25)
    loss = 0
    for i in range(num_stats):
        loss += (l(pred_stats[:, i], actual_stats[:,i]) * masks).sum()
        
    return loss
    
def Classification_Loss(pred : torch.Tensor, actual : torch.Tensor, masks : torch.Tensor):
    # Clip masks to match prediction time dimension
    time_steps = pred.size(1)
    masks = masks[:, :time_steps]
    
    batch_size = pred.size(0)
    num_classes = pred.size(2)
    
    pred_flat = pred.reshape(batch_size * time_steps, num_classes)
    actual_flat = actual.repeat_interleave(time_steps)
    
    criterion = nn.CrossEntropyLoss(reduction='none')
    loss_flat = criterion(pred_flat, actual_flat)
    
    loss = loss_flat.reshape(batch_size, time_steps)
    
    masked_loss = loss * masks
    total_loss = masked_loss.sum()
    
    return total_loss