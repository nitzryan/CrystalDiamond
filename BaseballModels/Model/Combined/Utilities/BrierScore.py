import torch

def Brier_Score(
        probs : torch.Tensor,          # <P_valid, T_batch, Classes>
        target_war : torch.Tensor,     # <P_valid>
        mask_labels : torch.Tensor     # <P_valid, T_dataset>
          ) -> tuple[torch.Tensor, torch.Tensor]:
  
    num_timesteps = probs.shape[1]
    num_classes = probs.shape[2]

    # Reduce dataset-max timesteps down to this batch's padded length
    mask_labels = mask_labels[:, :num_timesteps]
    ts_mask = mask_labels == 1  # <P_valid, T>

    probs_valid = probs[ts_mask]  # <M, Classes>
    actual_valid = target_war.unsqueeze(1).expand(-1, num_timesteps)[ts_mask]  # <M>

    onehot = torch.nn.functional.one_hot(actual_valid, num_classes=num_classes).to(probs_valid.dtype)

    sq_err = (probs_valid - onehot) ** 2        # <M, Classes>
    brier_per_class_sum = sq_err.sum(dim=0).cpu()
    brier_count = torch.tensor(probs_valid.shape[0], dtype=torch.float)

    return brier_per_class_sum, brier_count