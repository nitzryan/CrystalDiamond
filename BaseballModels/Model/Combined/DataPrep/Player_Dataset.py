import torch
from Pro.DataPrep.Data_Prep import Player_IO
from College.DataPrep.Data_Prep import College_IO
from Combined.DataPrep.Data_Prep import Combined_IO, Combined_Data_Prep
from sklearn.model_selection import train_test_split # type: ignore

class Combined_Player_Dataset(torch.utils.data.Dataset):
    def __init__(self, 
                pro_data, 
                pro_lengths, 
                pro_output_labels, 
                pro_output_war_values, 
                pro_mask_labels, 
                pro_mask_stats, 
                pro_mask_year, 
                pro_output_stats, 
                pro_output_positions, 
                pro_mask_mlb_value, 
                pro_output_mlb_value, 
                pro_variants_war_class, 
                pro_variants_war_regression, 
                pro_output_pt,
                pro_pt_levelYearGames,
                
                col_bio,
                col_data,
                col_lengths,
                col_output_draft,
                col_output_war,
                col_output_pos,
                col_output_off,
                col_output_def,
                col_output_pa,
                is_hitter : bool,
                col_mask_pos,):
        
        self.is_hitter = is_hitter
        
        self.pro_data = pro_data
        self.pro_lengths = pro_lengths
        
        self.pro_pt_levelYearGames = pro_pt_levelYearGames
        
        self.pro_o_war_buckets = pro_output_labels[:,:,0]
        self.pro_o_level_buckets = pro_output_labels[:,:,1]
        self.pro_o_pa_buckets = pro_output_labels[:,:,2]
        self.pro_o_war_values = pro_output_war_values
        self.pro_o_stats = pro_output_stats
        self.pro_o_positions = pro_output_positions
        self.pro_o_mlb_value = pro_output_mlb_value
        self.pro_o_pt = pro_output_pt
        
        self.pro_m_labels = pro_mask_labels
        self.pro_m_stats = pro_mask_stats
        self.pro_m_year = pro_mask_year
        self.pro_m_mlb_value = pro_mask_mlb_value
        
        self.pro_v_war_class = pro_variants_war_class
        self.pro_v_war_regression = pro_variants_war_regression
        self.pro_v_idx = 0
        self.pro_use_variants = False
        
        self.col_bio=col_bio
        self.col_data=col_data
        self.col_lengths=col_lengths
        self.col_output_draft=col_output_draft
        self.col_output_war=col_output_war
        self.col_output_pos=col_output_pos
        self.col_output_off=col_output_off
        self.col_output_def=col_output_def
        self.col_output_pa=col_output_pa
        self.col_mask_pos=col_mask_pos
    
        self.size_pro = sum(1 for l in self.pro_lengths if l > 0)
        self.size_col = sum(1 for l in self.col_lengths if l > 0)
    
    def __len__(self):
        return self.pro_data.size(dim=1)
    
    def GetProLength(self) -> int:
        return self.size_pro
    
    def GetColLength(self) -> int:
        return self.size_col
    
    def GetProMaxLength(self) -> int:
        return self.pro_data.shape[0]
    
    def GetProInputSize(self) -> int:
        return self.pro_data.shape[-1]
    
    def GetColMaxLength(self) -> int:
        return self.col_data.shape[0]
    
    def GetColInputSize(self) -> int:
        return self.col_data.shape[-1]
    
    # def get_output_size(self) -> int:
    #     return 3
    
    # def get_mask_size(self) -> int:
    #     return 1
    
    def should_augment_data(self, should_augment):
        self.should_augment = should_augment
        
    def should_use_variants(self, should_use : bool):
        self.use_variants = should_use
        
    def increase_variant(self):
        self.v_idx += 1
        if (self.v_idx >= self.v_war_class.size(2)):
            self.v_idx = 0
        
    def __GetProInput__(self, idx : int):
        return self.pro_data[:,idx], self.pro_lengths[idx], self.pro_pt_levelYearGames[:,idx],
        
    def __GetProOutput__(self, idx : int):
        return self.pro_o_war_buckets[:,idx], self.pro_o_level_buckets[:,idx], self.pro_o_pa_buckets[:,idx], self.pro_o_stats[:,idx], self.pro_o_positions[:,idx], self.pro_o_mlb_value[:,idx], self.pro_o_pt[:,idx], self.pro_o_war_values[:,idx]
        
    def __GetProMask__(self, idx : int):
        return (self.pro_m_labels[:,idx], self.pro_m_stats[:,idx], self.pro_m_year[:,idx], self.pro_m_mlb_value[:,idx])
        
    def __GetColInput__(self, idx : int):
        return self.col_data[:,idx], self.col_lengths[idx]
    
    def __GetColOutput__(self, idx : int, is_hitter : bool):
        if is_hitter:
            return self.col_output_draft[idx], self.col_output_war[idx], self.col_output_off[idx], self.col_output_def[idx], self.col_output_pa[idx], self.col_output_pos[idx]
        else:
            return self.col_output_draft[idx], self.col_output_war[idx]
        
    def __GetColMask__(self, idx : int):
        return self.col_mask_pos[idx],
        
    def __getitem__(self, idx):
        # if self.should_use_variants:
        #     return self.__GetProInput__(idx),\
        #         (self.v_war_class[:,idx,self.v_idx], self.o_level_buckets[:,idx], self.o_pa_buckets[:,idx], self.o_stats[:,idx], self.o_positions[:,idx], self.o_mlb_value[:,idx], self.o_pt[:,idx], self.o_war_values[:,idx]), \
        #         self.__GetProMask__(idx),
        # else:
            return self.__GetProInput__(idx), \
                self.__GetProOutput__(idx), \
                self.__GetProMask__(idx), \
                self.__GetColInput__(idx), \
                self.__GetColOutput__(idx, self.is_hitter), \
                self.__GetColMask__(idx)
    
