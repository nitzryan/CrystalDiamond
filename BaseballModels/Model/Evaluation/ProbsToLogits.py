import torch

def ProbsToLogits(probs : torch.Tensor) -> torch.Tensor:
    return torch.log(probs.clamp_min(1e-12))