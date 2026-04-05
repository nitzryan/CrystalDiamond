from DBTypes import *
from typing import Callable

class College_Output_Map:
    def __init__(
            self,
            map_draft_h : Callable[[DB_College_Player], float],
            map_draft_p : Callable[[DB_College_Player], float],
            map_pos_h : Callable[[DB_Model_College_HitterProStats], list[float]],
            map_pos_p : Callable[[DB_Model_College_PitcherProStats], list[float]],
            len_pos_h : int,
            len_pos_p : int,
            mask_pos_h : Callable[[DB_Model_College_HitterProStats], float],
            mask_pos_p : Callable[[DB_Model_College_PitcherProStats], float],
    ):
        
        self.map_draft_h = map_draft_h
        self.map_draft_p = map_draft_p
        
        self.map_pos_h = map_pos_h
        self.len_pos_h = len_pos_h
        self.mask_pos_h = mask_pos_h
        self.map_pos_p = map_pos_p
        self.len_pos_p = len_pos_p
        self.mask_pos_p = mask_pos_p
        
college_output_map = College_Output_Map(
    map_draft_h=lambda p : p.DraftOvrHitter,
    map_draft_p=lambda p : p.DraftOvrPitcher,
    
    map_pos_h=lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH],
    len_pos_h=9,
    mask_pos_h=lambda h : min(h.DefOuts / 1000, 1),
    map_pos_p=lambda p : [p.PercSP, p.PercRP],
    len_pos_p=2,
    mask_pos_p=lambda p : min(p.Outs / 500, 1),
)