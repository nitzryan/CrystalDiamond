import sys
from Data_Prep import Data_Prep, Player_IO
import torch
from Player_Dataset import Create_Test_Train_Datasets
import Player_Model
import Model_Train
from tqdm import tqdm
from Constants import device, db, DEFAULT_HIDDEN_SIZE_PITCHER, DEFAULT_NUM_LAYERS_PITCHER, DEFAULT_PITCHER_BATCH_SIZE, DEFAULT_PITCHER_NUM_EPOCHS
import Prep_Map
import Output_Map
import warnings

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
    
    cursor = db.cursor()
    model_idxs = cursor.execute("SELECT pitcherModelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        if model_id == 1:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 2:
            prep_map = Prep_Map.statsonly_prep_map
        elif model_id == 3:
            prep_map = Prep_Map.experimental_prep_map
        
        output_map = Output_Map.base_output_map
        
        data_prep = Data_Prep(prep_map, output_map)
        pitcher_io_list = data_prep.Generate_IO_Pitchers("WHERE lastMLBSeason<? AND signingYear<? AND isPitcher=?", (2025,2015,1), use_cutoff=True)
        
        batch_size = DEFAULT_PITCHER_BATCH_SIZE
        pitching_mutators = data_prep.Generate_Pitching_Mutators(batch_size, Player_IO.GetMaxLength(pitcher_io_list))
        
        cursor = db.cursor()
        cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}'")
        db.commit()
        
        for i in tqdm(range(num_models), desc="Training Pitcher Models", leave=False):
            train_dataset, test_dataset = Create_Test_Train_Datasets(pitcher_io_list, 0.25, i + 1) # Seed +1 so that it doesn't match pretraining, which is 0
            
            # Setup Model
            num_layers = DEFAULT_NUM_LAYERS_PITCHER
            hidden_size = DEFAULT_HIDDEN_SIZE_PITCHER
            network = Player_Model.RNN_Model(train_dataset.get_input_size(), num_layers, hidden_size, pitching_mutators, data_prep=data_prep, is_hitter=False)
            network = network.to(device)
            
            num_epochs = DEFAULT_PITCHER_NUM_EPOCHS
            model_name_pt = f"{model_name}_{i}"
            best_losses = Model_Train.trainAndGraph(network, 
                                                  train_dataset,
                                                  test_dataset,
                                                  num_epochs=num_epochs, 
                                                  batch_size=batch_size,
                                                  logging_interval=10000, 
                                                  early_stopping_cutoff=40, 
                                                  should_output=False, 
                                                  model_name=f"Models/{model_name_pt}",
                                                  save_last=True,
                                                  elements_to_save=[0])
            
            cursor = db.cursor()
            cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?)", (model_name, 0, best_losses[0], i, num_layers, hidden_size))
            db.commit()
        
        # Insert pitchers that were trained on so that they can be marked on the site
        cursor = db.cursor()
        for io in pitcher_io_list:
            if cursor.execute("SELECT COUNT(*) FROM PlayersInTrainingData WHERE mlbId=? AND modelIdx=?", (io.player.mlbId, model_id)).fetchone()[0] == 0:
                cursor.execute("INSERT INTO PlayersInTrainingData VALUES(?,?)", (io.player.mlbId,model_id))
        db.commit()