import torch
import torch.nn as nn
import torch.nn.functional as F
import Model.DraftPickSlot.Config as Config
import Model.DraftPickSlot.DataPrep as DataPrep

class WarNet(nn.Module):
    def __init__(
            self,
            in_features: int = DataPrep.NUM_FEATURES,
            num_classes: int = DataPrep.NUM_CLASSES,
            hidden_sizes: list[int] = [16,32,16]
    ) -> None:
        super().__init__()

        layers: list[nn.Module] = []
        prev = in_features
        for h in hidden_sizes:
            layers.append(nn.Linear(prev, h))
            layers.append(nn.ReLU())
            prev = h
        layers.append(nn.Linear(prev, num_classes))

        self.net = nn.Sequential(*layers)

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        return self.net(x)
    
    def predict_probability(self, x: torch.Tensor) -> torch.Tensor:
        return F.softmax(self.forward(x), dim=1)
    
def soft_cross_entropy(
        logits: torch.Tensor, target: torch.Tensor) -> torch.Tensor:
    
    log_probs = F.log_softmax(logits, dim=1)
    return -(target * log_probs).sum(dim=1).mean()