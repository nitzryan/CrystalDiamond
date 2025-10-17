from Data_Prep import Data_Prep, Player_IO
import torch
from Eval_Dataset import Eval_Dataset
from Player_Model import RNN_Model
from tqdm import tqdm
from Constants import device, db, WAR_BUCKET_AVG, VALUE_BUCKET_AVG
from DBTypes import *
import torch.nn.functional as F
import warnings
import Prep_Map
import Output_Map


if __name__ == "__main__":
    cursor = db.cursor()
    cursor.execute("DELETE FROM Output_PlayerWar WHERE isHitter=1")
    cursor.execute("DELETE FROM Output_HitterStats")
    db.commit()
    cursor = db.cursor()
    model_idxs = cursor.execute("SELECT hitterModelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Evaluating Architectures"):
        if model_id == 1:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 2:
            prep_map = Prep_Map.statsonly_prep_map
        
        output_map = Output_Map.base_output_map
        
        data_prep = Data_Prep(prep_map, output_map)
        hitter_io_list : list[Player_IO] = data_prep.Generate_IO_Hitters("WHERE isHitter=?", (1,), use_cutoff=False)
        
        lengths = torch.tensor([io.length for io in hitter_io_list])

        dates_padded = torch.nn.utils.rnn.pad_sequence([io.dates for io in hitter_io_list])
        x_padded = torch.nn.utils.rnn.pad_sequence([io.input for io in hitter_io_list])
        mask_prospect_padded = torch.nn.utils.rnn.pad_sequence([io.prospect_mask for io in hitter_io_list])
        eval_hitters_dataset = Eval_Dataset(x_padded, lengths, dates_padded, mask_prospect_padded)
        batch_size = 1000
        generator = torch.utils.data.DataLoader(eval_hitters_dataset, batch_size=batch_size, shuffle=False)
        
        # Setup Model
        cursor = db.cursor()
        mth = DB_Model_TrainingHistory.Select_From_DB(cursor, "WHERE ModelName=?", (model_name,))
        num_layers = mth[0].NumLayers
        hidden_size = mth[0].HiddenSize
        network = RNN_Model(x_padded[0].shape[1], num_layers, hidden_size, None, data_prep=data_prep, is_hitter=True)
        
        for m in tqdm(mth, desc="Evaluation Model Copies", leave=False):
            model_idx = int(m.ModelIdx)
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                network.load_state_dict(torch.load(f"Models/{m.ModelName}_{model_idx}.pt"))
            network.eval()
            network = network.to(device)

            for batch, (data, length, dtes, mask) in tqdm(enumerate(generator), total=len(generator), desc="Evaluating Hitters", leave=False):
                data, length, dtes, mask = data.to(device), length.to(device), dtes.to(device), mask.to(device)
                output_war, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPositions, output_mlbValue = network(data, length)
                output_war = F.softmax(output_war, dim=2)
                output_value = F.softmax(output_value, dim=2)
                
                war = torch.zeros(size=(output_war.size(0), output_war.size(1))).to(device)
                value = torch.zeros(size=(output_war.size(0), output_war.size(1))).to(device)
                for i in range(1, len(WAR_BUCKET_AVG)):
                    war[:,:] += output_war[:,:,i] * WAR_BUCKET_AVG[i]
                    value[:,:] += output_value[:,:,i] * VALUE_BUCKET_AVG[i]
                
                mask = mask[:,:output_war.shape[1]]
                dtes = dtes[:,:output_war.shape[1],:] # Network will only output largest length in batch
                mlbIds = dtes[:,:,0].unsqueeze(2)
                modelIdxs = torch.zeros(size=(mlbIds.shape[0], mlbIds.shape[1], 1), dtype=torch.long).to(device)
                modelIdxs[:,:,0] = model_idx
                dtes = dtes[:,:,1:]
                
                opw = torch.cat((mask.unsqueeze(-1), mlbIds, modelIdxs, dtes, output_war, war.unsqueeze(-1), output_value, value.unsqueeze(-1)), dim=2)
                
                db_data = torch.nn.utils.rnn.unpad_sequence(opw, length, batch_first=True)
                
                cursor = db.cursor()
                for dbd in db_data:
                    # Only log where player is actually a prospect
                    dbd = dbd[dbd[:,0] != 0]
                    d = dbd[:,1:]
                    vals = [tuple(x) for x in d.tolist()]
                    cursor.executemany(f"INSERT INTO Output_PlayerWar VALUES(?,{model_id},1,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", vals)
                db.commit()
                