import torch
import torch.nn as nn
import torch.nn.init as init
import torch.nn.functional as F
import math
from Pro.DataPrep.Data_Prep import Data_Prep
from Pro.DataPrep.Output_StatAggregation import NUM_HITTER_STATS, NUM_HITTER_BUCKETS_PER_STAT, NUM_PITCHER_STATS, NUM_PITCHER_BUCKETS_PER_STAT
from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, NUM_LEVELS

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

    def forward(self, x):
        for i, layer in enumerate(self.layers):
            x = layer(x)
            if i < len(self.layers) - 1:
                x = self.nonlin(x)
        return x

DEFAULT_WAR_BINARY_ARCH = LayerArch(layer_size=43, num_layers=5, nonlin=F.leaky_relu)
DEFAULT_WAR_ORDINAL_ARCH = LayerArch(layer_size=70, num_layers=2, nonlin=F.tanh)
DEFAULT_STATS_ARCH = LayerArch(layer_size=128, num_layers=2)
DEFAULT_PT_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_POS_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_LVL_ARCH = LayerArch(layer_size=128, num_layers=4)
DEFAULT_PA_ARCH = LayerArch(layer_size=32, num_layers=4)
DEFAULT_VALUE_ARCH = LayerArch(layer_size=64, num_layers=2)
DEFAULT_MLBSTAT_ARCH = LayerArch(layer_size=30, num_layers=2)

DEFAULT_WAR_BINARY_ARCH_P = LayerArch(layer_size=43, num_layers=5, nonlin=F.leaky_relu)
DEFAULT_WAR_ORDINAL_ARCH_P = LayerArch(layer_size=70, num_layers=2, nonlin=F.tanh)
DEFAULT_STATS_ARCH_P = LayerArch(layer_size=90, num_layers=2)
DEFAULT_PT_ARCH_P = LayerArch(layer_size=110, num_layers=2)
DEFAULT_POS_ARCH_P = LayerArch(layer_size=55, num_layers=2)
DEFAULT_LVL_ARCH_P = LayerArch(layer_size=150, num_layers=2)
DEFAULT_PA_ARCH_P = LayerArch(layer_size=40, num_layers=2)
DEFAULT_VALUE_ARCH_P = LayerArch(layer_size=120, num_layers=2)
DEFAULT_MLBSTAT_ARCH_P = LayerArch(layer_size=100, num_layers=3)

DEFAULT_PRO_HIDDEN_SIZE = 51
DEFAULT_PRO_NUM_LAYERS = 3

DEFAULT_INPUT_NOISE = 0
DEFAULT_DROPOUT = 0.283

DEFAULT_PRO_WEIGHT_DECAY = [9.7e-4,8.4e-3,1.3e-6,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7]
DEFAULT_PRO_WEIGHT_DECAY_P = [9.7e-4,8.4e-3,1.3e-6,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7,1e-7]

DEFAULT_LEARNING_RATES = [0.0076,0.018,0.018,0.003,0.003,0.003,0.003,0.003,0.003,0.003]
DEFAULT_LEARNING_RATES_P = [0.0076,0.018,0.018,0.003,0.003,0.003,0.003,0.003,0.003,0.003]

