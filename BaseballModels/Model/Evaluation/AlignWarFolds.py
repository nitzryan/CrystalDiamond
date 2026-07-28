from Model.Evaluation.Classes import WarFoldTargets, WarFoldPredictions, AlignedWarFolds
import torch

def EncodeWarFoldKeys(mlb_ids : torch.Tensor, dates : torch.Tensor) -> torch.Tensor:
    return (mlb_ids * 1000000) + (dates[:, 0] * 100) + dates[:, 1]

def AlignWarFolds(predictions : WarFoldPredictions, targets : WarFoldTargets) -> AlignedWarFolds:
    pred_keys = EncodeWarFoldKeys(predictions.mlb_ids, predictions.dates)
    target_keys = EncodeWarFoldKeys(targets.mlb_ids, targets.dates)
    
    sorted_keys, sorted_order = torch.sort(target_keys)
    slots = torch.searchsorted(sorted_keys, pred_keys).clamp(max=sorted_keys.numel() - 1)
    matched = sorted_keys[slots] == pred_keys
    target_idx = sorted_order[slots[matched]]
    
    return AlignedWarFolds(
        mlb_ids=predictions.mlb_ids[matched],
        probs=predictions.probs[matched],
        run_mask=predictions.run_mask[matched],
        target_war=targets.target_war[target_idx],
    )