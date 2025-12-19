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

NUM_LEVELS = 8

WAR_BUCKET_AVG = [0,0.5,3,7.5,15,25,35]
VALUE_BUCKET_AVG = [0,2.5,12.5,35,75,150,250]

DEFAULT_NUM_LAYERS_HITTER = 1
DEFAULT_HIDDEN_SIZE_HITTER = 150
DEFAULT_HITTER_BATCH_SIZE = 800
DEFAULT_HITTER_NUM_EPOCHS = 31

DEFAULT_NUM_LAYERS_PITCHER = 2
DEFAULT_HIDDEN_SIZE_PITCHER = 30
DEFAULT_PITCHER_BATCH_SIZE = 800
DEFAULT_PITCHER_NUM_EPOCHS = 61