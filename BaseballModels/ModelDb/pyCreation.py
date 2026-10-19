import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.pyCreation import pyCreation

output_files = ["../Model/ModelDBTypes.py", "../Misc/ModelDBTypes.py"]

pyCreation(
    db_name='Model.db',
    output_files=output_files,
    include_operations=True,
    insert_tables=True,
)