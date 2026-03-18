import torch
import torch.nn as nn
import torch.nn.functional as F

class MonotoneLinear(nn.Linear):
    """Linear layer with strictly positive weights (softplus) to preserve monotonicity."""
    def forward(self, input: torch.Tensor) -> torch.Tensor:
        # softplus(weight) > 0 for all elements — exactly like the paper's exp(W)
        positive_weight = F.softplus(self.weight)
        return F.linear(input, positive_weight, self.bias)

# Cannon, A.J. Non-crossing nonlinear regression quantiles by monotone composite quantile regression neural network, with application to rainfall extremes. Stoch Environ Res Risk Assess 32, 3207–3225 (2018). https://doi.org/10.1007/s00477-018-1573-6
class MCQRNN(nn.Module):
    '''Implements MCQRNN module that takes in percentile and input, output strictly increases/stays flat with
    increase in tau
    Activation function MUST be non-decreasing, or this will not guarentee that results grow as tau grows'''
    def __init__(
        self,
        feature_dim : int,
        hidden_dims: list,
        activation : nn.Module
    ):
        super().__init__()
        self.feature_dim = feature_dim
        self.activation = activation
        
        # First layer: separate handling for regular features + monotone tau
        self.fc_x = nn.Linear(feature_dim, hidden_dims[0])
        self.tau_weight_raw = nn.Parameter(torch.ones(1, hidden_dims[0]))
        
        self.bias1 = nn.Parameter(torch.zeros(hidden_dims[0]))
        
        # Remaining layers: All monotone
        self.monotone_layers = nn.ModuleList()
        prev_dim = hidden_dims[0]
        for h_dim in hidden_dims[1:]:
            layer = MonotoneLinear(prev_dim, h_dim)
            # Set to small positive
            nn.init.uniform_(layer.weight, a=0.0, b=0.01)
            if layer.bias is not None:
                nn.init.zeros_(layer.bias)
            self.monotone_layers.append(layer)
            prev_dim = h_dim
        
        # Get final output
        self.output_layer = MonotoneLinear(prev_dim, 1)
        nn.init.uniform_(self.output_layer.weight, a=0.0, b=0.005)
        self.output_layer.bias.data.fill_(0.0)
        
    def forward(self, x: torch.Tensor, tau: torch.Tensor) -> torch.Tensor:
        # X will be in <Player, Time, Quantiles, Data> form, move to <Player * Time * Quantiles, Data>
        batchSize, timeSteps, numQuantiles, featureSize = x.shape
        x = x.reshape((batchSize * timeSteps * numQuantiles, featureSize))
        
        tau = tau.reshape((batchSize * timeSteps * numQuantiles, 1))
        # Monotone contribution from tau (guaranteed non-decreasing)
        tau_contrib = tau * F.softplus(self.tau_weight_raw)
        
        # First layer has monotone portion from tau, non-monotone from rest of data
        fcx = self.fc_x(x)
        h = fcx + tau_contrib + self.bias1
        h = self.activation(h)
        
        # Propagate through monotone layers
        for layer in self.monotone_layers:
            h = self.activation(layer(h))
            
        output = self.output_layer(h)
        output = output.reshape(batchSize, timeSteps, numQuantiles)
        return output