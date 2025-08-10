import sqlite3

# Get Tables
db = sqlite3.connect('BaseballStats.db')
cursor = db.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()

insert_tables = ["Model_TrainingHistory", "Output_PlayerWar"]

class DB_Model_TrainingHistory:
    def __init__(self, values : tuple[any]):
        self.ModelName = values[0]
        self.Year = values[1]
        self.IsHitter = values[2]
        self.TestLoss = values[3]
        self.NumLayers = values[4]
        self.HiddenSize = values[5]
        self.ModelIdx = values[6]
        
    def To_Tuple(self) -> tuple[any]:
        return (self.ModelName, self.Year, self.IsHitter, self.TestLoss, self.NumLayers, self.HiddenSize, self.ModelIdx)
    
    @staticmethod 
    def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list['DB_Model_TrainingHistory']) -> None:
        cursor.executemany("INSERT INTO Model_TrainingHistory VALUES(?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])
    
    @staticmethod
    def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_TrainingHistory']:
        items = cursor.execute("SELECT * FROM Model_TrainingHistory " + conditional, values).fetchall()
        return [DB_Model_TrainingHistory(i) for i in items]

with open("../Model/DBTypes.py", "w") as file:
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