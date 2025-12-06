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
    lambda h : [h.ParkRunFactor, h.ParkHRFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wRC, h.SBRateRatio, h.SBPercRatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
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
    
__full_season_pa = 600
def mlb_hit_map(h : DB_Model_HitterValue) -> list[float]:
    pa1 = max(1, h.Pa1Year) / __full_season_pa
    pa2 = max(1, h.Pa2Year) / __full_season_pa
    pa3 = max(1, h.Pa3Year) / __full_season_pa
    return [h.War1Year / pa1, h.Off1Year / pa1, h.Bsr1Year / pa1, h.Def1Year / pa1,
            h.War2Year / pa2, h.Off2Year / pa2, h.Bsr2Year / pa2, h.Def2Year / pa2, 
            h.War3Year / pa3, h.Off3Year / pa3, h.Bsr3Year / pa3, h.Def3Year / pa3,
            h.Pa1Year, h.Pa2Year, h.Pa3Year]
__map_mlb_hitter_values : Callable[[DB_Model_HitterValue], list[float]] = mlb_hit_map
__mlb_hitter_values_size = 15

__full_season_ip = 150
__full_season_relief_ip = 60
def mlb_pit_map(p : DB_Model_PitcherValue) -> list[float]:
    ipsp1 = max(1, p.IPSP1Year) / __full_season_ip
    ipsp2 = max(1, p.IPSP2Year) / __full_season_ip
    ipsp3 = max(1, p.IPSP3Year) / __full_season_ip
    iprp1 = max(1, p.IPRP1Year) / __full_season_relief_ip
    iprp2 = max(1, p.IPRP2Year) / __full_season_relief_ip
    iprp3 = max(1, p.IPRP3Year) / __full_season_relief_ip
    return [p.WarSP1Year / ipsp1, p.WarRP1Year / iprp1, 
            p.WarSP2Year / ipsp2, p.WarRP2Year / iprp2, 
            p.WarSP3Year / ipsp3, p.WarRP3Year / iprp3, 
            p.IPSP1Year, p.IPRP1Year, p.IPSP2Year, p.IPRP2Year, p.IPSP3Year, p.IPRP3Year]
    
__map_mlb_pitcher_values : Callable[[DB_Model_PitcherValue], list[float]] = mlb_pit_map
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