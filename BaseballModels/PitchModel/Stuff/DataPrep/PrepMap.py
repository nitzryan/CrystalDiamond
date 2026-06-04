from DBTypes import *
from typing import Callable
import torch

class Prep_Map:
    def __init__(self,
                pitch_stuff_map : Callable[[DB_PitchStatcast], list[float]], pitch_stuff_size : int,
                pitch_loc_map : Callable[[DB_PitchStatcast], list[float]], pitch_loc_size : int,
                pitch_combined_map : Callable[[DB_PitchStatcast], list[float]], pitch_combined_size : int,
                pitch_overview_map : Callable[[DB_PitchStatcast], list[float]], pitch_overview_size : int,
                league_baseline_map : Callable[[DB_PitchDateAverages], list[float]], league_baseline_size : int,
                pitcher_game_map : Callable[[DB_PitcherStatcastGame], list[float]], pitcher_game_size : int,
                
                noise_stuff : torch.Tensor,
                noise_location : torch.Tensor,
    ):
        self.pitch_overview_map = pitch_overview_map
        self.pitch_overview_size = pitch_overview_size
        
        self.pitch_stuff_map = pitch_stuff_map
        self.pitch_stuff_size = pitch_stuff_size
        
        self.pitch_loc_map = pitch_loc_map
        self.pitch_loc_size = pitch_loc_size
        
        self.pitch_combined_map = pitch_combined_map
        self.pitch_combined_size = pitch_combined_size
        
        self.league_baseline_map = league_baseline_map
        self.league_baseline_size = league_baseline_size
        
        self.pitcher_game_map = pitcher_game_map
        self.pitcher_game_size = pitcher_game_size
        
        self.noise_stuff = noise_stuff
        self.noise_location = noise_location
        
def clamp(value : float, minimum : float, maximum : float) -> float:
    return min(max(value, minimum), maximum)
    
map_pitch_stuff : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [
        max(p.vStart, 65), 
        clamp(p.BreakAngle if p.BreakAngle < 180 else p.BreakAngle - 360, 0, 50),
        clamp(p.BreakInduced, -22, 25), 
        clamp(p.BreakHorizontal, -25, 25), 
        clamp(p.Extension, 4.5, 7.75), 
        clamp(p.x0, -4, 4), 
        clamp(p.z0, 1, 7), 
        clamp(p.SpinRate, 500, 3500), 
        clamp(p.SpinDirection, 50, 310)]
__size_stuff = 9
    
__noise_stuff = torch.tensor([
    0.2,
    1,
    0.2,
    0.2,
    0.05,
    0.02,
    0.02,
    10,
    1
])
    
__map_pitch_loc : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.pX, p.pZ, p.ZoneTop, p.ZoneBot]
__size_loc = 4

__map_pitch_loc_clamped : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [
        clamp(p.pX, -4, 4), 
        clamp(p.pZ, -1, 6), 
        clamp(p.ZoneTop, 2, 4.5), 
        clamp(p.ZoneBot, 1, 2.5)]
    
__noise_loc = torch.tensor([
    0.02,
    0.02,
    0.01,
    0.01,
])
    
__map_pitch_overview : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.CountBalls, p.CountStrike, p.HitIsR, p.PitIsR]
__size_overview = 4

__map_pitch_combined : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.PlateTime]
    #lambda p : [p.aX, p.aY, p.aZ, p.vX, p.vY, p.vZ]
__size_combined = 1#6

__map_league_baseline : Callable[[DB_PitchDateAverages], list[float]] = \
    lambda p : [p.FastballVelo, p.Fastball4SeamVert]
__size_baseline = 2

__map_pitcher_game : Callable[[DB_PitcherStatcastGame], list[float]] = \
    lambda g: [
        # Fastball group
        1 if g.FastballVelo is not None else 0,
        g.FastballVelo if g.FastballVelo is not None else 90,
        g.FastballBreakHoriz if g.FastballBreakHoriz is not None else 5,
        g.FastballBreakInduced if g.FastballBreakInduced is not None else 10,
        
        # Sinker group
        1 if g.SinkerVelo is not None else 0,
        g.SinkerVelo if g.SinkerVelo is not None else 90,
        g.SinkerBreakHoriz if g.SinkerBreakHoriz is not None else 5,
        g.SinkerBreakInduced if g.SinkerBreakInduced is not None else 10,
    ]
__size_game = 8
    
standard_prep_map = Prep_Map(
    pitch_stuff_map=map_pitch_stuff, pitch_stuff_size=__size_stuff,
    pitch_loc_map=__map_pitch_loc_clamped, pitch_loc_size=__size_loc,
    pitch_combined_map=__map_pitch_combined, pitch_combined_size=__size_combined,
    pitch_overview_map=__map_pitch_overview, pitch_overview_size=__size_overview,
    league_baseline_map=__map_league_baseline, league_baseline_size=__size_baseline,
    pitcher_game_map=__map_pitcher_game, pitcher_game_size=__size_game,
    
    noise_location=__noise_loc,
    noise_stuff=__noise_stuff
)