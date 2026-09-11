import torch.nn.functional as F
from enum import Enum, auto
from dataclasses import dataclass
import math
import optuna

ACTIVATION_FUNCTIONS = ["ReLU", "LeakyReLU", "GELU", "SiLU", "Tanh"]
ACTIVATION_MAP = {
        "ReLU": F.relu,
        "LeakyReLU": F.leaky_relu,
        "GELU": F.gelu,
        "SiLU": F.silu,
        "Tanh": F.tanh,
    }

# Configurable Explore/Exploit balance
class SearchWidth(Enum):
    WIDE = auto()
    NARROW = auto()
    
@dataclass(frozen=True)
class ParamSpec:
    name: str
    low: float = -1
    high: float = -1
    log: bool = False
    is_int: bool = False
    choices: list[str] | None = None
    narrow_frac: float = 0.2 # How much range (% of value) is varied in narrow test

    def __post_init__(self):
        if self.choices is None:
            assert self.low < self.high, f"ParamSpec '{self.name}': low must be < high"

    def suggest(self, trial: optuna.trial.Trial, default, width: SearchWidth):
        if self.choices is not None:
            if width is SearchWidth.NARROW:
                return default
            return trial.suggest_categorical(self.name, self.choices)

        low, high = self.low, self.high
        if width is SearchWidth.NARROW:
            span = self.narrow_frac * default
            low = max(low, default - span)
            high = min(high, default + span)
            
            # Low int should always round down, high int round up
            if self.is_int:
                low = round(low) if self.low == 0 else math.floor(low)
                high = math.ceil(high)

        if self.is_int:
            return trial.suggest_int(self.name, int(round(low)), int(round(high)), log=self.log)
        return trial.suggest_float(self.name, low, high, log=self.log)