from DBTypes import *
from typing import Callable
import torch
import numpy as np
from Constants import DTYPE

class Output_Map:
    def __init__(self,
                 map_hitter : Callable[[DB_Model_Players], float],
                 buckets_hitter : torch.Tensor,
                 map_pitcher : Callable[[DB_Model_Players], float],
                 buckets_pitcher : torch.Tensor):
        
        self.map_hitter = map_hitter
        self.map_pitcher = map_pitcher
        self.buckets_hitter = buckets_hitter
        self.buckets_pitcher = buckets_pitcher
    
    
war_map = Output_Map(
    map_hitter=lambda p : p.warHitter,
    map_pitcher=lambda p : p.warPitcher,
    buckets_hitter=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE),
    buckets_pitcher=torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE)
)

value_map = Output_Map(
    map_hitter=lambda p : p.valueHitter,
    map_pitcher=lambda p : p.valuePitcher,
    buckets_hitter=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE),
    buckets_pitcher=torch.tensor([0,5,20,50,100,200,np.inf], dtype=DTYPE)
)