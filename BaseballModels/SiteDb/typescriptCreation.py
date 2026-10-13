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
    BooleanTypes("PlayerYearPositions", ["IsHitter"]),
    BooleanTypes("TeamDraftPlayer", ["IsHitter"]),
                ]

from dataclasses import dataclass
@dataclass
class BlobShape:
    table : str
    column : str
    rows : int
    cols : int
    divisor : int # decoded value = uint16 / divisor

blob_shapes = [
    BlobShape("HitterMlbWarModel", "probsWar", rows=7, cols=8, divisor=65535),
    BlobShape("HitterMlbWarModel", "ranksWar", rows=7, cols=1, divisor=1),
    BlobShape("PitcherMlbWarModel", "probsWarSP", rows=7, cols=8, divisor=65535),
    BlobShape("PitcherMlbWarModel", "probsWarRP", rows=7, cols=8, divisor=65535),
    BlobShape("PitcherMlbWarModel", "ranksWarSP", rows=7, cols=1, divisor=1),
    BlobShape("PitcherMlbWarModel", "ranksWarRP", rows=7, cols=1, divisor=1),
]

# Get Tables
db = sqlite3.connect('Site.db')
cursor = db.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()

with open(f"../Site/site/src/ts/Global/DBtypes.ts", "w") as file:
    # Blob decoding helper used by type constructors
    file.write('''function decodeBlobU16(b64 : string, rows : number, cols : number, divisor : number) : number[][]
{
    const bin = atob(b64)
    if (bin.length !== rows * cols * 2)
        throw new Error(`decodeBlobU16: expected ${rows * cols * 2} bytes for ${rows}x${cols}, got ${bin.length}`)

    let result : number[][] = []
    for (let r = 0; r < rows; r++)
    {
        let row : number[] = []
        for (let c = 0; c < cols; c++)
        {
            const i = (r * cols + c) * 2
            row.push((bin.charCodeAt(i) | (bin.charCodeAt(i + 1) << 8)) / divisor)
        }
        result.push(row)
    }
    return result
}\n\n''')
    
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
            elif type == "BLOB":
                shape = next((b for b in blob_shapes if b.table == table and b.column == name), None)
                if shape is None:
                    raise Exception(f"BLOB {table}.{name} has no BlobShape entry in blob_shapes")
                typescript_type = "number[][]"
            else:
                raise Exception(f"Invalid SQLite type found: {type} for {name}")
            
            if notnull == 0:
                typescript_type += " | null"
            
            file.write(f"\tpublic {name} : {typescript_type}\n")
            if type == "BLOB":
                decode = f"decodeBlobU16(data['{name}'] as string, {shape.rows}, {shape.cols}, {shape.divisor})"
                if notnull == 0:
                    decode = f"data['{name}'] === null ? null : {decode}"
                constructorText += f"\t\tthis.{name} = {decode}\n"
            else:
                constructorText += f"\t\tthis.{name} = data['{name}'] as {typescript_type}\n"
            
        constructorText += "\t}\n"
        file.write("\n\t" + constructorText)
        file.write("}\n\n")
        