from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Pro.DataPrep import Prep_Map, Output_Map
from College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Pro.Model.Player_Model import RNN_Model as Pro_Model, LayerArch
from College.Model.College_Model import RNN_Model as College_Model
from Combined.Model.Model_Train import TrainAndGraph
from Pro.Model.Model_Train import NUM_ELEMENTS, ELEMENT_LIST
from Pro.Model.Player_Model import DEFAULT_PRO_WEIGHT_DECAY
import torch
import gc
from Constants import device
import numpy as np

if __name__ == "__main__":
    weights = np.logspace(-5, 0, 200)

    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(pro_player_condition="WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", pro_player_values=(2025,2015,1), pro_use_cutoff=True,
                                            col_player_condition="WHERE LastYear<=? AND isHitter=?", col_player_values=(2015, 1), col_use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(
        player_list=hitter_io_list, 
        is_hitter=True)
    
    # Only do shared, because it is the only one with significant change
    xs = []
    zs = []
    for weight in tqdm(weights, desc="Weights", leave=False):
        weight_list = DEFAULT_PRO_WEIGHT_DECAY
        weight_list[0] = weight
        
        pro_model = Pro_Model(
            input_size=train_dataset.GetProInputSize(),
            mutators=torch.empty(0),
            data_prep=data_prep.pro_data_prep,
            is_hitter=True,
            weight_decay=weight_list,
        ).to(device)
        col_model = College_Model(
            input_size=train_dataset.GetColInputSize(),
            data_prep=data_prep.college_data_prep,
            is_hitter=True,
            output_hidden_size=pro_model.GetHiddenSize(),
            output_num_layers=pro_model.GetNumLayers()
        ).to(device)
        
        best_loss, _, _ = TrainAndGraph(
            pro_network=pro_model,
            col_network=col_model,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            pro_model_name="Models/test_pro_hit",
            col_model_name="Models/test_col_hit",
            is_hitter=True,
            should_output=False,
            element_to_save=0,
        )
        
        del pro_model
        del col_model
        torch.cuda.empty_cache()
        gc.collect()   
        
        xs.append(weight)
        zs.append(best_loss)
            
    import matplotlib.pyplot as plt

    plt.plot(xs, zs, 'ko')
    plt.xlabel('Weight')
    plt.xscale('log')
    name = "Shared"
    plt.title(f'Test Loss for {name}')
    plt.savefig(f'Combined/Experiments/Results/ProWeightDecay/Hitters_WeightDecay_{name}_2.png', dpi=400)
    plt.clf()