from PitchModel.PitchTrackingDBTypes import *
from dataclasses import dataclass
from enum import Enum
from typing import Callable
import math
import torch

class Prep_Map:
    def __init__(self,
                pitch_stuff_map : Callable[[DB_PitchData], list[float]], pitch_stuff_size : int,
                pitch_combined_map : Callable[[DB_PitchData], list[float]], pitch_combined_size : int,
                pitch_stuff_pca_drop : int = 0,
                pitch_combined_pca_drop : int = 0,
                pitch_stuff_names : tuple[str, ...] | None = None,
                pitch_combined_names : tuple[str, ...] | None = None,
    ):
        for label, size, drop, names in (
                ("stuff", pitch_stuff_size, pitch_stuff_pca_drop, pitch_stuff_names),
                ("combined", pitch_combined_size, pitch_combined_pca_drop, pitch_combined_names)):
            if not 0 <= drop < size:
                raise ValueError(f"{label} pca_drop must be in [0, {size - 1}] for {size} features, got {drop}")
            if names is not None and len(names) != size:
                raise ValueError(f"{label} has {len(names)} feature names but a size of {size}")
        
        self.pitch_stuff_map = pitch_stuff_map
        self.pitch_stuff_size = pitch_stuff_size
        
        self.pitch_combined_map = pitch_combined_map
        self.pitch_combined_size = pitch_combined_size
        
        # Lowest-variance PCA components thrown away before the data reaches the model
        self.pitch_stuff_pca_drop = pitch_stuff_pca_drop
        self.pitch_combined_pca_drop = pitch_combined_pca_drop
        
        self.pitch_stuff_names = pitch_stuff_names
        self.pitch_combined_names = pitch_combined_names
    
    @property
    def pitch_stuff_pca_size(self) -> int:
        return self.pitch_stuff_size - self.pitch_stuff_pca_drop
    
    @property
    def pitch_combined_pca_size(self) -> int:
        return self.pitch_combined_size - self.pitch_combined_pca_drop
        
def clamp(value : float, minimum : float, maximum : float) -> float:
    return min(max(value, minimum), maximum)

##############################################################################################
# Configurable input representations, used to test how changing the inputs affects the loss

class SpinAxisEncoding(Enum):
    Raw = "Raw"             # Clamped degrees as a single value
    CosSin = "CosSin"       # (cos, sin) of the axis, so 0 and 360 degrees are the same input
    
class ZoneHeightEncoding(Enum):
    Raw = "Raw"             # ZoneTop, ZoneBot
    Relative = "Relative"   # (PlateZ - ZoneBot) / (ZoneTop - ZoneBot) only
    Both = "Both"           # ZoneTop, ZoneBot, and the relative height
    
@dataclass(frozen=True)
class PrepMapConfig:
    spin_axis : SpinAxisEncoding = SpinAxisEncoding.Raw
    zone_height : ZoneHeightEncoding = ZoneHeightEncoding.Raw   # Combined only, stuff has no location data
    stuff_pca_drop : int = 0
    combined_pca_drop : int = 0
    
    def Describe(self) -> str:
        return (f"SpinAxis={self.spin_axis.value}, ZoneHeight={self.zone_height.value}, "
                f"StuffPcaDrop={self.stuff_pca_drop}, CombinedPcaDrop={self.combined_pca_drop}")

# Matches the original hand-written maps, used by the trained models
STANDARD_PREP_MAP_CONFIG = PrepMapConfig()

# A short zone (bad tracking data) would otherwise blow up the relative height
_MIN_ZONE_HEIGHT = 0.5
_RELATIVE_HEIGHT_RANGE = (-3.0, 4.0)

def GetRelativeZoneHeight(p : DB_PitchData) -> float:
    """0 at the bottom of the zone, 1 at the top"""
    zone_height = max(p.ZoneTop - p.ZoneBot, _MIN_ZONE_HEIGHT)
    return clamp((p.PlateZ - p.ZoneBot) / zone_height, *_RELATIVE_HEIGHT_RANGE)

