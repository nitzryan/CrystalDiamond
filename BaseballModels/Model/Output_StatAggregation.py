from DBTypes import DB_Model_HitterStats, DB_Model_PitcherStats
import torch
from Output_Map import Output_Map
from typing import Callable
from Constants import HITTER_LEVEL_BUCKETS

def __IsDateValid(month : int, year : int, startMonth : int, endMonth : int, startYear : int, endYear : int) -> bool:
    afterStart : bool = (year > startYear) or (year == startYear and month > startMonth)
    beforeEnd : bool = (year < endYear) or (year == endYear and month <= endMonth)
    return afterStart and beforeEnd 

# endYear, endMonth, startYear inclusive, startMonth exclusive
def Aggregate_HitterStats(stats : list[DB_Model_HitterStats],
                          startMonth : int,
                          endMonth : int,
                          startYear : int,
                          endYear : int,
                          output_map : Output_Map) -> tuple[torch.Tensor, torch.Tensor, torch.Tensor]:
    
    filteredStats : list[DB_Model_HitterStats] = []
    for stat in stats:
        if __IsDateValid(month=stat.Month, year=stat.Year, startMonth=startMonth, endMonth=endMonth, startYear=startYear, endYear=endYear):
            filteredStats.append(stat)
    
    # Get weight to smooth results
    totalPa = 0
    for stat in filteredStats:
        totalPa += stat.PA
    
    lvlMask = torch.zeros(size=[len(HITTER_LEVEL_BUCKETS)], dtype=torch.float)
    statTensor = torch.zeros(size=[output_map.hitter_stats_size], dtype=torch.float)
    posTensor = torch.zeros(size=[output_map.hitter_positions_size], dtype=torch.float)
    
    if totalPa == 0: # Stats aggregation will return all zeros
        return lvlMask, statTensor, posTensor
    
    filteredWeights : list[float] = [stat.PA / totalPa for stat in filteredStats]
    
    for i, stat in enumerate(filteredStats):
        weight = filteredWeights[i]
        maskList = Output_Map.GetOutputMasks(stat)
        statList = output_map.map_hitter_output(stat)
        posList = output_map.map_hitter_positions(stat)
        
        lvlMask += torch.tensor(maskList) / 6 # don't apply weighting, weights shouldn't necessarily add to 1, /6 to normalize to 600PA rather than 100 for monthly
        statTensor += torch.tensor(statList) * weight
        posTensor += torch.tensor(posList) * weight
        
    return lvlMask, statTensor, posTensor

def Aggregate_PitcherStats(stats : list[DB_Model_HitterStats],
                          startMonth : int,
                          endMonth : int,
                          startYear : int,
                          endYear : int,
                          output_map : Output_Map) -> tuple[torch.Tensor, torch.Tensor, torch.Tensor]:
    
    filteredStats : list[DB_Model_PitcherStats] = []
    for stat in stats:
        if __IsDateValid(month=stat.Month, year=stat.Year, startMonth=startMonth, endMonth=endMonth, startYear=startYear, endYear=endYear):
            filteredStats.append(stat)
    
    # Get weight to smooth results
    totalPa = 0
    for stat in filteredStats:
        totalPa += stat.BF
    
    lvlMask = torch.zeros(size=[len(HITTER_LEVEL_BUCKETS)], dtype=torch.float)
    statTensor = torch.zeros(size=[output_map.pitcher_stats_size], dtype=torch.float)
    posTensor = torch.zeros(size=[output_map.pitcher_positions_size], dtype=torch.float)
    
    if totalPa == 0: # Stats aggregation will return all zeros
        return lvlMask, statTensor, posTensor
    
    filteredWeights : list[float] = [stat.BF / totalPa for stat in filteredStats]
    
    for i, stat in enumerate(filteredStats):
        weight = filteredWeights[i]
        maskList = Output_Map.GetOutputMasks(stat)
        statList = output_map.map_pitcher_output(stat)
        posList = output_map.map_pitcher_positions(stat)
        
        lvlMask += torch.tensor(maskList) / 6 # don't apply weighting, weights shouldn't necessarily add to 1, /6 to normalize to 600PA rather than 100 for monthly
        statTensor += torch.tensor(statList) * weight
        posTensor += torch.tensor(posList) * weight
        
    return lvlMask, statTensor, posTensor