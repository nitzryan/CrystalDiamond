from DBTypes import *
from typing import Callable

class College_Output_Map:
    def __init__(
            self,
            map_draft : Callable[[DB_College_Player], float],
    ):
        
        self.map_draft = map_draft
        
college_output_map = College_Output_Map(
    map_draft=lambda p : p.DraftOvr,
)