# Each segment is (names, function returning that many values), concatenated in order to build a map
_Segment = tuple[tuple[str, ...], Callable[[DB_PitchData], list[float]]]

_SEGMENT_MOVEMENT : _Segment = (
    ("Vel", "BreakInduced", "BreakHorizontal", "Extension", "SpinRate"),
    lambda p : [
        max(p.Vel, 65), 
        clamp(p.BreakInduced, -22, 25), 
        clamp(p.BreakHorizontal, -25, 25), 
        clamp(p.Extension, 4.5, 7.75), 
        clamp(p.SpinRate, 500, 3500),
    ])

_SEGMENTS_SPIN_AXIS : dict[SpinAxisEncoding, _Segment] = {
    SpinAxisEncoding.Raw: (
        ("SpinAxis",),
        lambda p : [clamp(p.SpinAxis, 50, 310)]),
    SpinAxisEncoding.CosSin: (
        ("SpinAxisCos", "SpinAxisSin"),
        lambda p : [math.cos(math.radians(p.SpinAxis)), math.sin(math.radians(p.SpinAxis))]),
}

_SEGMENT_SITUATION : _Segment = (
    ("HaaAboveAverage", "VaaAboveAverage", "PitchClass", "CountBalls", "CountStrike", "HitIsR", "PitIsR"),
    lambda p : [
        p.HaaAboveAverage,
        p.VaaAboveAverage,
        p.PitchClass,
        p.CountBalls,
        p.CountStrike,
        p.HitIsR,
        p.PitIsR,
    ])

_SEGMENT_LOCATION : _Segment = (
    ("PlateX", "PlateZ"),
    lambda p : [p.PlateX, p.PlateZ])

_SEGMENTS_ZONE_HEIGHT : dict[ZoneHeightEncoding, _Segment] = {
    ZoneHeightEncoding.Raw: (
        ("ZoneTop", "ZoneBot"),
        lambda p : [p.ZoneTop, p.ZoneBot]),
    ZoneHeightEncoding.Relative: (
        ("ZoneRelativeHeight",),
        lambda p : [GetRelativeZoneHeight(p)]),
    ZoneHeightEncoding.Both: (
        ("ZoneTop", "ZoneBot", "ZoneRelativeHeight"),
        lambda p : [p.ZoneTop, p.ZoneBot, GetRelativeZoneHeight(p)]),
}

def _ComposeSegments(segments : list[_Segment]) -> tuple[Callable[[DB_PitchData], list[float]], tuple[str, ...]]:
    names = tuple(name for segment_names, _ in segments for name in segment_names)
    functions = tuple(function for _, function in segments)
    
    def map_pitch(p : DB_PitchData) -> list[float]:
        values : list[float] = []
        for function in functions:
            values.extend(function(p))
        return values
    
    return map_pitch, names

def CreatePrepMap(config : PrepMapConfig) -> Prep_Map:
    stuff_segments = [_SEGMENT_MOVEMENT, _SEGMENTS_SPIN_AXIS[config.spin_axis], _SEGMENT_SITUATION]
    combined_segments = stuff_segments + [_SEGMENT_LOCATION, _SEGMENTS_ZONE_HEIGHT[config.zone_height]]
    
    stuff_map, stuff_names = _ComposeSegments(stuff_segments)
    combined_map, combined_names = _ComposeSegments(combined_segments)
    
    return Prep_Map(
        pitch_stuff_map=stuff_map, pitch_stuff_size=len(stuff_names),
        pitch_combined_map=combined_map, pitch_combined_size=len(combined_names),
        pitch_stuff_pca_drop=config.stuff_pca_drop,
        pitch_combined_pca_drop=config.combined_pca_drop,
        pitch_stuff_names=stuff_names,
        pitch_combined_names=combined_names,
    )

standard_prep_map = CreatePrepMap(STANDARD_PREP_MAP_CONFIG)