class RNN_Model(nn.Module):
    def __init__(self, input_size : int, 
                 
                mutators : torch.Tensor, 
                data_prep : Data_Prep, 
                is_hitter : bool, 
                input_noise : float = DEFAULT_INPUT_NOISE,
                rnn_droupout : float = DEFAULT_DROPOUT,
                num_layers : int = DEFAULT_PRO_NUM_LAYERS, 
                hidden_size : int = DEFAULT_PRO_HIDDEN_SIZE, 
                stats_arch : LayerArch = DEFAULT_STATS_ARCH,
                
                war_binary_arch : LayerArch = DEFAULT_WAR_BINARY_ARCH,
                war_ordinal_arch : LayerArch = DEFAULT_WAR_ORDINAL_ARCH,
                pt_arch : LayerArch = DEFAULT_PT_ARCH,
                pos_arch : LayerArch = DEFAULT_POS_ARCH,
                lvl_arch : LayerArch = DEFAULT_LVL_ARCH,
                pa_arch : LayerArch = DEFAULT_PA_ARCH,
                val_arch : LayerArch = DEFAULT_VALUE_ARCH,
                mlbstat_arch : LayerArch = DEFAULT_MLBSTAT_ARCH,
                
                stats_arch_p : LayerArch = DEFAULT_STATS_ARCH_P,
                war_binary_arch_p : LayerArch = DEFAULT_WAR_BINARY_ARCH,
                war_ordinal_arch_p : LayerArch = DEFAULT_WAR_ORDINAL_ARCH,
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
                
                rnn_nonlinearity : str = "relu",
                ):
        super().__init__()
        
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        
        if not is_hitter:
            stats_arch = stats_arch_p
            war_binary_arch = war_binary_arch_p
            war_ordinal_arch = war_ordinal_arch_p
            pt_arch = pt_arch_p
            pos_arch = pos_arch_p
            lvl_arch = lvl_arch_p
            pa_arch = pa_arch_p
            val_arch = val_arch_p
            mlbstat_arch = mlbstat_arch_p
            weight_decay = weight_decay_p
            learning_rates = learning_rates_p
        
        output_map = data_prep.output_map
        stats_size = output_map.hitter_stats_size if is_hitter else output_map.pitcher_stats_size
        
        self.input_noise = input_noise
        
        self.recurrent = nn.RNN(input_size=input_size+1, hidden_size=hidden_size, num_layers=num_layers, batch_first=False, dropout=rnn_droupout, nonlinearity=rnn_nonlinearity)
        
        num_war_classes = len(output_map.buckets_hitter_war)
        # Individual prediction layers
        # War predicted in 2-stage fashion: 1 to predict 0 or nonzero, 2nd is CORN on nonzero
        # Stage 1: binary 0 vs >0
        self.war_binary = war_binary_arch.Build(hidden_size, 1)
        # Stage 2: CORN over the (num_war_classes - 1) non-zero buckets -> (num_war_classes - 2) logits
        self.war_ordinal = war_ordinal_arch.Build(hidden_size, num_war_classes - 2)
        
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
        
        
        self.mutators = mutators
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
        for layer in [self.war_binary.layers[-1], self.war_ordinal.layers[-1],
                      self.pa.layers[-1], self.level.layers[-1]]:
            init.xavier_uniform_(layer.weight, gain=1.0)
            if layer.bias is not None:
                init.zeros_(layer.bias)
                
        # Set initial prediction to match the actual odds
        p_nonzero = 0.14
        init.constant_(self.war_binary.layers[-1].bias, math.log(p_nonzero / (1 - p_nonzero)))
                
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
                                           {'params': self.war_binary.parameters(), 'lr': learning_rates[1], 'weight_decay': weight_decay[1]},
                                           {'params': self.war_ordinal.parameters(), 'lr': learning_rates[2], 'weight_decay': weight_decay[2]},
                                           {'params': self.level.parameters(), 'lr': learning_rates[3], 'weight_decay': weight_decay[3]},
                                           {'params': self.pa.parameters(), 'lr': learning_rates[4], 'weight_decay': weight_decay[4]},
                                           {'params': self.yearStats.parameters(), 'lr': learning_rates[5], 'weight_decay': weight_decay[5]},
                                           {'params': self.pos.parameters(), 'lr': learning_rates[6], 'weight_decay': weight_decay[6]},
                                           {'params': self.value.parameters(), 'lr': learning_rates[7], 'weight_decay': weight_decay[7]},
                                           {'params': self.pt.parameters(), 'lr': learning_rates[8], 'weight_decay': weight_decay[8]},
                                           {'params': self.mlbstat.parameters(), 'lr': learning_rates[9], 'weight_decay': weight_decay[9]}])

        
    def to(self, *args, **kwargs):
        if self.mutators is not None:
            self.mutators = self.mutators.to(*args, **kwargs)
        return super(RNN_Model, self).to(*args, **kwargs)
    
    def GetHiddenSize(self) -> int:
        return self.hidden_size
    
    def GetNumLayers(self) -> int:
        return self.num_layers
    
    def forward(self, x, lengths, pt_levelYearGames, h0):
        if self.training and self.input_noise > 0:
            noise = torch.rand_like(x, requires_grad=False) * self.input_noise
            x += noise
        
        # Append sequence index
        timesteps = torch.arange(x.size(1), dtype=x.dtype, device=x.device)
        timesteps = timesteps.reshape(1, x.size(1), 1)
        timesteps = timesteps.expand(x.size(0), x.size(1), 1)
        x = torch.cat((x, timesteps), dim=-1)
        
        # Get entries for valid length
        lengths = lengths.to(torch.device("cpu")).long()
        
        # Compute
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        
        # Generate Player State
        #h0 = h0.transpose(0, 1).contiguous()
        packedOutput, _ = self.recurrent(packedInput, h0)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
            
        output_war_binary   = self.war_binary(output)
        output_war_ordinal  = self.war_ordinal(output)
        output_war = (output_war_binary, output_war_ordinal)
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
    
