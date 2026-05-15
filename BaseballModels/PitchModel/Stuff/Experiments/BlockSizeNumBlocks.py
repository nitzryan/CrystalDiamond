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

num_blocks = [8, 4, 2, 1]
block_sizes = [512, 256, 128, 64]

xs = []
ys = []
zs = [[] for _ in range(len(_TOTAL_OUTPUTS))]

# Iterate through training

for nb in tqdm(num_blocks, desc="Num Blocks"):
    for bs in tqdm(block_sizes, desc="Block Size", leave=False):
        if nb == 8 and bs == 512:
            continue
        
        xs.append(bs)
        ys.append(nb)
        
        network = PitchModel(data_prep,
            combined_pred_size_result=bs,
            combined_pred_size_swing=bs,
            combined_pred_size_inplay=bs,
            location_pred_size_inplay=bs,
            location_pred_size_result=bs,
            location_pred_size_swing=bs,
            stuff_pred_size_inplay=bs,
            stuff_pred_size_result=bs,
            stuff_pred_size_swing=bs,
            
            combined_pred_blocks_result=nb,
            combined_pred_blocks_swing=nb,
            combined_pred_blocks_inplay=nb,
            location_pred_blocks_inplay=nb,
            location_pred_blocks_result=nb,
            location_pred_blocks_swing=nb,
            stuff_pred_blocks_inplay=nb,
            stuff_pred_blocks_result=nb,
            stuff_pred_blocks_swing=nb,
            ).to(device)
        losses = TrainAndGraph(
            network=network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            batch_size=10000,
            num_epochs=1001,
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
    plt.figure(figsize=(1 * len(block_sizes), .75 * len(num_blocks) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=[x for x in block_sizes],
        yticklabels=[y for y in num_blocks],
    )
    plt.xlabel('Block Sizes')
    plt.ylabel('Num Blocks')
    plt.title(f'Test Loss for {name}')
    plt.savefig(f'Stuff/Experiments/Results/BlockSizeNumBlocks/{name}.png', dpi=400)
    plt.clf()