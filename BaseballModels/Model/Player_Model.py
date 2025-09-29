import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
from Output_Map import Output_Map

from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, HITTER_PEAK_WAR_BUCKETS

class RNN_Model(nn.Module):
    def __init__(self, input_size : int, num_layers : int, hidden_size : int, mutators : torch.Tensor, output_map : Output_Map, is_hitter : bool):
        super().__init__()
        
        self.pre1 = nn.Linear(input_size, input_size)
        self.pre2 = nn.Linear(input_size, input_size)
        self.pre3 = nn.Linear(input_size, input_size)
        
        self.rnn = nn.RNN(input_size=input_size, hidden_size=hidden_size, num_layers=num_layers, batch_first=False)
        self.linear_war1 = nn.Linear(hidden_size, hidden_size)
        self.linear_war2 = nn.Linear(hidden_size, hidden_size)
        self.linear_war3 = nn.Linear(hidden_size, len(output_map.buckets_hitter_war))
        self.linear_value1 = nn.Linear(hidden_size, hidden_size)
        self.linear_value2 = nn.Linear(hidden_size, hidden_size)
        self.linear_value3 = nn.Linear(hidden_size, len(output_map.buckets_hitter_value))
        self.linear_pwar1 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_pwar2 = nn.Linear(hidden_size // 2, len(HITTER_PEAK_WAR_BUCKETS))
        self.linear_level1 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_level2 = nn.Linear(hidden_size // 2, len(HITTER_LEVEL_BUCKETS))
        self.linear_pa1 = nn.Linear(hidden_size, hidden_size // 2)
        self.linear_pa2 = nn.Linear(hidden_size // 2, len(HITTER_PA_BUCKETS))
        
        # Predict next month stats
        # self.linear_stats1 = nn.Linear(hidden_size, hidden_size)
        # self.linear_stats2 = nn.Linear(hidden_size, hidden_size)
        # self.linear_stats3 = nn.Linear(hidden_size, hidden_size)
        # self.linear_stats4 = nn.Linear(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size))
        
        # self.linear_positions1 = nn.Linear(hidden_size, hidden_size)
        # self.linear_positions2 = nn.Linear(hidden_size, hidden_size)
        # self.linear_positions3 = nn.Linear(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_positions_size if is_hitter else output_map.pitcher_positions_size))
        
        # Predict next year stats
        self.linear_yearStats1 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearStats2 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearStats3 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearStats4 = nn.Linear(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size))
        
        self.linear_yearPositions1 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearPositions2 = nn.Linear(hidden_size, hidden_size)
        self.linear_yearPositions3 = nn.Linear(hidden_size, len(HITTER_LEVEL_BUCKETS) * (output_map.hitter_positions_size if is_hitter else output_map.pitcher_positions_size))
        
        self.mutators = mutators
        self.nonlin = F.relu
        #self.nonlin = F.leaky_relu
        
        for m in self.modules():
            if isinstance(m, nn.Linear):
                init.kaiming_normal_(m.weight, mode='fan_out', nonlinearity='relu')
                if m.bias is not None:
                    init.constant_(m.bias, 0)
        
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
        
        # Generate Value predictions
        output_value = self.nonlin(self.linear_value1(output))
        output_value = self.nonlin(self.linear_value2(output_value))
        output_value = self.linear_value3(output_value)
        
        # Generate Peak War Predictions
        output_pwar = self.nonlin(self.linear_pwar1(output))
        output_pwar = self.linear_pwar2(output_pwar)
        
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
        
        output_yearPositions = self.nonlin(self.linear_yearPositions1(output))
        output_yearPositions = self.nonlin(self.linear_yearPositions2(output_yearPositions))
        output_yearPositions = self.linear_yearPositions3(output_yearPositions)
        
        return output_war, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPositions
    
def Stats_L1_Loss(pred_stats, actual_stats, masks):
    actual_stats = actual_stats[:, :pred_stats.size(1)]
    masks = masks[:,:pred_stats.size(1)]
    
    batch_size = actual_stats.size(0)
    time_steps = actual_stats.size(1)
    output_size = actual_stats.size(2)
    mask_size = masks.size(2)
    
    pred_stats = pred_stats.reshape((batch_size * time_steps, mask_size, output_size))
    actual_stats = actual_stats.reshape((batch_size * time_steps, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    loss = nn.L1Loss(reduction='none')
    loss_0 = loss(pred_stats[:,0,:], actual_stats).sum(dim=1) * masks[:,0]
    loss_1 = loss(pred_stats[:,1,:], actual_stats).sum(dim=1) * masks[:,1]
    loss_2 = loss(pred_stats[:,2,:], actual_stats).sum(dim=1) * masks[:,2]
    loss_3 = loss(pred_stats[:,3,:], actual_stats).sum(dim=1) * masks[:,3]
    loss_4 = loss(pred_stats[:,4,:], actual_stats).sum(dim=1) * masks[:,4]
    loss_5 = loss(pred_stats[:,5,:], actual_stats).sum(dim=1) * masks[:,5]
    loss_6 = loss(pred_stats[:,6,:], actual_stats).sum(dim=1) * masks[:,6]
    loss_7 = loss(pred_stats[:,7,:], actual_stats).sum(dim=1) * masks[:,7]
    
    total_loss = loss_0.sum() + loss_1.sum() + loss_2.sum() + loss_3.sum() + loss_4.sum() + loss_5.sum() + loss_6.sum() + loss_7.sum()
    return total_loss
        
def Position_Classification_Loss(pred_positions, actual_positions, masks):
    actual_positions  = actual_positions[:, :pred_positions.size(1)]
    masks = masks[:, :pred_positions.size(1)]
    
    batch_size = actual_positions.size(0)
    time_steps = actual_positions.size(1)
    output_size = actual_positions.size(2)
    mask_size = masks.size(2)
    
    pred_positions = pred_positions.reshape((batch_size * time_steps, mask_size, output_size))
    actual_positions = actual_positions.reshape((batch_size * time_steps, output_size))
    masks = masks.reshape((batch_size * time_steps, mask_size))
    
    loss = nn.CrossEntropyLoss(reduction='none')
    loss_0 = loss(pred_positions[:,0,:], actual_positions) * masks[:,0]
    loss_1 = loss(pred_positions[:,1,:], actual_positions) * masks[:,1]
    loss_2 = loss(pred_positions[:,2,:], actual_positions) * masks[:,2]
    loss_3 = loss(pred_positions[:,3,:], actual_positions) * masks[:,3]
    loss_4 = loss(pred_positions[:,4,:], actual_positions) * masks[:,4]
    loss_5 = loss(pred_positions[:,5,:], actual_positions) * masks[:,5]
    loss_6 = loss(pred_positions[:,6,:], actual_positions) * masks[:,6]
    loss_7 = loss(pred_positions[:,7,:], actual_positions) * masks[:,7]
    
    total_loss = loss_0.sum() + loss_1.sum() + loss_2.sum() + loss_3.sum() + loss_4.sum() + loss_5.sum() + loss_6.sum() + loss_7.sum()
    return total_loss
    
    
def Classification_Loss(pred_war, pred_value, pred_pwar, pred_level, pred_pa, actual_war, actual_value, actual_pwar, actual_level, actual_pa, masks):
    actual_war = actual_war[:,:pred_war.size(1)]
    actual_value = actual_value[:,:pred_war.size(1)]
    actual_pwar = actual_pwar[:,:pred_pwar.size(1)]
    actual_level = actual_level[:,:pred_level.size(1)]
    actual_pa = actual_pa[:,:pred_pa.size(1)]
    masks = masks[:,:pred_level.size(1)]
    
    batch_size = actual_war.size(0)
    time_steps = actual_war.size(1)
    
    num_classes_war = pred_war.size(2)
    num_classes_value = pred_value.size(2)
    num_classes_pwar = pred_pwar.size(2)
    num_classes_level = pred_level.size(2)
    num_classes_pa = pred_pa.size(2)
    
    actual_war = actual_war.reshape((batch_size * time_steps,))
    actual_value = actual_value.reshape((batch_size * time_steps,))
    actual_pwar = actual_pwar.reshape((batch_size * time_steps,))
    actual_level = actual_level.reshape((batch_size * time_steps,))
    actual_pa = actual_pa.reshape((batch_size * time_steps,))
    
    masks = masks.reshape((batch_size, time_steps,))
    
    pred_war = pred_war.reshape((batch_size * time_steps, num_classes_war))
    pred_value = pred_value.reshape((batch_size * time_steps, num_classes_value))
    pred_pwar = pred_pwar.reshape((batch_size * time_steps, num_classes_pwar))
    pred_level = pred_level.reshape((batch_size * time_steps, num_classes_level))
    pred_pa = pred_pa.reshape((batch_size * time_steps, num_classes_pa))
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss_war = l(pred_war, actual_war)
    loss_value = l(pred_value, actual_value)
    loss_pwar = l(pred_pwar, actual_pwar)
    loss_level = l(pred_level, actual_level)
    loss_pa = l(pred_pa, actual_pa)
    
    loss_war = loss_war.reshape((batch_size, time_steps))
    loss_value = loss_value.reshape((batch_size, time_steps))
    loss_pwar = loss_pwar.reshape((batch_size, time_steps))
    loss_level = loss_level.reshape((batch_size, time_steps))
    loss_pa = loss_pa.reshape((batch_size, time_steps))
    
    masked_loss_war = loss_war * masks
    masked_loss_value = loss_value * masks
    masked_loss_pwar = loss_pwar * masks
    masked_loss_level = loss_level * masks
    masked_loss_pa = loss_pa * masks
    
    loss_sums_war = masked_loss_war.sum(dim=1)
    loss_sums_value = masked_loss_value.sum(dim=1)
    loss_sums_pwar = masked_loss_pwar.sum(dim=1)
    loss_sums_level = masked_loss_level.sum(dim=1)
    loss_sums_pa = masked_loss_pa.sum(dim=1)
    
    return loss_sums_war.sum(), loss_sums_value.sum(), loss_sums_pwar.sum(), loss_sums_level.sum(), loss_sums_pa.sum()