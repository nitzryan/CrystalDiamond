import torch
import sqlite3
import numpy as np
from pathlib import Path

use_cuda = torch.cuda.is_available()
if (use_cuda):
  device = torch.device("cuda")
else:
  device = torch.device("cpu")
  
__BASE_DIR = Path(__file__).parent.resolve()
__DB_PATH = __BASE_DIR / '../Db/BaseballStats.db'
__EXP_DB_PATH = __BASE_DIR / '../Db/Experiment.db'
__MODEL_DB_PATH = __BASE_DIR / "../ModelDb/Model.db"

db = sqlite3.connect(__DB_PATH)
model_db = sqlite3.connect(__MODEL_DB_PATH)
experimental_db = sqlite3.connect(__EXP_DB_PATH)

DTYPE = torch.float32

#HITTER_TOTAL_WAR_BUCKETS = torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE)
HITTER_PEAK_WAR_BUCKETS = torch.tensor([0, 0.5, 1, 2, 3, 4, 5, 7, np.inf], dtype=DTYPE)
HITTER_LEVEL_BUCKETS = torch.tensor([1,2,3,4,5,6,7,8], dtype=DTYPE)
HITTER_PA_BUCKETS = torch.tensor([0, 50, 200, 1000, 2000, np.inf], dtype=DTYPE)

#PITCHER_TOTAL_WAR_BUCKETS = torch.tensor([0,1,5,10,20,30,np.inf], dtype=DTYPE)
PITCHER_PEAK_WAR_BUCKETS = torch.tensor([0, 0.5, 1, 2, 3, 4, 5, 7, np.inf], dtype=DTYPE)
PITCHER_LEVEL_BUCKETS = torch.tensor([1,2,3,4,5,6,7,8], dtype=DTYPE)
PITCHER_BF_BUCKETS = torch.tensor([0, 50, 200, 1000, 2000, np.inf], dtype=DTYPE)

WARQUANTILE_VALUES = [0.05, 0.15, 0.25, 0.35, 0.5, 0.65, 0.75, 0.85, 0.95]
WARQUANTILE_INVS = [x-1 for x in WARQUANTILE_VALUES]

# Draft buckets
DRAFT_BUCKETS = torch.tensor([0, 5, 15, 30, 100, 500, np.inf], dtype=DTYPE)
DRAFT_MEANS = torch.tensor([4000, 3, 10.5, 22.5, 65.5, 300.5, 1000])

NUM_LEVELS = 8

WAR_BUCKET_AVG = [0,0.5,3,7.5,15,25,35]
VALUE_BUCKET_AVG = [0,2.5,12.5,35,75,150,250]

DEFAULT_NUM_LAYERS_HITTER = 1
DEFAULT_HIDDEN_SIZE_HITTER = 150
DEFAULT_HITTER_BATCH_SIZE = 800
DEFAULT_HITTER_NUM_EPOCHS = 41

DEFAULT_NUM_LAYERS_PITCHER = 2
DEFAULT_HIDDEN_SIZE_PITCHER = 30
DEFAULT_PITCHER_BATCH_SIZE = 800
DEFAULT_PITCHER_NUM_EPOCHS = 41

