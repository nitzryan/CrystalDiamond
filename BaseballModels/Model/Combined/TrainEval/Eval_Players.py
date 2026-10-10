
from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Combined.DataPrep.Player_Dataset import Combined_Player_Dataset
from Model.Pro.Model.Player_Model import Recurrent_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.Constants import *
from Model.Utilities import GetModelMaps
from Model.EvalStats import getOutputHitterStats, getOutputPitcherStats
from Model.ModelDBTypes import *

def Eval_Players(eval_update : bool, is_hitter : bool, train_only : bool):
    with torch.no_grad():
        BATCH_SIZE = 12000
        
        is_hitter_int = 1 if is_hitter else 0
        player_type = "hit" if is_hitter else "pit"
        college_table = "Output_College_Hitter" if is_hitter else "Output_College_Pitcher"
        stats_table = "Output_HitterStats" if is_hitter else "Output_PitcherStats"
        mlbwar_table = "Output_HitterMlbWar" if is_hitter else "Output_PitcherMlbWar"
        mlbwar_avg_table = "SingleYearHitterBucketAverages" if is_hitter else "SingleYearPitcherBucketAverages"
        mlbwar_slot_classes = MLB_WAR_HEAD_CLASSES_HITTER if is_hitter else MLB_WAR_HEAD_CLASSES_PITCHER
        war_col = "warHitter" if is_hitter else "warPitcher"
        eligible_col = "IsHitter" if is_hitter else "IsPitcher"
        pt_means_attr = f"__{player_type}lvlpt_means"
        pt_devs_attr = f"__{player_type}lvlpt_devs"
        stat_means_attr = f"__{player_type}lvlstat_means"
        stat_devs_attr = f"__{player_type}lvlstat_devs"
        arch_desc = "Evaluating Hitting Architectures" if is_hitter else "Evaluating Pitching Architectures"
        
        cursor = model_db.cursor()
        
        if eval_update:
            # Get the last date of data increment 1 month for pro, 1 year for college
            year, month = cursor.execute(f"SELECT year, month FROM Output_PlayerWarAggregation WHERE isHitter={is_hitter_int} ORDER BY year DESC, month DESC LIMIT 1").fetchone()
            month += 1
            if month > 9:
                month = 4
                year += 1
            
            college_year, = cursor.execute(f"SELECT year FROM {college_table} ORDER BY year DESC LIMIT 1;").fetchone()
            college_year += 1
        else:
            cursor.execute(f"DELETE FROM Output_PlayerWar WHERE isHitter={is_hitter_int}")
            cursor.execute(f"DELETE FROM Output_PlayerHighestLevel WHERE isHitter={is_hitter_int}")
            cursor.execute(f"DELETE FROM {college_table}")
            cursor.execute(f"DELETE FROM {stats_table}")
            cursor.execute(f"DELETE FROM {mlbwar_table}")
            model_db.commit()
        cursor = model_db.cursor()
        model_list = cursor.execute("SELECT modelName, id FROM ModelId ORDER BY id ASC").fetchall()
        
        # Get average WAR in each bucket
        base_db = db.cursor()
        war_bucket_averages = [0]
        for i in range(1, len(TOTAL_WAR_BUCKETS)):
            bucket_min = TOTAL_WAR_BUCKETS[i - 1].item()
            bucket_max = min(TOTAL_WAR_BUCKETS[i].item(), 100)
            war_bucket_averages.append(base_db.execute(f"SELECT AVG({war_col}) FROM Model_Players WHERE IsEligible=1 AND {eligible_col}=1 AND {war_col}>{bucket_min} AND {war_col}<={bucket_max}").fetchone()[0])
        if not eval_update:
            cursor.execute(f"DELETE FROM WarBucketAverages WHERE isHitter={is_hitter_int}")
            cursor.execute("INSERT INTO WarBucketAverages VALUES (?,?,?,?,?,?,?)", 
                (is_hitter_int, 
                 war_bucket_averages[1], 
                 war_bucket_averages[2],
                 war_bucket_averages[3],
                 war_bucket_averages[4],
                 war_bucket_averages[5],
                 war_bucket_averages[6],))
            model_db.commit()
            cursor = model_db.cursor()
        
        # Single-year average value of each bucket, per prediction offset
        mlbwar_bucket_averages = Get_MlbWar_Bucket_Averages(is_hitter)
        if not eval_update:
            cursor.execute(f"DELETE FROM {mlbwar_avg_table}")
            Insert_MlbWar_Bucket_Averages(cursor, mlbwar_avg_table, mlbwar_bucket_averages)
            model_db.commit()
            cursor = model_db.cursor()

        
        for model_name, model_id in tqdm(model_list, desc=arch_desc):
            # Get data for model
            pro_prep_map, pro_output_map, col_prep_map, col_output_map = GetModelMaps(model_id)
            
            data_prep = Combined_Data_Prep(
                prep_map=pro_prep_map,
                output_map=pro_output_map,
                college_prep_map=col_prep_map,
                college_output_map=col_output_map,
                save_name=f"Model/{GetDataPrepBinaryFile(model_id)}",
            ) if is_hitter else Combined_Data_Prep.Load_From_File(f"Model/{GetDataPrepBinaryFile(model_id)}")
            
            if eval_update:
                io_list = data_prep.Generate_IO_Hitters_Update(year, month, college_year) if is_hitter \
                        else data_prep.Generate_IO_Pitchers_Update(year, month, college_year)
            else:
                io_list = data_prep.Generate_IO_Hitters(is_training=train_only) if is_hitter \
                        else data_prep.Generate_IO_Pitchers(is_training=train_only)
            
            # Data to unnormalize values from model
            pro_pt_mean : torch.Tensor = data_prep.pro_data_prep.__getattribute__(pt_means_attr).to(device)
            pro_pt_devs : torch.Tensor = data_prep.pro_data_prep.__getattribute__(pt_devs_attr).to(device)
            pro_lvlstat_mean : torch.Tensor = data_prep.pro_data_prep.__getattribute__(stat_means_attr).to(device)
            pro_lvlstat_devs : torch.Tensor = data_prep.pro_data_prep.__getattribute__(stat_devs_attr).to(device)
            
            # Get Models
            cursor = model_db.cursor()
            mth_list = DB_Model_TrainingHistory.Select_From_DB(cursor, f"WHERE ModelName=? AND IsHitter={is_hitter_int}", (model_name,))
            
            # Load model architecture
            pro_network = ProModel.LoadFromFile(f"Model/Models/{model_name}_{player_type}_pro.json", data_prep.pro_data_prep)
            col_network = ColModel.LoadFromFile(f"Model/Models/{model_name}_{player_type}_col.json", data_prep.college_data_prep)
            
            # Set Model weights from training runs
            cursor = model_db.cursor()
            for mth in tqdm(mth_list, desc="Evaluating Model Copies", leave=False):
                model_run = int(mth.ModelRun)
                
                with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                    pro_network.load_state_dict(torch.load(f"Model/Models/pro_{mth.ModelName}_{model_run}_{player_type}.pt"))
                    col_network.load_state_dict(torch.load(f"Model/Models/col_{mth.ModelName}_{model_run}_{player_type}.pt"))
                pro_network.eval()
                col_network.eval()
                pro_network = pro_network.to(device)
                col_network = col_network.to(device)
                
                # Load players, ignoring any in training
                mlb_ids_set = set(x[0] for x in cursor.execute(f"SELECT mlbId FROM PlayersInTrainingData WHERE mlbId!=-1 AND isHitter={is_hitter_int} AND modelId=? AND modelRun=? AND isTrain=1", (model_id, model_run)).fetchall())
                tbc_ids_set = set(x[0] for x in cursor.execute(f"SELECT tbcId FROM PlayersInTrainingData WHERE tbcId!=-1 AND isHitter={is_hitter_int} AND modelId=? AND modelRun=? AND isTrain=1", (model_id, model_run)).fetchall())
                
                run_io_list = []
                for io in io_list:
                    if io.pro_io.player is not None and not io.pro_io.player.mlbId in mlb_ids_set:
                        run_io_list.append(io)
                    elif io.college_io.player is not None and not io.college_io.player.TBCId in tbc_ids_set:
                        run_io_list.append(io)
                
                # Create dataset to stream players to model
                dataset : Combined_Player_Dataset
                dataset, _ = Create_Test_Train_Datasets(player_list=run_io_list, is_hitter=is_hitter, device='cpu', eval_mode=True)
                
                n_samples = len(dataset)
                num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
                indices = torch.arange(n_samples)
                
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
                    *col_outputs_raw, i0 = col_network(col_data, col_length)
                    
                    # Get College Data
                    col_mask_valid = col_length > 0
                    col_outputs = [F.softmax(out[col_mask_valid], dim=-1) for out in col_outputs_raw]
                    
                    col_output_draft = col_outputs[0]
                    col_output_war = col_outputs[1]
                    
                    draftMean = torch.zeros(size=(col_output_draft.size(0), col_output_draft.size(1))).to(device)
                    for i in range(len(DRAFT_MEANS)):
                        draftMean[:,:] += col_output_draft[:,:,i] * DRAFT_MEANS[i]
                        
                    warMean = torch.zeros(size=(col_output_war.size(0), col_output_war.size(1))).to(device)
                    for i in range(1, len(war_bucket_averages)):
                        warMean[:,:] += col_output_war[:,:,i] * war_bucket_averages[i]
                        
                    # Format DB Tensor
                    col_dtes = dataset.col_dates.to(device)[batch_indices][col_mask_valid,:col_output_draft.shape[1]]
                    ids = col_dtes[:,:,0].unsqueeze(2)
                    years = col_dtes[:,:,1].unsqueeze(2)
                    col_model_runs = torch.zeros_like(years)
                    col_model_runs[:,:,0] = model_run
                        
                    db_input = torch.cat(
                        (ids, col_model_runs, years, col_outputs[0], draftMean.unsqueeze(-1),
                         col_outputs[1], warMean.unsqueeze(-1), *col_outputs[2:]), # All remaining outputs
                        dim=2
                    )
                    db_input = torch.nn.utils.rnn.unpad_sequence(db_input, col_length[col_mask_valid], batch_first=True)
                    
                    # SQL Query for college data
                    for d in db_input:
                        if eval_update:
                            vals = [tuple(x) for x in d.tolist() if x[2] == college_year]
                        else:
                            vals = [tuple(x) for x in d.tolist()]
                        if len(vals) > 0:
                            # Set the # of parameters to the number in the list
                            n_params = len(vals[0])
                            placeholders = ','.join(['?'] * (n_params - 1))
                            cursor.executemany(f"INSERT INTO {college_table} VALUES(?,{model_id},{placeholders})", vals)
                    
                    
                    # Run Through Pro Model
                    pro_data, pro_length, pro_pt_levelYearGames, player_demo, player_bios, months = pro_data
                    prospect_mask, _, _, _ = pro_masks
                    
                    mask_valid = pro_length > 0
                    pro_data = pro_data[mask_valid].to(device, non_blocking=True)
                    pro_length = pro_length[mask_valid].to(device, non_blocking=True)
                    pro_pt_levelYearGames = pro_pt_levelYearGames[mask_valid].to(device, non_blocking=True)
                    i0 = i0[mask_valid].to(device, non_blocking=True)
                    player_demo = player_demo[mask_valid].to(device, non_blocking=True)
                    player_bios = player_bios[mask_valid].to(device, non_blocking=True)
                    months = months[mask_valid].to(device, non_blocking=True)
                    
                    pro_output_war, pro_output_level, pro_output_pa, pro_output_stats, pro_output_pos, pro_output_pt, pro_output_mlbwar = pro_network(pro_data, pro_length, pro_pt_levelYearGames, i0, player_demo, player_bios, months)
                    
                    # Insert Pro Data
                    pro_output_war = F.softmax(pro_output_war, dim=2) 
                    pro_war = torch.zeros(size=(pro_output_war.size(0), pro_output_war.size(1))).to(device)
                    for i in range(1, len(war_bucket_averages)):
                        pro_war[:,:] += pro_output_war[:,:,i] * war_bucket_averages[i]
                    
                    pro_dates = dataset.pro_dates.to(device)[batch_indices][mask_valid,:pro_output_war.shape[1]]
                    mask = prospect_mask.to(device)[mask_valid,:pro_output_war.shape[1]]
                    pro_dtes = pro_dates[:,:]
                    mlbIds = pro_dates[:,:,0].unsqueeze(2)
                    pro_model_runs = torch.zeros(size=(mlbIds.shape[0], mlbIds.shape[1], 1), dtype=torch.long).to(device)
                    pro_model_runs[:,:,0] = model_run
                    pro_dtes = pro_dtes[:,:,1:]
                    
                    opw = torch.cat((mask.unsqueeze(-1), mlbIds, pro_model_runs, pro_dtes, pro_output_war, pro_war.unsqueeze(-1)), dim=2)
                    db_data = torch.nn.utils.rnn.unpad_sequence(opw, pro_length, batch_first=True)
                    for dbd in db_data:
                        # Only log where player is actually a prospect
                        dbd = dbd[dbd[:,0] != 0]
                        d = dbd[:,1:]
                        if eval_update:
                            vals = [tuple(x) for x in d.tolist() if x[2] == year and x[3] == month]
                        else:
                            vals = [tuple(x) for x in d.tolist()]
                        if len(vals) > 0:
                            cursor.executemany(f"INSERT INTO Output_PlayerWar VALUES(?,{model_id},{is_hitter_int},?,?,?,?,?,?,?,?,?,?,?)", vals)
                    
                    # Insert Pro Level
                    pro_output_level = F.softmax(pro_output_level, dim=2)
                    ophl = torch.cat((mask.unsqueeze(-1), mlbIds, pro_model_runs, pro_dtes, pro_output_level), dim=2)
                    db_data = torch.nn.utils.rnn.unpad_sequence(ophl, pro_length, batch_first=True)
                    for dbd in db_data:
                        # Only log where player is actually a prospect
                        dbd = dbd[dbd[:,0] != 0]
                        d = dbd[:,1:]
                        if eval_update:
                            vals = [tuple(x) for x in d.tolist() if x[2] == year and x[3] == month]
                        else:
                            vals = [tuple(x) for x in d.tolist()]
                        if len(vals) > 0:
                            cursor.executemany(f"INSERT INTO Output_PlayerHighestLevel VALUES(?,{model_id},{is_hitter_int},?,?,?,?,?,?,?,?,?,?,?)", vals)
                    
                    # Insert MLB WAR/PA/Outs predictions: one row per player/timestep
                    mlbwar_heads = Split_MlbWar_Heads(pro_output_mlbwar, mlbwar_slot_classes)
                    mlbwar_values = Build_MlbWar_Rows(mlbwar_heads, mlbwar_bucket_averages, Get_MlbWar_Row_Keys(pro_dtes[:,:,1]))
                    mlbwar_data = torch.cat((mlbIds, pro_model_runs, pro_dtes, mlbwar_values), dim=2)
                    mlbwar_data = torch.nn.utils.rnn.unpad_sequence(mlbwar_data, pro_length, batch_first=True)
                    for dbd in mlbwar_data:
                        d = dbd[1:]  # Skip initialization timestep
                        if eval_update:
                            vals = [tuple(x) for x in d.tolist() if x[2] == year and x[3] == month]
                        else:
                            vals = [tuple(x) for x in d.tolist()]
                        if len(vals) > 0:
                            n_params = len(vals[0])
                            placeholders = ','.join(['?'] * (n_params - 1))
                            cursor.executemany(f"INSERT INTO {mlbwar_table} VALUES(?,{model_id},{placeholders})", vals)

                    
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
                    
                    stats_tensor = getOutputHitterStats(pro_length, mlbIds, pro_dtes, pro_pt_values, pro_stats_values, pro_pos_values, model_id, model_run) if is_hitter \
                            else getOutputPitcherStats(pro_length, mlbIds, pro_dtes, pro_pt_values, pro_stats_values, pro_pos_values, model_id, model_run)
                    if eval_update:
                        mask = (stats_tensor[:, 3] == year) & (stats_tensor[:, 4] == month)
                        stats_tensor = stats_tensor[mask]
                    n_stats_cols = stats_tensor.shape[1]
                    stats_placeholders = ','.join(['?'] * n_stats_cols)
                    cursor.executemany(f"INSERT INTO {stats_table} VALUES({stats_placeholders})", stats_tensor.tolist())
                del dataset
            
            # Force VRAM to get cleared before allocating next iteration
            del io_list
            del pro_network
            del col_network
            torch.cuda.empty_cache()
            gc.collect()
            
            model_db.commit()
        
