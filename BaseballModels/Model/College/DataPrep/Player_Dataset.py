import torch
from College.DataPrep.Data_Prep import College_IO
from sklearn.model_selection import train_test_split # type: ignore

class College_Player_Dataset(torch.utils.data.Dataset):
    def __init__(self,
                 data,
                 lengths,
                 output_draft):
        
        self.data = data
        self.lengths = lengths
        self.output_draft = output_draft
        
    def __len__(self):
        return self.data.size(1)
    
    def get_max_length(self) -> int:
        return self.data.shape[0]
    
    def get_input_size(self) -> int:
        return self.data.shape[-1]
    
    def __getitem__(self, idx):
        return self.data[:, idx], self.lengths[idx], self.output_draft[:, idx]
    
def Create_Test_Train_Datasets(player_list : list[College_IO], test_size : float, random_state : int) -> tuple[College_Player_Dataset, College_Player_Dataset]:
    io_train : list[College_IO]
    io_test : list[College_IO]
    io_train, io_test = train_test_split(player_list, test_size=test_size, random_state=random_state)
    
    lengths_train = torch.tensor([io.length for io in io_train])
    lengths_test = torch.tensor([io.length for io in io_test])
    
    data_train = torch.nn.utils.rnn.pad_sequence([io.input for io in io_train])
    data_test = torch.nn.utils.rnn.pad_sequence([io.input for io in io_test])
    
    output_draft_train = torch.nn.utils.rnn.pad_sequence([io.output_draft for io in io_train])
    output_draft_test = torch.nn.utils.rnn.pad_sequence([io.output_draft for io in io_test])
    
    train_dataset = College_Player_Dataset(data_train, lengths_train, output_draft_train)
    test_dataset = College_Player_Dataset(data_test, lengths_test, output_draft_test)
    
    return (train_dataset, test_dataset)