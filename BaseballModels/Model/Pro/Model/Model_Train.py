import torch
from Model.Constants import device, TOTAL_WAR_BUCKETS
from Model.Pro.Model.Player_Model import Stats_Loss, Position_Classification_Loss, Classification_Loss, Mlb_Value_Loss_Hitter, Mlb_Value_Loss_Pitcher, MLB_Stat_Classification_Loss, Pt_Loss, Recurrent_Model
from Model.Combined.Model.GetWarClassCounts import *
from Model.Combined.Utilities.Types import *
from Model.Combined.Utilities.BrierScore import Brier_Score

from Model.Utilities import profiler

ELEMENT_LIST = ["WAR", "Level", "PA", "Stats", "Position", "MLBValue", "PlayingTime", "MLBStat"]
NUM_ELEMENTS = len(ELEMENT_LIST)

@profiler
def GetLossesPro(
  network : Recurrent_Model, 
  data : tuple, 
  targets : tuple, 
  masks : tuple, 
  i0 : torch.Tensor, 
  shouldBackprop : bool, 
  is_hitter: bool,
  pro_element_loss_scales : list[int] = [1] * NUM_ELEMENTS) -> ProLossResult | None:
  
  # Get Model Output
  data, length, pt_levelYearGames, player_demo, player_bios = data
  mask_valid = length > 0
  if mask_valid.sum() == 0:
    return None
  
  data = data[mask_valid].to(device, non_blocking=True)
  length = length[mask_valid].to(device, non_blocking=True)
  pt_levelYearGames = pt_levelYearGames[mask_valid].to(device, non_blocking=True)
  i0 = i0[mask_valid].to(device, non_blocking=True)
  player_demo = player_demo[mask_valid].to(device, non_blocking=True)
  player_bios = player_bios[mask_valid].to(device, non_blocking=True)
  
  output_war, output_level, output_pa, output_stats, output_pos, output_mlbValue, output_pt, output_mlbstat = network(data, length, pt_levelYearGames, i0, player_demo, player_bios)
  
  # Move targets and masks to GPU
  target_war, target_level, target_pa, target_yearStats, target_yearPos, target_mlbValue, target_pt, target_mlbstat = targets
  mask_labels, mask_stats, mask_year, mask_mlbValue, mask_mlbstat = masks
  
  target_war = target_war[mask_valid].to(device, non_blocking=True)
  target_level = target_level[mask_valid].to(device, non_blocking=True)
  target_pa = target_pa[mask_valid].to(device, non_blocking=True)
  target_yearStats = target_yearStats[mask_valid].to(device, non_blocking=True)
  target_yearPos = target_yearPos[mask_valid].to(device, non_blocking=True)
  target_mlbValue = target_mlbValue[mask_valid].to(device, non_blocking=True)
  target_pt = target_pt[mask_valid].to(device, non_blocking=True)
  target_mlbstat = target_mlbstat[mask_valid].to(device, non_blocking=True)
  
  mask_labels = mask_labels[mask_valid].to(device, non_blocking=True)
  mask_year = mask_year[mask_valid].to(device, non_blocking=True)
  mask_stats = mask_stats[mask_valid].to(device, non_blocking=True)
  mask_mlbValue = mask_mlbValue[mask_valid].to(device, non_blocking=True)
  mask_mlbstat = mask_mlbstat[mask_valid].to(device, non_blocking=True)
  
  # Track per-class WAR prediction/actual counts over valid timesteps
  with torch.no_grad():
    war_predicted_counts, war_actual_counts = GetWarClassCounts(output_war, target_war, mask_labels)
    brier_per_class_sum, brier_count = Brier_Score(output_war, target_war, mask_labels)
  
  # Get losses
  loss_war = Classification_Loss(output_war, target_war, mask_labels)
  loss_level = Classification_Loss(output_level, target_level, mask_labels)
  loss_pa = Classification_Loss(output_pa, target_pa, mask_labels)
  
  loss_yearStats = Stats_Loss(output_stats, target_yearStats, mask_stats)
  loss_yearPt = Pt_Loss(output_pt, target_pt)
  loss_yearPos = Position_Classification_Loss(output_pos, target_yearPos, mask_year)
  loss_mlbValue = Mlb_Value_Loss_Hitter(output_mlbValue, target_mlbValue, mask_mlbValue) if is_hitter else Mlb_Value_Loss_Pitcher(output_mlbValue, target_mlbValue, mask_mlbValue)
  loss_mlbStat = MLB_Stat_Classification_Loss(output_mlbstat, target_mlbstat, mask_mlbstat, is_hitter)
  
  # Scale how much each loss should effect the model
  losses = [loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_yearPt, loss_mlbValue, loss_mlbStat]
  for i in range(NUM_ELEMENTS):
    els = pro_element_loss_scales[i]
    if els != 1:
      losses[i] *= els
  
  if shouldBackprop:
    torch.autograd.backward(losses)
  
  for i in range(NUM_ELEMENTS):
    els = pro_element_loss_scales[i]
    if els != 1:
      losses[i] /= els
  
  return ProLossResult(
    losses=(loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt, loss_mlbStat),
    war_counts=WarClassCounts(predicted=war_predicted_counts, actual=war_actual_counts),
    brier=BrierAccumulator(per_class_sum=brier_per_class_sum, count=brier_count),
  )