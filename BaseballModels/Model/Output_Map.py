from DBTypes import *
from typing import Callable, Union
import torch
import numpy as np
from Constants import DTYPE
import math

class Output_Map:
    def __init__(self,
                 map_hitter_war : Callable[[DB_Model_Players], float],
                 buckets_hitter_war : torch.Tensor,
                 map_hitter_value : Callable[[DB_Model_Players], float],
                 buckets_hitter_value : torch.Tensor,
                 map_pitcher_war : Callable[[DB_Model_Players], float],
                 buckets_pitcher_war : torch.Tensor,
                 map_pitcher_value : Callable[[DB_Model_Players], float],
                 buckets_pitcher_value : torch.Tensor,
                 map_hitter_output : Callable[[DB_Model_HitterStats], list[float]],
                 hitter_stats_size : int,
                 map_hitter_positions : Callable[[DB_Model_HitterStats], list[float]],
                 hitter_positions_size : int,
                 map_pitcher_output : Callable[[DB_Model_PitcherStats], list[float]],
                 pitcher_stats_size : int,
                 map_pitcher_positions : Callable[[DB_Model_PitcherStats], list[float]],
                 pitcher_positions_size : int,
                 map_mlb_hitter_values : Callable[[DB_Model_HitterValue], list[float]],
                 mlb_hitter_values_size : int,
                 map_mlb_pitcher_values : Callable[[DB_Model_PitcherValue], list[float]],
                 mlb_pitcher_values_size : int
                 ):
        
        self.map_hitter_war = map_hitter_war
        self.map_hitter_value = map_hitter_value
        self.map_pitcher_war = map_pitcher_war
        self.map_pitcher_value = map_pitcher_value
        
        self.buckets_hitter_war = buckets_hitter_war
        self.buckets_hitter_value = buckets_hitter_value
        self.buckets_pitcher_war = buckets_pitcher_war
        self.buckets_pitcher_value = buckets_pitcher_value
        
        self.map_hitter_output = map_hitter_output
        self.hitter_stats_size = hitter_stats_size
        self.map_hitter_positions = map_hitter_positions
        self.hitter_positions_size = hitter_positions_size
        
        self.map_pitcher_output = map_pitcher_output
        self.pitcher_stats_size = pitcher_stats_size
        self.map_pitcher_positions = map_pitcher_positions
        self.pitcher_positions_size = pitcher_positions_size
        
        self.map_mlb_hitter_values = map_mlb_hitter_values
        self.mlb_hitter_values_size = mlb_hitter_values_size
        self.map_mlb_pitcher_values = map_mlb_pitcher_values
        self.mlb_pitcher_values_size = mlb_pitcher_values_size

    @staticmethod
    def GetOutputMasks(stats : Union[DB_Model_HitterStats, DB_Model_PitcherStats]) -> list[float]:
        if isinstance(stats, DB_Model_HitterStats):
            pa : int = stats.PA
            lvl : float = stats.LevelId
        else:
            pa : int = stats.BF
            lvl : float = stats.LevelId
        masks : list[float] = [0,0,0,0,0,0,0,0]
        
        lvlFloor : int = math.floor(lvl)
        lvlFrac = lvl - lvlFloor
        masks[lvlFloor - 1] = pa / 100 * (1 - lvlFrac)
        if lvlFrac > 0:
            masks[lvlFloor] = pa / 100 * lvlFrac
        
        return masks
    
    @staticmethod
    def GetProspectMask(stats : Union[DB_Model_HitterStats, DB_Model_PitcherStats]) -> float:
        return stats.TrainMask & 1
    
__map_hitter_output : Callable[[DB_Model_HitterStats], list[float]] = \
    lambda h : [h.ParkRunFactor, h.ParkHRFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wOBARatio, h.SBRateRatio, h.SBPercRatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
__map_hitter_positions : Callable[[DB_Model_HitterStats], list[float]] = \
    lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH]
__hitter_stats_size = 11
__hitter_positions_size = 9

__map_pitcher_output : Callable[[DB_Model_PitcherStats], list[float]] = \
    lambda p : [p.ParkRunFactor, p.ParkHRFactor, p.GBPercRatio, p.ERARatio, p.FIPRatio, p.wOBARatio, p.HRPercRatio, p.BBPercRatio, p.KPercRatio]
__map_pitcher_positions : Callable[[DB_Model_PitcherStats], list[float]] = \
    lambda p : [p.SpPerc, 1 - p.SpPerc]
__pitcher_stats_size = 9
__pitcher_positions_size = 2
    
__map_mlb_hitter_values : Callable[[DB_Model_HitterValue], list[list[float]]] = \
    lambda h : [h.War1Year, h.Off1Year, h.Def1Year, h.Bsr1Year, h.Rep1Year, h.Pa1Year, h.War2Year, h.Off2Year, h.Def2Year, h.Bsr2Year, h.Rep2Year, h.Pa2Year, h.War3Year, h.Off3Year, h.Def3Year, h.Bsr3Year, h.Rep3Year, h.Pa3Year]
__mlb_hitter_values_size = 18
__map_mlb_pitcher_values : Callable[[DB_Model_PitcherValue], list[list[float]]] = \
    lambda p : [p.WarSP1Year, p.WarRP1Year, p.IPSP1Year, p.IPRP1Year, p.WarSP2Year, p.WarRP2Year, p.IPSP2Year, p.IPRP2Year, p.WarSP3Year, p.WarRP3Year, p.IPSP3Year, p.IPRP3Year]
__mlb_pitcher_values_size = 12
    
base_output_map = Output_Map(
    map_hitter_war=lambda p : p.warHitter,
    map_hitter_value=lambda p : p.valueHitter,
    map_pitcher_war=lambda p : p.warPitcher,
    map_pitcher_value=lambda p : p.valuePitcher,
    buckets_hitter_war=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE),
    buckets_hitter_value=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
    buckets_pitcher_war=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE),
    buckets_pitcher_value=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
    map_hitter_output=__map_hitter_output,
    hitter_stats_size=__hitter_stats_size,
    map_hitter_positions=__map_hitter_positions,
    hitter_positions_size=__hitter_positions_size,
    map_pitcher_output=__map_pitcher_output,
    pitcher_stats_size=__pitcher_stats_size,
    map_pitcher_positions=__map_pitcher_positions,
    pitcher_positions_size=__pitcher_positions_size,
    map_mlb_hitter_values=__map_mlb_hitter_values,
    mlb_hitter_values_size=__mlb_hitter_values_size,
    map_mlb_pitcher_values=__map_mlb_pitcher_values,
    mlb_pitcher_values_size=__mlb_pitcher_values_size
)

# value_map = Output_Map(
#     map_hitter=lambda p : p.valueHitter,
#     map_pitcher=lambda p : p.valuePitcher,
#     buckets_hitter=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
#     buckets_pitcher=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
#     map_hitter_output=__map_hitter_output,
#     hitter_stats_size=__hitter_stats_size,
#     map_hitter_positions=__map_hitter_positions,
#     hitter_positions_size=__hitter_positions_size
# )