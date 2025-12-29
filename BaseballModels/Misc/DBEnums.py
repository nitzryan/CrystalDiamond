## This is not directly tied to the C# class, so this should be checked when using the python version
# The C# version is the source of truth, this is just for making visualizations for understanding

from enum import IntEnum

class PBP_Events(IntEnum):
    NONE = 0
    SINGLE = 1 << 0
    DOUBLE = 1 << 1
    TRIPLE = 1 << 2
    HR = 1 << 3
    BB = 1 << 4
    HBP = 1 << 5
    K = 1 << 6
    GIDP = 1 << 7
    POPOUT = 1 << 8
    GNDOUT = 1 << 9
    FLYOUT = 1 << 10
    FIELD_ERROR = 1 << 11
    SB = 1 << 12
    CS = 1 << 13
    PB = 1 << 14
    BALK = 1 << 15
    LINEOUT = 1 << 16
    DEF_INDIF = 1 << 17
    WILD_PITCH = 1 << 18
    FORCEOUT = 1 << 19
    SAC_FLY = 1 << 20
    RUNNER_OUT = 1 << 21
    BUNT_OUT = 1 << 22
    IBB = 1 << 23
    SAC_BUNT = 1 << 24
    FIELDERS_CHOICE = 1 << 25
    FB_DOUBLE_PLAY = 1 << 26
    FIELDERS_CHOICE_OUT = 1 << 27
    PICKOFF = 1 << 28
    TRIPLE_PLAY = 1 << 29
    OTHER = 1 << 30
    
class PBP_HitTrajectory(IntEnum):
    NONE = 0
    Groundball = 1 << 0
    Flyball = 1 << 1
    Linedrive = 1 << 2
    Popup = 1 << 3
    BuntGrounder = 1 << 4
    BuntPopup = 1 << 5
    BuntLinedrive = 1 << 6
    
class PBP_HitHardness(IntEnum):
    NONE = 0
    Soft = 1 << 0
    Medium = 1 << 1
    Hard = 1 << 2
    
class BaseOccupancy(IntEnum):
    Empty = 0
    B1 = 1 << 0
    B2 = 1 << 1
    B3 = 1 << 2
    Invalid = 1 << 3