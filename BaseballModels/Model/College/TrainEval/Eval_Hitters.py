
from Constants import device, model_db, DRAFT_MEANS
from tqdm import tqdm
from Utilities import GetCollegeModelMaps
from College.DataPrep.Data_Prep import College_Data_Prep
from College.DataPrep.Eval_Dataset import College_Eval_Dataset
from College.Model.College_Model import RNN_Model
import torch
from ModelDBTypes import *
import warnings
import torch.nn.functional as F

if __name__ == "__main__":
    cursor = model_db.cursor()
    cursor.execute("DELETE FROM Output_College WHERE isHitter=1")
    model_db.commit()
    cursor = model_db.cursor()
    model_idxs = cursor.execute("SELECT hitterModelName, id FROM ModelIdx_College ORDER BY id ASC").fetchall()
    
    batch_size = 4000
    
    for model_name, model_id in tqdm(model_idxs, desc="Evaluating Architectures"):
        prep_map, output_map = GetCollegeModelMaps(model_id)
        
        # Data
        data_prep = College_Data_Prep(prep_map, output_map)
        hitter_io_list = data_prep.Generate_IO_Hitters("WHERE isHitter=?", (1,), use_cutoff=False)
        eval_dataset = College_Eval_Dataset(hitter_io_list)
        generator = torch.utils.data.DataLoader(eval_dataset, batch_size=batch_size, shuffle=False)
        
        # Model
        cursor = model_db.cursor()
        mth = DB_Model_TrainingHistory_College.Select_From_DB(cursor, "WHERE ModelName=?", (model_name,))
        num_layers = mth[0].NumLayers
        hidden_size = mth[0].HiddenSize
        network = RNN_Model(eval_dataset.GetInputSize(), 
                            num_layers=num_layers, 
                            hidden_size=hidden_size, data_prep=data_prep, is_hitter=True)
        
        for m in tqdm(mth, desc="Evaluating Model Copies", leave=False):
            model_idx = int(m.ModelIdx)
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                network.load_state_dict(torch.load(f"Models/College_{m.ModelName}_{model_idx}_DraftPos.pt"))
            network.eval()
            network = network.to(device)
            
            for (data, lengths, dates) in tqdm(generator, total=len(generator), desc="Evaluating Hitters", leave=False):
                data, lengths, dates = data.to(device), lengths.to(device), dates.to(device)
                output_draft, output_pos = network(data, lengths)
                output_draft = F.softmax(output_draft, dim=-1)
                
                draftMean = torch.zeros(size=(output_draft.size(0), output_draft.size(1))).to(device)
                for i in range(len(DRAFT_MEANS)):
                    draftMean[:,:] += output_draft[:,:,i] * DRAFT_MEANS[i]
                    
                dates = dates[:,:output_draft.shape[1], :]
                ids = dates[:,:,0].unsqueeze(2)
                years = dates[:,:,1].unsqueeze(2)
                model_idxs = torch.zeros_like(years)
                model_idxs[:,:,0] = model_idx
                    
                db_input = torch.cat((ids, model_idxs, years, output_draft, draftMean.unsqueeze(-1)), dim=2)
                db_input = torch.nn.utils.rnn.unpad_sequence(db_input, lengths, batch_first=True)
                for d in db_input:
                    vals = [tuple(x) for x in d.tolist()]
                    cursor.executemany(f"INSERT INTO Output_College VALUES(?,{model_id},1,?,?,?,?,?,?,?,?,?,?)", vals)
                    
                model_db.commit()