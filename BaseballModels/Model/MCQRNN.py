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
        
        # Layers that will transform tau into a vector, multiple layers for nonlinearity
        # Techinically this could break monotonicity, but practically it shouldn't, and
        # if it does, it would break it for all outputs so easily noticeable
        self.tau_net = nn.ModuleList()
        tau_dim = 1
        for h in [16, 8]:
            layer = MonotoneLinear(tau_dim, h)
            nn.init.uniform_(layer.weight, 0.0, 0.1)
            nn.init.zeros_(layer.bias)
            self.tau_net.append(layer)
            tau_dim = h
    
        self.tau_net.append(MonotoneLinear(tau_dim, 1))
        
        # First layer: separate handling for regular features
        self.fc_x = nn.Linear(feature_dim, hidden_dims[0])
        
        self.bias1 = nn.Parameter(torch.zeros(hidden_dims[0]))
        
        # Remaining layers: All monotone
        self.monotone_layers = nn.ModuleList()
        prev_dim = hidden_dims[0] + 1 # 1 from tau
        for h_dim in hidden_dims[1:]:
            layer = nn.Linear(prev_dim, h_dim)
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
        # Monotone contribution from tau (guaranteed non-decreasing)
        # for layer in self.tau_net:
        #     tau = self.activation(layer(tau))
        
        # First layer has monotone portion from tau, non-monotone from rest of data
        fcx = self.fc_x(x)
        h = fcx + self.bias1
        h = self.activation(h)
        h = torch.concat((h, tau), dim=1)
        
        # Propagate through monotone layers
        for layer in self.monotone_layers:
            h = self.activation(layer(h))
            
        output = self.output_layer(h)
        output = output.reshape(batchSize, timeSteps, numQuantiles)
        return output