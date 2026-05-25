from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc

from Constants import device, pitch_db, db
from Shared import GetDataPrep
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import _MODEL_OUTPUTS
from PitchDBTypes import *

from line_profiler import LineProfiler
eval_profiler = LineProfiler()
_SHOULD_PROFILE = True

from Buckets import *

def GetPosNegScale(pos_sum : float, neg_sum : float) -> tuple[float, float]:
    s = pos_sum + neg_sum
    if abs(s) < 1:
        return 1,1
    
    d = (pos_sum * pos_sum) + (neg_sum * neg_sum)
    pos_scale = 1.0 - (pos_sum * s) / d
    neg_scale = 1.0 - (neg_sum * s) / d
    return pos_scale, neg_scale

@eval_profiler
def Eval_Pitches():
    BATCH_SIZE = 80000
    
    # Delete old data
    cursor = pitch_db.cursor()
    cursor.execute("DELETE FROM Output_PitchValue")
    pitch_db.commit()
    
    # Get Models
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    
    # Get last year that has data
    db_cursor = db.cursor()
    last_year = db_cursor.execute("SELECT Year FROM PitchStatcast ORDER BY Year DESC LIMIT 1").fetchone()[0]
    
    for model_id, model_name in tqdm(model_ids, desc="Evaluating Pitch Architectures"):
        data_prep = GetDataPrep(model_id)
            
        mth_list = DB_ModelTrainingHistory_PitchValue.Select_From_DB(cursor, "WHERE ModelId=?", (model_id,))
        network = PitchModel(data_prep=data_prep)
        
        for year in tqdm(range(2017, last_year + 1), desc="Years", leave=False):
            if year == last_year:
                last_month = db_cursor.execute("SELECT Max(Month) FROM PitchStatcast WHERE Year=?", (last_year,)).fetchone()[0]
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=last_month, mlb_only=True)
            else:
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=13, mlb_only=True)
            dataset, _ = CreateTestTrainDatasets(pitch_io_list, 
                eval_mode=True,
                dataset_device='cpu')
            
            n_samples = len(dataset)
            num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
            indices = torch.arange(n_samples)
            
            # TODO : Create the model through the arch stored in mth.  Will be the same for all runs for a model
            for mth in tqdm(mth_list, desc="Evaluating Model Copies", leave=False):
                with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                    network.load_state_dict(torch.load(f"Models/_{model_name}_{mth.ModelRun}.pt"))
                network.eval()
                network = network.to(device)
                
                # Iterate through pitches in batches
                with torch.no_grad():
                    for batch_i in tqdm(range(num_batches), desc="Batches", leave=False):
                        # Get data from dataset
                        start = batch_i * BATCH_SIZE
                        end = min(start + BATCH_SIZE, n_samples)
                        batch_indices = indices[start:end]
                        mappings, data, _, _ = dataset.GetEntries(batch_indices)
                        mapping_game_ids, mapping_pitch_nums = mappings
                        data = tuple(d.to(device, non_blocking=True) for d in data)
                        
                        # Run through model
                        outputs = network(data)
                        
                        # Get output
                        result_location = F.softmax(outputs[0], dim=-1)
                        swing_location = F.softmax(outputs[1], dim=-1)
                        inplay_location = F.softmax(outputs[2], dim=-1)
                        
                        result_stuff = F.softmax(outputs[len(_MODEL_OUTPUTS)], dim=-1)
                        swing_stuff = F.softmax(outputs[len(_MODEL_OUTPUTS) + 1], dim=-1)
                        inplay_stuff = F.softmax(outputs[len(_MODEL_OUTPUTS) + 2], dim=-1)
                        
                        result_combined = F.softmax(outputs[2 * len(_MODEL_OUTPUTS)], dim=-1)
                        swing_combined = F.softmax(outputs[(2 * len(_MODEL_OUTPUTS)) + 1], dim=-1)
                        inplay_combined = F.softmax(outputs[(2 * len(_MODEL_OUTPUTS)) + 2], dim=-1)
                            
                        # Get expected value of in-play
                        inplay_expected = data_prep.ip_bucket_value.to(result_combined.device)
                        inplay_expected_location = (inplay_location * inplay_expected).sum(dim=1, keepdim=True)
                        inplay_expected_stuff = (inplay_stuff * inplay_expected).sum(dim=1, keepdim=True)
                        inplay_expected_combined = (inplay_combined * inplay_expected).sum(dim=1, keepdim=True)
                        
                        db_data = [tuple(row.tolist()) for row in torch.cat((\
                            mapping_game_ids.unsqueeze(-1),
                            mapping_pitch_nums.unsqueeze(-1),
                            result_location.cpu(),
                            swing_location.cpu(),
                            inplay_expected_location.cpu(),
                            result_stuff.cpu(),
                            swing_stuff.cpu(),
                            inplay_expected_stuff.cpu(),
                            result_combined.cpu(),
                            swing_combined.cpu(),
                            inplay_expected_combined.cpu(),), dim=-1)]
                        
                        
                        cursor.executemany(f"INSERT INTO Output_PitchValue VALUES({model_id},?,?,{mth.ModelRun},{year},?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", db_data)
                        
                        # Clear Memory
                        del result_location
                        del swing_location
                        del inplay_location
                        del result_stuff
                        del swing_stuff
                        del inplay_stuff
                        del result_combined
                        del swing_combined
                        del inplay_combined
                        del inplay_expected_location
                        del inplay_expected_stuff
                        del inplay_expected_combined
                        del data
                        torch.cuda.empty_cache()
                        gc.collect()
                
            del dataset
            torch.cuda.empty_cache()
            gc.collect()
            
        del network
        torch.cuda.empty_cache()
        gc.collect()
        
    pitch_db.commit()