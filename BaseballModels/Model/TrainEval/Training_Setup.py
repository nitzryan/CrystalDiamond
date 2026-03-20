import sys
sys.path.append('..')
sys.path.append('../DataPrep/')
sys.path.append('../Model/')

from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

from datetime import datetime
dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = db.cursor()
cursor.execute(f"INSERT INTO ModelIdx VALUES(1,'Base_{day_str}_P','Base_{day_str}_H','Base_{day_str}')")
cursor.execute(f"INSERT INTO ModelIdx VALUES(2,'StatsOnly_{day_str}_P','StatsOnly_{day_str}_H','StatsOnly_{day_str}')")
cursor.execute(f"INSERT INTO ModelIdx VALUES(3,'MeanReg_{day_str}_P','MeanReg_{day_str}_H','MeanReg_{day_str}')")
db.commit()