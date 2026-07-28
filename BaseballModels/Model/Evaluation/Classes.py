from dataclasses import dataclass
import torch

@dataclass
class FoldSweepResult:
    num_folds : list[int]    # (N,) long, 1..N
    loss_war : list[float]   # (N,) float, per-sample
    brier : list[float]      # (N,) float, per-sample
    num_observations : int
    num_players : int
    
@dataclass
class FoldEnsembleResult:
    single_loss_war : float
    ensemble_loss_war : float
    single_brier : float
    ensemble_brier : float
    num_observations : int
    num_players : int
    mean_folds_per_observation : float
    
@dataclass
class WarFoldPredictions:
    mlb_ids : torch.Tensor   # (N,)        long
    dates : torch.Tensor     # (N, 2)      long, year / month
    probs : torch.Tensor     # (N, R, C)   float, padded across ModelRun
    run_mask : torch.Tensor  # (N, R)      bool, fold slot populated
    
@dataclass
class WarFoldTargets:
    mlb_ids : torch.Tensor    # (M,)    long
    dates : torch.Tensor      # (M, 2)  long, year / month
    target_war : torch.Tensor # (M,)    long, WAR bucket index
    
@dataclass
class AlignedWarFolds:
    mlb_ids : torch.Tensor    # (K,)       long
    probs : torch.Tensor      # (K, R, C)  float
    run_mask : torch.Tensor   # (K, R)     bool
    target_war : torch.Tensor # (K,)       long