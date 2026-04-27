from Constants import pitch_db

from datetime import datetime
dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = pitch_db.cursor()

cursor.execute("DELETE FROM Models_PitchValue")
cursor.execute(f"INSERT INTO Models_PitchValue VALUES(1,'Base_{day_str}')")
pitch_db.commit()