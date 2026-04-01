from College.DataPrep import Prep_Map, Output_Map
from College.DataPrep.Data_Prep import College_Data_Prep
from College.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Constants import device
from torch.optim import lr_scheduler
from torch.optim import Adam
from College.Model.College_Model import RNN_Model
from College.Model.Model_Train import trainAndGraph
from tqdm import tqdm
from matplotlib import pyplot as plt
import seaborn as sns

data_prep = College_Data_Prep(Prep_Map.college_base_prep_map, Output_Map.college_output_map)
hitter_io_list = data_prep.Generate_IO_Hitters("WHERE LastYear<=? AND isHitter=?", (2019, 1), use_cutoff=True)
train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, 0)

num_layers = range(1, 8)
hidden_sizes = range(5, 26, 5)

batch_size = 4000
num_epochs = 30
        
data = []
        
for nl in tqdm(num_layers, desc="Num Layers"):
    z = []
    for hs in tqdm(hidden_sizes, desc="Hidden Size", leave=False):
        network = RNN_Model(train_dataset.get_input_size(), 
                        nl, 
                        hs, 
                        data_prep=data_prep, 
                        is_hitter=True)
        network = network.to(device)

        optimizer = Adam(network.parameters(), lr=0.0025)
        scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=5, cooldown=5)

        best_losses = trainAndGraph(network,
                    train_dataset,
                    test_dataset,
                    scheduler,
                    optimizer,
                    batch_size,
                    num_epochs = num_epochs,
                    logging_interval=10,
                    early_stopping_cutoff=2000,
                    should_output=False,
                    model_name="Models/default_college_hitter",
                    save_last=True,
                    elements_to_save=[0])
    
        z.append(best_losses[0])
    data.append(z)
    
plt.figure(figsize=(1 * len(hidden_sizes), .75 * len(num_layers) + 2))
heatmap = sns.heatmap(data, xticklabels=hidden_sizes, yticklabels=num_layers, annot=True, fmt=".3f")
plt.savefig(f'College/Experiments/Results/Hitters_RecurrentStructureSmallHS.png', dpi=400)