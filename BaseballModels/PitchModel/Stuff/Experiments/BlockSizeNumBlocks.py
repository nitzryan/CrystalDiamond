from Stuff.DataPrep.DataPrep import DataPrep
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from Stuff.Model.ModelTrain import TrainAndGraph
from Constants import device, DATA_PREP_BINARY_ALL_FILE
from tqdm import tqdm

import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import torch
import gc

# Get Data
data_prep = DataPrep.Load_From_File(DATA_PREP_BINARY_ALL_FILE)
pitch_io_list = data_prep.GenerateIOPitches()
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list)
pitch_io_list = None # Clear Memory

num_blocks = [8, 4, 2, 1]
block_sizes = [256, 128, 64, 32]

model_variants = [ModelVariantType.Stuff, ModelVariantType.Combined]
model_outputs = [ModelOutputType.Result, ModelOutputType.SwingResults, ModelOutputType.InPlay]
# Iterate through training
for model_variant in tqdm(model_variants, desc="Model Variants"):
    for model_output in tqdm(model_outputs, desc="Model Outputs", leave=False):
        xs = []
        ys = []
        zs = []
        args = DEFAULT_ARGS_MAP[(model_variant, model_output)]
        train_dataset.SetOutputType(model_output)
        test_dataset.SetOutputType(model_output)
        
        for nb in tqdm(num_blocks, desc="Num Blocks", leave=False):
            for bs in tqdm(block_sizes, desc="Block Size", leave=False):
                xs.append(bs)
                ys.append(nb)
                
                args.num_blocks = nb
                args.block_size = bs
                
                network = PitchModel(args, data_prep).to(device)
                loss = TrainAndGraph(
                    network=network,
                    train_dataset=train_dataset,
                    test_dataset=test_dataset,
                    batch_size=30000,
                    num_epochs=1001,
                    model_name="Models/default",
                    should_output=False)
                
                zs.append(loss)
                    
                del network
                torch.cuda.empty_cache()
                gc.collect()    
                
                
        # Log Results
        df = pd.DataFrame({'x': xs, 'y': ys, 'z': zs})
        pivot_table = df.pivot(index='y', columns='x', values='z')
        plt.figure(figsize=(1 * len(block_sizes), .75 * len(num_blocks) + 2))
        sns.heatmap(
            pivot_table, 
            annot=True,
            fmt='.4f',
            cmap='viridis',
            linewidths=0.5,
        )
        plt.xlabel('Block Sizes')
        plt.ylabel('Num Blocks')
        plt.title(f'Test Loss for {model_variant.name} {model_output.name}')
        plt.savefig(f'Stuff/Experiments/Results/BlockSizeNumBlocks/{model_variant.name}_{model_output.name}.png', dpi=400)
        plt.clf()