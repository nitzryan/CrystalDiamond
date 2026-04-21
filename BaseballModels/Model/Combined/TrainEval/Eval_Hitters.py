from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc

from Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Pro.Model.Player_Model import RNN_Model as ProModel
from College.Model.College_Model import RNN_Model as ColModel
from Constants import device, model_db, DRAFT_MEANS, WAR_BUCKET_AVG, NUM_LEVELS
from Utilities import GetModelMaps
from EvalStats import getOutputHitterStats
from ModelDBTypes import *


if __name__ == "__main__":
    BATCH_SIZE = 4000
    
    cursor = model_db.cursor()
    cursor.execute("DELETE FROM Output_PlayerWar WHERE isHitter=1")
    cursor.execute("DELETE FROM Output_College_Hitter")
    cursor.execute("DELETE FROM Output_HitterStats")
    model_db.commit()
    cursor = model_db.cursor()
    model_idxs = cursor.execute("SELECT modelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Training Architectures"):
        # Get data for model
        pro_prep_map, pro_output_map, col_prep_map, col_output_map = GetModelMaps(model_id)
        
        data_prep = Combined_Data_Prep(
            prep_map=pro_prep_map,
            output_map=pro_output_map,
            college_prep_map=col_prep_map,
            college_output_map=col_output_map
        )
        
        hitter_io_list = data_prep.Generate_IO_Hitters(pro_player_condition="WHERE isHitter=?", pro_player_values=(1,), pro_use_cutoff=False,
                col_player_condition="WHERE isHitter=?", col_player_values=(1,), col_use_cutoff=False)
        
        dataset : Combined_Player_Dataset
        dataset, _ = Create_Test_Train_Datasets(hitter_io_list, 0, 0, is_hitter=True, device='cpu')
        del hitter_io_list
        
        n_samples = len(dataset)
        num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
        indices = torch.arange(n_samples)
        
        # Data to unnormalize values from model
        pro_mlb_value_mean : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hittervalues_means').to(device)
        pro_mlb_value_stds : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hittervalues_devs').to(device)
        pro_pt_mean : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hitlvlpt_means').to(device)
        pro_pt_devs : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hitlvlpt_devs').to(device)
        pro_lvlstat_mean : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hitlvlstat_means').to(device)
        pro_lvlstat_devs : torch.Tensor = data_prep.pro_data_prep.__getattribute__('__hitlvlstat_devs').to(device)
        pro_war_means : torch.Tensor = getattr(data_prep.pro_data_prep, "__hit_prospect_value_means").to(device)
        pro_war_devs : torch.Tensor = getattr(data_prep.pro_data_prep, "__hit_prospect_value_devs").to(device)
        
        # Get Models
        cursor = model_db.cursor()
        mth_list = DB_Model_TrainingHistory.Select_From_DB(cursor, "WHERE ModelName=? AND IsHitter=1", (model_name,))
        
        pro_network = ProModel(
            input_size=dataset.GetProInputSize(),
            mutators=torch.empty(0),
            data_prep=data_prep.pro_data_prep,
            num_layers=mth_list[0].ProNumLayers,
            hidden_size=mth_list[0].ProHiddenSize,
            is_hitter=True,
        )
        col_network = ColModel(
            input_size=dataset.GetColInputSize(),
            data_prep=data_prep.college_data_prep,
            is_hitter=True,
            output_hidden_size=pro_network.GetHiddenSize(),
            output_num_layers=pro_network.GetNumLayers(),
            num_layers=mth_list[0].ColNumLayers,
            hidden_size=mth_list[0].ColHiddenSize,
        )
        
        # Set Model weights from training runs
        cursor = model_db.cursor()
        for mth in tqdm(mth_list, desc="Evalulating Model Copies", leave=False):
            model_idx = int(mth.ModelIdx)
            
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                pro_network.load_state_dict(torch.load(f"Models/pro_{mth.ModelName}_{model_idx}_hit.pt"))
                col_network.load_state_dict(torch.load(f"Models/col_{mth.ModelName}_{model_idx}_hit.pt"))
            pro_network.eval()
            col_network.eval()
            pro_network = pro_network.to(device)
            col_network = col_network.to(device)
            
            # Iterate through players
            for batch_i in tqdm(range(num_batches), desc='Batches', leave=False):
                # Get Data
                start = batch_i * BATCH_SIZE
                end = min(start + BATCH_SIZE, n_samples)
                batch_indices = indices[start:end]
                pro_data, _, pro_masks, col_data, _, _ = dataset.get_batch(batch_indices)
                
                # Run Through College Model
                col_data, col_length = col_data
                col_data = col_data.to(device, non_blocking=True)
                col_length = col_length.to(device, non_blocking=True)
                with torch.no_grad():
                    col_output_draft, col_output_war, col_output_off, col_output_def, col_output_pa, col_output_pos, h0 = col_network(col_data, col_length)
                
                # Insert College Data
                col_mask_valid = col_length > 0

                col_output_draft = F.softmax(col_output_draft[col_mask_valid], dim=-1)
                col_output_war = F.softmax(col_output_war[col_mask_valid], dim=-1)
                col_output_off = F.softmax(col_output_off[col_mask_valid], dim=-1)
                col_output_def = F.softmax(col_output_def[col_mask_valid], dim=-1)
                col_output_pa = F.softmax(col_output_pa[col_mask_valid], dim=-1)
                col_output_pos = F.softmax(col_output_pos[col_mask_valid], dim=-1)
                
                draftMean = torch.zeros(size=(col_output_draft.size(0), col_output_draft.size(1))).to(device)
                for i in range(len(DRAFT_MEANS)):
                    draftMean[:,:] += col_output_draft[:,:,i] * DRAFT_MEANS[i]
                    
                warMean = torch.zeros(size=(col_output_war.size(0), col_output_war.size(1))).to(device)
                for i in range(1, len(WAR_BUCKET_AVG)):
                    warMean[:,:] += col_output_war[:,:,i] * WAR_BUCKET_AVG[i]
                    
                col_dtes = dataset.col_dates.to(device)[batch_indices][col_mask_valid,:col_output_draft.shape[1]]
                dates = col_dtes[:,:,:]
                ids = col_dtes[:,:,0].unsqueeze(2)
                years = col_dtes[:,:,1].unsqueeze(2)
                model_idxs = torch.zeros_like(years)
                model_idxs[:,:,0] = model_idx
                    
                db_input = torch.cat((ids, model_idxs, years, col_output_draft, draftMean.unsqueeze(-1), col_output_war, warMean.unsqueeze(-1), col_output_off, col_output_def, col_output_pa, col_output_pos), dim=2)
                db_input = torch.nn.utils.rnn.unpad_sequence(db_input, col_length[col_mask_valid], batch_first=True)
                for d in db_input:
                    vals = [tuple(x) for x in d.tolist()]
                    cursor.executemany(f"INSERT INTO Output_College_Hitter VALUES(?,{model_id},?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", vals)
                
                
                # Run Through Pro Model
                pro_data, pro_length, pro_pt_levelYearGames = pro_data
                prospect_mask, _, _, _, _ = pro_masks
                
                mask_valid = pro_length > 0
                pro_data = pro_data[mask_valid].to(device, non_blocking=True)
                pro_length = pro_length[mask_valid].to(device, non_blocking=True)
                pro_pt_levelYearGames = pro_pt_levelYearGames[mask_valid].to(device, non_blocking=True)
                h0 = h0[mask_valid].transpose(0, 1).to(device, non_blocking=True)
                
                with torch.no_grad():
                    pro_output_war, pro_output_level, pro_output_pa, pro_output_stats, pro_output_pos, pro_output_mlbValue, pro_output_pt, pro_output_mlbstat = pro_network(pro_data, pro_length, pro_pt_levelYearGames, h0)
                
                
                
                # Insert Pro Data
                pro_output_war = F.softmax(pro_output_war, dim=2) 
                pro_war = torch.zeros(size=(pro_output_war.size(0), pro_output_war.size(1))).to(device)
                for i in range(1, len(WAR_BUCKET_AVG)):
                    pro_war[:,:] += pro_output_war[:,:,i] * WAR_BUCKET_AVG[i]
                
                pro_dates = dataset.pro_dates.to(device)[batch_indices][mask_valid,:pro_output_war.shape[1]]
                mask = prospect_mask.to(device)[mask_valid,:pro_output_war.shape[1]]
                pro_dtes = pro_dates[:,:]
                mlbIds = pro_dates[:,:,0].unsqueeze(2)
                modelIdxs = torch.zeros(size=(mlbIds.shape[0], mlbIds.shape[1], 1), dtype=torch.long).to(device)
                modelIdxs[:,:,0] = model_idx
                pro_dtes = pro_dtes[:,:,1:]
                
                opw = torch.cat((mask.unsqueeze(-1), mlbIds, modelIdxs, pro_dtes, pro_output_war, pro_war.unsqueeze(-1)), dim=2)
                db_data = torch.nn.utils.rnn.unpad_sequence(opw, pro_length, batch_first=True)
                for dbd in db_data:
                    # Only log where player is actually a prospect
                    dbd = dbd[dbd[:,0] != 0]
                    d = dbd[:,1:]
                    vals = [tuple(x) for x in d.tolist()]
                    cursor.executemany(f"INSERT INTO Output_PlayerWar VALUES(?,{model_id},1,?,?,?,?,?,?,?,?,?,?,?)", vals)
                
                # Insert Pro Level Stats
                # Reshape into levels
                pro_output_pt = pro_output_pt.reshape((pro_output_pt.size(0), pro_output_pt.size(1), NUM_LEVELS, pro_output_pt.size(2) // NUM_LEVELS))
                pro_output_stats = pro_output_stats.reshape((pro_output_stats.size(0), pro_output_stats.size(1), NUM_LEVELS, pro_output_stats.size(2) // NUM_LEVELS))
                pro_output_pos = pro_output_pos.reshape((pro_output_pos.size(0), pro_output_pos.size(1), NUM_LEVELS, pro_output_pos.size(2) // NUM_LEVELS))
                
                # Transform output stats
                pro_pt_values = (pro_output_pt * pro_pt_devs) + pro_pt_mean
                pro_stats_values = (pro_output_stats * pro_lvlstat_devs) + pro_lvlstat_mean
                pro_pos_values = F.softmax(pro_output_pos, dim=3)
                
                pro_pt_values = pro_pt_values.to('cpu')
                pro_stats_values = pro_stats_values.to('cpu')
                pro_pos_values = pro_pos_values.to('cpu')
                pro_dtes = pro_dtes.to('cpu')
                pro_length = pro_length.to('cpu')
                mlbIds = mlbIds.to('cpu')
                
                stats_tensor = getOutputHitterStats(pro_length, mlbIds, pro_dtes, pro_pt_values, pro_stats_values, pro_pos_values, model_id, model_idx)
                cursor.executemany("INSERT INTO Output_HitterStats VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", stats_tensor.tolist())
        
        # Force VRAM to get cleared before allocating next iteration
        del pro_network
        del col_network
        del dataset
        torch.cuda.empty_cache()
        gc.collect()
        
        model_db.commit()