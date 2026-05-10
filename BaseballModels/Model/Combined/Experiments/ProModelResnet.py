from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Pro.DataPrep import Prep_Map, Output_Map
from College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Pro.Model.Player_Model import RNN_Model as Pro_Model, LayerArch
from College.Model.College_Model import RNN_Model as College_Model
from Combined.Model.Model_Train import TrainAndGraph
from Pro.Model.Model_Train import NUM_ELEMENTS, ELEMENT_LIST
from Pro.Model.Player_Model import DEFAULT_WARCLASS_ARCH, DEFAULT_STATS_ARCH, DEFAULT_PT_ARCH, DEFAULT_POS_ARCH, DEFAULT_LVL_ARCH, DEFAULT_PA_ARCH, DEFAULT_VALUE_ARCH, DEFAULT_MLBSTAT_ARCH
import torch
from Constants import device

if __name__ == "__main__":
    hidden_size_list = range(5, 56, 5)
    num_blocks_list = range(1, 8)

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
    for num_blocks in tqdm(num_blocks_list, desc="Num Blocks", leave=False):
        for hidden_size in tqdm(hidden_size_list, desc="Hidden Sizes", leave=False):
            pro_model = Pro_Model(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True,
                hidden_size=hidden_size,
                warclass_blocks = num_blocks,
                stats_blocks = num_blocks,
                pt_blocks = num_blocks,
                pos_blocks = num_blocks,
                lvl_blocks = num_blocks,
                pa_blocks = num_blocks,
                value_blocks = num_blocks,
                use_resnet=True
            ).to(device)
            col_model = College_Model(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=True,
                output_hidden_size=pro_model.GetHiddenSize(),
                output_num_layers=pro_model.GetNumLayers(),
                use_resnet=True
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
            
            xs.append(hidden_size)
            ys.append(num_blocks)
            zs.append(best_loss)
        
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt
    
    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(hidden_size_list), .75 * len(num_blocks_list) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=hidden_size_list,
        yticklabels=num_blocks_list,
    )
    plt.xlabel('Hidden Size')
    plt.ylabel('Num Blocks')
    plt.title(f'Test Loss for Pro Model')
    plt.savefig(f'Combined/Experiments/Results/Resnet/Hitters_ResBlocksHiddenSize.png', dpi=400)
    plt.clf()