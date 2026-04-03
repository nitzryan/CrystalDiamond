@echo off
SET DB_FILE=Model.db
if exist %db_file% (
	del %db_file%
)

echo creating database file
sqlite3 %DB_FILE% < creation.sql

python aggregatedTableCreator.py Output_HitterStats ModelIdx model,year,month,levelId,mlbId
python aggregatedTableCreator.py Output_PitcherStats ModelIdx model,year,month,levelId,mlbId
python aggregatedTableCreator.py Output_PlayerWar ModelIdx model,year,month,mlbId
python aggregatedTableCreator.py Output_College ModelIdx model,year,tbcId


python linqCreation.py
python pyCreation.py
PAUSE