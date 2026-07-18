@echo off
SET DB_FILE=Model.db
if exist %db_file% (
	del %db_file%
)

echo creating database file
sqlite3 %DB_FILE% < creation.sql

python aggregatedTableCreator.py Output_HitterStats ModelRun ModelId,year,month,levelId,mlbId
python aggregatedTableCreator.py Output_PitcherStats ModelRun ModelId,year,month,levelId,mlbId
python aggregatedTableCreator.py Output_PlayerWar ModelRun ModelId,year,month,mlbId
python aggregatedTableCreator.py Output_PlayerHighestLevel ModelRun ModelId,year,month,mlbId
python aggregatedTableCreator.py Output_College_Hitter ModelRun ModelId,year,tbcId
python aggregatedTableCreator.py Output_College_Pitcher ModelRun ModelId,year,tbcId

python linqCreation.py
python pyCreation.py
PAUSE