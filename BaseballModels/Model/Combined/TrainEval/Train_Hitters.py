from tqdm import tqdm
import torch
import gc

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Model.Combined.Model.Model_Train import TrainAndGraph
from Model.Pro.Model.Player_Model import RNN_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.Constants import device, model_db
from Model.Utilities import GetModelMaps
from Model.Combined.Utilities.GetVariableLossIndex import GetVariableLossIndex

def Train_Hitters(num_models : int):
    if num_models < 0:
        exit(1)
        
    model_cursor = model_db.cursor()
    model_idxs = model_cursor.execute("SELECT modelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Hitting Architectures"):
        model_cursor = model_db.cursor()
        model_cursor.execute("DELETE FROM PlayersInTrainingData WHERE modelIdx=?", (model_id,))
        model_db.commit()
        
        pro_prep_map, pro_output_map, col_prep_map, col_output_map = GetModelMaps(model_id)
        
        data_prep = Combined_Data_Prep(
            prep_map=pro_prep_map,
            output_map=pro_output_map,
            college_prep_map=col_prep_map,
            college_output_map=col_output_map
        )
        
        hitter_io_list = data_prep.Generate_IO_Hitters(is_training=True)
        
        model_cursor = model_db.cursor()
        model_cursor.execute(f"DELETE FROM Model_TrainingHistory WHERE ModelName='{model_name}' AND IsHitter=1")
        model_db.commit()
        
        for i in tqdm(range(num_models), desc="Training Hitter Models", leave=False):
            train_dataset : Combined_Player_Dataset
            test_dataset : Combined_Player_Dataset
            train_dataset, test_dataset = Create_Test_Train_Datasets(player_list=hitter_io_list, is_hitter=True, train_idx=i)
            
            model_name_pt = f"{model_name}_{i}"
            pro_network = ProModel(
                input_size=train_dataset.GetProInputSize(),
                mutators=torch.empty(0),
                data_prep=data_prep.pro_data_prep,
                is_hitter=True).to(device)
            col_network = ColModel(
                input_size=train_dataset.GetColInputSize(),
                data_prep=data_prep.college_data_prep,
                is_hitter=True,
                output_hidden_size=pro_network.GetHiddenSize(),
                output_num_layers=pro_network.GetNumLayers(),
            ).to(device)
            
            train_results = TrainAndGraph(
                pro_network=pro_network,
                col_network=col_network,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                pro_model_name=f"Model/Models/pro_{model_name_pt}_hit",
                col_model_name=f"Model/Models/col_{model_name_pt}_hit",
                is_hitter=True,
                should_output=False,
                show_progress_bar=True,
            )
            
            model_cursor = model_db.cursor()
            loss_index = GetVariableLossIndex(name="WAR", is_pro=False, is_hitter=True)
            model_cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?,?,?,?,?,?)", (model_name, 1, train_results.best_loss, train_results.test_losses[loss_index][-1], i, pro_network.GetNumLayers(), pro_network.GetHiddenSize(), col_network.GetNumLayers(), col_network.GetHiddenSize()))
            
            # Insert hitters that were trained on so that they can be marked on the site
            model_cursor.executemany("INSERT INTO PlayersInTrainingData VALUES(?,?,?,?,?,?)", [(
                ids[0],
                ids[1],
                model_id,
                i,
                1,
                1
                ) for ids in train_dataset.ids])
            model_cursor.executemany("INSERT INTO PlayersInTrainingData VALUES(?,?,?,?,?,?)", [(
                ids[0],
                ids[1],
                model_id,
                i,
                1,
                0
                ) for ids in test_dataset.ids])
            model_db.commit()
            
            # Force VRAM to get cleared before allocating next iteration
            del pro_network
            del col_network
            del train_dataset
            del test_dataset
            torch.cuda.empty_cache()
            gc.collect()