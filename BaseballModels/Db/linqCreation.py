import sqlite3

dbSetStrings = []
modelBuilderStrings = []

# Get Tables
db = sqlite3.connect('BaseballStats.db')
cursor = db.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
tables = cursor.fetchall()

for table, in tables:
    # Get table data
    cursor.execute(f"PRAGMA table_info({table})")
    vals = cursor.fetchall()
    # print(table)
    # print([description[0] for description in cursor.description])
    # print(vals)
    #Setup DbContext string
    dbSetStrings.append(f"public DbSet<{table}> {table} {{get; set;}}")
    modelBuilderString = f"modelBuilder.Entity<{table}>().HasKey(f => new " + "{"
    # Write type to class file
    with open(f"sqlTypes/{table}.cs", "w") as classFile:
        classFile.write("namespace Db\n{\n")
        classFile.write(f"\tpublic class {table}\n" + '\t{\n')
        for _, name, type, notnull, _, pk in vals:
            name = name.capitalize()
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
        
            if notnull == 0:
                csharp_type += '?'
            else:
                csharp_type = "required " + csharp_type
            classFile.write(f"\t\tpublic {csharp_type} {name} {{get; set;}}\n")
        
        modelBuilderString = modelBuilderString[:-1]
        modelBuilderString += "})"
        classFile.write('\t}\n}')
        modelBuilderStrings.append(modelBuilderString)
        
with open(f"SqliteDbContext.cs", "w") as file:
    file.write("using Microsoft.EntityFrameworkCore;\n\n")
    file.write("namespace Db\n{\n")
    file.write("\tpublic class SqliteDbContext : DbContext\n\t{\n")
    for setString in dbSetStrings:
        file.write("\t\t" + setString + "\n")
    file.write("\n\t\tpublic SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }\n")
    file.write("\n\t\tprotected override void OnModelCreating(ModelBuilder modelBuilder)\n\t\t{\n")
    for mbs in modelBuilderStrings:
        file.write("\t\t\t" + mbs + ";\n")
    file.write("\t\t}\n\t}\n}")