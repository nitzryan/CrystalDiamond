from DBTypes import *
from typing import Callable
import math

class Prep_Map:
    def __init__(self,
                 map_bio : Callable[[DB_Model_Players], list[float]], bio_size : int,
                 map_off : Callable[[DB_Model_HitterStats], list[float]], off_size : int,
                 map_bsr : Callable[[DB_Model_HitterStats], list[float]], bsr_size : int,
                 map_def : Callable[[DB_Model_HitterStats], list[float]], def_size : int,
                 map_hitterpt : Callable[[DB_Model_HitterStats], list[float]], hitterpt_size : int,
                 map_pitcherpt : Callable[[DB_Model_PitcherStats], list[float]], pitcherpt_size : int,
                 map_hitterlvl : Callable[[DB_Model_HitterStats], list[float]], hitterlvl_size : int,
                 map_pitcherlvl : Callable[[DB_Model_PitcherStats], list[float]], pitcherlvl_size : int,
                 map_pit : Callable[[DB_Model_PitcherStats], list[float]], pit_size : int,
                 map_hit_first : Callable[[DB_Model_HitterStats, float], list[float]], hitfirst_size : int,
                 map_pit_first : Callable[[DB_Model_PitcherStats, float], list[float]], pitfirst_size : int,
                 map_mlb_hit_value : Callable[[DB_Player_MonthlyWar], list[float]], mlb_hit_value_size : int,
                 map_mlb_pit_value : Callable[[DB_Player_MonthlyWar], list[float]], mlb_pit_value_size : int
                 ):
        
        self.map_bio = map_bio
        self.map_off = map_off
        self.map_bsr = map_bsr
        self.map_def = map_def
        self.map_hitterpt = map_hitterpt
        self.map_pitcherpt = map_pitcherpt
        self.map_hitterlvl = map_hitterlvl
        self.map_pitcherlvl = map_pitcherlvl
        self.map_pit = map_pit
        self.map_hit_first = map_hit_first
        self.map_pit_first = map_pit_first
        
        self.bio_size = bio_size
        self.off_size = off_size
        self.bsr_size = bsr_size
        self.def_size = def_size
        self.hitterpt_size = hitterpt_size
        self.pitcherpt_size = pitcherpt_size
        self.hitterlvl_size = hitterlvl_size
        self.pitcherlvl_size = pitcherlvl_size
        self.pit_size = pit_size
        self.hitfirst_size = hitfirst_size
        self.pitfirst_size = pitfirst_size
        
        self.map_mlb_hit_value = map_mlb_hit_value
        self.mlb_hit_value_size = mlb_hit_value_size
        self.map_mlb_pit_value = map_mlb_pit_value
        self.mlb_pit_value_size = mlb_pit_value_size
        
__map_mlb_hit_value : Callable[[DB_Player_MonthlyWar], list[float]] = \
    lambda h : [h.PA, h.WAR_h, h.OFF, h.DEF, h.BSR, h.REP]    
__map_mlb_pit_value : Callable[[DB_Player_MonthlyWar], list[float]] = \
    lambda p : [p.IP_SP, p.IP_RP, p.WAR_s, p.WAR_r]
__mlb_hit_value_size = 6
__mlb_pit_value_size = 4
       
        
# Comments are explained variance ratios
base_prep_map = Prep_Map(
    map_bio=lambda p : [p.ageAtSigningYear, math.log10(p.draftPick), math.log10(p.draftSignRank)],
    map_off=lambda h : [h.ParkHRFactor, h.ParkRunFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wRC, h.HRPercRatio, h.BBPercRatio, h.kPercRatio],
    map_bsr=lambda h : [h.SBRateRatio, h.SBPercRatio],
    map_def=lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH],
    map_hitterpt=lambda h : [h.PA, h.MonthFrac, h.InjStatus],
    map_pitcherpt=lambda p : [p.BF, p.MonthFrac, p.InjStatus],
    map_hitterlvl=lambda h : [h.Age, h.LevelId, h.Month],
    map_pitcherlvl=lambda p : [p.Age, p.LevelId, p.Month],
    map_pit=lambda p : [p.ParkRunFactor, p.ParkHRFactor, p.GBPercRatio, p.ERARatio, p.FIPRatio, p.wOBARatio, p.HRPercRatio, p.BBPercRatio, p.KPercRatio],
    map_pit_first=lambda p, y : 1 if p.Year == y else 0,
    map_hit_first=lambda h, y : 1 if h.Year == y else 0,
    map_mlb_hit_value=__map_mlb_hit_value,
    map_mlb_pit_value=__map_mlb_pit_value,
    mlb_hit_value_size=__mlb_hit_value_size,
    mlb_pit_value_size=__mlb_pit_value_size,
    bio_size=2,
    hitterlvl_size=3,
    hitterpt_size=3,
    off_size=7,
    bsr_size=2,
    def_size=9,
    hitfirst_size=1,
    pitcherlvl_size=3,
    pitcherpt_size=3,
    pit_size=8,
    pitfirst_size=1
)

