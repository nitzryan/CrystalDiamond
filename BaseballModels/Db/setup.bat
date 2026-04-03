@echo off
SET DB_FILE=BaseballStats.db
SET DB_BACKUP=Backup.db
if exist %db_backup% (
	del %db_backup%
)
if exist %db_file% (
	ren %db_file% %db_backup%
)
echo creating database file
sqlite3 %DB_FILE% < creation.sql

setlocal EnableDelayedExpansion
IF EXIST %DB_BACKUP% (
	REM Get all tables
	sqlite3 %DB_BACKUP% "SELECT name FROM sqlite_master WHERE type='table';" > tmp.txt
	
	REM Create temporary file for sql commands
	type NUL > tmp.sql
	echo Attach "%DB_FILE%" AS a; >> tmp.sql
	echo Attach "%DB_BACKUP%" AS b; >> tmp.sql
	for /f "tokens=*" %%a in (tmp.txt) do (
		set foo=%%a
		echo INSERT INTO A.!foo! SELECT * FROM B.!foo!; >> tmp.sql
	)
	
	REM update current tables
	sqlite3 < tmp.sql
	
	REM Cleaup like a good citizen
	del tmp.txt
	del tmp.sql
)
endlocal

python linqCreation.py
python pyCreation.py
PAUSE