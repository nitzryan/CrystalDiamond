from Data_Prep import Data_Prep
from sklearn.model_selection import train_test_split # type: ignore
import torch
from Player_Dataset import Hitter_Dataset
from Player_Model import RNN_Model, Classification_Loss
from torch.optim import lr_scheduler
import Model_Train
from tqdm import tqdm
from Constants import device
import seaborn as sns
from pylab import savefig
from matplotlib import pyplot as plt

if __name__ == "__main__":
    data_prep = Data_Prep()
    hitters, inputs, outputs, max_input_size, dates = data_prep.Generate_IO_Hitters("WHERE (lastProspectYear<? OR lastMLBSeason<?) AND isHitter=?", (2025,2025,1))
    
    batch_size = 1000
    x_scale : list[float] = [0,0.05,0.1,0.15,0.2]
    y_scale : list[float] = [0,0.05,0.1,0.15,0.2]
    
    data = []
    
    x_label = "Bio Deviations"
    y_label = "Playing Time Deviations"
    
    for x in tqdm(x_scale, desc=x_label):
        this_data = []
        for y in tqdm(y_scale, desc=y_label, leave=False):
            data_prep.Update_Mutators(hitbio_dev=x, hitpt_dev=y)
            hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, max_input_size)
            
            x_train, x_test, y_train, y_test = train_test_split(inputs, outputs, test_size=0.25, random_state=4980)

            train_lengths = torch.tensor([len(seq) for seq in x_train])
            test_lengths = torch.tensor([len(seq) for seq in x_test])

            x_train_padded = torch.nn.utils.rnn.pad_sequence(x_train)
            x_test_padded = torch.nn.utils.rnn.pad_sequence(x_test)
            y_train_padded = torch.nn.utils.rnn.pad_sequence(y_train)
            y_test_padded = torch.nn.utils.rnn.pad_sequence(y_test)
            train_hitters_dataset = Hitter_Dataset(x_train_padded, train_lengths, y_train_padded)
            test_hitters_dataset = Hitter_Dataset(x_test_padded, test_lengths, y_test_padded)
            
            # Setup Model
            num_layers = 3
            hidden_size = 30
            network = RNN_Model(x_train_padded[0].shape[1], num_layers, hidden_size, hitting_mutators)
            network = network.to(device)
            
            optimizer = torch.optim.Adam(network.parameters(), lr=0.003)
            scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=20, cooldown=5)
            loss_function = Classification_Loss
            
            num_epochs = 1000
            training_generator = torch.utils.data.DataLoader(train_hitters_dataset, batch_size=batch_size, shuffle=True)
            testing_generator = torch.utils.data.DataLoader(test_hitters_dataset, batch_size=batch_size, shuffle=False)
            
            best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, loss_function, optimizer, scheduler, num_epochs, logging_interval=10000, early_stopping_cutoff=40, should_output=False)
            this_data.append(best_loss)
        data.append(this_data)
        
        
    heatmap = sns.heatmap(data, xticklabels=x_scale, yticklabels=y_scale, annot=True, fmt=".3f")
    plt.xlabel(x_label)
    plt.ylabel(y_label)
    figure = heatmap.get_figure()
    figure.savefig('experiment.png', dpi=400)