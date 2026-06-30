import torch
import torch.nn.functional as F

from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Model.GetTimestepWarLoss import IterWarOutputs
from Combined.Utilities.Types import *
from Constants import device

def GetTimestepWarBrier(
    pro_network : Pro_Model,
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    is_hitter : bool,
    batch_size : int,
    num_bins : int = 10,
    min_pct_cutoff : float = 1.0, # Percentage, not fraction
    min_uncertainty : float = 1e-3,
) -> TimestepBrierResult:

    pro_network.eval()
    col_network.eval()

    stats : list[torch.Tensor] = None
    num_players_total : int = 0

    for output_war, target_war, mask_labels, num_valid in \
            IterWarOutputs(pro_network, col_network, dataset, is_hitter, batch_size):
        war_probs = F.softmax(output_war, dim=-1)
        batch_stats = GetTimestepBrierStats(war_probs, target_war, mask_labels, num_bins)

        if stats is None:
            stats = [torch.zeros_like(s) for s in batch_stats]
        for acc, b in zip(stats, batch_stats):
            acc += b
        num_players_total += num_valid

    n = stats[0]
    bs_model, uncertainty, resolution, reliability, bss = ComputeTimestepBrierMetrics(*stats)
    pct = n / max(num_players_total, 1) * 100

    # Keep a contiguous 0..last run; stop at the first failing timestep
    last = -1
    for t in range(n.shape[0]):
        if (n[t].item() == 0
                or pct[t].item() < min_pct_cutoff
                or uncertainty[t].item() < min_uncertainty):
            break
        last = t
    keep = last + 1

    return TimestepBrierResult(
        timesteps=list(range(keep)),
        bs_model=bs_model[:keep].tolist(),
        uncertainty=uncertainty[:keep].tolist(),
        resolution=resolution[:keep].tolist(),
        reliability=reliability[:keep].tolist(),
        bss=bss[:keep].tolist(),
        pct=pct[:keep].tolist(),
        counts=n[:keep].tolist(),
    )
    
def GetTimestepWarBrierCollege(
    col_network : Col_Model,
    dataset : Combined_Player_Dataset,
    batch_size : int,
    num_bins : int = 10,
    min_pct_cutoff : float = 1.0,
    min_uncertainty : float = 1e-3,
) -> TimestepBrierResult:

    col_network.eval()

    stats : list[torch.Tensor] | None = None
    num_players_total : int = 0

    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.arange(n_samples)
    max_T = dataset.GetColMaxLength()

    with torch.no_grad():
        for batch_i in range(num_batches):
            start = batch_i * batch_size
            end = min(start + batch_size, n_samples)
            batch_indices = indices[start:end]
            _, _, _, col_input, col_targets, _ = dataset.get_batch(batch_indices)

            data, length = col_input
            data = data.to(device, non_blocking=True)
            length = length.to(device, non_blocking=True)

            outputs = col_network(data, length)
            output_war = outputs[1]

            target_war = col_targets[1].to(device, non_blocking=True)

            max_len = output_war.size(1)
            mask_length = (torch.arange(max_len, device=length.device)
                          .unsqueeze(0) < length.unsqueeze(1))

            war_probs = F.softmax(output_war, dim=-1)
            T_batch = war_probs.size(1)
            if T_batch < max_T:
                pad_T = max_T - T_batch
                war_probs = F.pad(war_probs, (0, 0, 0, pad_T))
                mask_length = F.pad(mask_length, (0, pad_T))
            batch_stats = GetTimestepBrierStats(war_probs, target_war, mask_length, num_bins)

            num_valid = (length > 0).sum().item()

            if stats is None:
                stats = [torch.zeros_like(s) for s in batch_stats]
            for acc, b in zip(stats, batch_stats):
                acc += b
            num_players_total += num_valid

    n = stats[0]
    bs_model, uncertainty, resolution, reliability, bss = ComputeTimestepBrierMetrics(*stats)
    pct = n / max(num_players_total, 1) * 100

    last = -1
    for t in range(n.shape[0]):
        if (n[t].item() == 0
                or pct[t].item() < min_pct_cutoff
                or uncertainty[t].item() < min_uncertainty):
            break
        last = t
    keep = last + 1

    return TimestepBrierResult(
        timesteps=list(range(keep)),
        bs_model=bs_model[:keep].tolist(),
        uncertainty=uncertainty[:keep].tolist(),
        resolution=resolution[:keep].tolist(),
        reliability=reliability[:keep].tolist(),
        bss=bss[:keep].tolist(),
        pct=pct[:keep].tolist(),
        counts=n[:keep].tolist(),
    )
    
