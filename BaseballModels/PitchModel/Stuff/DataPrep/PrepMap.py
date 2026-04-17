from DBTypes import *
from typing import Callable

class Prep_Map:
    def __init__(self,
                pitch_stuff_map : Callable[[DB_PitchStatcast], list[float]], pitch_stuff_size : int,
                pitch_loc_map : Callable[[DB_PitchStatcast], list[float]], pitch_loc_size : int,
                pitch_overview_map : Callable[[DB_PitchStatcast], list[float]], pitch_overview_size : int,
    ):
        self.pitch_overview_map = pitch_overview_map
        self.pitch_overview_size = pitch_overview_size
        
        self.pitch_stuff_map = pitch_stuff_map
        self.pitch_stuff_size = pitch_stuff_size
        
        self.pitch_loc_map = pitch_loc_map
        self.pitch_loc_size = pitch_loc_size
        
        
        
__map_pitch_stuff : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.vStart, p.BreakAngle, p.BreakInduced, p.BreakHorizontal, p.Extension]
__size_stuff = 5
    
__map_pitch_loc : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.pX, p.pZ, p.ZoneTop, p.ZoneBot]
__size_loc = 4
    
__map_pitch_overview : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.PitchType, p.CountBalls, p.CountStrike, p.Outs, p.BaseOccupancy, p.HitIsR, p.PitIsR]
__size_overview = 7

standard_prep_map = Prep_Map(
    pitch_stuff_map=__map_pitch_stuff, pitch_stuff_size=__size_stuff,
    pitch_loc_map=__map_pitch_loc, pitch_loc_size=__size_loc,
    pitch_overview_map=__map_pitch_overview, pitch_overview_size=__size_overview,
)