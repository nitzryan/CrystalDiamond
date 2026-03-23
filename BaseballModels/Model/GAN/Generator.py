import torch
import torch.nn as nn

class Generator(nn.Module):
    def __init__(self, latent_dim : int = 100, hidden_size : int = 150, 
                 feature_size : int = 2, output_size : int = 5, 
                 num_layers : int = 2, max_len = 100):
        
        super().__init__()
        self.max_len = max_len
        self.latent_dim = latent_dim
        
        # Model Architecturre
        self.recurrent = nn.LSTM(feature_size + output_size, hidden_size, num_layers, batch_first=True, bidirectional=True)
        self.recurrent_project = nn.Linear(2 * hidden_size, feature_size)
        
        # Noise to hidden state + initial token
        self.init_hidden = nn.Linear(latent_dim + output_size, hidden_size * num_layers * 4)
        self.init_token = nn.Linear(latent_dim + output_size, feature_size)
        
    def forward(self, rand_noise : torch.Tensor, targets : torch.Tensor, masks : torch.Tensor, time_steps : int, lengths : torch.Tensor):
        batch_size = rand_noise.size(0)
        
        y = torch.cat((targets[:,0,:], masks[:,0,:]), dim=-1)
        z = torch.cat((rand_noise, y), dim=1)
        # Initial hidden & cell
        h0c0 = self.init_hidden(z)
        h0c0 = h0c0.view(self.recurrent.num_layers * 4, batch_size, self.recurrent.hidden_size)
        h0 = h0c0[:self.recurrent.num_layers * 2]
        c0 = h0c0[self.recurrent.num_layers * 2:]
        
        # First Token
        x = self.init_token(z).unsqueeze(1)
        
        # Iterate through sequence
        outputs = []
        hidden = (h0, c0)
        y = y.reshape(y.shape[0], 1, y.shape[1])
        for i in range(time_steps):
            x = torch.cat((x, y), dim=-1)
            x, hidden = self.recurrent(x, hidden)
            x = self.recurrent_project(x)
            outputs.append(x)
            
        outputs = torch.cat(outputs, dim=1)
        
        # Zero out all values above lengths
        arange = torch.arange(time_steps, device=outputs.device).unsqueeze(0)
        lengths_mask = (arange < lengths.unsqueeze(-1)).unsqueeze(-1).float()
        outputs = outputs * lengths_mask
        
        return outputs