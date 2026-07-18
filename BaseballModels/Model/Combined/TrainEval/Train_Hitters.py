from Model.Combined.TrainEval.Train_Players import Train_Players

def Train_Hitters(num_models : int):
    Train_Players(num_models, is_hitter=True)