@echo off
SET DB_FILE=PitchTracking.db
if exist %db_file% (
	del %db_file%
)
sqlite3 %DB_FILE% < creation.sql

python linqCreation.py
python pyCreation.py
PAUSE