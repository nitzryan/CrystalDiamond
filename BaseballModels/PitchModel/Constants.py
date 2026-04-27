import torch
import sqlite3
from pathlib import Path
from line_profiler import LineProfiler

use_cuda = torch.cuda.is_available()
if (use_cuda):
  device = torch.device("cuda")
else:
  device = torch.device("cpu")
  
__BASE_DIR = Path(__file__).parent.resolve()
DB_PATH = __BASE_DIR / '../Db/BaseballStats.db'
__PITCH_DB_PATH = __BASE_DIR / "../PitchDb/Pitch.db"

db = sqlite3.connect(DB_PATH)
pitch_db = sqlite3.connect(__PITCH_DB_PATH)

DTYPE = torch.float32

profiler = LineProfiler()