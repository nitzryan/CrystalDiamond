@echo off
SET DB_FILE=BaseballStats.db
ECHO Creating Database File
IF EXIST %DB_FILE% (
	DEL /Q %DB_FILE%
)
sqlite3 BaseballStats.db < creation.sql
PAUSE