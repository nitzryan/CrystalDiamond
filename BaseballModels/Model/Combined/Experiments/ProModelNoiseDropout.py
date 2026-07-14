from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Pro.DataPrep import Prep_Map, Output_Map
from Model.College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Model.Pro.Model.Player_Model import RNN_Model as Pro_Model
from Model.College.Model.College_Model import RNN_Model as College_Model
from Model.Combined.Model.Model_Train import TrainAndGraph
import torch
from Model.Constants import device

if __name__ == "__main__":
    noise_levels = [x / 250 for x in range(11)]
    dropout_levels = [x / 50 for x in range(11)]

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

    xs = []
    ys = []
    zs = []

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

    for noise in tqdm(noise_levels, desc="Noise"):
        for dropout in tqdm(dropout_levels, desc="Dropout", leave=False):
            pro_model = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True,
                input_noise=noise,
                rnn_droupout=dropout,
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
            )
            
            xs.append(noise)
            ys.append(dropout)
            zs.append(best_loss)
            
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt

    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(noise_levels), .75 * len(dropout_levels) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=[f"{x:.3f}" for x in noise_levels],
        yticklabels=[f"{y:.3f}" for y in dropout_levels],
    )
    plt.xlabel('Noise')
    plt.ylabel('Dropout')
    plt.title('Test Loss vs Pro RNN Architecture')
    plt.savefig(f'Combined/Experiments/Results/Hitters_ProRecurrent_NoiseDropout.png', dpi=400)