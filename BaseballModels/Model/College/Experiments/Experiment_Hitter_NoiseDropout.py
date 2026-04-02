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

noise_range = [0.025 * x for x in range(10)]
dropout_range = [0.025 * x for x in range(10)]

batch_size = 4000
num_epochs = 50
        
data = []
        
for noise in tqdm(noise_range, desc="Noise"):
    z = []
    for dropout in tqdm(dropout_range, desc="Dropout", leave=False):
        network = RNN_Model(train_dataset.get_input_size(), 
                        noise=noise,
                        dropout=dropout,
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
    
plt.figure(figsize=(1 * len(dropout_range), .75 * len(noise_range) + 2))
heatmap = sns.heatmap(data, xticklabels=dropout_range, yticklabels=noise_range, annot=True, fmt=".3f")
plt.xlabel('Dropout')
plt.ylabel('Noise')
plt.savefig(f'College/Experiments/Results/Hitters_NoiseDropout2.png', dpi=400)