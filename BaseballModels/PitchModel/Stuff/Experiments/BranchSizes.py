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
pitch_io_list = data_prep.GenerateIOPitches(use_cutoff=True)
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list, test_size=0.25, random_state=0)
pitch_io_list = None # Clear Memory

location_branch_sizes = range(5, 56, 10)
stuff_branch_sizes = range(5, 56, 10)

xs = []
ys = []
zs_location = []
zs_stuff = []
zs_combined = []

# Iterate through training

for lbs in tqdm(location_branch_sizes, desc="Location"):
    for sbs in tqdm(stuff_branch_sizes, desc="Stuff", leave=False):
        xs.append(lbs)
        ys.append(sbs)
        
        network = PitchModel(data_prep,
            location_branch_size=lbs,
            stuff_branch_size=sbs).to(device)
        losses = TrainAndGraph(
            network=network,
            train_dataset=train_dataset,
            test_dataset=test_dataset,
            batch_size=50000,
            num_epochs=1001,
            model_name="Models/default",
            should_output=False)
        
        zs_location.append(losses[0])
        zs_stuff.append(losses[len(_MODEL_OUTPUTS)])
        zs_combined.append(losses[2 * len(_MODEL_OUTPUTS)])
        
        
# Log Results
for zs, name in [(zs_location, "Location"), (zs_stuff, "Stuff"), (zs_combined, "Combined")]:
    df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
    pivot_table = df.pivot(index='y', columns='x', values='z')
    plt.figure(figsize=(1 * len(location_branch_sizes), .75 * len(stuff_branch_sizes) + 2))
    sns.heatmap(
        pivot_table, 
        annot=True,
        fmt='.3f',
        cmap='viridis',
        linewidths=0.5,
        xticklabels=[round(x) for x in location_branch_sizes],
        yticklabels=[round(y) for y in stuff_branch_sizes],
    )
    plt.xlabel('Location Branch Sizes')
    plt.ylabel('Stuff Branch Sizes')
    plt.title(f'Test Loss for {name}')
    plt.savefig(f'Stuff/Experiments/Results/BranchSizes_{name}.png', dpi=400)
    plt.clf()