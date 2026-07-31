import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.pyCreation import pyCreation

insert_tables = []
output_files = ["../SwingDecisions/Python/SwingDBTypes.py"]

pyCreation(
    db_name='SwingDecisions.db',
    insert_tables=insert_tables,
    output_files=output_files
)