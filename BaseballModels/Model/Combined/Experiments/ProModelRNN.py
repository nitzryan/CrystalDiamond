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

if __name__ == "__main__":

    batch_size = 1600
    num_epochs = 201

    pro_hidden_sizes = range(10, 81, 5)
    pro_num_layers = range(1, 8)

    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(pro_player_condition="WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", pro_player_values=(2025,2015,1), pro_use_cutoff=True,
                                            col_player_condition="WHERE LastYear<=? AND isHitter=?", col_player_values=(2015, 1), col_use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0, True)


    xs = []
    ys = []
    zs = []

    for num_layers in tqdm(pro_num_layers, desc="Num Layers"):
        for hidden_size in tqdm(pro_hidden_sizes, desc="Hidden Sizes", leave=False):
            if num_layers > 4 and hidden_size > 60:
                continue

            col_model = College_Model(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=True,
                output_hidden_size=hidden_size,
                output_num_layers=num_layers
            ).to(device)
            pro_model = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                num_layers=num_layers,
                hidden_size=hidden_size,
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True,
            ).to(device)
            
            best_loss = TrainAndGraph(
                pro_network=pro_model,
                col_network=col_model,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                batch_size=batch_size,
                num_epochs=num_epochs,
                pro_model_name="Models/test_pro_hit",
                col_model_name="Models/test_col_hit",
                is_hitter=True,
                should_output=False
            )
            
            xs.append(hidden_size)
            ys.append(num_layers)
            zs.append(best_loss)
            
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt

    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(pro_hidden_sizes), .75 * len(pro_num_layers) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=[round(x) for x in pro_hidden_sizes],
        yticklabels=[round(y) for y in pro_num_layers],
    )
    plt.xlabel('Hidden Size')
    plt.ylabel('Num Layers')
    plt.title('Test Loss vs Pro RNN Architecture')
    plt.savefig(f'Combined/Experiments/Results/Hitters_ProRecurrent_HiddenSizeNumLayers.png', dpi=400)