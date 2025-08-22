@echo off

REM Replace placeholder html
set "folder=./src/html/"
for %%F in ("%folder%*.html") do (
    python html_editor.py %%F
)

REM Compile typescript
tsc