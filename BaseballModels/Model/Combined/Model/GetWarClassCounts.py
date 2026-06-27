import torch

def GetWarClassCounts(
        probs : torch.Tensor,                             # <P_valid, T, Classes>
        target_war : torch.Tensor,                        # <Players_Valid>
        mask_labels : torch.Tensor                        # <Players_Valid, T>
          ) -> tuple[torch.Tensor, torch.Tensor]:
    
    num_timesteps = probs.shape[1]
    
    mask_labels = mask_labels[:, :num_timesteps]
    ts_mask = mask_labels == 1  # <P_valid, T>
    
    num_classes = probs.shape[2]
    
    # Predicted: sum softmax probabilities over valid timesteps (soft counts)
    predicted_sum = probs[ts_mask].sum(dim=0).cpu()
    
    # Actual: hard count per class over valid timesteps
    actual_classes = target_war.unsqueeze(1).expand(-1, num_timesteps)
    actual_valid = actual_classes[ts_mask]
    actual_counts = torch.bincount(actual_valid, minlength=num_classes).float().cpu()
    
    return predicted_sum, actual_counts