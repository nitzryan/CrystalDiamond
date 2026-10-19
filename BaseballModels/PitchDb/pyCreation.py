import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.pyCreation import pyCreation

output_files = ["../PitchModel/PitchDBTypes.py"]

pyCreation(
    db_name='Pitch.db',
    insert_tables=False,
    output_files=output_files
)