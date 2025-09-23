from Data_Prep import Data_Prep
import torch
from Eval_Dataset import Eval_Dataset
from Hitter_Model import RNN_Model
from tqdm import tqdm
from Constants import device, experimental_db
from DBTypes import DB_Model_TrainingHistory
import torch.nn.functional as F
import warnings
import Prep_Map
import Output_Map

if __name__ == "__main__":
    cursor = experimental_db.cursor()
    model_idxs = cursor.execute("SELECT hitterModelName, id FROM ModelIdx ORDER BY id ASC").fetchall()
    
    for model_name, model_id in tqdm(model_idxs, desc="Evaluating Architectures"):
        cursor = experimental_db.cursor()
        cursor.execute("DELETE FROM Output_PlayerWar WHERE model=? AND isHitter=?", (model_id,1))
        experimental_db.commit()
    
        if model_id == 1 or model_id == 2:
            prep_map = Prep_Map.base_prep_map
        elif model_id == 3 or model_id == 4:
            prep_map = Prep_Map.statsonly_prep_map
        
        if model_id == 1 or model_id == 3:
            output_map = Output_Map.war_map
        if model_id == 2 or model_id == 4:
            output_map = Output_Map.value_map
        
        data_prep = Data_Prep(prep_map, output_map)
        hitters, inputs, outputs, max_input_size, dates = data_prep.Generate_IO_Hitters("WHERE isHitter=?", (1,))
        
        lengths = torch.tensor([len(seq) for seq in inputs])

        x_padded = torch.nn.utils.rnn.pad_sequence(inputs)
        dates_padded = torch.nn.utils.rnn.pad_sequence(dates)
        eval_hitters_dataset = Eval_Dataset(x_padded, lengths, dates_padded)
        batch_size = 1000
        generator = torch.utils.data.DataLoader(eval_hitters_dataset, batch_size=batch_size, shuffle=False)
        
        # Setup Model
        cursor = experimental_db.cursor()
        mth = DB_Model_TrainingHistory.Select_From_DB(cursor, "WHERE ModelName=?", (model_name,))
        num_layers = mth[0].NumLayers
        hidden_size = mth[0].HiddenSize
        network = RNN_Model(x_padded[0].shape[1], num_layers, hidden_size, None, output_map=data_prep.output_map)
        
        for m in tqdm(mth, desc="Evaluation Model Copies", leave=False):
            model_idx = int(m.ModelIdx)
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                network.load_state_dict(torch.load(f"Models/{m.ModelName}_{model_idx}.pt"))
            network.eval()
            network = network.to(device)

            for batch, (data, length, dtes) in tqdm(enumerate(generator), total=len(generator), desc="Evaluating Hitters", leave=False):
                data, length, dtes = data.to(device), length.to(device), dtes.to(device)
                output_war, _, _, _ = network(data, length)
                #print(output_war.shape)
                #exit(1)
                #output_war = F.softmax(output_war.squeeze(0).squeeze(1), dim=1)
                output_war = F.softmax(output_war, dim=2)
                
                dtes = dtes[:,:output_war.shape[1],:] # Network will only output largest length in batch
                mlbIds = dtes[:,:,0].unsqueeze(2)
                modelIdxs = torch.zeros(size=(mlbIds.shape[0], mlbIds.shape[1], 1), dtype=torch.long).to(device)
                modelIdxs[:,:,0] = model_idx
                dtes = dtes[:,:,1:]
                opw = torch.cat((mlbIds, modelIdxs, dtes, output_war), dim=2)
                
                db_data = torch.nn.utils.rnn.unpad_sequence(opw, length, batch_first=True)
                
                cursor = experimental_db.cursor()
                for dbd in db_data:
                    vals = [tuple(x) for x in dbd.tolist()]
                    cursor.executemany(f"INSERT INTO Output_PlayerWar VALUES(?,{model_id},1,?,?,?,?,?,?,?,?,?,?)", vals)
                experimental_db.commit()
                