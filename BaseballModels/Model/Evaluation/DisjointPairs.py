import torch
import math
import itertools

def CountDisjointPairs(max_folds : int, n : int) -> int:
    return math.comb(max_folds, n) * math.comb(max_folds - n, n) // 2

def EnumerateDisjointPairs(max_folds : int, n : int) -> list[tuple[tuple[int, ...], tuple[int, ...]]]:
    pairs = []
    for a in itertools.combinations(range(max_folds), n):
        remaining = [i for i in range(max_folds) if i not in a]
        for b in itertools.combinations(remaining, n):
            if a < b:
                pairs.append((a, b))
    return pairs

def SampleDisjointFoldPairs(fold_probs : torch.Tensor, n : int) -> tuple[torch.Tensor, torch.Tensor]:
    max_folds = fold_probs.size(1)
    perm = torch.randperm(max_folds, device=fold_probs.device)
    idx_a = perm[:n]
    idx_b = perm[n:2 * n]
    return fold_probs[:, idx_a, :], fold_probs[:, idx_b, :]