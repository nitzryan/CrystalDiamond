from dataclasses import dataclass
from typing import Optional
import torch


from dataclasses import dataclass
import torch

@dataclass
class WarClassCounts:
    predicted: torch.Tensor
    actual: torch.Tensor

    @staticmethod
    def zeros_like(other: 'WarClassCounts') -> 'WarClassCounts':
        return WarClassCounts(
            predicted=torch.zeros_like(other.predicted),
            actual=torch.zeros_like(other.actual))

    def __iadd__(self, other: 'WarClassCounts') -> 'WarClassCounts':
        self.predicted += other.predicted
        self.actual += other.actual
        return self

@dataclass
class BrierAccumulator:
    per_class_sum: torch.Tensor
    count: torch.Tensor

    @staticmethod
    def zeros_like(other: 'BrierAccumulator') -> 'BrierAccumulator':
        return BrierAccumulator(
            per_class_sum=torch.zeros_like(other.per_class_sum),
            count=torch.zeros_like(other.count))

    def __iadd__(self, other: 'BrierAccumulator') -> 'BrierAccumulator':
        self.per_class_sum += other.per_class_sum
        self.count += other.count
        return self

@dataclass
class WarDistribution:
    pred_pct: list[float]
    actual_pct: list[float]

@dataclass
class ProLossResult:
    losses: tuple[torch.Tensor, ...]
    war_counts: WarClassCounts
    brier: BrierAccumulator

@dataclass
class CollegeLossResult:
    losses: tuple[torch.Tensor, ...]
    hidden: torch.Tensor
    war_counts: WarClassCounts
    brier: BrierAccumulator

@dataclass
class EpochResult:
    avg_loss: list[float]
    pro_war_dist: WarDistribution
    pro_brier_total: float
    col_war_dist: WarDistribution
    col_brier_total: float
    
@dataclass
class TimestepBrierResult:
    timesteps: list[int]
    bs_model: list[float]
    uncertainty: list[float]
    resolution: list[float]
    reliability: list[float]
    bss: list[float]
    pct: list[float]
    counts: list[float]
    
@dataclass
class TrainResults:
    best_loss: float
    best_epoch : int
    test_losses : list[list[float]]