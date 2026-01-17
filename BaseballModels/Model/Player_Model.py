import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from Data_Prep import Data_Prep
from itertools import chain

from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, NUM_LEVELS

def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters

class LayerArch:
    def __init__(self, layer_size : int, num_layers : int):
        self.layer_size = layer_size
        self.num_layers = num_layers

DEFAULT_WARCLASS_ARCH = LayerArch(layer_size=150, num_layers=2)
DEFAULT_STATS_ARCH = LayerArch(layer_size=70, num_layers=2)
DEFAULT_PT_ARCH = LayerArch(layer_size=130, num_layers=3)
DEFAULT_POS_ARCH = LayerArch(layer_size=30, num_layers=2)
DEFAULT_LVL_ARCH = LayerArch(layer_size=70, num_layers=2)
DEFAULT_PA_ARCH = LayerArch(layer_size=100, num_layers=2)
DEFAULT_VALUE_ARCH = LayerArch(layer_size=40, num_layers=2)

DEFAULT_WARCLASS_ARCH_P = LayerArch(layer_size=150, num_layers=3)
DEFAULT_STATS_ARCH_P = LayerArch(layer_size=90, num_layers=2)
DEFAULT_PT_ARCH_P = LayerArch(layer_size=110, num_layers=2)
DEFAULT_POS_ARCH_P = LayerArch(layer_size=55, num_layers=2)
DEFAULT_LVL_ARCH_P = LayerArch(layer_size=150, num_layers=2)
DEFAULT_PA_ARCH_P = LayerArch(layer_size=40, num_layers=2)
DEFAULT_VALUE_ARCH_P = LayerArch(layer_size=120, num_layers=2)

