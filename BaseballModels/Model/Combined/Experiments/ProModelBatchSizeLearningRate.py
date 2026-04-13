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

    batch_sizes = range(200, 4801, 400)
    learning_rates = [x / 2000 for x in range(10)]
    num_epochs = 201

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

    for batch_size in tqdm(batch_sizes, desc="Batch Size"):
        for lr in tqdm(learning_rates, desc="Dropout", leave=False):
            pro_model = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True,
            ).to(device)
            
            pro_model.optimizer.param_groups[0]["lr"] = lr
            
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
            
            xs.append(batch_size)
            ys.append(lr)
            zs.append(best_loss)
            
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt

    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(batch_sizes), .75 * len(learning_rates) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=["{x}" for x in batch_sizes],
        yticklabels=[f"{y:.4f}" for y in learning_rates],
    )
    plt.xlabel('Batch Size')
    plt.ylabel('Learning Rates')
    plt.title('Test Loss vs Training Params')
    plt.savefig(f'Combined/Experiments/Results/Hitters_ProBatchSizeLearningRate.png', dpi=400)