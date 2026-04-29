from Stuff.DataPrep.DataPrep import DataPrep
from Stuff.DataPrep.PrepMap import standard_prep_map
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import TrainAndGraph, _MODEL_OUTPUTS
from Constants import device
from tqdm import tqdm

import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

# Get Data
data_prep = DataPrep(standard_prep_map)
pitch_io_list = data_prep.GenerateIOPitches()
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list, test_size=0.25, random_state=0)
pitch_io_list = None # Clear Memory

num_blocks_range = range(0, 11, 2)
block_size_range = range(10, 101, 10)

xs = []
ys = []
zs = []

# Iterate through training
for num_blocks in tqdm(num_blocks_range, desc="Num Blocks"):
    for block_size in tqdm(block_size_range, desc="Block Size", leave=False):
        xs.append(block_size)
        ys.append(num_blocks)
        
        network = PitchModel(
            data_prep=data_prep,
            combined_pred_blocks_value=num_blocks,
            combined_pred_size_value=block_size,
        ).to(device)
        
        losses = TrainAndGraph(
            network=network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            num_epochs=2,
            model_name="Models/default",
            should_output=False)
        
        zs.append(losses[2 * len(_MODEL_OUTPUTS)])
        
# Log Results
df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
pivot_table = df.pivot(index='y', columns='x', values='z')
plt.figure(figsize=(1 * len(block_size_range), .75 * len(num_blocks_range) + 2))
sns.heatmap(
    pivot_table, 
    annot=True,
    fmt='.3f',
    cmap='viridis',
    linewidths=0.5,
    xticklabels=[f"{x:.3f}" for x in block_size_range],
    yticklabels=[y for y in num_blocks_range],
)
plt.xlabel('Block Size')
plt.ylabel('Num Blocks')
plt.title(f'Test Loss')
plt.savefig(f'Stuff/Experiments/Results/Pred_Combined.png', dpi=400)
plt.clf()