def Create_Test_Train_Datasets(player_list : list[Combined_IO], test_size : float, random_state : int, is_hitter : bool) -> tuple[Combined_Player_Dataset, Combined_Player_Dataset]:
    io_train : list[Combined_IO]
    io_test : list[Combined_IO]
    io_train, io_test = train_test_split(player_list, test_size=test_size, random_state=random_state)

    
    pro_lengths_train = torch.tensor([io.pro_io.length for io in io_train])
    pro_lengths_test = torch.tensor([io.pro_io.length for io in io_test])

    pro_pt_levelYearGames_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.pt_levelYearGames for io in io_train])
    pro_pt_levelYearGames_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.pt_levelYearGames for io in io_test])

    pro_data_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.input for io in io_train])
    pro_data_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.input for io in io_test])
    pro_output_labels_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.output for io in io_train])
    pro_output_labels_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.output for io in io_test])
    pro_output_war_values_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.prospect_value for io in io_train])
    pro_output_war_values_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.prospect_value for io in io_test])
    
    pro_mask_labels_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.prospect_mask for io in io_train])
    pro_mask_labels_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.prospect_mask for io in io_test])
    pro_mask_stats_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.stat_level_mask for io in io_train])
    pro_mask_stats_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.stat_level_mask for io in io_test])

    pro_mask_year_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_level_mask for io in io_train])
    pro_mask_year_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_level_mask for io in io_test])
    pro_output_stats_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_stat_output for io in io_train])
    pro_output_stats_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_stat_output for io in io_test])
    pro_output_pt_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.pt_year_output for io in io_train])
    pro_output_pt_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.pt_year_output for io in io_test])
    pro_output_positions_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_pos_output for io in io_train])
    pro_output_positions_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.year_pos_output for io in io_test])

    pro_mask_mlb_value_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.mlb_value_mask for io in io_train])
    pro_mask_mlb_value_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.mlb_value_mask for io in io_test])
    pro_output_mlb_value_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.mlb_value_stats for io in io_train])
    pro_output_mlb_value_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.mlb_value_stats for io in io_test])

    pro_variants_warclass_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.output_war_class_variants for io in io_train])
    pro_variants_warclass_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.output_war_class_variants for io in io_test])
    pro_variants_warregression_train = torch.nn.utils.rnn.pad_sequence([io.pro_io.output_war_regression_variants for io in io_train])
    pro_variants_warregression_test = torch.nn.utils.rnn.pad_sequence([io.pro_io.output_war_regression_variants for io in io_test])

    col_bio_train = [io.college_io.player for io in io_train]
    col_bio_test = [io.college_io.player for io in io_test]
    
    col_lengths_train = torch.tensor([io.college_io.length for io in io_train])
    col_lengths_test = torch.tensor([io.college_io.length for io in io_test])
    
    col_data_train = torch.nn.utils.rnn.pad_sequence([io.college_io.input for io in io_train])
    col_data_test = torch.nn.utils.rnn.pad_sequence([io.college_io.input for io in io_test])
    
    col_output_draft_train = torch.tensor([io.college_io.output_draft for io in io_train])
    col_output_draft_test = torch.tensor([io.college_io.output_draft for io in io_test])
    
    col_output_war_train = torch.tensor([io.college_io.output_war for io in io_train])
    col_output_war_test = torch.tensor([io.college_io.output_war for io in io_test])
    
    if is_hitter:
        col_output_off_train = torch.tensor([io.college_io.output_off for io in io_train])
        col_output_off_test = torch.tensor([io.college_io.output_off for io in io_test])
        col_output_def_train = torch.tensor([io.college_io.output_def for io in io_train])
        col_output_def_test = torch.tensor([io.college_io.output_def for io in io_test])
        col_output_pa_train = torch.tensor([io.college_io.output_pa for io in io_train])
        col_output_pa_test = torch.tensor([io.college_io.output_pa for io in io_test])
    else:
        col_output_off_train = None
        col_output_off_test = None
        col_output_def_train = None
        col_output_def_test = None
        col_output_pa_train = None
        col_output_pa_test = None
    
    col_output_pos_train = torch.stack([io.college_io.output_pos for io in io_train])
    col_output_pos_test = torch.stack([io.college_io.output_pos for io in io_test])
    col_mask_pos_train = torch.tensor([io.college_io.mask_pos for io in io_train])
    col_mask_pos_test = torch.tensor([io.college_io.mask_pos for io in io_test])

    train_dataset = Combined_Player_Dataset(
        pro_data = pro_data_train, 
        pro_lengths = pro_lengths_train, 
        pro_output_labels = pro_output_labels_train, 
        pro_output_war_values = pro_output_war_values_train, 
        pro_mask_labels = pro_mask_labels_train, 
        pro_mask_stats = pro_mask_stats_train, 
        pro_mask_year = pro_mask_year_train, 
        pro_output_stats = pro_output_stats_train, 
        pro_output_positions = pro_output_positions_train, 
        pro_mask_mlb_value = pro_mask_mlb_value_train, 
        pro_output_mlb_value = pro_output_mlb_value_train, 
        pro_variants_war_class = pro_variants_warclass_train, 
        pro_variants_war_regression = pro_variants_warregression_train, 
        pro_output_pt = pro_output_pt_train,
        pro_pt_levelYearGames= pro_pt_levelYearGames_train,
        
        col_bio=col_bio_train,
        col_data=col_data_train, 
        col_lengths=col_lengths_train, 
        col_output_draft=col_output_draft_train,
        col_output_war=col_output_war_train,
        col_output_off=col_output_off_train,
        col_output_def=col_output_def_train,
        col_output_pa=col_output_pa_train,
        col_output_pos=col_output_pos_train,
        col_mask_pos=col_mask_pos_train,
        is_hitter=is_hitter)
    
    test_dataset = Combined_Player_Dataset(
        pro_data = pro_data_test, 
        pro_lengths = pro_lengths_test, 
        pro_output_labels = pro_output_labels_test, 
        pro_output_war_values = pro_output_war_values_test, 
        pro_mask_labels = pro_mask_labels_test, 
        pro_mask_stats = pro_mask_stats_test, 
        pro_mask_year = pro_mask_year_test, 
        pro_output_stats = pro_output_stats_test, 
        pro_output_positions = pro_output_positions_test, 
        pro_mask_mlb_value = pro_mask_mlb_value_test, 
        pro_output_mlb_value = pro_output_mlb_value_test, 
        pro_variants_war_class = pro_variants_warclass_test, 
        pro_variants_war_regression = pro_variants_warregression_test, 
        pro_output_pt = pro_output_pt_test,
        pro_pt_levelYearGames= pro_pt_levelYearGames_test,
        
        col_bio=col_bio_test,
        col_data=col_data_test, 
        col_lengths=col_lengths_test, 
        col_output_draft=col_output_draft_test,
        col_output_war=col_output_war_test,
        col_output_off=col_output_off_test,
        col_output_def=col_output_def_test,
        col_output_pa=col_output_pa_test,
        col_output_pos=col_output_pos_test,
        col_mask_pos=col_mask_pos_test,
        is_hitter=is_hitter)
    
    train_dataset.should_use_variants(True)
    test_dataset.should_use_variants(False)
    
    return train_dataset, test_dataset