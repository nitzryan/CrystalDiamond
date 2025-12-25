@echo off

REM Replace placeholder html
set "folder=./src/html/"
for %%F in ("%folder%*.html") do (
    python html_editor.py %%F
)

REM Create CSS Files
python css_bundler.py player.css player.css
python css_bundler.py rankings.css ranking.css
python css_bundler.py teams.css ranking.css
python css_bundler.py home.css home.css
python css_bundler.py stats.css stats.css,ranking_selector.css

REM Compile typescript
tsc -b