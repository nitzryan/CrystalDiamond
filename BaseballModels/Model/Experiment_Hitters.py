from Data_Prep import Data_Prep, Player_IO
from sklearn.model_selection import train_test_split # type: ignore
import torch
import Prep_Map
import Output_Map
from torch.optim import lr_scheduler
from Player_Dataset import Player_Dataset, Create_Test_Train_Datasets
import Model_Train
from Player_Model import RNN_Model
from tqdm import tqdm
from Constants import device
import seaborn as sns
from pylab import savefig
from matplotlib import pyplot as plt
import sys
from Constants import DEFAULT_HIDDEN_SIZE_HITTER, DEFAULT_NUM_LAYERS_HITTER
import numpy as np

if __name__ == "__main__":
    if (len(sys.argv) != 2):
        print(f"1 Argument expected (ExperimentHitters.py <filename no extension>), {len(sys.argv)} provided")
        exit(1)
    
    filename = sys.argv[1]
    
    batch_size = 600
    xs = [x / 300 for x in range(100)]
    
    data_class = []
    data_stats = []
    
    x_label = "Noise"
    y_labelClass = "Class Test Loss"
    y_labelStats = "Stats Test Loss"
    
    data_prep = Data_Prep(Prep_Map.base_prep_map, Output_Map.base_output_map)
    hitter_io_list = data_prep.Generate_IO_Hitters("WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", (2025,2015,1), use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0)
    
    num_layers = DEFAULT_NUM_LAYERS_HITTER
    hidden_size = DEFAULT_HIDDEN_SIZE_HITTER
    batch_size = 800
    hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, Player_IO.GetMaxLength(hitter_io_list))
    for noise in tqdm(xs, desc=x_label):
        network = RNN_Model(train_dataset.get_input_size(), num_layers, hidden_size, hitting_mutators, data_prep=data_prep, is_hitter=True, noiseScale=noise)
        network = network.to(device)

        training_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
        testing_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
        
        
        best_loss = Model_Train.trainAndGraph(network, 
                                                  train_dataset,
                                                  test_dataset,
                                                  batch_size=batch_size,
                                                  num_epochs=41, 
                                                  logging_interval=10000, 
                                                  early_stopping_cutoff=40, 
                                                  should_output=False, 
                                                  model_name=f"Models/exp",
                                                  save_last=True,
                                                  elements_to_save=[0,3])
        data_class.append(best_loss[0])
        data_stats.append(best_loss[1])
        
    #heatmap = sns.heatmap(data, xticklabels=xs, yticklabels=ys, annot=True, fmt=".3f")
    plt.plot(xs, data_class)
    plt.xlabel(x_label)
    plt.ylabel(y_labelClass)
    plt.savefig(f'{filename}_Class.png', dpi=400)
    plt.close()
    plt.plot(xs, data_stats)
    plt.xlabel(x_label)
    plt.ylabel(y_labelStats)
    plt.savefig(f'{filename}_Stats.png', dpi=400)