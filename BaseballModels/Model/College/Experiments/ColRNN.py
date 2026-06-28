from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Pro.DataPrep import Prep_Map, Output_Map
from College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as College_Model
from Combined.Model.Model_Train import TrainAndGraph
import torch
import gc
from Constants import device
from Combined.Utilities.GetVariableLossIndex import GetVariableLossIndex

if __name__ == "__main__":
    hidden_sizes = [8, 12, 16, 24, 32, 48, 64]
    num_layers_list = [2, 3, 4, 5, 6]

    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(is_training=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(
        player_list=hitter_io_list, 
        is_hitter=True)

    xs = []
    ys = []
    zs = []
    
    loss_index = GetVariableLossIndex(name="WAR", is_pro=False, is_hitter=True)

    for num_layers in tqdm(reversed(num_layers_list), desc="Num Layers", total=len(num_layers_list)):
        for hidden_size in tqdm(reversed(hidden_sizes), desc="Hidden Sizes", leave=False, total=len(hidden_sizes)):
            
            pro_model = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True,
                use_resnet=False
            ).to(device)
            col_model = College_Model(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=True,
                num_layers=num_layers,
                hidden_size=hidden_size,
                output_hidden_size=pro_model.hidden_size,
                output_num_layers=pro_model.num_layers,
                use_resnet=False,
            ).to(device)
            
            train_results = TrainAndGraph(
                pro_network=pro_model,
                col_network=col_model,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                pro_model_name="Models/test_pro_hit",
                col_model_name="Models/test_col_hit",
                is_hitter=True,
                should_output=False,
                early_stopping_cutoff=10,
            )

            del col_model
            del pro_model       
            torch.cuda.empty_cache()
            gc.collect()     
            
            xs.append(hidden_size)
            ys.append(num_layers)
            zs.append(train_results.test_losses[loss_index][train_results.best_epoch])
            
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt

    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(hidden_sizes), .75 * len(num_layers_list) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
    )
    plt.xlabel('Hidden Size')
    plt.ylabel('Num Layers')
    plt.title('Test Loss vs Col RNN Architecture')
    plt.savefig(f'College/Experiments/Results/Hitters_RNN_HiddenSizeNumLayers.png', dpi=400)