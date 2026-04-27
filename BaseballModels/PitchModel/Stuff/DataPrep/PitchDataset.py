import torch
from sklearn.model_selection import train_test_split
from Constants import device
from DBTypes import *

class PitchIO:
    def __init__(self,
        # Data needed to identify in DB
        game_id : int,
        pitch_num : int,
                 
        # Data for model
        data_overview : torch.Tensor,
        data_loc : torch.Tensor,
        data_stuff : torch.Tensor,
        data_pitcher_game : torch.Tensor,
        data_league_avg : torch.Tensor,
        
        # Output for model
        output_value : int,
        output_runs : int,
        output_outs : int,
        output_swung : int,
        output_contact : int,
        output_inplay : int,
    ):
        self.game_id = game_id
        self.pitch_num = pitch_num
        
        self.data_overview = data_overview
        self.data_loc = data_loc
        self.data_stuff = data_stuff
        self.data_pitcher_game = data_pitcher_game
        self.data_league_avg = data_league_avg
        
        self.output_value = output_value
        self.output_runs = output_runs
        self.output_outs = output_outs
        self.output_swung = output_swung
        self.output_contact = output_contact
        self.output_inplay = output_inplay

class PitchDataset(torch.utils.data.Dataset):
    def __init__(self,
                mapping_game_ids : torch.Tensor,
                mapping_pitch_nums : torch.Tensor,
                 
                data_overview : torch.Tensor,
                data_loc : torch.Tensor,
                data_stuff : torch.Tensor,
                data_pitcher_game : torch.Tensor,
                data_league_avg : torch.Tensor,
                
                output_value : torch.Tensor,
                output_runs : torch.Tensor,
                output_outs : torch.Tensor,
                output_swung : torch.Tensor,
                output_contact : torch.Tensor,
                output_inplay : torch.Tensor,):
        
        self.mapping_game_ids = mapping_game_ids
        self.mapping_pitch_nums = mapping_pitch_nums
        
        self.data_overview = data_overview.t().to(device=device, non_blocking=True)
        self.data_loc = data_loc.t().to(device=device, non_blocking=True)
        self.data_stuff = data_stuff.t().to(device=device, non_blocking=True)
        self.data_pitcher_game = data_pitcher_game.t().to(device=device, non_blocking=True)
        self.data_league_avg = data_league_avg.t().to(device=device, non_blocking=True)
        
        self.output_value = output_value.to(device=device, non_blocking=True)
        self.output_runs = output_runs.to(device=device, non_blocking=True)
        self.output_outs = output_outs.to(device=device, non_blocking=True)
        self.output_swung = output_swung.to(device=device, non_blocking=True)
        self.output_contact = output_contact.to(device=device, non_blocking=True)
        self.output_inplay = output_inplay.to(device=device, non_blocking=True)
        
    def __len__(self):
        return self.data_overview.size(dim=0)
    
    def GetEntries(self, batch_indices : torch.Tensor) -> tuple[tuple[torch.Tensor, ...], tuple[torch.Tensor, ...], tuple[torch.Tensor, ...]]:
        # Mappings used to connect data to specific pitch
        mappings = (
            self.mapping_game_ids[batch_indices],
            self.mapping_pitch_nums[batch_indices],
        )
        
        # Data used to feed into model
        data = (
            self.data_overview[batch_indices],
            self.data_loc[batch_indices],
            self.data_stuff[batch_indices],
            self.data_pitcher_game[batch_indices],
            self.data_league_avg[batch_indices]
        )
        
        # Data used to evaluate model
        targets = (
            self.output_value[batch_indices],
            self.output_runs[batch_indices],
            self.output_outs[batch_indices],
            self.output_swung[batch_indices],
            self.output_contact[batch_indices],
            self.output_inplay[batch_indices]
        )
        
        return mappings, data, targets
            
def CreateTestTrainDatasets(data : list[list[PitchIO]], test_size : float, random_state : int) -> tuple[PitchDataset, PitchDataset]:
    # Create Test/Train keeping a player entirely inside of 1 dataset
    if test_size > 0:
        io_train, io_test = train_test_split(data, test_size=test_size, random_state=random_state)
    else:
        io_train = data
        io_test = [data[0]] # Allow for test code to run without breaking, will discard later
    io_train : list[PitchIO] = [item for sublist in io_train for item in sublist]
    io_test : list[PitchIO] = [item for sublist in io_test for item in sublist]

    # =================== MAPPING FEATURES ===================
    mapping_game_ids_train = torch.tensor([io.game_id for io in io_train], dtype=torch.long)
    mapping_pitch_num_train = torch.tensor([io.pitch_num for io in io_train], dtype=torch.long)
    
    mapping_game_ids_test = torch.tensor([io.game_id for io in io_test], dtype=torch.long)
    mapping_pitch_num_test = torch.tensor([io.pitch_num for io in io_test], dtype=torch.long)
    
    # ==================== INPUT FEATURES ====================

    data_overview_train     = torch.stack([io.data_overview     for io in io_train], dim=1)
    data_loc_train          = torch.stack([io.data_loc          for io in io_train], dim=1)
    data_stuff_train        = torch.stack([io.data_stuff        for io in io_train], dim=1)
    data_pitcher_game_train = torch.stack([io.data_pitcher_game for io in io_train], dim=1)
    data_league_avg_train   = torch.stack([io.data_league_avg   for io in io_train], dim=1)

    data_overview_test      = torch.stack([io.data_overview     for io in io_test], dim=1)
    data_loc_test           = torch.stack([io.data_loc          for io in io_test], dim=1)
    data_stuff_test         = torch.stack([io.data_stuff        for io in io_test], dim=1)
    data_pitcher_game_test  = torch.stack([io.data_pitcher_game for io in io_test], dim=1)
    data_league_avg_test    = torch.stack([io.data_league_avg   for io in io_test], dim=1)


    # ==================== TARGETS / OUTPUTS ====================

    output_value_train      = torch.tensor([io.output_value     for io in io_train])
    output_runs_train       = torch.tensor([io.output_runs      for io in io_train])
    output_outs_train       = torch.tensor([io.output_outs      for io in io_train])
    output_swung_train      = torch.tensor([io.output_swung     for io in io_train])
    output_contact_train    = torch.tensor([io.output_contact   for io in io_train])
    output_inplay_train     = torch.tensor([io.output_inplay    for io in io_train])

    output_value_test       = torch.tensor([io.output_value     for io in io_test])
    output_runs_test        = torch.tensor([io.output_runs      for io in io_test])
    output_outs_test        = torch.tensor([io.output_outs      for io in io_test])
    output_swung_test       = torch.tensor([io.output_swung     for io in io_test])
    output_contact_test     = torch.tensor([io.output_contact   for io in io_test])
    output_inplay_test      = torch.tensor([io.output_inplay    for io in io_test])
    
    train_dataset = PitchDataset(
        mapping_game_ids_train,
        mapping_pitch_num_train,
        
        data_overview_train,
        data_loc_train,
        data_stuff_train,
        data_pitcher_game_train,
        data_league_avg_train,
        
        output_value_train,
        output_runs_train,
        output_outs_train,
        output_swung_train,
        output_contact_train,
        output_inplay_train,
    )
    
    test_dataset = PitchDataset(
        mapping_game_ids_test,
        mapping_pitch_num_test,
        
        data_overview_test,
        data_loc_test,
        data_stuff_test,
        data_pitcher_game_test,
        data_league_avg_test,
        
        output_value_test,
        output_runs_test,
        output_outs_test,
        output_swung_test,
        output_contact_test,
        output_inplay_test,
    )
    
    return train_dataset, test_dataset