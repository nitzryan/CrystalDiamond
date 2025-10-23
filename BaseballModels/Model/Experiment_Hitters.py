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
    
    batch_size = 200
    xs = np.linspace(0.0, 2.0, num=41)
    
    data = []
    
    x_label = "Mutator Scale"
    y_label = "Test Loss"
    
    data_prep = Data_Prep(Prep_Map.base_prep_map, Output_Map.base_output_map)
    hitter_io_list = data_prep.Generate_IO_Hitters("WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", (2025,2015,1), use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0)
    
    hidden_size = DEFAULT_HIDDEN_SIZE_HITTER
    num_layers = DEFAULT_NUM_LAYERS_HITTER
    for mutator_size in tqdm(xs, desc=y_label):
        data_prep.Update_Mutators(off_dev=mutator_size,
                                  bsr_dev=mutator_size,
                                  def_dev=mutator_size,
                                  hitpt_dev=mutator_size,
                                  hitlevel_dev=mutator_size,
                                  hitbio_dev=mutator_size,
                                  mlb_hitstat_dev=mutator_size)
        hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, Player_IO.GetMaxLength(hitter_io_list))
        
        network = RNN_Model(train_dataset.get_input_size(), num_layers, hidden_size, hitting_mutators, data_prep=data_prep, is_hitter=True)
        network = network.to(device)

        optimizer = torch.optim.Adam(network.parameters(), lr=0.005)
        scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=10, cooldown=10)

        num_epochs = 1000
        training_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
        testing_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
        
        best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, len(train_dataset), len(test_dataset), optimizer, scheduler, num_epochs, logging_interval=25, early_stopping_cutoff=50, should_output=False, save_last=False)
        data.append(best_loss)
        
        
    #heatmap = sns.heatmap(data, xticklabels=xs, yticklabels=ys, annot=True, fmt=".3f")
    plt.plot(xs, data)
    plt.xlabel(x_label)
    plt.ylabel(y_label)
    plt.savefig(f'{filename}.png', dpi=400)