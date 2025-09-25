import sys
from Data_Prep import Data_Prep, Hitter_IO
from sklearn.model_selection import train_test_split # type: ignore
import torch
from Hitter_Dataset import Hitter_Dataset
import Hitter_Model
from torch.optim import lr_scheduler
import Model_Train
from tqdm import tqdm
from Constants import device, db
import Prep_Map
import Output_Map

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
        if model_id == 1 or model_id == 2:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 3 or model_id == 4:
            prep_map = Prep_Map.statsonly_prep_map
        
        if model_id == 1 or model_id == 3:
            output_map = Output_Map.war_map
        if model_id == 2 or model_id == 4:
            output_map = Output_Map.value_map
        
        data_prep = Data_Prep(prep_map, output_map)
        hitter_io_list = data_prep.Generate_IO_Hitters("WHERE (lastMLBSeason<?) AND isHitter=?", (2025,1))
        
        batch_size = 200
        hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, Hitter_IO.GetMaxLength(hitter_io_list))
        
        cursor = db.cursor()
        cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}'")
        db.commit()
        
        models = [(2,20),(2,25),(2,30),(2,15),(3,20),(3,25),(3,30),(3,15),(4,20),(4,25),(4,30),(4,15)]
        for i, (num_layers, hidden_size) in enumerate(tqdm(models, desc="Training Hitter Models", leave=False)):
            best_loss = 10
            count = 0
            while best_loss > 5 and count < 3: # Throw away trainings that get stuck in local minima
                io_train : list[Hitter_IO]
                io_test : list[Hitter_IO]
                io_train, io_test = train_test_split(hitter_io_list, test_size=0.25, random_state=0)

                train_lengths = torch.tensor([io.length for io in io_train])
                test_lengths = torch.tensor([io.length for io in io_test])

                x_train_padded = torch.nn.utils.rnn.pad_sequence([io.input for io in io_train])
                x_test_padded = torch.nn.utils.rnn.pad_sequence([io.input for io in io_test])
                y_prospect_train_padded = torch.nn.utils.rnn.pad_sequence([io.output for io in io_train])
                y_prospect_test_padded = torch.nn.utils.rnn.pad_sequence([io.output for io in io_test])
                y_stats_train_padded = torch.nn.utils.rnn.pad_sequence([io.stat_output for io in io_train])
                y_stats_test_padded = torch.nn.utils.rnn.pad_sequence([io.stat_output for io in io_test])
                y_position_train_padded = torch.nn.utils.rnn.pad_sequence([io.position_output for io in io_train])
                y_position_test_padded = torch.nn.utils.rnn.pad_sequence([io.position_output for io in io_test])
                mask_prospect_train_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_train])
                mask_prospect_test_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in io_test])
                mask_level_train_padded = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_train])
                mask_level_test_padded = torch.nn.utils.rnn.pad_sequence([io.stat_level_mask for io in io_test])
                train_hitters_dataset = Hitter_Dataset(x_train_padded, train_lengths, y_prospect_train_padded, y_stats_train_padded, y_position_train_padded, mask_prospect_train_padded, mask_level_train_padded)
                test_hitters_dataset = Hitter_Dataset(x_test_padded, test_lengths, y_prospect_test_padded, y_stats_test_padded, y_position_test_padded, mask_prospect_test_padded, mask_level_test_padded)
                
                # Setup Model
                # num_layers = 3
                # hidden_size = 30
                network = Hitter_Model.RNN_Model(x_train_padded[0].shape[1], num_layers, hidden_size, hitting_mutators, output_map=data_prep.output_map)
                network = network.to(device)
                
                optimizer = torch.optim.Adam(network.parameters(), lr=0.003)
                scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=20, cooldown=5)
                
                num_epochs = 500
                training_generator = torch.utils.data.DataLoader(train_hitters_dataset, batch_size=batch_size, shuffle=True)
                testing_generator = torch.utils.data.DataLoader(test_hitters_dataset, batch_size=batch_size, shuffle=False)
                
                model_name_pt = f"{model_name}_{i}"
                best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, len(train_hitters_dataset), len(test_hitters_dataset), Hitter_Model.Classification_Loss, Hitter_Model.Stats_L1_Loss, Hitter_Model.Position_Classification_Loss, optimizer, scheduler, num_epochs, logging_interval=10000, early_stopping_cutoff=40, should_output=False, model_name=f"Models/{model_name_pt}.pt")
                count += 1
            
            cursor = db.cursor()
            cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?)", (model_name, 1, best_loss, i, num_layers, hidden_size))
            db.commit()
            
        # Insert hitters that were trained on so that they can be marked on the site
        cursor = db.cursor()
        cursor.executemany("INSERT INTO PlayersInTrainingData VALUES(?,?)", [(h.mlbId,model_id) for h in hitter_io_list.hitters])
        db.commit()