class RNN_Model(nn.Module):
    def __init__(self, input_size : int, 
                 num_layers : int, 
                 hidden_size : int, 
                 mutators : torch.Tensor, 
                 data_prep : Data_Prep, 
                 is_hitter : bool, 
                 stats_arch : LayerArch = DEFAULT_STATS_ARCH,
                 warclass_arch : LayerArch = DEFAULT_WARCLASS_ARCH,
                 pt_arch : LayerArch = DEFAULT_PT_ARCH,
                 pos_arch : LayerArch = DEFAULT_POS_ARCH,
                 lvl_arch : LayerArch = DEFAULT_LVL_ARCH,
                 pa_arch : LayerArch = DEFAULT_PA_ARCH,
                 val_arch : LayerArch = DEFAULT_VALUE_ARCH,
                 stats_arch_p : LayerArch = DEFAULT_STATS_ARCH_P,
                 warclass_arch_p : LayerArch = DEFAULT_WARCLASS_ARCH_P,
                 pt_arch_p : LayerArch = DEFAULT_PT_ARCH_P,
                 pos_arch_p : LayerArch = DEFAULT_POS_ARCH_P,
                 lvl_arch_p : LayerArch = DEFAULT_LVL_ARCH_P,
                 pa_arch_p : LayerArch = DEFAULT_PA_ARCH_P,
                 val_arch_p : LayerArch = DEFAULT_VALUE_ARCH_P):
        super().__init__()
        
        if not is_hitter:
            stats_arch = stats_arch_p
            warclass_arch_p = warclass_arch_p
            pt_arch_p = pt_arch_p
            pos_arch_p = pos_arch_p
            lvl_arch_p = lvl_arch_p
            pa_arch_p = pa_arch_p
            val_arch_p = val_arch_p
        
        output_map = data_prep.output_map
        
        self.pre1 = nn.Linear(input_size, input_size)
        self.pre2 = nn.Linear(input_size, input_size)
        self.pre3 = nn.Linear(input_size, input_size)
        
        self.recurrent = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False)
        #self.recurrent = nn.LSTM(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False)
        self.linear_warFirst = nn.Linear(hidden_size, warclass_arch.layer_size)
        self.linear_warArray = nn.ModuleList(nn.Linear(warclass_arch.layer_size, warclass_arch.layer_size) for _ in range(warclass_arch.num_layers - 2))
        self.linear_warLast = nn.Linear(warclass_arch.layer_size, len(output_map.buckets_hitter_war))
        
        self.linear_lvlFirst = nn.Linear(hidden_size, lvl_arch.layer_size)
        self.linear_lvlArray = nn.ModuleList(nn.Linear(lvl_arch.layer_size, lvl_arch.layer_size) for _ in range(lvl_arch.num_layers - 2))
        self.linear_lvlLast = nn.Linear(lvl_arch.layer_size, len(HITTER_LEVEL_BUCKETS))
        
        self.linear_paFirst = nn.Linear(hidden_size, pa_arch.layer_size)
        self.linear_paArray = nn.ModuleList(nn.Linear(pa_arch.layer_size, pa_arch.layer_size) for _ in range(pa_arch.num_layers - 2))
        self.linear_paLast = nn.Linear(pa_arch.layer_size, len(HITTER_PA_BUCKETS))
        
        # Predict next year stats
        stats_size = output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size
        
        self.linear_yearStatsFirst = nn.Linear(hidden_size, stats_arch.layer_size)
        self.linear_yearStatsArray = nn.ModuleList(nn.Linear(stats_arch.layer_size, stats_arch.layer_size) for _ in range(stats_arch.num_layers - 2))
        self.linear_yearStatsLast = nn.Linear(stats_arch.layer_size, NUM_LEVELS * stats_size)
        self.register_buffer('stat_offsets', data_prep.Get_HitStat_Offset() if is_hitter else data_prep.Get_PitStat_Offset())
        self.yearStats_output_transform = nn.Softplus(threshold=0.25)
        
        self.linear_posFirst = nn.Linear(hidden_size, pos_arch.layer_size)
        self.linear_posArray = nn.ModuleList(nn.Linear(pos_arch.layer_size, pos_arch.layer_size) for _ in range(pos_arch.num_layers - 2))
        self.linear_posLast = nn.Linear(pos_arch.layer_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_positions_size if is_hitter else output_map.pitcher_positions_size))
        
        # Predict playing time
        self.linear_ptFirst = nn.Linear(hidden_size + len(HITTER_LEVEL_BUCKETS), pt_arch.layer_size)
        self.linear_ptArray = nn.ModuleList(nn.Linear(pt_arch.layer_size, pt_arch.layer_size) for _ in range(pt_arch.num_layers - 2))
        self.linear_ptLast = nn.Linear(pt_arch.layer_size, NUM_LEVELS * output_map.hitter_pt_size if is_hitter else NUM_LEVELS * output_map.pitcher_pt_size)
        self.register_buffer('pt_offset', data_prep.Get_HitPt_Offset() if is_hitter else data_prep.Get_PitPt_Offset())
        
        # Predict MLB Value
        self.linear_valueFirst = nn.Linear(hidden_size, val_arch.layer_size)
        self.linear_valueArray = nn.ModuleList(nn.Linear(val_arch.layer_size, val_arch.layer_size) for _ in range(val_arch.num_layers - 2))
        self.linear_valueLast = nn.Linear(val_arch.layer_size, (output_map.mlb_hitter_values_size if is_hitter else output_map.mlb_pitcher_values_size))
        
        self.mutators = mutators
        self.nonlin = F.relu
        self.nonlin = F.tanh
        self.nonlin = F.leaky_relu
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
        for layer in [self.linear_warLast, self.linear_paLast, self.linear_lvlLast]:
            init.xavier_uniform_(layer.weight, gain=1.0)
            if layer.bias is not None:
                init.zeros_(layer.bias)
                
        # Set softmax-regression layers
        stat_mean = -self.stat_offsets.mean()
        pt_mean = -self.pt_offset.mean()
        for layer in [self.linear_ptLast]:
            init.xavier_uniform_(layer.weight, gain=init.calculate_gain('tanh'))
                
        init.constant_(self.linear_yearStatsLast.bias, 0)
        init.constant_(self.linear_ptLast.bias, pt_mean * 0.75)
        
        # Set PA/IP to kaiming_uniform
        init.kaiming_uniform_(self.linear_valueFirst.weight, mode='fan_in', nonlinearity='relu')
        for vf in self.linear_valueArray:
            init.kaiming_uniform_(vf.weight, mode='fan_in', nonlinearity='relu')
        init.kaiming_uniform_(self.linear_valueLast.weight, mode='fan_in', nonlinearity='relu')
    
        # Create parameter groups for differentiating learning rates
        shared_params = GetParameters([self.pre1, self.pre2, self.pre3, self.recurrent])
        war_class_params = GetParameters(chain([self.linear_warFirst, self.linear_warLast], self.linear_warArray))
        level_params = GetParameters(chain([self.linear_lvlFirst, self.linear_lvlLast], self.linear_lvlArray))
        pa_params = GetParameters(chain([self.linear_paFirst, self.linear_paLast], self.linear_paArray))
        yearStat_params = GetParameters(chain([self.linear_yearStatsFirst, self.linear_yearStatsLast], self.linear_yearStatsArray))
        yearPos_params = GetParameters(chain([self.linear_posFirst, self.linear_posLast], self.linear_posArray))
        mlbValue_params = GetParameters(chain([self.linear_valueFirst, self.linear_valueLast], self.linear_valueArray))
        yearPt_params = GetParameters(chain([self.linear_ptFirst, self.linear_ptLast], self.linear_ptArray))
        
        self.optimizer = torch.optim.Adam([{'params': shared_params, 'lr': 0.00125},
                                           {'params': war_class_params, 'lr': 0.00125},
                                           {'params': level_params, 'lr': 0.013},
                                           {'params': pa_params, 'lr': 0.003},
                                           {'params': yearStat_params, 'lr': 0.015},
                                           {'params': yearPos_params, 'lr': 0.01},
                                           {'params': mlbValue_params, 'lr': 0.005},
                                           {'params': yearPt_params, 'lr': 0.012}]) \
                                        \
                        if is_hitter else \
                        torch.optim.Adam([{'params': shared_params, 'lr': 0.00125},
                                           {'params': war_class_params, 'lr': 0.01},
                                           {'params': level_params, 'lr': 0.01},
                                           {'params': pa_params, 'lr': 0.02},
                                           {'params': yearStat_params, 'lr': 0.01},
                                           {'params': yearPos_params, 'lr': 0.025},
                                           {'params': mlbValue_params, 'lr': 0.01},
                                           {'params': yearPt_params, 'lr': 0.018}])
        
    def to(self, *args, **kwargs):
        if self.mutators is not None:
            self.mutators = self.mutators.to(*args, **kwargs)
        return super(RNN_Model, self).to(*args, **kwargs)
    
    def forward(self, x, lengths, pt_levelYearGames):
        # if self.training and self.mutators is not None:
        #     x += self.mutators[:x.size(0), :x.size(1), :]
        
        # Apply transformation to data before entering network
        # x = self.nonlin(self.pre1(x))
        # x = self.nonlin(self.pre2(x))
        # x = self.nonlin(self.pre3(x))
        
        lengths = lengths.to(torch.device("cpu")).long()
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
            
        # Generate War predictions
        output_war = self.nonlin(self.linear_warFirst(output))
        for ys in self.linear_warArray:
            output_war = self.nonlin(ys(output_war))
        output_war = self.linear_warLast(output_war)
        
        # Generate Level Predictions
        output_level = self.nonlin(self.linear_lvlFirst(output))
        for ys in self.linear_lvlArray:
            output_level = self.nonlin(ys(output_level))
        output_level = self.linear_lvlLast(output_level)
        
        # Generate PA Predictions
        output_pa = self.nonlin(self.linear_paFirst(output))
        for ys in self.linear_paArray:
            output_pa = self.nonlin(ys(output_pa))
        output_pa = self.linear_paLast(output_pa)
        
        # Generate Year Stats Predictions
        output_yearStats = self.nonlin(self.linear_yearStatsFirst(output))
        for ys in self.linear_yearStatsArray:
            output_yearStats = self.nonlin(ys(output_yearStats))
        output_yearStats = self.linear_yearStatsLast(output_yearStats)
        #output_yearStats = self.yearStats_output_transform(self.linear_yearStats4(output_yearStats)) + self.stat_offsets
        
        
        output_yearPositions = self.nonlin(self.linear_posFirst(output))
        for ys in self.linear_posArray:
            output_yearPositions = self.nonlin(ys(output_yearPositions))
        output_yearPositions = self.linear_posLast(output_yearPositions)
        
        # Generate PT Predictions
        pt_levelYearGames = pt_levelYearGames[:, :output.size(1), :]
        # print(output.shape)
        # print(pt_levelYearGames.shape)
        output_pt = torch.cat((output, pt_levelYearGames), dim=-1)
        output_pt = F.tanh(self.linear_ptFirst(output_pt))
        for ys in self.linear_ptArray:
            output_pt = F.tanh(ys(output_pt))
        output_pt = self.softplus(self.linear_ptLast(output_pt)) + self.pt_offset
        
        # Generate MLB Value Predictions
        output_mlbValue = self.nonlin(self.linear_valueFirst(output))
        for ys in self.linear_valueArray:
            output_mlbValue = self.nonlin(ys(output_mlbValue))
        output_mlbValue = self.linear_valueLast(output_mlbValue)
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
        
        return output_war, output_level, output_pa, output_yearStats, output_yearPositions, output_mlbValue, output_pt
    
    
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
    
