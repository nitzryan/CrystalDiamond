import sys
from tqdm import tqdm
import torch
import gc

from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Combined.Model.Model_Train import TrainAndGraph
from Pro.Model.Player_Model import RNN_Model as ProModel
from College.Model.College_Model import RNN_Model as ColModel
from Constants import device, model_db
from Utilities import GetModelMaps

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
        
    model_cursor = model_db.cursor()
    model_idxs = model_cursor.execute("SELECT modelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        pro_prep_map, pro_output_map, col_prep_map, col_output_map = GetModelMaps(model_id)
        
        data_prep = Combined_Data_Prep(
            prep_map=pro_prep_map,
            output_map=pro_output_map,
            college_prep_map=col_prep_map,
            college_output_map=col_output_map
        )
        
        pitcher_io_list = data_prep.Generate_IO_Pitchers(pro_player_condition="WHERE lastMLBSeason<? AND signingYear<? AND isPitcher=?", pro_player_values=(2025,2015,1), pro_use_cutoff=True,
                col_player_condition="WHERE LastYear<=? AND isPitcher=?", col_player_values=(2015, 1), col_use_cutoff=True)
        
        model_cursor = model_db.cursor()
        model_cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}' AND IsHitter=0")
        model_db.commit()
        
        for i in tqdm(range(num_models), desc="Training Pitcher Models", leave=False):
            train_dataset : Combined_Player_Dataset
            test_dataset : Combined_Player_Dataset
            train_dataset, test_dataset = Create_Test_Train_Datasets(pitcher_io_list, 0.25, i + 1, False)
            
            model_name_pt = f"{model_name}_{i}"
            pro_network = ProModel(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=False,).to(device)
            col_network = ColModel(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=False,
                output_hidden_size=pro_network.GetHiddenSize(),
                output_num_layers=pro_network.GetNumLayers(),
            ).to(device)
            
            best_loss, best_loss_college, best_epoch = TrainAndGraph(
                pro_network=pro_network,
                col_network=col_network,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                pro_model_name=f"Models/pro_{model_name_pt}_pit",
                col_model_name=f"Models/col_{model_name_pt}_pit",
                is_hitter=False,
                should_output=False,
            )
            
            model_cursor = model_db.cursor()
            model_cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?,?,?,?)", (model_name, 0, best_loss, best_loss_college, i, pro_network.GetNumLayers(), pro_network.GetHiddenSize(), col_network.GetNumLayers(), col_network.GetHiddenSize()))
            model_db.commit()
            
            # Force VRAM to get cleared before allocating next iteration
            del pro_network
            del col_network
            del train_dataset
            del test_dataset
            torch.cuda.empty_cache()
            gc.collect()
            
            
        # Insert hitters that were trained on so that they can be marked on the site
        model_cursor = model_db.cursor()
        for io in pitcher_io_list:
            if model_cursor.execute("SELECT COUNT(*) FROM PlayersInTrainingData WHERE mlbId=? AND tbcId=? AND modelIdx=?", (
                    io.pro_io.player.mlbId if io.pro_io.player is not None else -1,
                    io.college_io.player.TBCId if io.college_io.player is not None else -1, model_id)).fetchone()[0] == 0:
                
                model_cursor.execute("INSERT INTO PlayersInTrainingData VALUES(?,?,?)", (
                        io.pro_io.player.mlbId if io.pro_io.player is not None else -1,
                        io.college_io.player.TBCId if io.college_io.player is not None else -1,
                        model_id))
        model_db.commit()