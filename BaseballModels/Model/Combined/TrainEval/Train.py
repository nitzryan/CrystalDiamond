from Model.Constants import model_db, NUM_MODEL_VARIANTS
from Model.Combined.TrainEval.Train_Players import Train_Players
from Model.Combined.TrainEval.Eval_Players import Eval_Players

cursor = model_db.cursor()
cursor.execute("DELETE FROM ModelId")
model_db.commit()

from datetime import datetime
dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = model_db.cursor()
cursor.execute(f"INSERT INTO ModelId VALUES(1,'Base_{day_str}')")
model_db.commit()

Train_Players(NUM_MODEL_VARIANTS, True)
Train_Players(NUM_MODEL_VARIANTS, False)
Eval_Players(eval_update=False, is_hitter=True, train_only=False)
Eval_Players(eval_update=False, is_hitter=False, train_only=False)