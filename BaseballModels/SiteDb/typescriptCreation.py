import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import sqlite3

dbSetStrings = []
modelBuilderStrings = []

from DbShared.linqCreation import BooleanTypes

boolean_types = [
    BooleanTypes("DraftRank", ["IsHitter", "IsEligible", "TrainingBias"]),
    BooleanTypes("Player", ["IsHitter", "IsPitcher", "InTraining"]),
    BooleanTypes("PlayerModel", ["IsHitter", "TrainingBias"]),
    BooleanTypes("PlayerRank", ["IsHitter", "TrainingBias"]),
    BooleanTypes("PlayerYearPositions", ["IsHitter"])
                ]

# Get Tables
db = sqlite3.connect('Site.db')
cursor = db.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()

with open(f"../Site/site/src/ts/Global/DBtypes.ts", "w") as file:
    for table, in tables:
        # Get table data
        cursor.execute(f"PRAGMA table_info({table})")
        vals = cursor.fetchall()
        constructorText = "constructor(data : JsonObject)\n\t{\n"
        file.write(f"class DB_{table}\n{{\n")
        for _, name, type, notnull, _, pk in vals:
            if type == "INTEGER":
                typescript_type = "number"
                for bt in boolean_types:
                    if bt.table == table:
                        for col in bt.columns:
                            if col.capitalize() == name.capitalize():
                                typescript_type = "boolean"
                                
            elif type == "REAL":
                typescript_type = "number"
            elif type == "TEXT":
                typescript_type = "string"
            else:
                raise Exception(f"Invalid SQLite type found: {type} for {name}")
            
            file.write(f"\tpublic {name} : {typescript_type}\n")
            constructorText += f"\t\tthis.{name} = data['{name}'] as {typescript_type}\n"
            
        constructorText += "\t}\n"
        file.write("\n\t" + constructorText)
        file.write("}\n\n")
        