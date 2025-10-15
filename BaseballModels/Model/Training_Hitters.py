import sys
from Data_Prep import Data_Prep, Player_IO
from sklearn.model_selection import train_test_split # type: ignore
import torch
from Player_Dataset import Player_Dataset, Create_Test_Train_Datasets
import Player_Model
from torch.optim import lr_scheduler
import Model_Train
from tqdm import tqdm
from Constants import device, db, DEFAULT_NUM_LAYERS_HITTER, DEFAULT_HIDDEN_SIZE_HITTER
import Prep_Map
import Output_Map
import warnings

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
    
    cursor = db.cursor()
    cursor.execute("DELETE FROM PlayersInTrainingData")
    db.commit()
        
    cursor = db.cursor()
    model_idxs = cursor.execute("SELECT hitterModelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        if model_id == 1:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 2:
            prep_map = Prep_Map.statsonly_prep_map
        
        output_map = Output_Map.base_output_map
        
        data_prep = Data_Prep(prep_map, output_map)
        hitter_io_list = data_prep.Generate_IO_Hitters("WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", (2025,2015,1), use_cutoff=True)
        
        batch_size = 200
        hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, Player_IO.GetMaxLength(hitter_io_list))
        
        cursor = db.cursor()
        cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}'")
        db.commit()
        
        for i in tqdm(range(num_models), desc="Training Hitter Models", leave=False):
            train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.25, i + 1) # Seed +1 so that it doesn't match pretraining, which is 0
            
            # Setup Model
            num_layers = DEFAULT_NUM_LAYERS_HITTER
            hidden_size = DEFAULT_HIDDEN_SIZE_HITTER
            network = Player_Model.RNN_Model(train_dataset.get_input_size(), num_layers, hidden_size, hitting_mutators, data_prep=data_prep, is_hitter=True)
            # Warning for loading model, but these are trusted
            with warnings.catch_warnings():
                warnings.filterwarnings("ignore", category=FutureWarning)
                if model_id == 1:
                    network.load_state_dict(torch.load("Models/default_hitter.pt"))
                elif model_id == 2:
                    network.load_state_dict(torch.load("Models/default_statsonly_hitter.pt"))
                
            network = network.to(device)
            
            optimizer = torch.optim.Adam(network.parameters(), lr=0.001)
            scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=10, cooldown=10)
            
            num_epochs = 500
            training_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
            testing_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
            
            model_name_pt = f"{model_name}_{i}"
            best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, len(train_dataset), len(test_dataset), optimizer, scheduler, num_epochs, logging_interval=10000, early_stopping_cutoff=40, should_output=False, model_name=f"Models/{model_name_pt}.pt")
            
            cursor = db.cursor()
            cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?)", (model_name, 1, best_loss, i, num_layers, hidden_size))
            db.commit()
            
        # Insert hitters that were trained on so that they can be marked on the site
        cursor = db.cursor()
        cursor.executemany("INSERT INTO PlayersInTrainingData VALUES(?,?)", [(io.player.mlbId,model_id) for io in hitter_io_list])
        db.commit()