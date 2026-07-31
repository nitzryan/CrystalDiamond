import sqlite3
from dataclasses import dataclass

@dataclass
class AutoincrementPair:
    table : str
    column : str
    
@dataclass
class TypeOverride:
    table : str
    column : str
    structName : str
    
@dataclass
class BooleanTypes:
    table : str
    columns : list[str]

def linqCreation(
            db_name : str,
            autoincrement_pairs : list[AutoincrementPair],
            type_overrides : list[TypeOverride],
            boolean_types : list[BooleanTypes],
            namespace : str,
            dbContextName : str) -> None:
    
    db = sqlite3.connect(db_name)
    cursor = db.cursor()
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
    tables = cursor.fetchall()
    
    dbSetStrings = []
    modelBuilderStrings = []
    
    for table, in tables:
        # Get table data
        cursor.execute(f"PRAGMA table_info({table})")
        vals = cursor.fetchall()
        #Setup DbContext string
        dbSetStrings.append(f"public DbSet<{table}> {table} {{get; set;}}")
        modelBuilderString = f"modelBuilder.Entity<{table}>().HasKey(f => new " + "{"
        cloneFunctionString = f"\n\t\tpublic {table} Clone()\n\t\t{{\n\t\t\treturn new {table}\n\t\t\t{{\n\t\t\t"
        # Write type to class file
        with open(f"sqlTypes/{table}.cs", "w") as classFile:
            classFile.write(f"namespace {namespace}\n{{\n")
            classFile.write(f"\tpublic class {table}\n" + '\t{\n')
            for _, name, type, notnull, _, pk in vals:
                name = name[0].capitalize() + name[1:]
                cloneFunctionString += f"\t{name} = this.{name},\n\t\t\t"
                
                # Need to write primary keys
                if pk > 0:
                    modelBuilderString += f"f.{name},"
                # Convert SQLite type to C# type
                
                if type == "INTEGER":
                    csharp_type = "int"
                elif type == "REAL":
                    csharp_type = "float"
                elif type == "TEXT":
                    csharp_type = "string"
                else:
                    raise Exception(f"Invalid SQLite type found: {type} for {name}")
            
                for to in type_overrides:
                    if (to.table == table) and (to.column == name):
                        csharp_type = to.structName
            
                for bt in boolean_types:
                    if bt.table == table:
                        for col in bt.columns:
                            if col == name:
                                csharp_type = "bool"
                                break
            
                ap = AutoincrementPair(table=table, column=name)
                if notnull == 0:
                    csharp_type += '?'
                elif not ap in autoincrement_pairs:
                    csharp_type = "required " + csharp_type
                classFile.write(f"\t\tpublic {csharp_type} {name} {{get; set;}}\n")
            
            modelBuilderString = modelBuilderString[:-1]
            modelBuilderString += "})"
            
            classFile.write(cloneFunctionString + '};\n\t\t}\n')
            classFile.write('\t}\n}')
            modelBuilderStrings.append(modelBuilderString)
            
    with open(f"{dbContextName}.cs", "w") as file:
        file.write("using Microsoft.EntityFrameworkCore;\n\n")
        file.write(f"namespace {namespace}\n{{\n")
        file.write(f"\tpublic class {dbContextName} : DbContext\n\t{{\n")
        for setString in dbSetStrings:
            file.write("\t\t" + setString + "\n")
        file.write(f"\n\t\tpublic {dbContextName}(DbContextOptions<{dbContextName}> options) : base(options) {{ }}\n")
        file.write("\n\t\tprotected override void OnModelCreating(ModelBuilder modelBuilder)\n\t\t{\n")
        for mbs in modelBuilderStrings:
            file.write("\t\t\t" + mbs + ";\n")
        file.write("\t\t}\n\t}\n}")