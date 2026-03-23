import torch
import numpy as np
from scipy.stats import ks_2samp

def KS_Test_TimeSeries(
            real : torch.Tensor,
            fake : torch.Tensor) -> float:
    
    real_np = real.detach().cpu().numpy().astype(np.float64)
    fake_np = fake.detach().cpu().numpy().astype(np.float64)
    
    batch_size, time_steps, feature_size = real.shape
    assert real_np.shape == fake_np.shape
    
    real_np = real_np.reshape((batch_size * time_steps, feature_size))
    fake_np = fake_np.reshape((batch_size * time_steps, feature_size))
    
    ks_stats = np.zeros((feature_size))
    pvalues = np.zeros((feature_size))
    
    for f in range(feature_size):
        stat, pval = ks_2samp(real_np[:,f], fake_np[:,f], alternative='two-sided')
        ks_stats[f] = stat
        pvalues[f] = pval
            
    return ks_stats.mean()