@echo off
SET DB_FILE=Site.db
if exist %db_file% (
	del %db_file%
)

echo creating database file
sqlite3 %DB_FILE% < creation.sql

python linqCreation.py
PAUSE