@echo off
SET DB_FILE=Pitch.db
if exist %db_file% (
	del %db_file%
)

echo creating database file
sqlite3 %DB_FILE% < creation.sql

python aggregatedTableCreator.py Output_PitchValue ModelRun model,gameId,pitchId

python linqCreation.py
python pyCreation.py
PAUSE