from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelTrain import TrainAndGraph
from PitchModel.Constants import device, DATA_PREP_BINARY_ALL_FILE
from PitchModel.Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from tqdm import tqdm

import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import gc
import torch

# Get Data
data_prep = DataPrep.Load_From_File(DATA_PREP_BINARY_ALL_FILE)
pitch_io_list = data_prep.GenerateIOPitches()
train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list)
pitch_io_list = None # Clear Memory

batch_sizes = [80000, 40000, 20000, 10000, 5000, 2500]
learning_rates = [0.1, 0.03, 0.01, 0.003, 0.001, 0.0003, 0.0001]

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
        
        for batch_size in tqdm(batch_sizes, desc="Batch Sizes", leave=False):
            for lr in tqdm(learning_rates, desc="Learning Rate", leave=False):
                xs.append(batch_size)
                ys.append(lr)
                
                args.learning_rate = lr
                
                network = PitchModel(args, data_prep).to(device)
                loss = TrainAndGraph(
                    network=network,
                    train_dataset=train_dataset,
                    test_dataset=test_dataset,
                    batch_size=batch_size,
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
        plt.figure(figsize=(1 * len(batch_sizes), .75 * len(learning_rates) + 2))
        sns.heatmap(
            pivot_table, 
            annot=True,
            fmt='.3f',
            cmap='viridis',
            linewidths=0.5,
            xticklabels=[x for x in batch_sizes],
            yticklabels=[f"{y:.0e}" for y in learning_rates],
        )
        plt.xlabel('Location Branch Sizes')
        plt.ylabel('Stuff Branch Sizes')
        plt.title(f'Test Loss')
        plt.savefig(f'Stuff/Experiments/Results/BatchSizeLearningRates/{model_variant.name}_{model_output.name}.png', dpi=400)
        plt.clf()