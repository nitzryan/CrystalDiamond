from dataclasses import dataclass
import torch
import numpy as np

@dataclass
class FoldAccuracyResult:
    num_folds : list[int]    # (N,) long, 1..N
    loss_war : list[float]   # (N,) float, per-sample
    brier : list[float]      # (N,) float, per-sample
    
    num_observations : int
    num_players : int
    
@dataclass
class FoldRepeatabilityResult:
    num_folds : list[int]           # (N,) 1..max_folds // 2
    corr : list[float]              # mean Pearson r between disjoint groups
    mae : list[float]               # mean |warA - warB|
    noise_sd : list[float]          # per-prediction fold-noise SD
    signal_sd : list[float]         # noise-corrected SD of true prediction spread
    reliability : list[float]       # signal_var / total_var, in [0, 1]
    paired_war : dict[int, np.ndarray]  # K -> (M, 2) array of paired expected WAR
    num_observations : int
    num_players : int
    
@dataclass
class FoldAgreementStats:
    corr : float         # Pearson r between the two disjoint group predictions
    mae : float          # mean |warA - warB|
    noise_sd : float     # per-prediction fold-noise SD
    signal_sd : float    # noise-corrected SD of the true prediction spread
    reliability : float  # signal var / total var, in [0, 1]
    
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