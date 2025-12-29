import sqlite3

# Get Tables
db = sqlite3.connect('BaseballStats.db')
cursor = db.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()

insert_tables = ["Model_TrainingHistory", "Output_PlayerWar"]

for fn in ["../Model/DBTypes.py", "../Misc/DBTypes.py"]:
    with open(fn, "w") as file:
        file.write("import sqlite3\n\n")
        for table, in tables:
            cursor.execute(f"PRAGMA table_info({table})")
            vars = cursor.fetchall()
            constructor_string = ""
            tuple_string = ""
            class_name = "DB_" + table
            insert_string = ""
            for i, (_, name, type, notnull, _, pk) in enumerate(vars):
                constructor_string += f"\t\tself.{name} = values[{i}]\n"
                tuple_string += f"self.{name},"
                insert_string += "?,"
            tuple_string = tuple_string[:-1]
            insert_string = insert_string[:-1]
            constructor_string += f"\n\tNUM_ELEMENTS = {len(vars)}\n"
                
            file.write(
    f'''class {class_name}:
\tdef __init__(self, values : tuple[any]):
{constructor_string}
                            
\tdef To_Tuple(self) -> tuple[any]:
\t\treturn ({tuple_string})
                        
\t@staticmethod
\tdef Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['{class_name}']:
\t\titems = cursor.execute("SELECT * FROM {table} " + conditional, values).fetchall()
\t\treturn [{class_name}(i) for i in items]\n\n''')
        
        if table in insert_tables:
            file.write(
f'''\t@staticmethod 
\tdef Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list['{class_name}']) -> None:
\t\tcursor.executemany("INSERT INTO {table} VALUES({insert_string})", [i.To_Tuple() for i in items])
''')
            
        file.write("\n##############################################################################################\n")