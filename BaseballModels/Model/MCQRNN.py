import torch
import torch.nn as nn
import torch.nn.functional as F

class MonotoneLinear(nn.Linear):
    """Linear layer with strictly positive weights (softplus) to preserve monotonicity."""
    def forward(self, input: torch.Tensor) -> torch.Tensor:
        # softplus(weight) > 0 for all elements — exactly like the paper's exp(W)
        positive_weight = F.softplus(self.weight)
        return F.linear(input, positive_weight, self.bias)

# class MonotoneTauModule(nn.Module):
#     def __init__(self, hiddenDims : list[int], activation : nn.Module):
#         super().__init__()
#         self.activation = activation
#         self.layers = nn.ModuleList()
#         prevDim = 1
#         for dim in hiddenDims:
#             self.layers.append(MonotoneLinear(prevDim, dim))
#             prevDim = dim
            
#         self.size = hiddenDims[-1]
        
#     def forward(self, tau : torch.Tensor) -> torch.Tensor:
#         for layer in self.layers:
#             tau = self.activation(layer(tau))
            
#         return tau

# Cannon, A.J. Non-crossing nonlinear regression quantiles by monotone composite quantile regression neural network, with application to rainfall extremes. Stoch Environ Res Risk Assess 32, 3207–3225 (2018). https://doi.org/10.1007/s00477-018-1573-6
class MCQRNN(nn.Module):
    '''Implements MCQRNN module that takes in percentile and input, output strictly increases/stays flat with
    increase in tau
    Activation function MUST be non-decreasing, or this will not guarentee that results grow as tau grows'''
    def __init__(
        self,
        feature_dim : int,
        hidden_dims: list[int],
        tau_dims : list[int],
        activation : nn.Module
    ):
        super().__init__()
        self.feature_dim = feature_dim
        self.activation = activation
        
        # self.tau_network = MonotoneTauModule(tau_dims, F.softplus)
        
        # First layer: separate handling for regular features
        self.fc_x = nn.Linear(feature_dim, hidden_dims[0])
        self.bias1 = nn.Parameter(torch.zeros(hidden_dims[0]))
        
        # Remaining layers: All monotone
        self.monotone_layers = nn.ModuleList()
        prev_dim = hidden_dims[0] + 2
        for h_dim in hidden_dims[1:]:
            layer = MonotoneLinear(prev_dim, h_dim)
            nn.init.uniform_(layer.weight, a=0, b=0.1)
            if layer.bias is not None:
                nn.init.zeros_(layer.bias)
            self.monotone_layers.append(layer)
            prev_dim = h_dim
        
        # Get final output
        self.output_layer = MonotoneLinear(prev_dim, 1)
        nn.init.uniform_(self.output_layer.weight, a=0.0, b=0.1)
        self.output_layer.bias.data.fill_(0.0)
        
    def forward(self, x: torch.Tensor, tau: torch.Tensor) -> torch.Tensor:
        # X will be in <Player, Time, Quantiles, Data> form, move to <Player * Time * Quantiles, Data>
        batchSize, timeSteps, numQuantiles, featureSize = x.shape
        x = x.reshape((batchSize * timeSteps * numQuantiles, featureSize))
        
        tau = tau.reshape((batchSize * timeSteps * numQuantiles, 1))
        # tau_vector = self.tau_network(tau)
        
        # First layer has monotone portion from tau, non-monotone from rest of data
        fcx = self.fc_x(x)
        h = fcx + self.bias1
        h = self.activation(h)
        h = torch.concat((h, tau, -(torch.log(1 - tau))), dim=1)
        
        # Propagate through monotone layers
        for layer in self.monotone_layers:
            h = self.activation(layer(h))
            
        output = self.output_layer(h)
        output = output.reshape(batchSize, timeSteps, numQuantiles)
        return output