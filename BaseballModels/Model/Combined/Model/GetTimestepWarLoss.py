from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

from College.Model.Model_Train import GetLossesHitter, GetLossesPitcher
from Constants import device

from Pro.Model.Player_Model import War_TwoStage_Loss

import torch

def GetTimestepWarLoss(
    pro_network : Pro_Model,
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    is_hitter : bool,
    batch_size : int,
    min_pct_cutoff : float,
) -> tuple[list[float], list[float]]:
    """
    Separate (non-training) pass that computes, per timestep:
        ts_avg_loss : average WAR loss over players with mask_labels==1 at that timestep
        ts_pct      : % of all valid samples that occur at that timestep
    """
    pro_network.eval()
    col_network.eval()

    ts_loss_sum_total : torch.Tensor = None
    ts_count_total : torch.Tensor = None
    num_players_total : int = 0

    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.arange(n_samples)

    with torch.no_grad():
        for batch_i in range(num_batches):
            start = batch_i * batch_size
            end = min(start + batch_size, n_samples)
            batch_indices = indices[start:end]
            pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks = dataset.get_batch(batch_indices)

            if is_hitter:
                _, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=False)
            else:
                _, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=False)

            # Forward pass for WAR output only (mirrors GetLosses)
            data, length, pt_levelYearGames = pro_data
            mask_valid = length > 0
            data = data[mask_valid].to(device, non_blocking=True)
            length = length[mask_valid].to(device, non_blocking=True)
            pt_levelYearGames = pt_levelYearGames[mask_valid].to(device, non_blocking=True)
            h0 = h0[mask_valid].transpose(0, 1).to(device, non_blocking=True)

            output_war, *_ = pro_network(data, length, pt_levelYearGames, h0)
            output_war_binary, output_war_ordinal = output_war

            target_war = pro_targets[0][mask_valid].to(device, non_blocking=True)
            mask_labels = pro_masks[0][mask_valid].to(device, non_blocking=True)

            T = output_war_binary.shape[1]
            T_dataset = mask_labels.shape[1]

            batch_loss_sum, batch_count = GetTimestepWarLossFromOutput(
                output_war_binary, output_war_ordinal, target_war, mask_labels)

            if ts_loss_sum_total is None:
                ts_loss_sum_total = torch.zeros_like(batch_loss_sum)
                ts_count_total = torch.zeros_like(batch_count)
            ts_loss_sum_total += batch_loss_sum
            ts_count_total += batch_count
            num_players_total += int(mask_valid.sum().item())

    ts_avg_loss = (ts_loss_sum_total / ts_count_total.clamp(min=1)).tolist()
    ts_pct = (ts_count_total / max(num_players_total, 1) * 100).tolist()
    
    # Keep up to the last timestep that meets the coverage cutoff
    last = max((i for i, p in enumerate(ts_pct) if p >= min_pct_cutoff), default=0)
    timesteps = list(range(last + 1))
    
    return timesteps, ts_avg_loss[:last + 1], ts_pct[:last + 1]

def GetTimestepWarLossFromOutput(
        output_war_binary : torch.Tensor,   # <N, T_batch, 1>
        output_war_ordinal : torch.Tensor,  # <N, T_batch, K-2>
        target_war : torch.Tensor,          # <N>
        mask_labels : torch.Tensor          # <N, T_dataset>
          ) -> tuple[torch.Tensor, torch.Tensor]:
    """
    For one batch, returns:
        ts_loss_sum : <T_dataset> summed WAR loss over players with mask_labels==1
                      at each timestep.
        ts_count    : <T_dataset> number of valid (mask_labels==1) players at each timestep.
    """
    T = output_war_binary.shape[1]
    T_dataset = mask_labels.shape[1]

    ts_loss_sum = torch.zeros(T_dataset, dtype=torch.float)
    ts_count = torch.zeros(T_dataset, dtype=torch.float)

    mask_t = mask_labels[:, :T].bool()
    for t in range(T):
        count = mask_t[:, t].sum()
        if count == 0:
            continue
        loss_t = War_TwoStage_Loss(
            output_war_binary[:, t:t + 1, :],    # <N, 1, 1>
            output_war_ordinal[:, t:t + 1, :],   # <N, 1, K-2>
            target_war,                          # <N>
            mask_labels[:, t:t + 1],             # <N, 1>
        )
        ts_loss_sum[t] = loss_t.item()
        ts_count[t] = count.item()

    return ts_loss_sum, ts_count