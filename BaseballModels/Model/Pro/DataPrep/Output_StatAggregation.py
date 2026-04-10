from DBTypes import DB_Model_HitterStats, DB_Model_PitcherStats
import torch
from Pro.DataPrep.Output_Map import Output_Map
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
                          output_map : Output_Map,
                          start_idx : int) -> tuple[torch.Tensor, int]:
    
    filteredStats : list[DB_Model_HitterStats] = []
    
    # Start from last good index, stop after good index found and then bad found
    good_stat_found = False
    next_start_index = start_idx
    for i, stat in enumerate(stats[start_idx:], start=start_idx):
        if __IsDateValid(month=stat.Month, year=stat.Year, startMonth=startMonth, endMonth=endMonth, startYear=startYear, endYear=endYear):
            filteredStats.append(stat)
            
            # Check for first good datapoint
            if not good_stat_found:
                next_start_index = i + 1
                good_stat_found = True
                
        # If good found, first bad datapoint
        elif good_stat_found:
            break
    
    # Get weight to smooth results
    totalPa = 0
    for stat in filteredStats:
        totalPa += stat.PA
    
    posTensor = torch.zeros(size=[output_map.hitter_positions_size], dtype=torch.float)
    
    if totalPa == 0: # Stats aggregation will return all zeros
        return posTensor, next_start_index
    
    filteredWeights : list[float] = [stat.PA / totalPa for stat in filteredStats]
    
    for i, stat in enumerate(filteredStats):
        weight = filteredWeights[i]
        posList = output_map.map_hitter_positions(stat)
        posTensor += torch.tensor(posList) * weight
        
    return posTensor, next_start_index

def Aggregate_PitcherStats(stats : list[DB_Model_HitterStats],
                          startMonth : int,
                          endMonth : int,
                          startYear : int,
                          endYear : int,
                          output_map : Output_Map,
                          start_idx : int) -> tuple[torch.Tensor, int]:
    
    filteredStats : list[DB_Model_PitcherStats] = []
    
    # Start from last good index, stop after good index found and then bad found
    good_stat_found = False
    next_start_index = start_idx
    for i, stat in enumerate(stats[start_idx:], start=start_idx):
        if __IsDateValid(month=stat.Month, year=stat.Year, startMonth=startMonth, endMonth=endMonth, startYear=startYear, endYear=endYear):
            filteredStats.append(stat)
            
            # Check for first good datapoint
            if not good_stat_found:
                next_start_index = i + 1
                good_stat_found = True
                
        # If good found, first bad datapoint
        elif good_stat_found:
            break
    
    # Get weight to smooth results
    totalPa = 0
    for stat in filteredStats:
        totalPa += stat.BF
    
    posTensor = torch.zeros(size=[output_map.pitcher_positions_size], dtype=torch.float)
    
    if totalPa == 0: # Stats aggregation will return all zeros
        return posTensor, next_start_index
    
    filteredWeights : list[float] = [stat.BF / totalPa for stat in filteredStats]
    
    for i, stat in enumerate(filteredStats):
        weight = filteredWeights[i]
        posList = output_map.map_pitcher_positions(stat)
        posTensor += torch.tensor(posList) * weight
        
    return posTensor, next_start_index