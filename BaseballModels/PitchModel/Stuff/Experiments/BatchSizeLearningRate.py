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
import gc
import torch

# Get Data

data_prep = DataPrep(standard_prep_map)
pitch_io_list = data_prep.GenerateIOPitches()
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list, test_size=0.25, random_state=0)
pitch_io_list = None # Clear Memory

batch_sizes = range(5000, 40001, 5000)
learning_rates = [x / 1000 for x in range(3,9)]


xs = []
ys = []
zs = []

# Iterate through training
for batch_size in tqdm(batch_sizes, desc="Batch Sizes"):
    for lr in tqdm(learning_rates, desc="Learning Rate", leave=False):
        xs.append(batch_size)
        ys.append(lr)
        
        network = PitchModel(data_prep).to(device)
        losses = TrainAndGraph(
            network=network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            batch_size=batch_size,
            learning_rate = lr,
            num_epochs=1001,
            model_name="Models/default",
            should_output=False)
        
        zs.append(losses[2 * len(_MODEL_OUTPUTS)])
        del network
        torch.cuda.empty_cache()         # release cached unused GPU memory
        gc.collect()
        
# Log Results
df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
pivot_table = df.pivot(index='y', columns='x', values='z')
plt.figure(figsize=(1 * len(batch_sizes), .75 * len(learning_rates) + 2))
sns.heatmap(
    pivot_table, 
    annot=True,
    fmt='.3f',
    cmap='viridis',
    linewidths=0.5,
    xticklabels=[x for x in batch_sizes],
    yticklabels=[f"{y:.3f}" for y in learning_rates],
)
plt.xlabel('Location Branch Sizes')
plt.ylabel('Stuff Branch Sizes')
plt.title(f'Test Loss')
plt.savefig(f'Stuff/Experiments/Results/BatchSizeLearningRate2.png', dpi=400)
plt.clf()