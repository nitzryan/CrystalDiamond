import torch
from dataclasses import dataclass

class PitchIO:
    def __init__(self,
        # Data needed to identify in DB
        game_id : int,
        pitch_num : int,
        pitcher_id : int,
        level_id : int,
                 
        # Data for model
        data_stuff : torch.Tensor,
        data_combined : torch.Tensor,
        
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
        
        self.data_stuff = data_stuff
        self.data_combined = data_combined
        
        self.output_type = output_type
        self.output_swing = output_swing
        self.output_inplay = output_inplay
        
        self.mask_swing = mask_swing
        self.mask_inplay = mask_inplay
        
@dataclass
class PitchIOData:
    data : list[list[PitchIO]]
    validation_data : list[list[PitchIO]] | None