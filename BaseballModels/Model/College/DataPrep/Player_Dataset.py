import torch
from DBTypes import *
from College.DataPrep.Data_Prep import College_IO
from sklearn.model_selection import train_test_split # type: ignore

class College_Player_Dataset(torch.utils.data.Dataset):
    def __init__(self,
                 bio : list[DB_College_Player],
                 data,
                 lengths,
                 output_draft,
                 output_war,
                 output_pos,
                 output_off,
                 output_def,
                 output_pa,
                 is_hitter : bool,
                 mask_pos,):
        
        self.bio = bio
        self.data = data
        self.lengths = lengths
        self.output_draft = output_draft
        self.output_war = output_war
        self.output_pos = output_pos
        self.output_off = output_off
        self.output_def = output_def
        self.output_pa = output_pa
        self.mask_pos = mask_pos
        self.is_hitter = is_hitter
        
    def __len__(self):
        return self.data.size(1)
    
    def get_max_length(self) -> int:
        return self.data.shape[0]
    
    def get_input_size(self) -> int:
        return self.data.shape[-1]
    
    def __getitem__(self, idx):
        if self.is_hitter:
            return self.data[:, idx], \
                self.lengths[idx], \
                (self.output_draft[idx], self.output_war[idx], self.output_off[idx], self.output_def[idx], self.output_pa[idx], self.output_pos[idx]), \
                self.mask_pos[idx]
        else:
            return self.data[:, idx], \
                self.lengths[idx], \
                (self.output_draft[idx], self.output_war[idx], self.output_pos[idx]), \
                self.mask_pos[idx]
    
def Create_Test_Train_Datasets(player_list : list[College_IO], test_size : float, random_state : int, is_hitter : bool) -> tuple[College_Player_Dataset, College_Player_Dataset]:
    io_train : list[College_IO]
    io_test : list[College_IO]
    io_train, io_test = train_test_split(player_list, test_size=test_size, random_state=random_state)
    
    bio_train = [io.player for io in io_train]
    bio_test = [io.player for io in io_test]
    
    lengths_train = torch.tensor([io.length for io in io_train])
    lengths_test = torch.tensor([io.length for io in io_test])
    
    data_train = torch.nn.utils.rnn.pad_sequence([io.input for io in io_train])
    data_test = torch.nn.utils.rnn.pad_sequence([io.input for io in io_test])
    
    output_draft_train = torch.tensor([io.output_draft for io in io_train])
    output_draft_test = torch.tensor([io.output_draft for io in io_test])
    
    output_war_train = torch.tensor([io.output_war for io in io_train])
    output_war_test = torch.tensor([io.output_war for io in io_test])
    
    if is_hitter:
        output_off_train = torch.tensor([io.output_off for io in io_train])
        output_off_test = torch.tensor([io.output_off for io in io_test])
        output_def_train = torch.tensor([io.output_def for io in io_train])
        output_def_test = torch.tensor([io.output_def for io in io_test])
        output_pa_train = torch.tensor([io.output_pa for io in io_train])
        output_pa_test = torch.tensor([io.output_pa for io in io_test])
    else:
        output_off_train = None
        output_off_test = None
        output_def_train = None
        output_def_test = None
        output_pa_train = None
        output_pa_test = None
    
    output_pos_train = torch.stack([io.output_pos for io in io_train])
    output_pos_test = torch.stack([io.output_pos for io in io_test])
    mask_pos_train = torch.tensor([io.mask_pos for io in io_train])
    mask_pos_test = torch.tensor([io.mask_pos for io in io_test])
    
    train_dataset = College_Player_Dataset(bio=bio_train, 
        data=data_train, 
        lengths=lengths_train, 
        output_draft=output_draft_train,
        output_war=output_war_train,
        output_off=output_off_train,
        output_def=output_def_train,
        output_pa=output_pa_train,
        output_pos=output_pos_train,
        mask_pos=mask_pos_train,
        is_hitter=is_hitter)
    test_dataset = College_Player_Dataset(bio=bio_test, 
        data=data_test, 
        lengths=lengths_test, 
        output_draft=output_draft_test,
        output_war=output_war_test,
        output_off=output_off_test,
        output_def=output_def_test,
        output_pa=output_pa_test,
        output_pos=output_pos_test,
        mask_pos=mask_pos_test,
        is_hitter=is_hitter)
    
    return (train_dataset, test_dataset)