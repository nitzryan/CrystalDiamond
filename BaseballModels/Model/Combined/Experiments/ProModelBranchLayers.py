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
    layer_sizes = range(10, 141, 10)
    num_layers_list = range(1, 5)

    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(pro_player_condition="WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", pro_player_values=(2025,2015,1), pro_use_cutoff=True,
                                            col_player_condition="WHERE LastYear<=? AND isHitter=?", col_player_values=(2015, 1), col_use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0, True)

    for i in tqdm(range(NUM_ELEMENTS), desc="Branches"):
        xs = []
        ys = []
        zs = []
        for num_layers in tqdm(num_layers_list, desc="Num Layers", leave=False):
            for layer_size in tqdm(layer_sizes, desc="Layer Sizes", leave=False):
                
                layer_arch = LayerArch(layer_size=layer_size, num_layers=num_layers)
                
                # Assign modified layer to specific branch
                warclass_arch : LayerArch = DEFAULT_WARCLASS_ARCH if i != 0 else layer_arch
                lvl_arch : LayerArch = DEFAULT_LVL_ARCH if i != 1 else layer_arch
                pa_arch : LayerArch = DEFAULT_PA_ARCH if i != 2 else layer_arch
                stats_arch : LayerArch = DEFAULT_STATS_ARCH if i != 3 else layer_arch
                pos_arch : LayerArch = DEFAULT_POS_ARCH if i != 4 else layer_arch
                val_arch : LayerArch = DEFAULT_VALUE_ARCH if i != 5 else layer_arch
                pt_arch : LayerArch = DEFAULT_PT_ARCH if i != 6 else layer_arch
                mlbstat_arch : LayerArch = DEFAULT_MLBSTAT_ARCH if i != 7 else layer_arch
                
                
                pro_model = Pro_Model(
                    input_size=train_dataset.GetProInputSize(),
                    mutators=torch.empty(0),
                    data_prep=data_prep.pro_data_prep,
                    is_hitter=True,
                    warclass_arch=warclass_arch,
                    lvl_arch=lvl_arch,
                    pa_arch=pa_arch,
                    stats_arch=stats_arch,
                    pos_arch=pos_arch,
                    val_arch=val_arch,
                    pt_arch=pt_arch,
                    mlbstat_arch=mlbstat_arch,
                ).to(device)
                col_model = College_Model(
                    input_size=train_dataset.GetColInputSize(),
                    data_prep=data_prep.college_data_prep,
                    is_hitter=True,
                    output_hidden_size=pro_model.GetHiddenSize(),
                    output_num_layers=pro_model.GetNumLayers()
                ).to(device)
                
                best_loss, _ = TrainAndGraph(
                    pro_network=pro_model,
                    col_network=col_model,
                    train_dataset=train_dataset,
                    test_dataset=test_dataset,
                    pro_model_name="Models/test_pro_hit",
                    col_model_name="Models/test_col_hit",
                    is_hitter=True,
                    should_output=False,
                    element_to_save=i,
                )
                
                xs.append(layer_size)
                ys.append(num_layers)
                zs.append(best_loss)
            
        import pandas as pd
        import seaborn as sns
        import matplotlib.pyplot as plt

        df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
        pivot_table = df.pivot(index='y', columns='x', values='z')
        plt.figure(figsize=(1 * len(layer_sizes), .75 * len(num_layers_list) + 2))
        sns.heatmap(
            pivot_table, 
            annot=True,
            fmt='.3f',
            cmap='viridis',
            linewidths=0.5,
            xticklabels=[round(x) for x in layer_sizes],
            yticklabels=[round(y) for y in num_layers_list],
        )
        plt.xlabel('Hidden Size')
        plt.ylabel('Num Layers')
        plt.title(f'Test Loss for {ELEMENT_LIST[i]}')
        plt.savefig(f'Combined/Experiments/Results/BranchLayerArch/Hitters_LayerArch_{ELEMENT_LIST[i]}.png', dpi=400)
        plt.clf()