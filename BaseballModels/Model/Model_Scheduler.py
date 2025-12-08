from torch.optim import Optimizer
from torch import inf

class Model_Scheduler_ReduceOnPlateauGroups:
    def __init__(self, optimizer, parameter_map : list[list[int]], mode='min', factor=0.5, patience=10,
                 threshold=1e-4, threshold_mode='rel', cooldown=0,
                 min_lr=0, eps=1e-8, verbose=False):

        if factor >= 1.0:
            raise ValueError('Factor should be < 1.0.')
        self.factor = factor

        # Attach optimizer
        if not isinstance(optimizer, Optimizer):
            raise TypeError('{} is not an Optimizer'.format(
                type(optimizer).__name__))
        self.optimizer = optimizer

        if isinstance(min_lr, (list, tuple)):
            if len(min_lr) != len(optimizer.param_groups):
                raise ValueError("expected {} min_lrs, got {}".format(
                    len(optimizer.param_groups), len(min_lr)))
            self.min_lrs = list(min_lr)
        else:
            self.min_lrs = [min_lr] * len(optimizer.param_groups)

        self.parameter_map = parameter_map
        parameter_map_len = len(parameter_map)
        self.patience = patience
        self.verbose = verbose
        self.cooldown = cooldown
        self.cooldown_counter = [0] * parameter_map_len
        self.mode = mode
        self.threshold = threshold
        self.threshold_mode = threshold_mode
        self.best = [None] * parameter_map_len
        self.num_bad_epochs = [None] * parameter_map_len
        self.mode_worse = [None] * parameter_map_len  # the worse value for the chosen mode
        self.eps = eps
        self.last_epoch = 0
        self._init_is_better(mode=mode, threshold=threshold,
                             threshold_mode=threshold_mode)
        self._reset()

    def _reset(self):
        """Resets num_bad_epochs counter and cooldown counter."""
        self.best = [self.mode_worse] * len(self.best)
        self.cooldown_counter = [0] * len(self.cooldown_counter)
        self.num_bad_epochs = [0] * len(self.num_bad_epochs)

    def step(self, metrics : list[float]):
        # convert `metrics` to float, in case it's a zero-dim Tensor
        epoch = self.last_epoch + 1
        self.last_epoch = epoch

        if len(metrics) != len(self.parameter_map):
            raise ValueError('Metrics and parameter_mapping must be same length')
        
        for i, metric in enumerate(metrics):
            if self.is_better(metric, self.best[i]):
                self.best[i] = metric
                self.num_bad_epochs[i] = 0
            else:
                self.num_bad_epochs[i] += 1

            if self.in_cooldown(i):
                self.cooldown_counter[i] -= 1
                self.num_bad_epochs[i] = 0  # ignore any bad epochs in cooldown

            if self.num_bad_epochs[i] > self.patience:
                self._reduce_lr(epoch, i)
                self.cooldown_counter[i] = self.cooldown
                self.num_bad_epochs[i] = 0

        self._last_lr = [group['lr'] for group in self.optimizer.param_groups]

    def _reduce_lr(self, epoch : int, parameter_idx : int):
        parameter_group_idxs = self.parameter_map[parameter_idx]
        for i in parameter_group_idxs:
            param_group = self.optimizer.param_groups[i]
            old_lr = float(param_group['lr'])
            new_lr = max(old_lr * self.factor, self.min_lrs[i])
            if old_lr - new_lr > self.eps:
                param_group['lr'] = new_lr
                if self.verbose:
                    epoch_str = ("%.2f" if isinstance(epoch, float) else
                                 "%.5d") % epoch
                    print('Epoch {}: reducing learning rate'
                          ' of group {} to {:.4e}.'.format(epoch_str, i, new_lr))

    def in_cooldown(self, idx : int):
        return self.cooldown_counter[idx] > 0

    def is_better(self, a, best):
        if self.mode == 'min' and self.threshold_mode == 'rel':
            rel_epsilon = 1. - self.threshold
            return a < best * rel_epsilon

        elif self.mode == 'min' and self.threshold_mode == 'abs':
            return a < best - self.threshold

        elif self.mode == 'max' and self.threshold_mode == 'rel':
            rel_epsilon = self.threshold + 1.
            return a > best * rel_epsilon

        else:  # mode == 'max' and epsilon_mode == 'abs':
            return a > best + self.threshold

    def _init_is_better(self, mode, threshold, threshold_mode):
        if mode not in {'min', 'max'}:
            raise ValueError('mode ' + mode + ' is unknown!')
        if threshold_mode not in {'rel', 'abs'}:
            raise ValueError('threshold mode ' + threshold_mode + ' is unknown!')

        if mode == 'min':
            self.mode_worse = inf
        else:  # mode == 'max':
            self.mode_worse = -inf

        self.mode = mode
        self.threshold = threshold
        self.threshold_mode = threshold_mode

    def state_dict(self):
        return {key: value for key, value in self.__dict__.items() if key != 'optimizer'}

    def load_state_dict(self, state_dict):
        self.__dict__.update(state_dict)
        self._init_is_better(mode=self.mode, threshold=self.threshold, threshold_mode=self.threshold_mode)