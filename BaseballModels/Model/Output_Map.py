from DBTypes import *
from typing import Callable
import torch
import numpy as np
from Constants import DTYPE
import math

class Output_Map:
    def __init__(self,
                 map_hitter : Callable[[DB_Model_Players], float],
                 buckets_hitter : torch.Tensor,
                 map_pitcher : Callable[[DB_Model_Players], float],
                 buckets_pitcher : torch.Tensor,
                 map_hitter_output : Callable[[DB_Model_HitterStats], list[float]],
                 hitter_stats_size : int,
                 map_hitter_positions : Callable[[DB_Model_HitterStats], list[float]],
                 hitter_positions_size : int
                 ):
        
        self.map_hitter = map_hitter
        self.map_pitcher = map_pitcher
        self.buckets_hitter = buckets_hitter
        self.buckets_pitcher = buckets_pitcher
        self.map_hitter_output = map_hitter_output
        self.hitter_stats_size = hitter_stats_size
        self.map_hitter_positions = map_hitter_positions
        self.hitter_positions_size = hitter_positions_size

    @staticmethod
    def GetHitterOutputMasks(stats : DB_Model_HitterStats) -> list[float]:
        pa : int = stats.PA
        lvl : float = stats.LevelId
        masks : list[float] = [0,0,0,0,0,0,0,0]
        
        lvlFloor : int = math.floor(lvl)
        lvlFrac = lvl - lvlFloor
        masks[lvlFloor - 1] = pa / 100 * (1 - lvlFrac)
        if lvlFrac > 0:
            masks[lvlFloor] = pa / 100 * lvlFrac
        
        return masks
    
    @staticmethod
    def GetProspectMask(stats : DB_Model_HitterStats) -> float:
        return stats.TrainMask & 1
    
__map_hitter_output : Callable[[DB_Model_HitterStats], list[float]] = \
    lambda h : [h.ParkRunFactor, h.ParkHRFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wOBARatio, h.SBRateRatio, h.SBPercRatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
__map_hitter_positions : Callable[[DB_Model_HitterStats], list[float]] = \
    lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH]
__hitter_stats_size = 11
__hitter_positions_size = 9
    
war_map = Output_Map(
    map_hitter=lambda p : p.warHitter,
    map_pitcher=lambda p : p.warPitcher,
    buckets_hitter=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE),
    buckets_pitcher=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE),
    map_hitter_output=__map_hitter_output,
    hitter_stats_size=__hitter_stats_size,
    map_hitter_positions=__map_hitter_positions,
    hitter_positions_size=__hitter_positions_size
)

value_map = Output_Map(
    map_hitter=lambda p : p.valueHitter,
    map_pitcher=lambda p : p.valuePitcher,
    buckets_hitter=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
    buckets_pitcher=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
    map_hitter_output=__map_hitter_output,
    hitter_stats_size=__hitter_stats_size,
    map_hitter_positions=__map_hitter_positions,
    hitter_positions_size=__hitter_positions_size
)