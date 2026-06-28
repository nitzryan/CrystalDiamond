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
from College.Model.Model_Train import HITTER_ELEMENT_LIST
from Pro.Model.Model_Train import NUM_ELEMENTS
from Pro.Model.Player_Model import LayerArch

if __name__ == "__main__":
    layer_sizes = [8, 16, 32, 64, 128, 256]
    num_layers_list = [1, 2, 4]

    data_prep = Combined_Data_Prep(
        Prep_Map.base_prep_map, 
        Output_Map.base_output_map,
        C_Prep_Map.college_base_prep_map,
        C_Output_Map.college_output_map)

    hitter_io_list = data_prep.Generate_IO_Hitters(is_training=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(
        player_list=hitter_io_list, 
        is_hitter=True)

    num_vars = len(HITTER_ELEMENT_LIST)

    xs = []
    ys = []
    zs = [[] for _ in range(num_vars)]

    for num_layers in tqdm(reversed(num_layers_list), desc="Num Layers", total=len(num_layers_list)):
        for layer_size in tqdm(reversed(layer_sizes), desc="Layer Sizes", leave=False, total=len(layer_sizes)):
            layer_arch = LayerArch(layer_size=layer_size, num_layers=num_layers)
            
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
                output_hidden_size=pro_model.hidden_size,
                output_num_layers=pro_model.num_layers,
                
                draft_arch=layer_arch,
                pos_arch=layer_arch,
                war_arch=layer_arch,
                off_arch=layer_arch,
                def_arch=layer_arch,
                pa_arch=layer_arch
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
            
            xs.append(layer_size)
            ys.append(num_layers)
            
            college_losses = [train_results.test_losses[i][train_results.best_epoch] for i in range(NUM_ELEMENTS, NUM_ELEMENTS + num_vars)]
            for i, loss in enumerate(college_losses):
                zs[i].append(loss)
            
    import pandas as pd
    import seaborn as sns
    import matplotlib.pyplot as plt

    for i, name in enumerate(HITTER_ELEMENT_LIST):
        df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs[i]})
        pivot_table = df.pivot(index='y', columns='x', values='z')
        plt.figure(figsize=(1 * len(layer_sizes), .75 * len(num_layers_list) + 2))
        sns.heatmap(
            pivot_table, 
            annot=True,
            fmt='.3f',
            cmap='viridis',
            linewidths=0.5,
        )
        plt.xlabel('Layer Size')
        plt.ylabel('Num Layers')
        plt.title(f'Test Loss vs COL {name} LayerArch')
        plt.savefig(f'College/Experiments/Results/LayerArch/Hitters_{name}.png', dpi=400)