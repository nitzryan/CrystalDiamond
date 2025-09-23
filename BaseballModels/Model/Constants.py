import torch
import sqlite3
import numpy as np

use_cuda = torch.cuda.is_available()
if (use_cuda):
  device = torch.device("cuda")
else:
  device = torch.device("cpu")
  
db = sqlite3.connect('../Db/BaseballStats.db')
experimental_db = sqlite3.connect('../Db/Experiment.db')

DTYPE = torch.float32

#HITTER_TOTAL_WAR_BUCKETS = torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE)
HITTER_PEAK_WAR_BUCKETS = torch.tensor([0, 0.5, 1, 2, 3, 4, 5, 7, np.inf], dtype=DTYPE)
HITTER_LEVEL_BUCKETS = torch.tensor([1,2,3,4,5,6,7,8], dtype=DTYPE)
HITTER_PA_BUCKETS = torch.tensor([0, 50, 200, 1000, 2000, np.inf], dtype=DTYPE)

#PITCHER_TOTAL_WAR_BUCKETS = torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE)
PITCHER_PEAK_WAR_BUCKETS = torch.tensor([0, 0.5, 1, 2, 3, 4, 5, 7, np.inf], dtype=DTYPE)
PITCHER_LEVEL_BUCKETS = torch.tensor([1,2,3,4,5,6,7,8], dtype=DTYPE)
PITCHER_BF_BUCKETS = torch.tensor([0, 50, 200, 1000, 2000, np.inf], dtype=DTYPE)