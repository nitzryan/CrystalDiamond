import torch
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
        data_combined : torch.Tensor,
        data_pitcher_game : torch.Tensor,
        data_league_avg : torch.Tensor,
        
        # Output for model
        output_type : int,
        output_swing : int,
        output_inplay : int,
        
        # Masks for model
        mask_swing : float,
        mask_inplay : float
    ):
        self.game_id = game_id
        self.pitch_num = pitch_num
        
        self.data_overview = data_overview
        self.data_loc = data_loc
        self.data_stuff = data_stuff
        self.data_combined = data_combined
        self.data_pitcher_game = data_pitcher_game
        self.data_league_avg = data_league_avg
        
        self.output_type = output_type
        self.output_swing = output_swing
        self.output_inplay = output_inplay
        
        self.mask_swing = mask_swing
        self.mask_inplay = mask_inplay

def split_sublist(
    sublist: list,  # list[DATA_CLASS] for one class
    N: int,         # train points per test point
    M: int,         # total number of training runs
    C: int,         # current iteration
) -> tuple[list, list]:
    train = []
    test = []
    N = N + 1
    for i, item in enumerate(sublist):
        g = i % M
        if (g % N) == (C % N):
            test.append(item)
        else:
            train.append(item)
    
    return train, test

class PitchDataset(torch.utils.data.Dataset):
    def __init__(self,
                mapping_game_ids : torch.Tensor,
                mapping_pitch_nums : torch.Tensor,
                 
                data_overview : torch.Tensor,
                data_loc : torch.Tensor,
                data_stuff : torch.Tensor,
                data_combined : torch.Tensor,
                data_pitcher_game : torch.Tensor,
                data_league_avg : torch.Tensor,
                
                output_type : torch.Tensor,
                output_swing : torch.Tensor,
                output_inplay : torch.Tensor,
                
                mask_swing : torch.Tensor,
                mask_inplay : torch.Tensor,
                
                dataset_device = device
                ):
        
        self.mapping_game_ids = mapping_game_ids
        self.mapping_pitch_nums = mapping_pitch_nums
        
        self.data_overview = data_overview.t().to(device=dataset_device, non_blocking=True)
        self.data_loc = data_loc.t().to(device=dataset_device, non_blocking=True)
        self.data_stuff = data_stuff.t().to(device=dataset_device, non_blocking=True)
        self.data_combined = data_combined.t().to(device=dataset_device, non_blocking=True)
        self.data_pitcher_game = data_pitcher_game.t().to(device=dataset_device, non_blocking=True)
        self.data_league_avg = data_league_avg.t().to(device=dataset_device, non_blocking=True)
        
        self.output_type = output_type.to(device=dataset_device, non_blocking=True)
        self.output_swing = output_swing.to(device=dataset_device, non_blocking=True)
        self.output_inplay = output_inplay.to(device=dataset_device, non_blocking=True)
        
        self.mask_swing = mask_swing.to(device=dataset_device, non_blocking=True)
        self.mask_inplay = mask_inplay.to(device=dataset_device, non_blocking=True)
        
    def __len__(self):
        return self.data_overview.size(dim=0)
    
    def GetEntries(self, batch_indices : torch.Tensor) -> tuple[tuple[torch.Tensor, ...], tuple[torch.Tensor, ...], tuple[torch.Tensor, ...], tuple[torch.Tensor, ...]]:
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
            self.data_combined[batch_indices],
            self.data_pitcher_game[batch_indices],
            self.data_league_avg[batch_indices]
        )
        
        # Data used to evaluate model
        targets = (
            self.output_type[batch_indices],
            self.output_swing[batch_indices],
            self.output_inplay[batch_indices],
        )
        
        # Masks to turn parts of model on/off
        masks = (
            torch.ones(batch_indices.shape[0], dtype=torch.float, device=self.mask_inplay.device),
            self.mask_swing[batch_indices],
            self.mask_inplay[batch_indices],
        )
        
        return mappings, data, targets, masks
            
def CreateTestTrainDatasets(
    data : list[list[PitchIO]], 
    dataset_device = device,
    eval_mode : bool = False,
    train_test_ratio : int = 3,
    total_training_runs : int = 4,
    train_idx : int = 0) -> tuple[PitchDataset, PitchDataset]:
    
    
    # Create Test/Train keeping a player entirely inside of 1 dataset
    if not eval_mode:
        io_train, io_test = split_sublist(data, train_test_ratio, total_training_runs, train_idx)
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
    data_combined_train     = torch.stack([io.data_combined     for io in io_train], dim=1)
    data_pitcher_game_train = torch.stack([io.data_pitcher_game for io in io_train], dim=1)
    data_league_avg_train   = torch.stack([io.data_league_avg   for io in io_train], dim=1)

    data_overview_test      = torch.stack([io.data_overview     for io in io_test], dim=1)
    data_loc_test           = torch.stack([io.data_loc          for io in io_test], dim=1)
    data_stuff_test         = torch.stack([io.data_stuff        for io in io_test], dim=1)
    data_combined_test      = torch.stack([io.data_combined     for io in io_test], dim=1)
    data_pitcher_game_test  = torch.stack([io.data_pitcher_game for io in io_test], dim=1)
    data_league_avg_test    = torch.stack([io.data_league_avg   for io in io_test], dim=1)


    # ==================== TARGETS / OUTPUTS ====================

    output_type_train       = torch.tensor([io.output_type     for io in io_train])
    output_swing_train      = torch.tensor([io.output_swing    for io in io_train])
    output_inplay_train     = torch.tensor([io.output_inplay     for io in io_train])

    output_type_test        = torch.tensor([io.output_type     for io in io_test])
    output_swing_test       = torch.tensor([io.output_swing    for io in io_test])
    output_inplay_test      = torch.tensor([io.output_inplay     for io in io_test])
    
    mask_inplay_train       = torch.tensor([io.mask_inplay     for io in io_train])
    mask_swing_train        = torch.tensor([io.mask_swing      for io in io_train])
    
    mask_inplay_test       = torch.tensor([io.mask_inplay     for io in io_test])
    mask_swing_test        = torch.tensor([io.mask_swing      for io in io_test])
    
    train_dataset = PitchDataset(
        mapping_game_ids_train,
        mapping_pitch_num_train,
        
        data_overview_train,
        data_loc_train,
        data_stuff_train,
        data_combined_train,
        data_pitcher_game_train,
        data_league_avg_train,
        
        output_type_train,
        output_swing_train,
        output_inplay_train,
        
        mask_swing_train,
        mask_inplay_train,
        
        dataset_device=dataset_device,
    )
    
    test_dataset = PitchDataset(
        mapping_game_ids_test,
        mapping_pitch_num_test,
        
        data_overview_test,
        data_loc_test,
        data_stuff_test,
        data_combined_test,
        data_pitcher_game_test,
        data_league_avg_test,
        
        output_type_test,
        output_swing_test,
        output_inplay_test,
        
        mask_swing_test,
        mask_inplay_test,
        
        dataset_device=dataset_device,
    )
    
    return train_dataset, test_dataset