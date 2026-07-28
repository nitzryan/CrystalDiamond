from Model.Evaluation.Classes import WarFoldPredictions
from Model.Constants import model_db

import torch

def GetProWarPredictions(model_id : int, is_hitter : bool) -> WarFoldPredictions:
    is_hitter_int = 1 if is_hitter else 0
    cursor = model_db.cursor()
    
    # Restricting to PlayersInTrainingData drops the players that are scored but never labeled
    rows = cursor.execute(
        "SELECT mlbId, year, month, ModelRun, war0, war1, war2, war3, war4, war5, war6 "
        "FROM Output_PlayerWar "
        f"WHERE ModelId=? AND isHitter={is_hitter_int} AND mlbId IN ("
        f"  SELECT mlbId FROM PlayersInTrainingData WHERE modelId=? AND isHitter={is_hitter_int} AND mlbId!=-1"
        ")", (model_id, model_id)).fetchall()
    
    raw = torch.tensor(rows, dtype=torch.float64)
    keys = raw[:, :3].long()
    runs = raw[:, 3].long()
    probs = raw[:, 4:].float()
    
    unique_keys, inverse = torch.unique(keys, dim=0, return_inverse=True)
    num_runs = int(runs.max().item()) + 1
    
    fold_probs = torch.zeros(unique_keys.size(0), num_runs, probs.size(1))
    fold_probs[inverse, runs] = probs
    
    run_mask = torch.zeros(unique_keys.size(0), num_runs, dtype=torch.bool)
    run_mask[inverse, runs] = True
    
    return WarFoldPredictions(
        mlb_ids=unique_keys[:, 0],
        dates=unique_keys[:, 1:],
        probs=fold_probs,
        run_mask=run_mask,
    )