from Model.Evaluation.Classes import FoldEnsembleResult
from Model.Constants import device

from Model.Evaluation.AlignWarFolds import AlignWarFolds
from Model.Evaluation.Classes import *
from Model.Evaluation.GetProWarPredictions import GetProWarPredictions
from Model.Evaluation.GetProWarTargets import GetProWarTargets
from Model.Evaluation.ProbsToLogits import ProbsToLogits

from Model.Pro.Model.Player_Model import Classification_Loss
from Model.Combined.Utilities.BrierScore import Brier_Score

from tqdm import tqdm
import math
import torch
import itertools

def EvalFoldEnsemble(model_id : int, is_hitter : bool) -> FoldEnsembleResult:
    with torch.no_grad():
        targets = GetProWarTargets(model_id, is_hitter)
        predictions = GetProWarPredictions(model_id, is_hitter)
        aligned = AlignWarFolds(predictions, targets)
        
        # An observation needs two out-of-training folds before averaging means anything
        fold_counts = aligned.run_mask.sum(dim=1)
        comparable = fold_counts >= 2
        
        probs = aligned.probs[comparable].to(device)
        run_mask = aligned.run_mask[comparable].to(device)
        target_war = aligned.target_war[comparable].to(device)
        mlb_ids = aligned.mlb_ids[comparable]
        fold_counts = fold_counts[comparable].to(device)
        
        n_obs, n_runs, n_buckets = probs.shape
        
        # Single fold: every populated (observation, fold) pair scored as its own sample
        single_probs = probs[run_mask]                                          # (num_valid, n_buckets)
        single_output = ProbsToLogits(single_probs).unsqueeze(1)                # (num_valid, 1, n_buckets)
        single_target = target_war.unsqueeze(1).expand_as(run_mask)[run_mask]   # (num_valid,)
        single_mask = torch.ones(single_probs.size(0), 1, dtype=torch.bool, device=device)
        
        single_loss = Classification_Loss(single_output, single_target, single_mask)
        single_brier_sum, single_brier_count = Brier_Score(single_output, single_target, single_mask)
        
        # Ensemble: mean probability across that observation's populated folds
        ensemble_probs = (probs * run_mask.unsqueeze(-1).float()).sum(dim=1) / fold_counts.unsqueeze(-1).float()
        ensemble_output = ProbsToLogits(ensemble_probs).unsqueeze(1)
        ensemble_target = target_war
        ensemble_mask = torch.ones(n_obs, 1, dtype=torch.bool, device=probs.device)
        
        ensemble_loss = Classification_Loss(ensemble_output, ensemble_target, ensemble_mask)
        ensemble_brier_sum, ensemble_brier_count = Brier_Score(ensemble_output, ensemble_target, ensemble_mask)
        
        return FoldEnsembleResult(
            single_loss_war=single_loss.item(),
            ensemble_loss_war=ensemble_loss.item(),
            single_brier=(single_brier_sum.sum() / single_brier_count).item(),
            ensemble_brier=(ensemble_brier_sum.sum() / ensemble_brier_count).item(),
            num_observations=n_obs,
            num_players=int(mlb_ids.unique().numel()),
            mean_folds_per_observation=fold_counts.float().mean().item(),
        )