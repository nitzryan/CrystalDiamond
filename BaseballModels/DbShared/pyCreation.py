import sqlite3

def pyCreation(
            db_name : str,
            insert_tables : bool,
            output_files : list[str],
            include_operations : bool = False
            ) -> None:
    db = sqlite3.connect(db_name)
    cursor = db.cursor()
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
    tables = cursor.fetchall()

    for fn in output_files:
        with open(fn, "w") as file:
            file.write("from __future__ import annotations\nimport sqlite3\n\n")
            for table, in tables:
                cursor.execute(f"PRAGMA table_info({table})")
                vars = cursor.fetchall()
                constructor_string = ""
                tuple_string = ""
                class_name = "DB_" + table
                insert_string = ""
                table_addition_string = ""
                table_mult_string = ""
                slots_string = ""
                for i, (_, name, type, notnull, _, pk) in enumerate(vars):
                    constructor_string += f"\t\tself.{name} = values[{i}]\n"
                    tuple_string += f"self.{name},"
                    insert_string += "?,"
                    slots_string += f"'{name}',"
                    if include_operations and type == "REAL":
                        table_addition_string += f"\t\t\tself.{name} += other.{name}\n"
                        table_mult_string += f"\t\tself.{name} *= m\n"
                tuple_string = tuple_string[:-1]
                insert_string = insert_string[:-1]
                slots_string = slots_string[:-1]
                constructor_string += f"\n\tNUM_ELEMENTS = {len(vars)}\n"
                    
                if table_addition_string == "":
                    table_addition_string = "\t\t\traise Exception('Type invalid for addition')"
                    table_mult_string = "\t\traise Exception('Type invalid for multiplication')"
                    
                file.write(
        f'''class {class_name}:
\t__slots__ = ({slots_string})

\tdef __init__(self, values : tuple[any]):
{constructor_string}
                            
\tdef To_Tuple(self) -> tuple[any]:
\t\treturn ({tuple_string})
                        
\t@staticmethod
\tdef Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[{class_name}]:
\t\titems = cursor.execute("SELECT * FROM {table} " + conditional, values).fetchall()
\t\treturn [{class_name}(i) for i in items]\n\n''')
            
                if insert_tables:
                    file.write(
f'''\t@staticmethod 
\tdef Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[{class_name}]) -> None:
\t\tcursor.executemany("INSERT INTO {table} VALUES({insert_string})", [i.To_Tuple() for i in items])
''')
                if include_operations:
                    file.write(
f'''
\tdef __add__(self, other : {class_name}) -> {class_name}:
\t\tif isinstance(other, {class_name}):
{table_addition_string}
\t\treturn self
\t\treturn NotImplemented
                    
\tdef __mul__(self, m : float) -> {class_name}:
{table_mult_string}
\t\treturn self
''')
                file.write("\n##############################################################################################\n")