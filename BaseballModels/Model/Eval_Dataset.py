import torch

class Eval_Dataset(torch.utils.data.Dataset):
    def __init__(self, data : torch.Tensor, lengths : torch.Tensor, dates : torch.Tensor):
        self.data = data
        self.lengths = lengths
        self.dates = dates
        
    def __len__(self):
        return self.data.size(dim=1)
    
    def __getitem__(self, idx : int):
        return self.data[:,idx], self.lengths[idx], self.dates[:,idx]