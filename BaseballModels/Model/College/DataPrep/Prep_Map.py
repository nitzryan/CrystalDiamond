from DBTypes import *
from typing import Callable
import math

class College_Prep_Map:
    def __init__(self,
            map_bio : Callable[[DB_College_Player], list[float]], bio_size : int, 
            map_hitstats : Callable[[DB_Model_College_HitterYear], list[float]], hitstats_size : int,
            map_def : Callable[[DB_Model_College_HitterYear], list[float]], def_size : int,
            map_pitstats : Callable[[DB_Model_College_PitcherYear], list[float]], pitstats_size : int,):
        
        self.map_bio = map_bio
        self.map_hitstats = map_hitstats
        self.map_def = map_def
        self.map_pitstats = map_pitstats
        
        self.bio_size = bio_size
        self.hitstats_size = hitstats_size
        self.def_size = def_size
        self.pitstats_size = pitstats_size
    
def MapBats(bats : str) -> int:
    match bats.capitalize():
        case "R":
            return 1
        case "L":
            return 2
        case "S":
            return 3
        case _: # Default to Right handed
            return 1
        
def MapThrows(throws : str) -> int:
    match throws.capitalize():
        case "R":
            return 1
        case "L":
            return 2
        case _: # Default to Right handed
            return 1
       
def MapPos(pos : int) -> list[float]:
    pos_list = [0] * 10
    # P through DH
    for i in range(10):
        if pos & (1 << i) > 0:
            pos_list[i] += 1
            
    # IF
    if pos & (1 << 10) > 0:
        pos_list[2] += 1/4
        pos_list[3] += 1/4
        pos_list[4] += 1/4
        pos_list[5] += 1/4
        
    # OF
    if pos & (1 << 11) > 0:
        pos_list[6] += 1/3
        pos_list[7] += 1/3
        pos_list[8] += 1/3
        
    # L1 normalization
    s = sum(pos_list)
    pos_list = [x / s for x in pos_list]
    return pos_list
        
        
college_base_prep_map = College_Prep_Map(
    map_bio=lambda p : [MapBats(p.Bats), MapThrows(p.Throws)],
    map_hitstats=lambda h : [h.ExpYears, h.Age, h.ParkRunFactor, h.ConfScore, h.PA, h.HR, h.SB, h.CS, h.BB, h.K, h.AVG, h.OBP, h.SLG, h.Height],
    map_def=lambda h : MapPos(h.Pos),
    map_pitstats=lambda p : [p.ExpYears, p.Age, p.ParkRunFactor, p.ConfScore, p.G, p.GS, p.Outs, p.ERA, p.H9, p.HR9, p.BB9, p.K9, p.WHIP, p.Age, p.Height],
    
    bio_size=2,
    hitstats_size=14,
    def_size=10,
    pitstats_size=15
)