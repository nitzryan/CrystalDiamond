import math
import numpy as np
from Model.Evaluation.Classes import FoldAgreementStats

def MeasureGroupAgreement(pairs : np.ndarray) -> FoldAgreementStats:
    """pairs: (M, 2) expected WAR from two disjoint fold groups of equal size."""
    a, b = pairs[:, 0], pairs[:, 1]
    diff = a - b
    
    noise_sd = float(diff.std() / math.sqrt(2.0))
    total_sd = float(pairs.reshape(-1).std())
    signal_var = max(total_sd ** 2 - noise_sd ** 2, 0.0)
    
    return FoldAgreementStats(
        corr=float(np.corrcoef(a, b)[0, 1]),
        mae=float(np.abs(diff).mean()),
        noise_sd=noise_sd,
        signal_sd=math.sqrt(signal_var),
        reliability=signal_var / total_sd ** 2 if total_sd > 0 else 0.0,
    )