def Prospect_WarRegression_Loss(pred_war, actual_war, masks):
    actual_war = actual_war[:,:pred_war.size(1)]
    masks = masks[:,:pred_war.size(1)]
    
    batch_size = actual_war.size(0)
    time_steps = actual_war.size(1)
    
    actual_war = actual_war.reshape((batch_size * time_steps,))
    masks = masks.reshape((batch_size, time_steps,))
    pred_war = pred_war.reshape((batch_size * time_steps,))
    
    l = nn.HuberLoss(reduction='none')
    loss = l(pred_war, actual_war)
    loss = loss.reshape((batch_size, time_steps))
    loss *= masks
    
    return loss.sum(dim=1).sum()
    
def Classification_Loss(pred_war, pred_level, pred_pa, actual_war, actual_level, actual_pa, masks):
    actual_war = actual_war[:,:pred_war.size(1)]
    actual_level = actual_level[:,:pred_level.size(1)]
    actual_pa = actual_pa[:,:pred_pa.size(1)]
    masks = masks[:,:pred_level.size(1)]
    
    batch_size = actual_war.size(0)
    time_steps = actual_war.size(1)
    
    num_classes_war = pred_war.size(2)
    num_classes_level = pred_level.size(2)
    num_classes_pa = pred_pa.size(2)
    
    actual_war = actual_war.reshape((batch_size * time_steps,))
    actual_level = actual_level.reshape((batch_size * time_steps,))
    actual_pa = actual_pa.reshape((batch_size * time_steps,))
    
    masks = masks.reshape((batch_size, time_steps,))
    
    pred_war = pred_war.reshape((batch_size * time_steps, num_classes_war))
    pred_level = pred_level.reshape((batch_size * time_steps, num_classes_level))
    pred_pa = pred_pa.reshape((batch_size * time_steps, num_classes_pa))
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss_war = l(pred_war, actual_war)
    loss_level = l(pred_level, actual_level)
    loss_pa = l(pred_pa, actual_pa)
    
    loss_war = loss_war.reshape((batch_size, time_steps))
    loss_level = loss_level.reshape((batch_size, time_steps))
    loss_pa = loss_pa.reshape((batch_size, time_steps))
    
    masked_loss_war = loss_war * masks
    masked_loss_level = loss_level * masks
    masked_loss_pa = loss_pa * masks
    
    loss_sums_war = masked_loss_war.sum(dim=1)
    loss_sums_level = masked_loss_level.sum(dim=1)
    loss_sums_pa = masked_loss_pa.sum(dim=1)
    
    return loss_sums_war.sum(), loss_sums_level.sum(), loss_sums_pa.sum()