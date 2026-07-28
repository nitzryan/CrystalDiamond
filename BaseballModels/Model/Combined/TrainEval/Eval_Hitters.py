import sys

from Model.Combined.TrainEval.Eval_Players import Eval_Players


def Eval_Hitters(eval_update : bool, train_only : bool):
    Eval_Players(eval_update, is_hitter=True, train_only=train_only)
        
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(f"Expected 2 input arguments, recieved {len(sys.argv) - 1}")
        
    request_type = sys.argv[1]
    train_only = int(sys.argv[2]) == 1
    if request_type == "All":
        Eval_Hitters(False, train_only)
    elif request_type == "Update":
        Eval_Hitters(True, train_only)
    else:
        print(f"Expected 'All' or 'Update', recieved {request_type}")