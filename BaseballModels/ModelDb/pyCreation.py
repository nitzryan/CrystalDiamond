import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.pyCreation import pyCreation

insert_tables = ["Model_TrainingHistory", "Output_PlayerWar"]
output_files = ["../Model/ModelDBTypes.py", "../Misc/ModelDBTypes.py"]

pyCreation(
    db_name='Model.db',
    insert_tables=insert_tables,
    output_files=output_files
)