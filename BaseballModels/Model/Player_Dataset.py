import torch
from Data_Prep import Player_IO
from sklearn.model_selection import train_test_split # type: ignore

class Player_Dataset(torch.utils.data.Dataset):
    def __init__(self, data, lengths, labels, war_values, mask_labels, mask_stats, year_mask, year_stats, year_positions, mlb_value_mask, mlb_value_stats):
        self.data = data
        self.lengths = lengths
        
        self.twar_buckets = labels[:,:,0]
        self.level_buckets = labels[:,:,1]
        self.pa_buckets = labels[:,:,2]
        
        self.war_values = war_values
        
        self.mask_labels = mask_labels
        self.mask_stats = mask_stats
        
        self.year_mask = year_mask
        self.year_stats = year_stats
        self.year_positions = year_positions
        
        self.mlb_value_mask = mlb_value_mask
        self.mlb_value_stats = mlb_value_stats
    
    def __len__(self):
        return self.data.size(dim=1)
    
    def get_input_size(self) -> int:
        return self.data.shape[2]
    
    def should_augment_data(self, should_augment):
        self.should_augment = should_augment
        
    def __getitem__(self, idx):
        return self.data[:,idx], self.lengths[idx], self.twar_buckets[:,idx], self.war_values[:,idx], self.level_buckets[:,idx], self.pa_buckets[:,idx], self.mask_labels[:,idx], self.mask_stats[:,idx], self.year_mask[:,idx], self.year_stats[:,idx], self.year_positions[:,idx], self.mlb_value_mask[:,idx], self.mlb_value_stats[:,idx]
    
def Create_Test_Train_Datasets(player_list : list[Player_IO], test_size : float, random_state : int) -> tuple[Player_Dataset, Player_Dataset]:
    io_train : list[Player_IO]
    io_test : list[Player_IO]
    io_train, io_test = train_test_split(player_list, test_size=test_size, random_state=random_state)

    train_lengths = torch.tensor([io.length for io in io_train])
    test_lengths = torch.tensor([io.length for io in io_test])

    x_train_padded = torch.nn.utils.rnn.pad_sequence([io.input for io in io_train])
    x_test_padded = torch.nn.utils.rnn.pad_sequence([io.input for io in io_test])
    y_prospect_train_padded = torch.nn.utils.rnn.pad_sequence([io.output for io in io_train])
    y_prospect_test_padded = torch.nn.utils.rnn.pad_sequence([io.output for io in io_test])
    y_prospect_value_train_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_value for io in io_train])
    y_prospect_value_test_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_value for io in io_test])
    
    mask_prospect_train_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_train])
    mask_prospect_test_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_test])
    mask_level_train_padded = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_train])
    mask_level_test_padded = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_test])

    mask_year_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_train])
    mask_year_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_test])
    y_year_stats_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_train])
    y_year_stats_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_test])
    y_year_position_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_train])
    y_year_position_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_test])

    mlb_value_mask_train_padded = torch.nn.utils.rnn.pad_sequence([io.mlb_value_mask for io in io_train])
    mlb_value_mask_test_padded = torch.nn.utils.rnn.pad_sequence([io.mlb_value_mask for io in io_test])
    mlb_value_stats_train_padded = torch.nn.utils.rnn.pad_sequence([io.mlb_value_stats for io in io_train])
    mlb_value_stats_test_padded = torch.nn.utils.rnn.pad_sequence([io.mlb_value_stats for io in io_test])

    train_dataset = Player_Dataset(x_train_padded, train_lengths, y_prospect_train_padded, y_prospect_value_train_padded, mask_prospect_train_padded, mask_level_train_padded, mask_year_train_padded, y_year_stats_train_padded, y_year_position_train_padded, mlb_value_mask_train_padded, mlb_value_stats_train_padded)
    test_dataset = Player_Dataset(x_test_padded, test_lengths, y_prospect_test_padded, y_prospect_value_test_padded, mask_prospect_test_padded, mask_level_test_padded, mask_year_test_padded, y_year_stats_test_padded, y_year_position_test_padded, mlb_value_mask_test_padded, mlb_value_stats_test_padded)
    
    return train_dataset, test_dataset