def Insert_MlbWar_Bucket_Averages(cursor : sqlite3.Cursor, table : str, averages : list[torch.Tensor]) -> None:
    for key in MLB_WAR_AVERAGE_KEYS:
        vals = [key] + [x for avg in averages for x in avg[key].tolist()]
        placeholders = ','.join(['?'] * len(vals))
        cursor.execute(f"INSERT INTO {table} VALUES({placeholders})", vals)
        
def Get_MlbWar_Bucket_Averages(is_hitter : bool) -> list[torch.Tensor]:
    # Average actual value in each bucket of each slot, from single-year target rows on a full-season scale.
    # Row key 0 = full-year targets, row key m (4-8) = partial-year targets from source month m.
    # Returns one [MLB_WAR_ROW_COUNT, C_k] tensor per slot.
    cursor = db.cursor()
    if is_hitter:
        rows = cursor.execute("SELECT Offset, Month, WAR, PA FROM Model_HitterWarValues").fetchall()
        slot_bounds = [MLB_YEAR_WAR_BUCKETS_HITTER, MLB_YEAR_PA_BUCKETS_HITTER]
    else:
        rows = cursor.execute("SELECT Offset, Month, outsSP, WarSP, outsRP, WarRP FROM Model_PitcherWarValues").fetchall()
        slot_bounds = [MLB_YEAR_OUTS_BUCKETS_SP, MLB_YEAR_WAR_BUCKETS_SP, MLB_YEAR_OUTS_BUCKETS_RP, MLB_YEAR_WAR_BUCKETS_RP]

    data = torch.tensor(rows, dtype=DTYPE)
    offsets = data[:, 0].long()
    months = data[:, 1].long()

    # Same scaling and keying as Generate_IO
    is_partial = (offsets == 0) & (months < SEASON_END_MONTH)
    denom = (SEASON_END_MONTH - months).clamp(min=1).to(DTYPE)
    scale = torch.where(is_partial, SEASON_LENGTH_MONTHS / denom, torch.ones_like(denom))
    keys = torch.where(is_partial, months, torch.zeros_like(months))

    averages = []
    for k, bounds in enumerate(slot_bounds):
        values = data[:, 2 + k] * scale # Need to get bucket for scale value
        num_classes = len(bounds) + 1
        buckets = torch.bucketize(values, bounds)
        values /= scale # Need to store actual value
        idx = keys * num_classes + buckets

        sums = torch.zeros(MLB_WAR_ROW_COUNT * num_classes, dtype=DTYPE).index_add_(0, idx, values)
        counts = torch.zeros(MLB_WAR_ROW_COUNT * num_classes, dtype=DTYPE).index_add_(0, idx, torch.ones_like(values))
        averages.append((sums / counts.clamp(min=1)).reshape(MLB_WAR_ROW_COUNT, num_classes))
    return averages

