import sqlite3
from DBTypes import *
from Utilities import Generate_Mean_Std

def Generate_Hitters_Training() -> any:
    # Get hitters from Model_Players that have exhausted prospect status without MLB
    # Or have exhausted their service time before FA
    db = sqlite3.Connection('../Db/BaseballStats.db')
    cursor = db.cursor()
    hitters = DB_Model_Players.Select_From_DB(cursor, "WHERE lastProspectYear<? AND lastMLBSeason<? AND isHitter=?", (10000,10000,1))
    means, devs = Generate_Mean_Std(DB_Model_Players, hitters)
    print(means.peakWarHitter)
    print(devs.peakWarHitter)
    
    
if __name__ == "__main__":
    Generate_Hitters_Training()