def WarTwoStageProbs(
                binary_logits : torch.Tensor,   # <..., 1>
                ordinal_logits : torch.Tensor   # <..., K_ord - 1>
            ) -> torch.Tensor:                  # <..., K_ord + 1>  (= num_war_classes)
    
    p_nonzero = torch.sigmoid(binary_logits)   # P(y > 0)
    p_zero = 1.0 - p_nonzero

    # CORN: conditional probs q_k = P(y_ord > k | y_ord > k-1); cumulative = P(y_ord > k)
    q = torch.sigmoid(ordinal_logits)
    cum_gt = torch.cumprod(q, dim=-1)                     # <..., K_ord-1>  P(y_ord > k), k=0..K_ord-2

    ones = torch.ones_like(cum_gt[..., :1])
    zeros = torch.zeros_like(cum_gt[..., :1])
    S = torch.cat([ones, cum_gt], dim=-1)                 # P(y_ord > m-1)
    T = torch.cat([cum_gt, zeros], dim=-1)                # P(y_ord > m)
    p_ord = S - T                                         # <..., K_ord>  per-bucket prob within non-zero set

    probs_nonzero = p_nonzero * p_ord                     # scale by P(y > 0)
    probs = torch.cat([p_zero, probs_nonzero], dim=-1)    # <..., K_ord + 1>
    return probs
    
def CornLoss(
            logits : torch.Tensor,   # <M, num_ord_classes - 1>
            y : torch.Tensor,        # <M>, values in [0, num_ord_classes - 1]
            num_ord_classes : int
        ) -> torch.Tensor:
    
    losses = logits.new_zeros(())
    num_examples = 0
    for i in range(num_ord_classes - 1):
        mask = y >= i                       # samples still "in play" for task i (y > i-1)
        if mask.sum() < 1:
            continue
        label = (y[mask] > i).float()
        pred = logits[mask, i]
        losses = losses + F.binary_cross_entropy_with_logits(pred, label, reduction='sum')
        num_examples += int(mask.sum())
    if num_examples == 0:
        return logits.sum() * 0.0
    return losses
    
def War_TwoStage_Loss(
            binary_logits : torch.Tensor,    # <P, T, 1>
            ordinal_logits : torch.Tensor,   # <P, T, K_ord - 1>
            target_war : torch.Tensor,       # <P>
            mask_labels : torch.Tensor       # <P, T>
        ) -> torch.Tensor:
    
    P, T, _ = binary_logits.shape
    mask_labels = mask_labels[:, :T]
    num_ord_classes = ordinal_logits.shape[-1] + 1

    target = target_war.unsqueeze(1).expand(P, T)   # broadcast player label across timesteps
    valid = mask_labels.bool()

    binary_logits = binary_logits.squeeze(-1)[valid]   # <N>
    ordinal_logits = ordinal_logits[valid]             # <N, K_ord-1>
    target = target[valid]                             # <N>

    # Stage 1: 0 vs >0 over all valid timesteps
    binary_target = (target > 0).float()
    loss_binary = F.binary_cross_entropy_with_logits(binary_logits, binary_target, reduction='sum')

    # Stage 2: CORN over non-zero buckets only, conditional on y > 0
    nonzero = target > 0
    ord_target = (target[nonzero] - 1).long()          # map classes 1..K-1 -> ordinal 0..K_ord-1
    loss_ordinal = CornLoss(ordinal_logits[nonzero], ord_target, num_ord_classes)

    return loss_binary + loss_ordinal
    
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
    
def Classification_Loss(pred_level, pred_pa, actual_level, actual_pa, masks):
    masks = masks[:,:pred_level.size(1)]
    
    batch_size = pred_level.size(0)
    time_steps = pred_level.size(1)
    
    num_classes_level = pred_level.size(2)
    num_classes_pa = pred_pa.size(2)
    
    masks = masks.reshape((batch_size, time_steps,))
    
    pred_level = pred_level.reshape((batch_size * time_steps, num_classes_level))
    pred_pa = pred_pa.reshape((batch_size * time_steps, num_classes_pa))
    
    actual_level = actual_level.repeat_interleave(time_steps)
    actual_pa = actual_pa.repeat_interleave(time_steps)
    
    l = nn.CrossEntropyLoss(reduction='none')
    loss_level = l(pred_level, actual_level)
    loss_pa = l(pred_pa, actual_pa)
    
    loss_level = loss_level.reshape((batch_size, time_steps))
    loss_pa = loss_pa.reshape((batch_size, time_steps))
    
    masked_loss_level = loss_level * masks
    masked_loss_pa = loss_pa * masks
    
    loss_sums_level = masked_loss_level.sum(dim=1)
    loss_sums_pa = masked_loss_pa.sum(dim=1)
    
    return loss_sums_level.sum(), loss_sums_pa.sum()