def Split_MlbWar_Heads(pred : torch.Tensor, classes : list[int]) -> list[torch.Tensor]:
    # pred: [B, T, M * sum(classes)] -> one probability tensor per slot, each [B, T, M, C_k]
    batch_size = pred.size(0)
    time_steps = pred.size(1)
    pred = pred.reshape(batch_size, time_steps, NUM_MLB_YEAR_OFFSETS, sum(classes))
    return [F.softmax(slot, dim=-1) for slot in torch.split(pred, classes, dim=-1)]

def Build_MlbWar_Rows(heads : list[torch.Tensor], averages : list[torch.Tensor], row_keys : torch.Tensor) -> torch.Tensor:
    # heads[k]: [B, T, M, C_k] probabilities; averages[k]: [MLB_WAR_ROW_COUNT, C_k]; row_keys: [B, T, M]
    # Returns [B, T, M * sum(C_k + 1)], ordered offset -> slot -> (bucket probabilities, expected value)
    parts = []
    for m in range(NUM_MLB_YEAR_OFFSETS):
        for head, avg in zip(heads, averages):
            probs = head[:, :, m, :]
            avg_rows = avg.to(probs.device)[row_keys[:, :, m]]   # [B, T, C_k]
            expected = (probs * avg_rows).sum(dim=-1, keepdim=True)
            parts.append(probs)
            parts.append(expected)
    return torch.cat(parts, dim=2)

def Get_MlbWar_Row_Keys(months : torch.Tensor) -> torch.Tensor:
    # months: [B, T] source month of each timestep -> [B, T, M] average-table row keys
    keys = torch.zeros(months.size(0), months.size(1), NUM_MLB_YEAR_OFFSETS, dtype=torch.long, device=months.device)
    keys[:, :, 0] = torch.where(months < SEASON_END_MONTH, months, torch.zeros_like(months))
    return keys