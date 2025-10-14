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

if __name__ == "__main__":
    if (len(sys.argv) != 2):
        print(f"1 Argument expected (ExperimentHitters.py <filename no extension>), {len(sys.argv)} provided")
        exit(1)
    
    filename = sys.argv[1]
    
    batch_size = 200
    xs : list[int] = [2,3,4]
    ys : list[int] = [26,28,30,32,34]
    
    data = []
    
    x_label = "Num Layers"
    y_label = "Hidden Size"
    
    data_prep = Data_Prep(Prep_Map.base_prep_map, Output_Map.base_output_map)
    pitcher_io_list = data_prep.Generate_IO_Pitchers("WHERE lastMLBSeason<? AND signingYear<? AND isPitcher=?", (2025,2015,1), use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(pitcher_io_list, 0.25, 4980)
    
    for hidden_size in tqdm(ys, desc=y_label):
        this_data = []
        for num_layers in tqdm(xs, desc=x_label, leave=False):
            mutators = data_prep.Generate_Pitching_Mutators(batch_size, Player_IO.GetMaxLength(pitcher_io_list))
            
            network = RNN_Model(train_dataset.get_input_size(), num_layers, hidden_size, mutators, data_prep=data_prep, is_hitter=False)
            network = network.to(device)

            optimizer = torch.optim.Adam(network.parameters(), lr=0.005)
            scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=10, cooldown=10)

            num_epochs = 1000
            training_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
            testing_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
            
            best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, len(train_dataset), len(test_dataset), optimizer, scheduler, num_epochs, logging_interval=25, early_stopping_cutoff=50, should_output=False, save_last=False)
            this_data.append(best_loss)
        data.append(this_data)
        
        
    heatmap = sns.heatmap(data, xticklabels=xs, yticklabels=ys, annot=True, fmt=".3f")
    plt.xlabel(x_label)
    plt.ylabel(y_label)
    figure = heatmap.get_figure()
    figure.savefig(f'{filename}.png', dpi=400)