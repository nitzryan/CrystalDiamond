from College.Model.College_Model import RNN_Model
from College.Model.Model_Train import trainAndGraph
from College.DataPrep.Data_Prep import College_Data_Prep
from College.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from tqdm import tqdm
from Constants import device, model_db
import sys
from Utilities import GetCollegeModelMaps
from torch.optim import lr_scheduler, Adam

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
        
    batch_size = 4000
    num_epochs = 50
        
    model_cursor = model_db.cursor()
    model_idxs = model_cursor.execute("SELECT pitcherModelName, id FROM ModelIdx_College ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        model_cursor = model_db.cursor()
        model_cursor.execute(f"DELETE FROM Model_TrainingHistory_College WHERE ModelName='{model_name}'")
        model_db.commit()
        
        prep_map, output_map = GetCollegeModelMaps(model_id)
        
        data_prep = College_Data_Prep(prep_map, output_map)
        pitcher_io_list = data_prep.Generate_IO_Pitchers("WHERE LastYear<=? AND isPitcher=?", (2017, 1), use_cutoff=True)
        
        
        for i in tqdm(range(num_models), desc="Training Pitcher Models", leave=False):
            train_dataset, test_dataset = Create_Test_Train_Datasets(pitcher_io_list, 0.25, i + 1, is_hitter=False)
            network = RNN_Model(train_dataset.get_input_size(), data_prep, is_hitter=False)
            network = network.to(device)
            
            optimizer = Adam(network.parameters(), lr=0.0025)
            scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=5, cooldown=5)
            
            model_name_pt = model_name_pt = f"{model_name}_{i}"
            best_losses = trainAndGraph(network=network, 
                                        training_dataset=train_dataset, 
                                        testing_dataset=test_dataset,
                                        scheduler=scheduler,
                                        optimizer=optimizer,
                                        num_epochs=num_epochs,
                                        batch_size=batch_size,
                                        logging_interval=10000,
                                        is_hitter=False,
                                        should_output=False,
                                        model_name=f"Models/College_{model_name_pt}",
                                        save_last=False,
                                        elements_to_save=[1])
            
            model_cursor = model_db.cursor()
            model_cursor.execute("INSERT INTO Model_TrainingHistory_College VALUES (?,?,?,?,?,?)", (model_name, 0, best_losses[0], i, network.num_layers, network.hidden_size))
            model_db.commit()
        
        model_cursor = model_db.cursor()
        for io in pitcher_io_list:
            if model_cursor.execute("SELECT COUNT(*) FROM PlayersInTrainingData_College WHERE tbcId=? AND modelIdx=?", (io.player.TBCId, model_id)).fetchone()[0] == 0:
                model_cursor.execute("INSERT INTO PlayersInTrainingData_College VALUES(?,?)", (io.player.TBCId,model_id))
        model_db.commit()