############################################
def GetTimestepBrierStats(
        probs : torch.Tensor,        # <N, T_batch, C>
        target_war : torch.Tensor,   # <N>
        mask_labels : torch.Tensor,  # <N, T_dataset>
        num_bins : int,
          ) -> tuple[torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor]:

    _, T_batch, C = probs.shape
    T_dataset = mask_labels.shape[1]
    M = num_bins

    valid_counts = torch.zeros(T_dataset)
    sq_err_sum = torch.zeros(T_dataset)
    class_outcome_sum = torch.zeros(T_dataset, C)
    bin_count = torch.zeros(T_dataset, C, M)
    bin_pred_sum = torch.zeros(T_dataset, C, M)
    bin_out_sum = torch.zeros(T_dataset, C, M)

    onehot = F.one_hot(target_war, num_classes=C).float()   # <N, C>
    valid = mask_labels[:, :T_batch].bool()                 # <N, T_batch>

    for t in range(T_batch):
        vt = valid[:, t]
        nt = int(vt.sum().item())
        if nt == 0:
            continue
        p_t = probs[vt, t, :]        # <nt, C>
        o_t = onehot[vt, :]          # <nt, C>

        valid_counts[t] = nt
        sq_err_sum[t] = ((p_t - o_t) ** 2).sum().item()
        class_outcome_sum[t] = o_t.sum(dim=0).cpu()

        bins_t = (p_t * M).long().clamp(0, M - 1)   # <nt, C>
        for c in range(C):
            bc = bins_t[:, c]
            bin_count[t, c]    += torch.bincount(bc, minlength=M).float().cpu()
            bin_pred_sum[t, c] += torch.bincount(bc, weights=p_t[:, c], minlength=M).cpu()
            bin_out_sum[t, c]  += torch.bincount(bc, weights=o_t[:, c], minlength=M).cpu()

    return valid_counts, sq_err_sum, class_outcome_sum, bin_count, bin_pred_sum, bin_out_sum

def ComputeTimestepBrierMetrics(
        n : torch.Tensor,              # <T_dataset>
        sq_err_sum : torch.Tensor,     # <T_dataset>
        class_outcome_sum : torch.Tensor,  # <T_dataset, C>
        bin_count : torch.Tensor,      # <T_dataset, C, M>
        bin_pred_sum : torch.Tensor,   # <T_dataset, C, M>
        bin_out_sum : torch.Tensor,    # <T_dataset, C, M>
          ) -> tuple[torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor]:
    
    n_safe = n.clamp(min=1)

    base_rate = class_outcome_sum / n_safe.unsqueeze(-1)          # <T_dataset, C> per-timestep base rate
    uncertainty = (base_rate * (1 - base_rate)).sum(dim=-1)        # <T_dataset> = Brier of the reference
    bs_model = sq_err_sum / n_safe                        # <T_dataset>

    bc_safe = bin_count.clamp(min=1)
    bin_mean_pred = bin_pred_sum / bc_safe                         # <T_dataset, C, M>
    bin_obj_freq = bin_out_sum / bc_safe                      # <T_dataset, C, M>

    reliability = (bin_count * (bin_mean_pred - bin_obj_freq) ** 2).sum(dim=(1, 2)) / n_safe
    resolution  = (bin_count * (bin_obj_freq - base_rate.unsqueeze(-1)) ** 2).sum(dim=(1, 2)) / n_safe

    # Uncertainty is bs_ref
    bss = 1 - bs_model / uncertainty.clamp(min=1e-12)

    return bs_model, uncertainty, resolution, reliability, bss
