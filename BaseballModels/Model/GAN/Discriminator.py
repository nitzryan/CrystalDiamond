import torch
import torch.nn as nn

class Discriminator(nn.Module):
    def __init__(self, output_size : int, feature_size : int, 
                 hidden_size : int = 200, num_layers : int = 2):
        super().__init__()
        
        self.recurrent = nn.LSTM(feature_size + output_size, hidden_size, num_layers, 
                            batch_first=True, bidirectional=True)
        
        self.prediction_layer = nn.Linear(hidden_size * 2, 1)
        
    def forward(self, data : torch.Tensor, lengths : torch.Tensor, targets : torch.Tensor, masks : torch.Tensor):
        # Evaluate at each state
        lengths = lengths.to(torch.device("cpu")).long()
        
        batch_size = data.shape[0]
        time_steps = data.shape[1]
        targets = targets.reshape(batch_size, time_steps, targets.shape[1] // time_steps)
        masks = masks.reshape(batch_size, time_steps, masks.shape[1] // time_steps)
        
        y = torch.cat((targets, masks), dim=-1)
        
        x = torch.cat((data, y), dim=-1)
        packedInput = nn.utils.rnn.pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        packedOutput, _ = self.recurrent(packedInput)
        output, _ = nn.utils.rnn.pad_packed_sequence(packedOutput, batch_first=True)
        
        output_predictions = self.prediction_layer(output)
        
        return torch.sigmoid(output_predictions)