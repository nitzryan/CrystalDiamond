from Constants import pitch_db

from datetime import datetime
from Stuff.TrainEval.Train_Pitches import Train_Pitches
from Stuff.TrainEval.Eval_Pitches import Eval_Pitches

dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = pitch_db.cursor()

cursor.execute("DELETE FROM Models_PitchValue")
cursor.execute(f"INSERT INTO Models_PitchValue VALUES(1,'Seperate_{day_str}')")
cursor.execute(f"INSERT INTO Models_PitchValue VALUES(2,'Combined_{day_str}')")
pitch_db.commit()

Train_Pitches(4)
Eval_Pitches()