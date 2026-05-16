from Stuff.DataPrep.DataPrep import DataPrep
from Stuff.DataPrep.PrepMap import standard_prep_map
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import TrainAndGraph, _TOTAL_OUTPUTS
from Constants import device
from tqdm import tqdm

import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import torch
import gc

# Get Data

data_prep = DataPrep(standard_prep_map)
pitch_io_list = data_prep.GenerateIOPitches()
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list, test_size=0.25, random_state=0)
pitch_io_list = None # Clear Memory

dropout_list = [0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6]
wd_list = [1e-5, 3e-5, 1e-4, 3e-4, 1e-3, 3e-3, 1e-2, 3e-2]

xs = []
ys = []
zs = [[] for _ in range(len(_TOTAL_OUTPUTS))]

# Iterate through training

for dropout in tqdm(dropout_list, desc="Dropout"):
    for weight_decay in tqdm(wd_list, desc="Weight Decay", leave=False):
        xs.append(dropout)
        ys.append(weight_decay)
        
        network = PitchModel(data_prep,
            combined_pred_dropout_result=dropout,
            combined_pred_dropout_swing=dropout,
            combined_pred_dropout_inplay=dropout,
            location_pred_dropout_inplay=dropout,
            location_pred_dropout_result=dropout,
            location_pred_dropout_swing=dropout,
            stuff_pred_dropout_inplay=dropout,
            stuff_pred_dropout_result=dropout,
            stuff_pred_dropout_swing=dropout,
            
            combined_result_weight_decay=weight_decay,
            combined_swing_weight_decay=weight_decay,
            combined_inplay_weight_decay=weight_decay,
            location_inplay_weight_decay=weight_decay,
            location_result_weight_decay=weight_decay,
            location_swing_weight_decay=weight_decay,
            stuff_inplay_weight_decay=weight_decay,
            stuff_result_weight_decay=weight_decay,
            stuff_swing_weight_decay=weight_decay,
            ).to(device)
        losses = TrainAndGraph(
            network=network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            batch_size=10000,
            model_name="Models/default",
            should_output=False)
        
        for i, loss in enumerate(losses):
            zs[i].append(loss)
            
        del network
        torch.cuda.empty_cache()
        gc.collect()    
        
        
# Log Results
for i, name in enumerate(_TOTAL_OUTPUTS):
    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs[i]})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(dropout_list), .75 * len(wd_list) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        xticklabels=[f"{x:.2f}" for x in dropout_list],
        yticklabels=[f"{y:.0e}" for y in wd_list],
        linewidths=0.5,
    )
    plt.xlabel('Dropout')
    plt.ylabel('Weight Decay')
    plt.title(f'Test Loss for {name}')
    plt.savefig(f'Stuff/Experiments/Results/DropoutWeightDecay/{name}.png', dpi=400)
    plt.clf()