from Model.Evaluation.Classes import WarFoldTargets
from Model.Utilities import GetModelMaps
from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Combined.DataPrep.Player_Dataset import Combined_Player_Dataset, Create_Test_Train_Datasets

import torch

def GetProWarTargets(model_id : int, is_hitter : bool) -> WarFoldTargets:
    pro_prep_map, pro_output_map, col_prep_map, col_output_map = GetModelMaps(model_id)
    
    data_prep = Combined_Data_Prep(
        prep_map=pro_prep_map,
        output_map=pro_output_map,
        college_prep_map=col_prep_map,
        college_output_map=col_output_map
    )
    
    io_list = data_prep.Generate_IO_Hitters(is_training=True) if is_hitter \
            else data_prep.Generate_IO_Pitchers(is_training=True)
    
    dataset : Combined_Player_Dataset
    dataset, _ = Create_Test_Train_Datasets(player_list=io_list, is_hitter=is_hitter, device='cpu', eval_mode=True)
    
    dates = dataset.pro_dates               # (N, T, 3) -> mlbId, year, month
    target_war = dataset.pro_o_war_buckets  # (N,)
    mask_labels = dataset.pro_m_labels      # (N, T)
    lengths = dataset.pro_lengths           # (N,)
    
    # A timestep is a target only where the sequence is real and the player is a labeled prospect
    num_players, num_steps = mask_labels.shape
    steps = torch.arange(num_steps).unsqueeze(0)
    valid = (steps < lengths.unsqueeze(1)) & mask_labels.bool()
    
    valid_dates = dates[valid]
    targets = WarFoldTargets(
        mlb_ids=valid_dates[:, 0].long(),
        dates=valid_dates[:, 1:].long(),
        target_war=target_war.unsqueeze(1).expand(num_players, num_steps)[valid].long(),
    )
    
    return targets