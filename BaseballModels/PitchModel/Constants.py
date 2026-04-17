import torch
import sqlite3
from pathlib import Path

use_cuda = torch.cuda.is_available()
if (use_cuda):
  device = torch.device("cuda")
else:
  device = torch.device("cpu")
  
__BASE_DIR = Path(__file__).parent.resolve()
__DB_PATH = __BASE_DIR / '../Db/BaseballStats.db'
__MODEL_DB_PATH = __BASE_DIR / "../ModelDb/Model.db"

db = sqlite3.connect(__DB_PATH)
model_db = sqlite3.connect(__MODEL_DB_PATH)

DTYPE = torch.float32