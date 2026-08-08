from PitchModel.Constants import pitch_db, NUM_TRAINING_VARIANTS

from datetime import datetime
from PitchModel.Stuff.TrainEval.Train_Pitches import Train_Pitches
from PitchModel.Stuff.TrainEval.Eval_Pitches import Eval_Pitches

dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = pitch_db.cursor()

cursor.execute("DELETE FROM Models_PitchValue")
cursor.execute(f"INSERT INTO Models_PitchValue VALUES(1,'Base_1Year_{day_str}')")
cursor.execute(f"INSERT INTO Models_PitchValue VALUES(2,'Base_2Year_{day_str}')")
pitch_db.commit()

START_YEAR = 2020
END_YEAR = 2026
Train_Pitches(num_variants=NUM_TRAINING_VARIANTS, start_year=START_YEAR, end_year=END_YEAR - 1)
Eval_Pitches(end_year=END_YEAR)