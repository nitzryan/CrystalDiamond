import sqlite3
import matplotlib.pyplot as plt
import statistics
import numpy as np
from tqdm import tqdm
import math

db = sqlite3.connect('../Db/BaseballStats.db')
cursor = db.cursor()
years = range(2005, 2026)

leagues = cursor.execute(f"SELECT DISTINCT LeagueId FROM Player_Fielder_MonthStats").fetchall()
colors = plt.cm.tab20(np.linspace(0, 1, len(leagues)))
for idx, (league,) in enumerate(tqdm(leagues)):
    abbr, = cursor.execute(f"SELECT abbr FROM Leagues WHERE id={league}").fetchone()
    xs = []
    ys = []
    for year in years:
        fieldValues = cursor.execute(f"SELECT SUM(d_RAA) FROM Player_Fielder_MonthStats WHERE Year={year} AND LeagueId={league} AND Position!=10 GROUP BY MlbId, Month").fetchall()
        values = [fv for fv, in fieldValues]
        if len(values) == 0:
            continue
        # Create probability density 
        stddev = statistics.stdev(values)
        
        xs.append(year)
        ys.append(stddev)
        
    plt.plot(xs, ys, 'o', label=abbr, color=colors[idx])  
 
plt.title(f'Std Dev for Fielding Stats of Different Leagues')
plt.grid(True)
plt.legend(bbox_to_anchor=(1.01, 1), loc='upper left')
plt.xlim(2005, 2026)
plt.xticks([y for y in years if y % 2 == 1])
plt.tight_layout()
plt.savefig(f'LeagueFielding/stddev.png')