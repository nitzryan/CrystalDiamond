import torch
import sqlite3
from pathlib import Path

use_cuda = torch.cuda.is_available()
if (use_cuda):
  device = torch.device("cuda")
else:
  device = torch.device("cpu")
  
__BASE_DIR = Path(__file__).parent.resolve()
__TRACKING_DB_PATH = __BASE_DIR / '../PitchTrackingDb/PitchTracking.db'
__PITCH_DB_PATH = __BASE_DIR / "../PitchDb/Pitch.db"

tracking_db = sqlite3.connect(__TRACKING_DB_PATH)
pitch_db = sqlite3.connect(__PITCH_DB_PATH)

DTYPE = torch.float32

MLB_LEVEL_ID = 1

# Pitch Results
PITCH_RESULT_CALLEDSTRIKE = 1
PITCH_RESULT_SWINGSTRIKE = 2
PITCH_RESULT_FOUL = 3
PITCH_RESULT_BALL = 4
PITCH_RESULT_INPLAY = 5
PITCH_RESULT_HBP = 6

INIT_PITCH_RESULT_RATES = {
    PITCH_RESULT_CALLEDSTRIKE: 0.162,
    PITCH_RESULT_SWINGSTRIKE: 0.120,
    PITCH_RESULT_FOUL: 0.183,
    PITCH_RESULT_BALL: 0.358,
    PITCH_RESULT_INPLAY: 0.174,
    PITCH_RESULT_HBP: 0.003,
}

NUM_TRAINING_VARIANTS = 12
TRAIN_TEST_RATIO = 3
assert NUM_TRAINING_VARIANTS % (TRAIN_TEST_RATIO + 1) == 0

BUCKET_INPLAY_VALUE = torch.tensor([-0.2, -0.1, 0, 0.1, 0.2, 0.3, 0.5])
INIT_INPLAY_BUCKET_RATES = [0.05, 0.10, 0.15, 0.20, 0.20, 0.15, 0.10, 0.05]
assert len(INIT_INPLAY_BUCKET_RATES) == BUCKET_INPLAY_VALUE.size(0) + 1

DATA_PREP_BINARY_ALL_FILE = "Binaries/DataPrepAll.pkl"