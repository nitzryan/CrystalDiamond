from PitchModel.Constants import *

def _check_fold_parameters(N : int, M : int) -> int:
    if N < 1 or M < 1 or M % (N + 1) != 0:
        raise ValueError(f"total_training_runs ({M}) must be a positive multiple of train_test_ratio + 1 ({N + 1})")
    return M // (N + 1)

def _test_group_offsets(num_groups : int, groups_per_test : int) -> tuple[int, ...]:
    def search(chosen : tuple[int, ...], used : frozenset[int]) -> tuple[int, ...] | None:
        if len(chosen) == groups_per_test:
            return chosen
        for d in range(chosen[-1] + 1, num_groups):
            new_diffs : set[int] = set()
            valid = True
            for c in chosen:
                forward = (d - c) % num_groups
                backward = (c - d) % num_groups
                # forward == backward means the pair would repeat half a cycle later
                if forward == backward or forward in used or backward in used or forward in new_diffs or backward in new_diffs:
                    valid = False
                    break
                new_diffs.update((forward, backward))
            if valid:
                result = search(chosen + (d,), used | new_diffs)
                if result is not None:
                    return result
        return None
    
    offsets = search((0,), frozenset())
    if offsets is None:
        raise ValueError(f"No fold design exists with {num_groups} groups and {groups_per_test} groups per test set "
                         f"where no two groups are tested together twice")
    return offsets

def _test_groups(N : int, M : int, C : int) -> frozenset[int]:
    offsets = _test_group_offsets(M, _check_fold_parameters(N, M))
    return frozenset((C + d) % M for d in offsets)

def GetDisjointTrainIndices(
        count : int,
        train_test_ratio : int = TRAIN_TEST_RATIO,
        total_training_runs : int = NUM_TRAINING_VARIANTS) -> list[int]:
    """Run indices whose test sets share no pitcher group (e.g. for cross-validation averaging)."""
    chosen : list[int] = []
    used : set[int] = set()
    for c in range(total_training_runs):
        groups = _test_groups(train_test_ratio, total_training_runs, c)
        if used.isdisjoint(groups):
            chosen.append(c)
            used |= groups
        if len(chosen) == count:
            return chosen
    raise ValueError(f"Only {len(chosen)} runs with disjoint test sets exist for {total_training_runs} runs "
                     f"at ratio {train_test_ratio}; {count} requested")