import sys

from Model.Combined.TrainEval.Eval_Players import Eval_Players


def Eval_Hitters(eval_update : bool):
    Eval_Players(eval_update, is_hitter=True)
        
if __name__ == "__main__":
    if len(sys.argv) != 2:
        print(f"Expected 1 input argument recieved {len(sys.argv) - 1}")
        
    request_type = sys.argv[1]
    if request_type == "All":
        Eval_Hitters(False)
    elif request_type == "Update":
        Eval_Hitters(True)
    else:
        print(f"Expected 'All' or 'Update', recieved {request_type}")