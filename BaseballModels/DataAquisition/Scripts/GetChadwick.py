import sqlite3
import warnings
from tqdm import tqdm
import requests
import pandas as pd
import io

def _Apply_Chadwick_Register(db : sqlite3.Connection):
    FILE_NAME = "https://github.com/chadwickbureau/register/raw/master/data/people-"
    FILE_EXT = ".csv"
    FILE_NUM = [0,1,2,3,4,5,6,7,8,9,"a","b","c","d","e","f"]
    
    db.rollback()
    cursor = db.cursor()
    cursor.execute("BEGIN TRANSACTION")

    with warnings.catch_warnings():
        warnings.simplefilter('ignore', UserWarning)
        for num in tqdm(FILE_NUM, desc="Reading Chadwick Files", leave=False):
            file = FILE_NAME + str(num) + FILE_EXT
            response = requests.get(file)
            if response.status_code != 200:
                print(f"Failed to Get {file} : {response.status_code}")
                continue
            
            reqData = response.content
            df = pd.read_csv(io.StringIO(reqData.decode('utf-8')), on_bad_lines="skip", low_memory=False)
            df = df[df['key_fangraphs'].notna()][df['key_mlbam'].notna()]
            df['key_mlbam'] = df['key_mlbam'].astype('Int64')
            df['key_fangraphs'] = df['key_fangraphs'].astype('Int64')
            for row in df.itertuples():
                cursor.execute(f"UPDATE Player Set fangraphsId='{row.key_fangraphs}' WHERE mlbId='{row.key_mlbam}'")
            
        cursor.execute("END TRANSACTION")
        db.commit()
        
if __name__ == "__main__":
    db = sqlite3.connect("../../../../Db/BaseballStats.db")
    _Apply_Chadwick_Register(db)
    exit(0)