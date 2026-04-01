from DBTypes import *
from typing import Callable
import torch
import numpy as np
from Constants import DTYPE

class College_Output_Map:
    def __init__(
            self,
            map_draft : Callable[[DB_College_Player], float],
            buckets_draft : torch.Tensor
    ):
        
        self.map_draft = map_draft
        self.buckets_draft = buckets_draft
        
college_output_map = College_Output_Map(
    map_draft=lambda p : p.DraftOvr,
    buckets_draft=torch.tensor([0, 10, 30, 100, 500, np.inf], dtype=DTYPE)
)