experimental_prep_map = Prep_Map(
    map_bio=lambda p : [p.ageAtSigningYear, math.log10(p.draftPick), math.log10(p.draftSignRank)],
    map_off=lambda h : [math.sqrt(h.AVGRatio), math.sqrt(h.OBPRatio), math.sqrt(h.ISORatio), h.wRC, math.sqrt(h.HRPercRatio), math.sqrt(h.BBPercRatio), math.sqrt(h.kPercRatio), h.crOFF, h.crDEF, h.crWAR],
    map_bsr=lambda h : [math.sqrt(h.SBRateRatio), h.SBPercRatio],
    map_def=lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH],
    map_hitterpt=lambda h : [h.PA, h.MonthFrac, h.InjStatus],
    map_pitcherpt=lambda p : [p.BF, p.MonthFrac, p.InjStatus],
    map_hitterlvl=lambda h : [h.Age, h.LevelId, h.Month],
    map_pitcherlvl=lambda p : [p.Age, p.LevelId, p.Month],
    map_pit=lambda p : [p.GBPercRatio, math.sqrt(p.ERARatio), math.sqrt(p.FIPRatio + 1.5), math.sqrt(p.wOBARatio), math.sqrt(p.HRPercRatio), math.sqrt(p.BBPercRatio), math.sqrt(p.KPercRatio), p.crWAR],
    map_pit_first=lambda p, y : 1 if p.Year == y else 0,
    map_hit_first=lambda h, y : 1 if h.Year == y else 0,
    map_mlb_hit_value=__map_mlb_hit_value,
    map_mlb_pit_value=__map_mlb_pit_value,
    mlb_hit_value_size=__mlb_hit_value_size,
    mlb_pit_value_size=__mlb_pit_value_size,
    bio_size=2,
    hitterlvl_size=3,
    hitterpt_size=3,
    off_size=7,
    bsr_size=2,
    def_size=4,
    hitfirst_size=1,
    pitcherlvl_size=3,
    pitcherpt_size=3,
    pit_size=7,
    pitfirst_size=1
)

statsonly_prep_map = Prep_Map(
    map_bio=lambda p : [p.ageAtSigningYear],
    map_off=lambda h : [h.ParkHRFactor, h.ParkRunFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wRC, h.HRPercRatio, h.BBPercRatio, h.kPercRatio],
    map_bsr=lambda h : [h.SBRateRatio, h.SBPercRatio],
    map_def=lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH],
    map_hitterpt=lambda h : [h.PA, h.MonthFrac, h.InjStatus],
    map_pitcherpt=lambda p : [p.BF, p.MonthFrac, p.InjStatus],
    map_hitterlvl=lambda h : [h.Age, h.LevelId, h.Month],
    map_pitcherlvl=lambda p : [p.Age, p.LevelId, p.Month],
    map_pit=lambda p : [p.ParkRunFactor, p.ParkHRFactor, p.GBPercRatio, p.ERARatio, p.FIPRatio, p.wOBARatio, p.HRPercRatio, p.BBPercRatio, p.KPercRatio],
    map_pit_first=lambda p, y : 1 if p.Year == y else 0,
    map_hit_first=lambda h, y : 1 if h.Year == y else 0,
    map_mlb_hit_value=__map_mlb_hit_value,
    map_mlb_pit_value=__map_mlb_pit_value,
    mlb_hit_value_size=__mlb_hit_value_size,
    mlb_pit_value_size=__mlb_pit_value_size,
    bio_size=1,
    hitterlvl_size=3,
    hitterpt_size=3,
    off_size=7,
    bsr_size=2,
    def_size=9,
    hitfirst_size=1,
    pitcherlvl_size=3,
    pitcherpt_size=3,
    pit_size=8,
    pitfirst_size=1
)