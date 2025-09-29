import sys
from Data_Prep import Data_Prep, Player_IO
from sklearn.model_selection import train_test_split # type: ignore
import torch
from Player_Dataset import Player_Dataset
import Player_Model
from torch.optim import lr_scheduler
import Model_Train
from tqdm import tqdm
from Constants import device, experimental_db
import Prep_Map
import Output_Map

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
        
    cursor = experimental_db.cursor()
    model_idxs = cursor.execute("SELECT pitcherModelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        if model_id == 1:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 2:
            prep_map = Prep_Map.statsonly_prep_map
        
        output_map = Output_Map.base_output_map
        
        data_prep = Data_Prep(prep_map, output_map)
        pitcher_io_list = data_prep.Generate_IO_Pitchers("WHERE lastMLBSeason<? AND signingYear<? AND isPitcher=?", (2025,2015,1), use_cutoff=True)
        
        batch_size = 200
        pitching_mutators = data_prep.Generate_Pitching_Mutators(batch_size, Player_IO.GetMaxLength(pitcher_io_list))
        
        cursor = experimental_db.cursor()
        cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}'")
        experimental_db.commit()
        for i in tqdm(range(num_models), desc="Training Pitcher Models", leave=False):
            io_train : list[Player_IO]
            io_test : list[Player_IO]
            io_train, io_test = train_test_split(pitcher_io_list, test_size=0.25, random_state=i + 1) # Seed +1 so that it doesn't match pretraining, which is 0

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
            mask_year_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_train])
            mask_year_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_level_mask for io in io_test])
            y_year_stats_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_train])
            y_year_stats_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_stat_output for io in io_test])
            y_year_position_train_padded = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_train])
            y_year_position_test_padded = torch.nn.utils.rnn.pad_sequence([io.year_pos_output for io in io_test])
            train_dataset = Player_Dataset(x_train_padded, train_lengths, y_prospect_train_padded, y_stats_train_padded, y_position_train_padded, mask_prospect_train_padded, mask_level_train_padded, mask_year_train_padded, y_year_stats_train_padded, y_year_position_train_padded)
            test_dataset = Player_Dataset(x_test_padded, test_lengths, y_prospect_test_padded, y_stats_test_padded, y_position_test_padded, mask_prospect_test_padded, mask_level_test_padded, mask_year_test_padded, y_year_stats_test_padded, y_year_position_test_padded)
            
            # Setup Model
            num_layers = 4
            hidden_size = 20
            network = Player_Model.RNN_Model(x_train_padded[0].shape[1], num_layers, hidden_size, pitching_mutators, output_map=data_prep.output_map, is_hitter=False)
            network = network.to(device)
            
            optimizer = torch.optim.Adam(network.parameters(), lr=0.004)
            scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=10, cooldown=1)
            
            num_epochs = 500
            training_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
            testing_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
            
            model_name_pt = f"{model_name}_{i}"
            best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, len(train_dataset), len(test_dataset), Player_Model.Classification_Loss, Player_Model.Stats_L1_Loss, Player_Model.Position_Classification_Loss, optimizer, scheduler, num_epochs, logging_interval=10000, early_stopping_cutoff=40, should_output=False, model_name=f"Models/{model_name_pt}.pt")
            
            cursor = experimental_db.cursor()
            cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?)", (model_name, 0, best_loss, i, num_layers, hidden_size))
            experimental_db.commit()
        
        # Insert pitchers that were trained on so that they can be marked on the site
        cursor = experimental_db.cursor()
        for io in pitcher_io_list:
            if cursor.execute("SELECT COUNT(*) FROM PlayersInTrainingData WHERE mlbId=? AND modelIdx=?", (io.player.mlbId, model_id)).fetchone()[0] == 0:
                cursor.execute("INSERT INTO PlayersInTrainingData VALUES(?,?)", (io.player.mlbId,model_id))
        experimental_db.commit()