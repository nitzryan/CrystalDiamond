import torch
from dataclasses import dataclass

from PitchModel.Constants import device, NUM_TRAINING_VARIANTS, TRAIN_TEST_RATIO
from PitchModel.Stuff.Model.ModelOutputType import ModelOutputType
from PitchModel.Stuff.DataPrep.PitchIO import PitchIO, PitchIOData
from PitchModel.DBTypes import *


def _split_sublist(
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
                 
                data_stuff : torch.Tensor,
                data_combined : torch.Tensor,
                
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
        
        self.data_stuff = data_stuff.t().to(device=dataset_device, non_blocking=True)
        self.data_combined = data_combined.t().to(device=dataset_device, non_blocking=True)
        
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
                self.data_stuff.size(dim=0),
                dtype=torch.float,
                device=dataset_device
            ),
            ModelOutputType.SwingResults: self.mask_swing,
            ModelOutputType.InPlay: self.mask_inplay,
        }
        
        current_output_type
        self.SetOutputType(current_output_type)
        
    def __len__(self):
        return self.data_stuff.size(dim=0)
    
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
            self.data_stuff[filtered_indices],
            self.data_combined[filtered_indices],
        )
        
        # Data used to evaluate model
        target = self.current_targets[filtered_indices]
        
        return mappings, data, target
       
@dataclass
class PitchDatasets:
    train : PitchDataset
    test : PitchDataset
    val_seen : PitchDataset | None
    val_unseen : PitchDataset | None
    
    def SetOutputType(self, output_type : ModelOutputType) -> None:
        self.train.SetOutputType(output_type)
        self.test.SetOutputType(output_type)
        if self.val_seen is not None:
            self.val_seen.SetOutputType(output_type)
        if self.val_unseen is not None:
            self.val_unseen.SetOutputType(output_type)
            
def _BuildPitchDataset(io_list : list[PitchIO], dataset_device = device) -> PitchDataset:
    return PitchDataset(
        [io.pitcher_id for io in io_list],

        torch.tensor([io.game_id     for io in io_list], dtype=torch.long),
        torch.tensor([io.pitch_num   for io in io_list], dtype=torch.long),
        torch.tensor([io.pitcher_id  for io in io_list], dtype=torch.long),
        torch.tensor([io.level_id    for io in io_list], dtype=torch.long),

        torch.stack([io.data_stuff    for io in io_list], dim=1),
        torch.stack([io.data_combined for io in io_list], dim=1),

        torch.tensor([io.output_type   for io in io_list]),
        torch.tensor([io.output_swing  for io in io_list]),
        torch.tensor([io.output_inplay for io in io_list]),

        torch.tensor([io.mask_swing  for io in io_list]),
        torch.tensor([io.mask_inplay for io in io_list]),

        dataset_device=dataset_device,
    )
          
# Goes through validation data, looks for pitchers that were in the training data to 
# segregate them from pitchers unseen by training, whether they were in test or not
def _SplitValidationByPitcher(
    validation_data : list[list[PitchIO]],
    train_pitcher_ids : set[int]) -> tuple[list[list[PitchIO]], list[list[PitchIO]]]:

    val_seen : list[list[PitchIO]] = []
    val_unseen : list[list[PitchIO]] = []
    for sublist in validation_data:
        if len(sublist) == 0:
            continue
        if sublist[0].pitcher_id in train_pitcher_ids:
            val_seen.append(sublist)
        else:
            val_unseen.append(sublist)

    return val_seen, val_unseen

def CreateTestTrainDatasets(
                io_data : PitchIOData,
                dataset_device = device,
                eval_mode : bool = False,
                train_test_ratio : int = TRAIN_TEST_RATIO,
                total_training_runs : int = NUM_TRAINING_VARIANTS,
                train_idx : int = 0) -> PitchDatasets:
    
    
    data = io_data.data
    validation_data = io_data.validation_data
    # Create Test/Train keeping a player entirely inside of 1 dataset
    if not eval_mode:
        io_train, io_test = _split_sublist(data, train_test_ratio, total_training_runs, train_idx)
    else:
        io_train = data
        io_test = [data[0]] # Allow for test code to run without breaking, will discard later

    # Validation year is split on whether the pitcher appeared in the training set.
    # Anything else (test-set pitchers + pitchers never seen at all) lands in val_unseen.
    val_seen_dataset : PitchDataset | None = None
    val_unseen_dataset : PitchDataset | None = None
    if validation_data is not None:
        train_pitcher_ids = {sublist[0].pitcher_id for sublist in io_train if len(sublist) > 0}
        io_val_seen, io_val_unseen = _SplitValidationByPitcher(validation_data, train_pitcher_ids)
        
        io_val_seen : list[PitchIO] = [item for sublist in io_val_seen for item in sublist]
        io_val_unseen : list[PitchIO] = [item for sublist in io_val_unseen for item in sublist]
        
        if len(io_val_seen) > 0:
            val_seen_dataset = _BuildPitchDataset(io_val_seen, dataset_device)
        if len(io_val_unseen) > 0:
            val_unseen_dataset = _BuildPitchDataset(io_val_unseen, dataset_device)
    
    io_train : list[PitchIO] = [item for sublist in io_train for item in sublist]
    io_test : list[PitchIO] = [item for sublist in io_test for item in sublist]

    train_dataset = _BuildPitchDataset(io_train, dataset_device)
    test_dataset = _BuildPitchDataset(io_test, dataset_device)
    
    return PitchDatasets(
        train=train_dataset,
        test=test_dataset,
        val_seen=val_seen_dataset,
        val_unseen=val_unseen_dataset,
    )