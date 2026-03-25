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
    
    # Check on a per-feature distribution
    real_np = real_np.reshape((batch_size * time_steps, feature_size))
    fake_np = fake_np.reshape((batch_size * time_steps, feature_size))
    
    ks_stats = np.zeros((feature_size))
    
    for f in range(feature_size):
        stat, _ = ks_2samp(real_np[:,f], fake_np[:,f], alternative='two-sided')
        ks_stats[f] = stat
            
    # Check on a per-feature, per-timestep distribution
    real_np = real_np.reshape((batch_size, time_steps, feature_size))
    fake_np = fake_np.reshape((batch_size, time_steps, feature_size))
    
    ks_stats_timestep = np.zeros((time_steps, feature_size))
    for t in range(time_steps):
        for f in range(feature_size):
            stat, _ = ks_2samp(real_np[:,t,f], fake_np[:,t,f], alternative='two-sided')
            ks_stats_timestep[t,f] = stat
            
    return ks_stats.mean(), ks_stats_timestep.mean()