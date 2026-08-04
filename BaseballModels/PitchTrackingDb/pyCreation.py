import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.pyCreation import pyCreation

insert_tables = []
output_files = ["../PitchModel/PitchTrackingDBTypes.py"]

pyCreation(
    db_name='PitchTracking.db',
    insert_tables=insert_tables,
    output_files=output_files
)