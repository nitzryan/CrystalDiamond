from PitchModel.PitchTrackingDBTypes import *
from typing import Callable
import torch

class Prep_Map:
    def __init__(self,
                pitch_stuff_map : Callable[[DB_PitchData], list[float]], pitch_stuff_size : int,
                pitch_combined_map : Callable[[DB_PitchData], list[float]], pitch_combined_size : int,
    ):
        
        self.pitch_stuff_map = pitch_stuff_map
        self.pitch_stuff_size = pitch_stuff_size
        
        self.pitch_combined_map = pitch_combined_map
        self.pitch_combined_size = pitch_combined_size
        
def clamp(value : float, minimum : float, maximum : float) -> float:
    return min(max(value, minimum), maximum)
    
__map_pitch_stuff : Callable[[DB_PitchData], list[float]] = \
    lambda p : [
        max(p.Vel, 65), 
        clamp(p.BreakInduced, -22, 25), 
        clamp(p.BreakHorizontal, -25, 25), 
        clamp(p.Extension, 4.5, 7.75), 
        clamp(p.SpinRate, 500, 3500), 
        clamp(p.SpinAxis, 50, 310),
        p.HaaAboveAverage,
        p.VaaAboveAverage,
        p.PitchClass,
        p.CountBalls,
        p.CountStrike,
        p.HitIsR,
        p.PitIsR,
        p.LevelId
    ]
__size_stuff = 14
    
__map_pitch_combined : Callable[[DB_PitchData], list[float]] = \
    lambda p : [
            max(p.Vel, 65), 
            clamp(p.BreakInduced, -22, 25), 
            clamp(p.BreakHorizontal, -25, 25), 
            clamp(p.Extension, 4.5, 7.75), 
            clamp(p.SpinRate, 500, 3500), 
            clamp(p.SpinAxis, 50, 310),
            p.HaaAboveAverage,
            p.VaaAboveAverage,
            p.PitchClass,
            p.CountBalls,
            p.CountStrike,
            p.HitIsR,
            p.PitIsR,
            p.PlateX,
            p.PlateZ,
            p.ZoneTop,
            p.ZoneBot,
            p.LevelId
    ]
__size_combined = 18

    
standard_prep_map = Prep_Map(
    pitch_stuff_map=__map_pitch_stuff, pitch_stuff_size=__size_stuff,
    pitch_combined_map=__map_pitch_combined, pitch_combined_size=__size_combined,
)