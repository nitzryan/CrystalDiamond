import torch
from PitchModel.Constants import device, NUM_TRAINING_VARIANTS, TRAIN_TEST_RATIO
from PitchModel.Stuff.Model.ModelOutputType import ModelOutputType
from PitchModel.DBTypes import *

class PitchIO:
    def __init__(self,
        # Data needed to identify in DB
        game_id : int,
        pitch_num : int,
        pitcher_id : int,
        level_id : int,
                 
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
        self.pitcher_id = pitcher_id
        self.level_id = level_id
        
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
                ids : list[int],
                
                mapping_game_ids : torch.Tensor,
                mapping_pitch_nums : torch.Tensor,
                mapping_pitcher_ids : torch.Tensor,
                mapping_level_ids : torch.Tensor,
                 
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
                
                current_output_type: ModelOutputType = ModelOutputType.Result,
                
                dataset_device = device
                ):
        
        self.ids = list(set(ids)) # Unique players
        
        self.mapping_game_ids = mapping_game_ids
        self.mapping_pitch_nums = mapping_pitch_nums
        self.mapping_pitcher_ids = mapping_pitcher_ids
        self.mapping_level_ids = mapping_level_ids
        
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
        
        self.output_targets = {
            ModelOutputType.Result: self.output_type,
            ModelOutputType.SwingResults: self.output_swing,
            ModelOutputType.InPlay: self.output_inplay,
        }
        
        self.output_masks = {
            ModelOutputType.Result: torch.ones(
                self.data_overview.size(dim=0),
                dtype=torch.float,
                device=dataset_device
            ),
            ModelOutputType.SwingResults: self.mask_swing,
            ModelOutputType.InPlay: self.mask_inplay,
        }
        
        current_output_type
        self.SetOutputType(current_output_type)
        
    def __len__(self):
        return self.data_overview.size(dim=0)
    
    def SetOutputType(self, output_type: ModelOutputType):
        self.current_output_type = output_type
        self.current_targets = self.output_targets[output_type]
        self.current_mask = self.output_masks[output_type]
    
    def GetEntries(self, batch_indices : torch.Tensor, eval_mode : bool) -> tuple[tuple[torch.Tensor, ...], tuple[torch.Tensor, ...], tuple[torch.Tensor, ...], tuple[torch.Tensor, ...]]:
        # Only get valid entries
        if eval_mode:
            filtered_indices = batch_indices
        else:
            keep = (self.current_mask[batch_indices] > 0.0).to(batch_indices.device)
            filtered_indices = batch_indices[keep]
        
        # Mappings used to connect data to specific pitch
        mappings = (
            self.mapping_game_ids[filtered_indices],
            self.mapping_pitch_nums[filtered_indices],
            self.mapping_pitcher_ids[filtered_indices],
            self.mapping_level_ids[filtered_indices],
        )
        
        # Data used to feed into model
        data = (
            self.data_overview[filtered_indices],
            self.data_loc[filtered_indices],
            self.data_stuff[filtered_indices],
            self.data_combined[filtered_indices],
            self.data_pitcher_game[filtered_indices],
            self.data_league_avg[filtered_indices]
        )
        
        # Data used to evaluate model
        target = self.current_targets[filtered_indices]
        
        return mappings, data, target
            
def CreateTestTrainDatasets(
    data : list[list[PitchIO]], 
    dataset_device = device,
    eval_mode : bool = False,
    train_test_ratio : int = TRAIN_TEST_RATIO,
    total_training_runs : int = NUM_TRAINING_VARIANTS,
    train_idx : int = 0) -> tuple[PitchDataset, PitchDataset]:
    
    
    # Create Test/Train keeping a player entirely inside of 1 dataset
    if not eval_mode:
        io_train, io_test = split_sublist(data, train_test_ratio, total_training_runs, train_idx)
    else:
        io_train = data
        io_test = [data[0]] # Allow for test code to run without breaking, will discard later
    io_train : list[PitchIO] = [item for sublist in io_train for item in sublist]
    io_test : list[PitchIO] = [item for sublist in io_test for item in sublist]

    # =================== PLAYER IDS =========================
    ids_train = [io.pitcher_id for io in io_train]
    ids_test = [io.pitcher_id for io in io_test]

    # =================== MAPPING FEATURES ===================
    mapping_game_ids_train = torch.tensor([io.game_id for io in io_train], dtype=torch.long)
    mapping_pitch_num_train = torch.tensor([io.pitch_num for io in io_train], dtype=torch.long)
    mapping_pitcher_ids_train     = torch.tensor([io.pitcher_id for io in io_train], dtype=torch.long)
    mapping_level_ids_train = torch.tensor([io.level_id for io in io_train], dtype=torch.long)
    
    mapping_game_ids_test = torch.tensor([io.game_id for io in io_test], dtype=torch.long)
    mapping_pitch_num_test = torch.tensor([io.pitch_num for io in io_test], dtype=torch.long)
    mapping_pitcher_ids_test     = torch.tensor([io.pitcher_id for io in io_test], dtype=torch.long)
    mapping_level_ids_test = torch.tensor([io.level_id for io in io_test], dtype=torch.long)
    
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
        ids_train,
        
        mapping_game_ids_train,
        mapping_pitch_num_train,
        mapping_pitcher_ids_train,
        mapping_level_ids_train,
        
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
        ids_test,
        
        mapping_game_ids_test,
        mapping_pitch_num_test,
        mapping_pitcher_ids_test,
        mapping_level_ids_test,
        
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