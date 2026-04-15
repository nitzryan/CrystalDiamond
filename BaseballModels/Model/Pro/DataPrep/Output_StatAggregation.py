from DBTypes import *
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

# endYear, endMonth, startYear inclusive, startMonth exclusive
NUM_HITTER_STATS = 13
NUM_HITTER_BUCKETS_PER_STAT = 21
PA_STAT_RATE = 600
_HALF_BUCKETS = NUM_HITTER_BUCKETS_PER_STAT // 2
_LARGE_DELTA = 4
_MED_DELTA = 8
_SMALL_DELTA = 10
BUCKET_HIT1B = torch.tensor([x / _SMALL_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_HIT2B = torch.tensor([x / _MED_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_HIT3B = torch.tensor([x / _LARGE_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_HR = torch.tensor([x / _MED_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_BB = torch.tensor([x / _MED_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_HBP = torch.tensor([x / _LARGE_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_K = torch.tensor([x / _MED_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_SB = torch.tensor([x / _LARGE_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_CS = torch.tensor([x / _LARGE_DELTA for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_PF = torch.tensor([1 + ((x - _HALF_BUCKETS) / 50) for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_BSR = torch.tensor([(x - _HALF_BUCKETS) for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_DRAA = torch.tensor([(x - _HALF_BUCKETS) * 2 for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)
BUCKET_DPOS = torch.tensor([(x - _HALF_BUCKETS) for x in range(NUM_HITTER_BUCKETS_PER_STAT - 1)], dtype=torch.float)

def Aggregate_HitterMlbBuckets(stats : list[DB_Model_HitterLevelStats],
                          startMonth : int,
                          endMonth : int,
                          startYear : int,
                          endYear : int,
                          output_map : Output_Map,
                          start_idx : int) -> tuple[torch.Tensor, int]:
    
    filteredStats : list[DB_Model_HitterLevelStats] = []
    
    # Start from last good index, stop after good index found and then bad found
    good_stat_found = False
    next_start_index = start_idx
    for i, stat in enumerate(stats[start_idx:], start=start_idx):
        if __IsDateValid(month=stat.Month, year=stat.Year, startMonth=startMonth, endMonth=endMonth, startYear=startYear, endYear=endYear):
            if stat.LevelId == 0:
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
        totalPa += stat.Pa
    
    mask = min(totalPa / 50, 1)
    statTensor = torch.zeros(NUM_HITTER_STATS, dtype=torch.long)
    
    
    if totalPa == 0: # Stats aggregation will return all zeros
        return statTensor, mask, next_start_index
    
    # MLB rate stats
    hit1B = sum([s.Hit1B * s.Pa for s in filteredStats])
    hit2B = sum([s.Hit2B * s.Pa for s in filteredStats])
    hit3B = sum([s.Hit3B * s.Pa for s in filteredStats])
    hitHR = sum([s.HitHR * s.Pa for s in filteredStats])
    hitBB = sum([s.BB * s.Pa for s in filteredStats])
    hitHBP = sum([s.HBP * s.Pa for s in filteredStats])
    hitK = sum([s.K * s.Pa for s in filteredStats])
    hitSB = sum([s.SB * s.Pa for s in filteredStats])
    hitCS = sum([s.CS * s.Pa for s in filteredStats])
    parkFactor = sum([s.ParkRunFactor * s.Pa for s in filteredStats])
    # Counting Stats
    bsr = sum([s.BSR for s in filteredStats])
    draa = sum([s.DRAA for s in filteredStats])
    dpos = sum([s.DPOS for s in filteredStats])
    
    statTensor[0] = torch.bucketize(hit1B / totalPa, BUCKET_HIT1B)
    statTensor[1] = torch.bucketize(hit2B / totalPa, BUCKET_HIT2B)
    statTensor[2] = torch.bucketize(hit3B / totalPa, BUCKET_HIT3B)
    statTensor[3] = torch.bucketize(hitHR / totalPa, BUCKET_HR)
    statTensor[4] = torch.bucketize(hitBB / totalPa, BUCKET_BB)
    statTensor[5] = torch.bucketize(hitHBP / totalPa, BUCKET_HBP)
    statTensor[6] = torch.bucketize(hitK / totalPa, BUCKET_K)
    statTensor[7] = torch.bucketize(hitCS / totalPa, BUCKET_CS)
    statTensor[8] = torch.bucketize(hitSB / totalPa, BUCKET_SB)
    statTensor[9] = torch.bucketize(parkFactor / totalPa, BUCKET_PF)
    statTensor[10] = torch.bucketize(bsr / totalPa * PA_STAT_RATE, BUCKET_BSR)
    statTensor[11] = torch.bucketize(draa / totalPa * PA_STAT_RATE, BUCKET_DRAA)
    statTensor[12] = torch.bucketize(dpos / totalPa * PA_STAT_RATE, BUCKET_DPOS)
        
    return statTensor, mask, next_start_index

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