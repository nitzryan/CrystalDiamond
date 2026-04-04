from DBTypes import *
from typing import Callable

class College_Output_Map:
    def __init__(
            self,
            map_draft_h : Callable[[DB_College_Player], float],
            map_draft_p : Callable[[DB_College_Player], float],
    ):
        
        self.map_draft_h = map_draft_h
        self.map_draft_p = map_draft_p
        
college_output_map = College_Output_Map(
    map_draft_h=lambda p : p.DraftOvrHitter,
    map_draft_p=lambda p : p.DraftOvrPitcher,
)