from Model.Constants import model_db, NUM_MODEL_VARIANTS
from Model.Combined.TrainEval.Train_Hitters import Train_Hitters
from Model.Combined.TrainEval.Train_Pitchers import Train_Pitchers
from Model.Combined.TrainEval.Eval_Hitters import Eval_Hitters
from Model.Combined.TrainEval.Eval_Pitchers import Eval_Pitchers

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

num_models = NUM_MODEL_VARIANTS

Train_Hitters(num_models)
Train_Pitchers(num_models)
Eval_Hitters(eval_update=False, train_only=True)
Eval_Pitchers(eval_update=False, train_only=True)