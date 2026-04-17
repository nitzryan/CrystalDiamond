import torch
from sklearn.model_selection import train_test_split

class PitchIO:
    def __init__(self,
        data_overview : torch.Tensor,
        data_loc : torch.Tensor,
        data_stuff : torch.Tensor,
        
        output_value : int,
        output_runs : int,
        output_outs : int,
        output_swung : int,
        output_contact : int,
        output_inplay : int,
    ):
        
        self.data_overview = data_overview
        self.data_loc = data_loc
        self.data_stuff = data_stuff
        
        self.output_value = output_value
        self.output_runs = output_runs
        self.output_outs = output_outs
        self.output_swung = output_swung
        self.output_contact = output_contact
        self.output_inplay = output_inplay

class PitchDataset(torch.utils.data.Dataset):
    def __init__(self,
                data_overview : torch.Tensor,
                data_loc : torch.Tensor,
                data_stuff : torch.Tensor,
                
                output_value : torch.Tensor,
                output_runs : torch.Tensor,
                output_outs : torch.Tensor,
                output_swung : torch.Tensor,
                output_contact : torch.Tensor,
                output_inplay : torch.Tensor,):
        
        self.data_overview = data_overview
        self.data_loc = data_loc
        self.data_stuff = data_stuff
        
        self.output_value = output_value
        self.output_runs = output_runs
        self.output_outs = output_outs
        self.output_swung = output_swung
        self.output_contact = output_contact
        self.output_inplay = output_inplay
        
    def __len__(self):
        return self.data_overview.size(dim=1)
    
    def __getitem__(self, idx):
        return (self.data_overview[:,idx], self.data_loc[:,idx], self.data_stuff[:,idx]), \
            (self.output_value[idx], self.output_runs[idx], self.output_outs[idx], self.output_swung[idx], self.output_contact[idx], self.output_inplay[idx])
            
def CreateTestTrainDatasets(data : list[PitchIO], test_size : float, random_state : int) -> tuple[PitchDataset, PitchDataset]:
    io_train, io_test = train_test_split(data, test_size=test_size, random_state=random_state)
    
    data_overview_train = torch.stack([io.data_overview for io in io_train], dim=1)
    data_loc_train      = torch.stack([io.data_loc      for io in io_train], dim=1)
    data_stuff_train    = torch.stack([io.data_stuff    for io in io_train], dim=1)
    
    data_overview_test  = torch.stack([io.data_overview for io in io_test], dim=1)
    data_loc_test       = torch.stack([io.data_loc      for io in io_test], dim=1)
    data_stuff_test     = torch.stack([io.data_stuff    for io in io_test], dim=1)
    
    output_value_train  = torch.tensor([io.output_value  for io in io_train])
    output_runs_train   = torch.tensor([io.output_runs   for io in io_train])
    output_outs_train   = torch.tensor([io.output_outs   for io in io_train])
    output_swung_train  = torch.tensor([io.output_swung  for io in io_train])
    output_contact_train = torch.tensor([io.output_contact for io in io_train])
    output_inplay_train = torch.tensor([io.output_inplay for io in io_train])
    
    output_value_test   = torch.tensor([io.output_value  for io in io_test])
    output_runs_test    = torch.tensor([io.output_runs   for io in io_test])
    output_outs_test    = torch.tensor([io.output_outs   for io in io_test])
    output_swung_test   = torch.tensor([io.output_swung  for io in io_test])
    output_contact_test = torch.tensor([io.output_contact for io in io_test])
    output_inplay_test  = torch.tensor([io.output_inplay for io in io_test])
    
    train_dataset = PitchDataset(
        data_overview_train,
        data_loc_train,
        data_stuff_train,
        output_value_train,
        output_runs_train,
        output_outs_train,
        output_swung_train,
        output_contact_train,
        output_inplay_train,
    )
    
    test_dataset = PitchDataset(
        data_overview_test,
        data_loc_test,
        data_stuff_test,
        output_value_test,
        output_runs_test,
        output_outs_test,
        output_swung_test,
        output_contact_test,
        output_inplay_test,
    )
    
    return train_dataset, test_dataset