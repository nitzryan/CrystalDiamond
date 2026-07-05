from Constants import model_db
from Combined.TrainEval.Train_Hitters import Train_Hitters
from Combined.TrainEval.Train_Pitchers import Train_Pitchers
from Combined.TrainEval.Eval_Hitters import Eval_Hitters
from Combined.TrainEval.Eval_Pitchers import Eval_Pitchers

cursor = model_db.cursor()
cursor.execute("DELETE FROM ModelIdx")
model_db.commit()

from datetime import datetime
dt = datetime.now()
year = dt.year
month = dt.month
months = ["", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]
day = dt.day
day_str = f"{day}{months[month]}{year}"

cursor = model_db.cursor()
cursor.execute(f"INSERT INTO ModelIdx VALUES(1,'Base_{day_str}')")
model_db.commit()

num_models = 12

Train_Hitters(num_models)
Train_Pitchers(num_models)
Eval_Hitters(eval_update=False)
Eval_Pitchers(eval_update=False)