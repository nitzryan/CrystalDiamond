from DBTypes import *
from typing import Callable

class Prep_Map:
    def __init__(self,
                pitch_stuff_map : Callable[[DB_PitchStatcast], list[float]], pitch_stuff_size : int,
                pitch_loc_map : Callable[[DB_PitchStatcast], list[float]], pitch_loc_size : int,
                pitch_overview_map : Callable[[DB_PitchStatcast], list[float]], pitch_overview_size : int,
                league_baseline_map : Callable[[DB_PitchDateAverages], list[float]], league_baseline_size : int,
                pitcher_game_map : Callable[[DB_PitcherStatcastGame], list[float]], pitcher_game_size : int,
    ):
        self.pitch_overview_map = pitch_overview_map
        self.pitch_overview_size = pitch_overview_size
        
        self.pitch_stuff_map = pitch_stuff_map
        self.pitch_stuff_size = pitch_stuff_size
        
        self.pitch_loc_map = pitch_loc_map
        self.pitch_loc_size = pitch_loc_size
        
        self.league_baseline_map = league_baseline_map
        self.league_baseline_size = league_baseline_size
        
        self.pitcher_game_map = pitcher_game_map
        self.pitcher_game_size = pitcher_game_size
        

    
__map_pitch_stuff : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.vStart, p.BreakAngle, p.BreakInduced, p.BreakHorizontal, p.Extension, p.x0, p.z0, p.SpinRate, p.SpinDirection, p.aX, p.aY, p.aZ]
__size_stuff = 12
    
__map_pitch_loc : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.pX, p.pZ, p.ZoneTop, p.ZoneBot]
__size_loc = 4
    
__map_pitch_overview : Callable[[DB_PitchStatcast], list[float]] = \
    lambda p : [p.PitchType, p.CountBalls, p.CountStrike, p.Outs, p.BaseOccupancy, p.HitIsR, p.PitIsR]
__size_overview = 7

__map_league_baseline : Callable[[DB_PitchDateAverages], list[float]] = \
    lambda p : [p.Extension, \
        p.FastballVelo, p.Fastball4SeamVert, p.Fastball4SeamHoriz, \
        p.SinkerVelo, p.SinkerVert, p.SinkerHoriz, \
        p.CurveballVelo, p.CurveballVert, p.CurveballHoriz]
__size_baseline = 10

__map_pitcher_game : Callable[[DB_PitcherStatcastGame], list[float]] = \
    lambda g: [
        # Fastball group
        1 if g.FastballVelo is not None else 0,
        g.FastballVelo if g.FastballVelo is not None else 0,
        g.FastballBreakHoriz if g.FastballVelo is not None else 0,
        g.FastballBreakInduced if g.FastballVelo is not None else 0,
        
        # Sinker group
        1 if g.SinkerVelo is not None else 0,
        g.SinkerVelo if g.SinkerVelo is not None else 0,
        g.SinkerBreakHoriz if g.SinkerVelo is not None else 0,
        g.SinkerBreakInduced if g.SinkerVelo is not None else 0,
    ]
__size_game = 8
    
standard_prep_map = Prep_Map(
    pitch_stuff_map=__map_pitch_stuff, pitch_stuff_size=__size_stuff,
    pitch_loc_map=__map_pitch_loc, pitch_loc_size=__size_loc,
    pitch_overview_map=__map_pitch_overview, pitch_overview_size=__size_overview,
    league_baseline_map=__map_league_baseline, league_baseline_size=__size_baseline,
    pitcher_game_map=__map_pitcher_game, pitcher_game_size=__size_game
)