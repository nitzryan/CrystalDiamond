import torch
from Data_Prep import Player_IO
from sklearn.model_selection import train_test_split # type: ignore

class Player_Dataset(torch.utils.data.Dataset):
    def __init__(self, 
                 data, 
                 lengths, 
                 output_labels, 
                 output_war_values, 
                 mask_labels, 
                 mask_stats, 
                 mask_year, 
                 output_stats, 
                 output_positions, 
                 mask_mlb_value, 
                 output_mlb_value, 
                 variants_war_class, 
                 variants_war_regression, 
                 output_pt):
        
        self.data = data
        self.lengths = lengths
        
        self.o_war_buckets = output_labels[:,:,0]
        self.o_level_buckets = output_labels[:,:,1]
        self.o_pa_buckets = output_labels[:,:,2]
        self.o_war_values = output_war_values
        self.o_stats = output_stats
        self.o_positions = output_positions
        self.o_mlb_value = output_mlb_value
        self.o_pt = output_pt
        
        self.m_labels = mask_labels
        self.m_stats = mask_stats
        self.m_year = mask_year
        self.m_mlb_value = mask_mlb_value
        
        self.v_war_class = variants_war_class
        self.v_war_regression = variants_war_regression
        self.v_idx = 0
        self.use_variants = False
    
    def __len__(self):
        return self.data.size(dim=1)
    
    def get_input_size(self) -> int:
        return self.data.shape[2]
    
    def should_augment_data(self, should_augment):
        self.should_augment = should_augment
        
    def should_use_variants(self, should_use : bool):
        self.use_variants = should_use
        
    def increase_variant(self):
        self.v_idx += 1
        if (self.v_idx >= self.v_war_class.size(2)):
            self.v_idx = 0
        
    def __getitem__(self, idx):
        if self.should_use_variants:
            return self.data[:,idx], self.lengths[idx], \
                (self.v_war_class[:,idx,self.v_idx], self.o_level_buckets[:,idx], self.o_pa_buckets[:,idx], self.v_war_regression[:,idx,self.v_idx], self.o_stats[:,idx], self.o_positions[:,idx], self.o_mlb_value[:,idx], self.o_pt[:,idx]), \
                (self.m_labels[:,idx], self.m_stats[:,idx], self.m_year[:,idx], self.m_mlb_value[:,idx])
        else:
            return self.data[:,idx], self.lengths[idx], \
                (self.o_war_buckets[:,idx], self.o_level_buckets[:,idx], self.o_pa_buckets[:,idx], self.o_war_values[:,idx], self.o_year_stats[:,idx], self.o_year_positions[:,idx], self.o_mlb_value[:,idx], self.o_pt[:,idx]), \
                (self.m_labels[:,idx], self.m_stats[:,idx], self.m_year[:,idx], self.m_mlb_value[:,idx])
    
def Create_Test_Train_Datasets(player_list : list[Player_IO], test_size : float, random_state : int) -> tuple[Player_Dataset, Player_Dataset]:
    io_train : list[Player_IO]
    io_test : list[Player_IO]
    io_train, io_test = train_test_split(player_list, test_size=test_size, random_state=random_state)

    lengths_train = torch.tensor([io.length for io in io_train])
    lengths_test = torch.tensor([io.length for io in io_test])

    data_train = torch.nn.utils.rnn.pad_sequence([io.input for io in io_train])
    data_test = torch.nn.utils.rnn.pad_sequence([io.input for io in io_test])
    output_labels_train = torch.nn.utils.rnn.pad_sequence([io.output for io in io_train])
    output_labels_test = torch.nn.utils.rnn.pad_sequence([io.output for io in io_test])
    output_war_values_train = torch.nn.utils.rnn.pad_sequence([io.prospect_value for io in io_train])
    output_war_values_test = torch.nn.utils.rnn.pad_sequence([io.prospect_value for io in io_test])
    
    mask_labels_train = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_train])
    mask_labels_test = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_test])
    mask_stats_train = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_train])
    mask_stats_test = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_test])

    mask_year_train = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_train])
    mask_year_test = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_test])
    output_stats_train = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_train])
    output_stats_test = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_test])
    output_pt_train = torch.nn.utils.rnn.pad_sequence([io.pt_year_output for io in io_train])
    output_pt_test = torch.nn.utils.rnn.pad_sequence([io.pt_year_output for io in io_test])
    output_positions_train = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_train])
    output_positions_test = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_test])

    mask_mlb_value_train = torch.nn.utils.rnn.pad_sequence([io.mlb_value_mask for io in io_train])
    mask_mlb_value_test = torch.nn.utils.rnn.pad_sequence([io.mlb_value_mask for io in io_test])
    output_mlb_value_train = torch.nn.utils.rnn.pad_sequence([io.mlb_value_stats for io in io_train])
    output_mlb_value_test = torch.nn.utils.rnn.pad_sequence([io.mlb_value_stats for io in io_test])

    variants_warclass_train = torch.nn.utils.rnn.pad_sequence([io.output_war_class_variants for io in io_train])
    variants_warclass_test = torch.nn.utils.rnn.pad_sequence([io.output_war_class_variants for io in io_test])
    variants_warregression_train = torch.nn.utils.rnn.pad_sequence([io.output_war_regression_variants for io in io_train])
    variants_warregression_test = torch.nn.utils.rnn.pad_sequence([io.output_war_regression_variants for io in io_test])

    train_dataset = Player_Dataset(data = data_train, 
                 lengths = lengths_train, 
                 output_labels = output_labels_train, 
                 output_war_values = output_war_values_train, 
                 mask_labels = mask_labels_train, 
                 mask_stats = mask_stats_train, 
                 mask_year = mask_year_train, 
                 output_stats = output_stats_train, 
                 output_positions = output_positions_train, 
                 mask_mlb_value = mask_mlb_value_train, 
                 output_mlb_value = output_mlb_value_train, 
                 variants_war_class = variants_warclass_train, 
                 variants_war_regression = variants_warregression_train, 
                 output_pt = output_pt_train)
    
    test_dataset = Player_Dataset(data = data_test, 
                 lengths = lengths_test, 
                 output_labels = output_labels_test, 
                 output_war_values = output_war_values_test, 
                 mask_labels = mask_labels_test, 
                 mask_stats = mask_stats_test, 
                 mask_year = mask_year_test, 
                 output_stats = output_stats_test, 
                 output_positions = output_positions_test, 
                 mask_mlb_value = mask_mlb_value_test, 
                 output_mlb_value = output_mlb_value_test, 
                 variants_war_class = variants_warclass_test, 
                 variants_war_regression = variants_warregression_test, 
                 output_pt = output_pt_test)
    
    train_dataset.should_use_variants(True)
    test_dataset.should_use_variants(False)
    
    return train_dataset, test_dataset