from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Pro.DataPrep.Prep_Map import MakeMeanRevertPrepMap
from Model.Pro.DataPrep.Output_Map import base_output_map
from Model.College.DataPrep.Output_Map import college_output_map
from Model.College.DataPrep.Prep_Map import college_base_prep_map

from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Constants import device

from Model.Pro.Model.Player_Model import Recurrent_Model as Pro_Model
from Model.College.Model.College_Model import RNN_Model as Col_Model
from Model.Combined.Model.Model_Train import TrainAndGraph

from tqdm import tqdm
import torch
import gc
import matplotlib.pyplot as plt
import sys

def MeanRevertTest(is_hitter : bool) -> None:
    xs = []
    ys = []
    
    NUM_FOLDS = 6
    
    for revert_factor in tqdm(range(1, 102, 5), desc="Reversion Factors"):
        mean_revert_prep_map = MakeMeanRevertPrepMap(revert_factor)
        data_prep = Combined_Data_Prep(
            prep_map=mean_revert_prep_map,
            output_map=base_output_map,
            college_prep_map=college_base_prep_map,
            college_output_map=college_output_map
        )
        
        io_list = data_prep.Generate_IO_Hitters(is_training=True) if is_hitter else data_prep.Generate_IO_Pitchers(is_training=True)
        
        loss = 0
        for fold in tqdm(range(NUM_FOLDS), desc="Training Folds", leave=False):
            train_dataset, test_dataset = Create_Test_Train_Datasets(
                player_list=io_list, 
                is_hitter=is_hitter,
                train_idx=fold)
            
            pro_network = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                data_prep=data_prep.pro_data_prep,
                is_hitter=is_hitter,
            ).to(device)
            col_network = Col_Model(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=is_hitter,
                output_init_state_size=pro_network.GetInitStateSize(),
            ).to(device)
            
            train_results = TrainAndGraph(
                pro_network=pro_network,
                col_network=col_network,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                is_hitter=is_hitter,
                should_output=False,
                col_model_name="Model/Models/no_name_col",
                pro_model_name="Model/Models/no_name_pro",
            )
            
            del train_dataset
            del test_dataset
            del pro_network
            del col_network
            torch.cuda.empty_cache()
            gc.collect()
            
            loss += train_results.best_loss
            
        # Model Blew Up
        if loss > (NUM_FOLDS * 10):
            continue
            
        xs.append(revert_factor)
        ys.append(loss)
        
    
    plt.plot(xs, ys, 'ko')
    plt.ylabel('Loss Across Folds')
    plt.xlabel("PA" if is_hitter else "BF")
    plt.title(f'Loss vs Reversion for {"Hitters" if is_hitter else "Pitchers"}')
    plt.savefig(f'Model/Combined/Tuning/MeanRevertTest{"Hitter" if is_hitter else "Pitcher"}.png')
    
if __name__ == "__main__":
    if (len(sys.argv) != 2):
        raise Exception(f"Expected 2 args, recieved {len(sys.argv)}")
    is_hitter_int = int(sys.argv[1])
    
    MeanRevertTest(is_hitter_int == 1)