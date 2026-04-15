from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Pro.DataPrep import Prep_Map, Output_Map
from College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as College_Model
from Combined.Model.Model_Train import TrainAndGraph
import torch
from Constants import device
import math
from Pro.Model.Model_Train import NUM_ELEMENTS, ELEMENT_LIST, ELEMENT_LOSS_SCALES
import matplotlib.pyplot as plt

if __name__ == "__main__":
    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(pro_player_condition="WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", pro_player_values=(2025,2015,1), pro_use_cutoff=True,
                                            col_player_condition="WHERE LastYear<=? AND isHitter=?", col_player_values=(2015, 1), col_use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0, True)

    param_values = [0.02, 0.04, 0.06, 0.08, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 1.8, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6]
    param_idx = range(NUM_ELEMENTS)

    for i in tqdm(param_idx, desc="Output Branches"):
        xs = []
        ys = []
        for pv in tqdm(param_values, desc="Scale Values", leave=False):
            pro_model = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            mutators=torch.empty(0),
            data_prep=data_prep.pro_data_prep,
            is_hitter=True,
            ).to(device)
            col_model = College_Model(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=True,
                output_hidden_size=pro_model.GetHiddenSize(),
                output_num_layers=pro_model.GetNumLayers()
            ).to(device)
            
            ELEMENT_LOSS_SCALES[i] = pv
            
            best_loss, last_epoch = TrainAndGraph(
                pro_network=pro_model,
                col_network=col_model,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                pro_model_name="Models/test_pro_hit",
                col_model_name="Models/test_col_hit",
                is_hitter=True,
                should_output=False
            )
            
            xs.append(pv)
            ys.append(best_loss)
            
    

        # Test Loss
        plt.scatter(xs, ys)
        plt.xlabel('Parameter Scale')
        plt.ylabel('Test Loss')
        plt.title(f'Parameter Scale for {ELEMENT_LIST[i]}')
        plt.tight_layout(pad=0.25)
        plt.savefig(f'Combined/Experiments/Results/BranchLearningRates/Hitters_{ELEMENT_LIST[i]}_LearningRate.png', dpi=400)
        plt.clf()