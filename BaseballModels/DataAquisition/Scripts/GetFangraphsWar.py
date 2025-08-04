import sqlite3
import pybaseball
import sys
from tqdm import tqdm

def _Update_Fangraphs_War(db : sqlite3.Connection, year):
    cursor = db.cursor()
    cursor.execute("DELETE FROM Player_YearlyWar WHERE year=?", (year,))
    db.commit()
    cursor = db.cursor()
    
    cursor.execute("BEGIN TRANSACTION")
    hittingStats = pybaseball.batting_stats(year, qual=0, stat_columns=["PA", "BsR", "Off", "Def", "WAR", "OPS"])
    for row in tqdm(hittingStats.itertuples(), desc="Fangraphs Hitter War"):
        try:
            mlbId = cursor.execute("SELECT mlbId FROM Player WHERE fangraphsId=?", (row.IDfg,)).fetchone()[0]
            if mlbId == None or cursor.execute("SELECT COUNT(*) FROM Player_YearlyWar WHERE mlbId=? AND year=? AND position=?", (mlbId, year, "hitting")).fetchone()[0] > 0:
                continue
            
            cursor.execute("INSERT INTO Player_YearlyWar VALUES(?,?,?,?,?,?,?,?)", (mlbId, year, "hitting", row.PA, row.WAR, row.Off, row.Def, row.BsR))
        except: # Player doesn't exist in table
            pass
    
    
    # Similar to above, W is needed
    pitchingStats = pybaseball.pitching_stats(year, qual=0, stat_columns = ["IP", "WAR", "W"])
    for row in tqdm(pitchingStats.itertuples(), desc="Fangraphs Pitcher War"):
        try:
            mlbId = cursor.execute("SELECT mlbId FROM Player WHERE fangraphsId=?", (row.IDfg,)).fetchone()[0]
            if mlbId == None or cursor.execute("SELECT COUNT(*) FROM Player_YearlyWar WHERE mlbId=? AND year=? and position=?", (mlbId, year, "pitching")).fetchone()[0] > 0:
                continue
            
            innings, subinnings = str(row.IP).split('.')
            outs = 3 * int(innings) + int(subinnings)
            
            cursor.execute("INSERT INTO Player_YearlyWar VALUES(?,?,?,?,?,?,?,?)", (mlbId, year, "pitching", outs, row.WAR, 0, 0, 0))
        except:
            pass
        
    cursor.execute("END TRANSACTION")
    
if __name__ == "__main__":
    db = sqlite3.connect("../../../../Db/BaseballStats.db")
    year = int(sys.argv[1])
    _Update_Fangraphs_War(db, year)
    exit(0)