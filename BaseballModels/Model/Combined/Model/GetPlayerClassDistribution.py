import torch
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

def GetPlayerClassDistribution(
    dataset : Combined_Player_Dataset,
    batch_size : int,
    num_war_classes : int = 0,
) -> list[float]:
    all_targets : list[torch.Tensor] = []

    n_samples = len(dataset)
    num_batches = (n_samples + batch_size - 1) // batch_size
    indices = torch.arange(n_samples)

    for batch_i in range(num_batches):
        start = batch_i * batch_size
        end = min(start + batch_size, n_samples)
        batch_indices = indices[start:end]
        pro_data, pro_targets, _, _, _, _ = dataset.get_batch(batch_indices)

        _, length, _ = pro_data
        target_war = pro_targets[0]
        mask_valid = length > 0 # Has Pro Data
        all_targets.append(target_war[mask_valid])

    all_targets = torch.cat(all_targets)
    counts = torch.bincount(all_targets, minlength=num_war_classes).float()
    percentages = (counts / counts.sum() * 100).tolist()
    return percentages