import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from Data_Prep import Data_Prep

from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, NUM_LEVELS

def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters

class RNN_Model(nn.Module):
    def __init__(self, input_size : int, num_layers : int, hidden_size : int, mutators : torch.Tensor, data_prep : Data_Prep, is_hitter : bool):
        super().__init__()
        
        output_map = data_prep.output_map
        
        self.pre1 = nn.Linear(input_size, input_size)
        self.pre2 = nn.Linear(input_size, input_size)
        self.pre3 = nn.Linear(input_size, input_size)
        
        self.rnn = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False)
        self.linear_war1 = nn.Linear(hidden_size, hidden_size)
        self.linear_war2 = nn.Linear(hidden_size, hidden_size)
        self.linear_war3 = nn.Linear(hidden_size, len(output_map.buckets_hitter_war))
        self.linear_level1 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_level2 = nn.Linear(hidden_size // 2, len(HITTER_LEVEL_BUCKETS))
        self.linear_pa1 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_pa2 = nn.Linear(hidden_size // 2, len(HITTER_PA_BUCKETS))
        
        # Predict specific war value
        self.linear_regr_war1 = nn.Linear(hidden_size, hidden_size)
        self.linear_regr_war2 = nn.Linear(hidden_size, hidden_size)
        self.linear_regr_war3 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_regr_war4 = nn.Linear(hidden_size // 2, 1)
        
        # Predict next year stats
        self.linear_yearStats1 = nn.Linear(hidden_size, hidden_size * 2)
        self.linear_yearStats2 = nn.Linear(hidden_size * 2, hidden_size * 3)
        self.linear_yearStats3 = nn.Linear(hidden_size * 3, hidden_size * 4)
        self.linear_yearStats4 = nn.Linear(hidden_size * 4, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size))
        self.register_buffer('stat_offsets', data_prep.Get_HitStat_Offset() if is_hitter else data_prep.Get_PitStat_Offset())
        
        self.linear_yearPositions1 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearPositions2 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearPositions3 = nn.Linear(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_positions_size if is_hitter else output_map.pitcher_positions_size))
        
        # Predict playing time
        self.linear_pt1 = nn.Linear(hidden_size, hidden_size)
        self.linear_pt2 = nn.Linear(hidden_size, hidden_size)
        self.linear_pt3 = nn.Linear(hidden_size, NUM_LEVELS * output_map.hitter_pt_size if is_hitter else NUM_LEVELS * output_map.pitcher_pt_size)
        self.register_buffer('pt_offset', data_prep.Get_HitPt_Offset() if is_hitter else data_prep.Get_PitPt_Offset())
        
        # Predict MLB Value
        self.linear_mlb_value1 = nn.Linear(hidden_size, hidden_size)
        self.linear_mlb_value2 = nn.Linear(hidden_size, hidden_size)
        self.linear_mlb_value3 = nn.Linear(hidden_size, hidden_size)
        self.linear_mlb_value4 = nn.Linear(hidden_size, (output_map.mlb_hitter_values_size if is_hitter else output_map.mlb_pitcher_values_size))
        
        self.mutators = mutators
        self.nonlin = F.relu
        self.softplus = nn.Softplus()
        self.pa_offset1, self.pa_offset2, self.pa_offset3 = data_prep.Get_Pa_Offsets()
        self.register_buffer('ip_offsets', data_prep.Get_Ip_Offsets())
        self.is_hitter = is_hitter
        #self.nonlin = F.leaky_relu
        
        # Initialize weights
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='relu')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
        
        # Set softmax-classification layers to Xavier for uniform initial predictions
        for layer in [self.linear_war3, self.linear_pa2, self.linear_level2]:
            init.xavier_uniform_(layer.weight, gain=1.0)
            if layer.bias is not None:
                init.zeros_(layer.bias)
                
        # Set softmax-regression layers to Xavier for uniform across softmax dim
        for layer in [self.linear_yearStats4, self.linear_yearPositions3]:
            init.xavier_uniform_(layer.weight, gain=init.calculate_gain('sigmoid'))
            if layer.bias is not None:
                init.zeros_(layer.bias)
        
        # Set PA/IP to kaiming_uniform
        init.kaiming_uniform_(self.linear_mlb_value1.weight, mode='fan_in', nonlinearity='relu')
        init.kaiming_uniform_(self.linear_mlb_value2.weight, mode='fan_in', nonlinearity='relu')
        init.kaiming_uniform_(self.linear_mlb_value3.weight, mode='fan_in', nonlinearity='relu')
        init.kaiming_uniform_(self.linear_mlb_value4.weight, mode='fan_in', nonlinearity='relu')
    
        # Create parameter groups for differentiating learning rates
        shared_params = GetParameters([self.pre1, self.pre2, self.pre3, self.rnn])
        war_class_params = GetParameters([self.linear_war1, self.linear_war2, self.linear_war3])
        war_regression_params = GetParameters([self.linear_regr_war1, self.linear_regr_war2, self.linear_regr_war3, self.linear_regr_war4])
        level_params = GetParameters([self.linear_level1, self.linear_level2])
        pa_params = GetParameters([self.linear_pa1, self.linear_pa2])
        yearStat_params = GetParameters([self.linear_yearStats1, self.linear_yearStats2, self.linear_yearStats3, self.linear_yearStats4])
        yearPos_params = GetParameters([self.linear_yearPositions1, self.linear_yearPositions2, self.linear_yearPositions3])
        mlbValue_params = GetParameters([self.linear_mlb_value1, self.linear_mlb_value2, self.linear_mlb_value3, self.linear_mlb_value4])
        yearPt_params = GetParameters([self.linear_pt1, self.linear_pt2, self.linear_pt3])
        
        self.optimizer = torch.optim.Adam([{'params': shared_params, 'lr': 0.0025},
                                           {'params': war_class_params, 'lr': 0.01},
                                           {'params': level_params, 'lr': 0.01},
                                           {'params': pa_params, 'lr': 0.01},
                                           {'params': yearStat_params, 'lr': 0.01},
                                           {'params': yearPos_params, 'lr': 0.01},
                                           {'params': mlbValue_params, 'lr': 0.01},
                                           {'params': war_regression_params, 'lr': 0.01},
                                           {'params': yearPt_params, 'lr': 0.01}])
                                           
                                           
                                           
        
    def to(self, *args, **kwargs):
        if self.mutators is not None:
            self.mutators = self.mutators.to(*args, **kwargs)
        return super(RNN_Model, self).to(*args, **kwargs)
    
    def forward(self, x, lengths):
        if self.training and self.mutators is not None:
            x += self.mutators[:x.size(0), :x.size(1), :]
        
        # Apply transformation to data before entering network
        x = self.nonlin(self.pre1(x))
        x = self.nonlin(self.pre2(x))
        x = self.nonlin(self.pre3(x))
        
        lengths = lengths.to(torch.device("cpu")).long()
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        packedOutput, _ = self.rnn(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
            
        # Generate War predictions
        output_war = self.nonlin(self.linear_war1(output))
        output_war = self.nonlin(self.linear_war2(output_war))
        output_war = self.linear_war3(output_war)
        
        output_regression_war = self.nonlin(self.linear_regr_war1(output))
        output_regression_war = self.nonlin(self.linear_regr_war2(output_regression_war))
        output_regression_war = self.nonlin(self.linear_regr_war3(output_regression_war))
        output_regression_war = self.linear_regr_war4(output_regression_war)
        
        # Generate Level Predictions
        output_level = self.nonlin(self.linear_level1(output))
        output_level = self.linear_level2(output_level)
        
        # Generate PA Predictions
        output_pa = self.nonlin(self.linear_pa1(output))
        output_pa = self.linear_pa2(output_pa)
        
        # Generate Year Stats Predictions
        output_yearStats = self.nonlin(self.linear_yearStats1(output))
        output_yearStats = self.nonlin(self.linear_yearStats2(output_yearStats))
        output_yearStats = self.nonlin(self.linear_yearStats3(output_yearStats))
        output_yearStats = self.linear_yearStats4(output_yearStats)
        #output_yearStats = self.softplus(self.linear_yearStats4(output_yearStats)) + self.stat_offsets
        
        output_yearPositions = self.nonlin(self.linear_yearPositions1(output))
        output_yearPositions = self.nonlin(self.linear_yearPositions2(output_yearPositions))
        output_yearPositions = self.linear_yearPositions3(output_yearPositions)
        
        # Generate PT Predictions
        output_pt = self.nonlin(self.linear_pt1(output))
        output_pt = self.nonlin(self.linear_pt2(output_pt))
        output_pt = self.softplus(self.linear_pt3(output_pt)) + self.pt_offset
        
        # Generate MLB Value Predictions
        output_mlbValue = self.nonlin(self.linear_mlb_value1(output))
        output_mlbValue = self.nonlin(self.linear_mlb_value2(output_mlbValue))
        output_mlbValue = self.nonlin(self.linear_mlb_value3(output_mlbValue))
        output_mlbValue = self.linear_mlb_value4(output_mlbValue)
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
        
        return output_war, output_regression_war, output_level, output_pa, output_yearStats, output_yearPositions, output_mlbValue, output_pt
    
    
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
    
    loss = nn.HuberLoss(reduction='none')
    #loss = nn.L1Loss(reduction='none')
    #loss = nn.MSELoss(reduction='none')
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
    
    loss = nn.HuberLoss(reduction='none')
    l = loss(pred_pt, actual_pt).sum()
    # for x in range(NUM_LEVELS):
    #     l += (loss(pred_pt[:,x,:], actual_pt[:,x,:]).sum(dim=1)).sum()
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
    
    #loss = nn.L1Loss(reduction='none')
    loss = nn.MSELoss(reduction='none')
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
    loss = nn.MSELoss(reduction='none')
    
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