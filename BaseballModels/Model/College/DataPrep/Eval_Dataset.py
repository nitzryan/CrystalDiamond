import torch
from College.DataPrep.Data_Prep import College_IO

class College_Eval_Dataset(torch.utils.data.Dataset):
    def __init__(self, players : list[College_IO]):
        self.lengths = torch.tensor([p.length for p in players])
        self.dates = torch.nn.utils.rnn.pad_sequence([p.dates for p in players])
        self.data = torch.nn.utils.rnn.pad_sequence([p.input for p in players])
        
    def __len__(self):
        return self.data.size(dim=1)
    
    def GetInputSize(self) -> int:
        return self.data.size(dim=-1)
    
    def __getitem__(self, idx : int):
        return self.data[:,idx], self.lengths[idx], self.dates[:,idx]