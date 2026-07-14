from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Pro.DataPrep import Prep_Map, Output_Map
from Model.College.DataPrep import Prep_Map as C_Prep_Map, Output_Map as C_Output_Map
from tqdm import tqdm
from Model.Pro.Model.Player_Model import RNN_Model as Pro_Model, LayerArch
from Model.College.Model.College_Model import RNN_Model as College_Model
from Model.Combined.Model.Model_Train import TrainAndGraph
from Model.Pro.Model.Model_Train import NUM_ELEMENTS, ELEMENT_LIST, DEFAULT_PRO_ELEMENT_LOSS_SCALES
import torch
import gc
from Model.Constants import device

def sci(n):
    if n == 0: return "0e0"
    s = f"{n:.0e}"
    mant, exp = s.split('e')
    exp = exp.lstrip('+').lstrip('0') or '0'
    return f"{mant}e{exp}"

if __name__ == "__main__":
    prospect_scales = [1e-4, 1e-3, 1e-2, 1e-1, 1, 1e1]
    branch_scales = [1e-4, 1e-3, 1e-2, 1e-1, 1, 1e1]

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

    for i in tqdm(range(1, NUM_ELEMENTS), desc="Branches"):
        xs = []
        ys = []
        zs = []
        for ps in tqdm(prospect_scales, desc="Prospect Scale", leave=False):
            for bs in tqdm(branch_scales, desc="Branch Scale", leave=False):
                pro_element_loss_scales = DEFAULT_PRO_ELEMENT_LOSS_SCALES
                pro_element_loss_scales[0] = ps
                pro_element_loss_scales[i] = bs
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
                
                best_loss, _, _ = TrainAndGraph(
                    pro_network=pro_model,
                    col_network=col_model,
                    train_dataset=train_dataset,
                    test_dataset=test_dataset,
                    pro_model_name="Models/test_pro_hit",
                    col_model_name="Models/test_col_hit",
                    is_hitter=True,
                    should_output=False,
                    num_epochs=101,
                    pro_element_loss_scales=pro_element_loss_scales
                )
                
                del pro_model
                del col_model
                torch.cuda.empty_cache()
                gc.collect()   
                
                xs.append(ps)
                ys.append(bs)
                zs.append(best_loss)
            
        import pandas as pd
        import seaborn as sns
        import matplotlib.pyplot as plt

        df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
        pivot_table = df.pivot(index='y', columns='x', values='z')
        plt.figure(figsize=(1 * len(prospect_scales), .75 * len(branch_scales) + 2))
        sns.heatmap(
            pivot_table, 
            annot=True,
            fmt='.3f',
            cmap='viridis',
            linewidths=0.5,
            xticklabels=[sci(x) for x in prospect_scales],
            yticklabels=[sci(y) for y in branch_scales],
        )
        plt.xlabel('Prospect Scale')
        plt.ylabel('Branch Scale')
        plt.title(f'Test Loss for {ELEMENT_LIST[i]}')
        plt.savefig(f'Combined/Experiments/Results/BranchLearningScales/Hitter_{ELEMENT_LIST[i]}.png', dpi=400)
        plt.clf()