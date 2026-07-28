from Model.Evaluation.Classes import FoldSweepResult
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

def Eval_FoldSweep(model_id : int, is_hitter : bool, num_draws : int = 16) -> FoldSweepResult:
    with torch.no_grad():
        targets = GetProWarTargets(model_id, is_hitter)
        predictions = GetProWarPredictions(model_id, is_hitter)
        aligned = AlignWarFolds(predictions, targets)
        
        # Every observation has to supply the same number of folds or the curve isn't comparable across n
        fold_counts = aligned.run_mask.sum(dim=1)
        max_folds = int(fold_counts.max().item())
        if not (fold_counts == max_folds).all():
            raise ValueError(f"Not all observations have the same fold count. "
                             f"Found counts: {fold_counts.unique().tolist()}")
        
        probs = aligned.probs.to(device)
        run_mask = aligned.run_mask.to(device)
        target_war = aligned.target_war.to(device)
        mlb_ids = aligned.mlb_ids
        
        n_obs = probs.size(0)
        fold_probs = probs[run_mask].reshape(n_obs, max_folds, probs.size(-1))
        sample_mask = torch.ones(n_obs, 1, dtype=torch.bool, device=device)
        
        loss_war = []
        brier = []
        
        for n in tqdm(range(1, max_folds + 1), desc="Sweeping Fold Counts"):
            total_combinations = math.comb(max_folds, n)
            draws = min(total_combinations, num_draws)
            exhaustive = draws == total_combinations
            
            loss_total = 0.0
            brier_sum_total = 0.0
            brier_count_total = 0.0
            
            for draw_idx in range(draws):
                if exhaustive:
                    subset_probs = GetFoldCombination(fold_probs, n, draw_idx)
                else:
                    subset_probs = SampleFoldSubset(fold_probs, n)
                    
                output = ProbsToLogits(subset_probs.mean(dim=1)).unsqueeze(1)
                
                loss_total += Classification_Loss(output, target_war, sample_mask).item()
                
                brier_per_class_sum, brier_count = Brier_Score(output, target_war, sample_mask)
                brier_sum_total += brier_per_class_sum.sum().item()
                brier_count_total += float(brier_count)
            
            loss_war.append(loss_total / (n_obs * draws))
            brier.append(brier_sum_total / brier_count_total)
        
        return FoldSweepResult(
            num_folds=list(range(1, max_folds + 1)),
            loss_war=loss_war,
            brier=brier,
            num_observations=n_obs,
            num_players=int(mlb_ids.unique().numel()),
        )
        
def SampleFoldSubset(fold_probs : torch.Tensor, n : int) -> torch.Tensor:
    scores = torch.rand(fold_probs.size(0), fold_probs.size(1), device=fold_probs.device)
    chosen = scores.topk(n, dim=1, largest=False).indices
    return fold_probs.gather(1, chosen.unsqueeze(-1).expand(-1, -1, fold_probs.size(-1)))

def GetFoldCombination(fold_probs : torch.Tensor, n : int, combo_idx : int) -> torch.Tensor:
    combos = list(itertools.combinations(range(fold_probs.size(1)), n))
    return fold_probs[:, combos[combo_idx], :]