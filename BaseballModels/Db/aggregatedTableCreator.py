import sqlite3
import sys
import re

if __name__ == "__main__":
    table = sys.argv[1]
    remove_cols = sys.argv[2]
    idxs_cols = sys.argv[3] if len(sys.argv) >= 4 else None
    cols = remove_cols.split(',')
    new_table = table + "Aggregation"

    db = sqlite3.connect('BaseballStats.db')
    cursor = db.cursor()
    
    if cursor.execute("SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name=?", (new_table,)).fetchone()[0] == 1:
        print(f"Table {table} already exists")
        exit(1)
    
    sql_query : str = cursor.execute("SELECT sql FROM sqlite_schema WHERE type='table' AND name=?", (table,)).fetchone()[0]
    for col in cols:
        # Remove Column
        sql_query = re.sub(rf'^\s*"{re.escape(col)}".*?,\n', '', sql_query, flags=re.MULTILINE)
        # Remove PK
        sql_query = re.sub(rf''',?\s*"{re.escape(col)}"''', '', sql_query)
        
    sql_query = sql_query.replace(table, new_table)
    
    cursor.execute(sql_query)
    if idxs_cols is not None:
        idxs_cols = idxs_cols.split(',')
        idx_query = f'CREATE INDEX "idx_{new_table}" ON "{new_table}" (\n'
        for ic in idxs_cols:
            idx_query += f'\t"{ic}",\n'
        idx_query = idx_query[:-2]
        idx_query += "\n);"
        cursor.execute(idx_query)
    
    db.commit()