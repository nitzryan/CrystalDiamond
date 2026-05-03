from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc

from Constants import device, pitch_db, db
from Shared import GetModelMaps
from Stuff.DataPrep.DataPrep import DataPrep
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import _MODEL_OUTPUTS
from PitchDBTypes import *

from line_profiler import LineProfiler
eval_profiler = LineProfiler()
_SHOULD_PROFILE = True

from Buckets import *
import matplotlib.pyplot as plt

def GetPosNegScale(pos_sum : float, neg_sum : float) -> tuple[float, float]:
    s = pos_sum + neg_sum
    if abs(s) < 1:
        return 1,1
    
    d = (pos_sum * pos_sum) + (neg_sum * neg_sum)
    pos_scale = 1.0 - (pos_sum * s) / d
    neg_scale = 1.0 - (neg_sum * s) / d
    return pos_scale, neg_scale

@eval_profiler
def main():
    BATCH_SIZE = 80000
    
    # Delete old data
    cursor = pitch_db.cursor()
    cursor.execute("DELETE FROM Output_PitchValue")
    pitch_db.commit()
    
    # Get Models
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    
    for model_id, model_name in tqdm(model_ids, desc="Evaluating Architectures"):
        # Get last year that has data
        db_cursor = db.cursor()
        last_year = db_cursor.execute("SELECT Year FROM PitchStatcast ORDER BY Year DESC LIMIT 1").fetchone()[0]
        
        # Generate Data
        prep_map = GetModelMaps(model_id)
        data_prep = DataPrep(prep_map)
        bucket_value = data_prep.bucket_value.to(device, non_blocking=True)
        abs_bucket_value = data_prep.bucket_value.abs().to(device, non_blocking=True)
        
        pos_bucket_value = bucket_value * (bucket_value > 0).float()
        neg_bucket_value = bucket_value * (bucket_value < 0).float()
        
        mth_list = DB_ModelTrainingHistory_PitchValue.Select_From_DB(cursor, "WHERE ModelId=?", (model_id,))
        network = PitchModel(data_prep=data_prep)
        
        for year in tqdm(range(2017, last_year + 1), desc="Years", leave=False):
            if year == last_year:
                last_month = db_cursor.execute("SELECT Max(Month) FROM PitchStatcast WHERE Year=?", (last_year,)).fetchone()[0]
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=last_month, mlb_only=True)
            else:
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=13, mlb_only=True)
            dataset, _ = CreateTestTrainDatasets(pitch_io_list, 0, 0, 'cpu')
            
            n_samples = len(dataset)
            num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
            indices = torch.arange(n_samples)
            
            # TODO : Create the model through the arch stored in mth.  Will be the same for all runs for a model
            for mth in tqdm(mth_list, desc="Evaluating Model Copies", leave=False):
                model_idx = mth.ModelId
                
                with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                    network.load_state_dict(torch.load(f"Models/{model_name}_{mth.ModelRun}.pt"))
                network.eval()
                network = network.to(device)
                
                # Track runs above/below average for pitches
                pitch_data = []
                runs_positive_stuff = 0
                runs_positive_location = 0
                runs_positive_combined = 0
                runs_negative_stuff = 0
                runs_negative_location = 0
                runs_negative_combined = 0
                
                sum_value = 0
                
                # Iterate through pitches in batches
                with torch.no_grad():
                    for batch_i in tqdm(range(num_batches), desc="Batches", leave=False):
                        # Get data from dataset
                        start = batch_i * BATCH_SIZE
                        end = min(start + BATCH_SIZE, n_samples)
                        batch_indices = indices[start:end]
                        mappings, data, _ = dataset.GetEntries(batch_indices)
                        mapping_game_ids, mapping_pitch_nums = mappings
                        data = tuple(d.to(device, non_blocking=True) for d in data)
                        
                        # Run through model
                        outputs = network(data)
                        
                        # Get output
                        value_location = F.softmax(outputs[0], dim=-1)
                        value_stuff = F.softmax(outputs[len(_MODEL_OUTPUTS)], dim=-1)
                        value_combined = F.softmax(outputs[2 * len(_MODEL_OUTPUTS)], dim=-1)
                        
                        sum_value += (value_combined * bucket_value).sum().item()
                        
                        # Get run values from results
                        pos_exp_value_location = (value_location * pos_bucket_value).sum(dim=1).to('cpu')
                        pos_exp_value_stuff = (value_stuff * pos_bucket_value).sum(dim=1).to('cpu')
                        pos_exp_value_combined = (value_combined * pos_bucket_value).sum(dim=1).to('cpu')
                        
                        runs_positive_stuff += pos_exp_value_stuff.sum().item()
                        runs_positive_location += pos_exp_value_location.sum().item()
                        runs_positive_combined += (value_combined * pos_bucket_value).sum().item()
                        
                        neg_exp_value_location = (value_location * neg_bucket_value).sum(dim=1).to('cpu')
                        neg_exp_value_stuff = (value_stuff * neg_bucket_value).sum(dim=1).to('cpu')
                        neg_exp_value_combined = (value_combined * neg_bucket_value).sum(dim=1).to('cpu')
                        
                        runs_negative_stuff += neg_exp_value_stuff.sum().item()
                        runs_negative_location += neg_exp_value_location.sum().item()
                        runs_negative_combined += neg_exp_value_combined.sum().item()
                        
                        # Get expected abs value (used for variance)
                        abs_value_location = (value_location * abs_bucket_value).sum(dim=1).to('cpu')
                        abs_value_stuff = (value_stuff * abs_bucket_value).sum(dim=1).to('cpu')
                        abs_value_combined = (value_combined * abs_bucket_value).sum(dim=1).to('cpu')
                        
                    
                        # Log Results
                        cursor = pitch_db.cursor()
                        
                        for i in range(len(mapping_game_ids)):
                            pitch_data.append((
                                model_id, 
                                mapping_game_ids[i].item(), 
                                mapping_pitch_nums[i].item(), 
                                mth.ModelRun,
                                abs_value_combined[i].item(),
                                pos_exp_value_stuff[i].item(),
                                pos_exp_value_location[i].item(),
                                pos_exp_value_combined[i].item(),
                                neg_exp_value_stuff[i].item(),
                                neg_exp_value_location[i].item(),
                                neg_exp_value_combined[i].item(),
                            ))
                            
                        
                        
                        
                        # Clear Memory
                        del value_location
                        del value_stuff
                        del value_combined
                        del pos_exp_value_location
                        del pos_exp_value_stuff
                        del pos_exp_value_combined
                        del neg_exp_value_location
                        del neg_exp_value_stuff
                        del neg_exp_value_combined
                        del abs_value_location
                        del abs_value_stuff
                        del abs_value_combined
                        torch.cuda.empty_cache()
                        gc.collect()
                        
                
                # Scale positive and negative events so that the expected value is 0
                pos_scale_stuff, neg_scale_stuff = GetPosNegScale(runs_positive_stuff, runs_negative_stuff)
                pos_scale_location, neg_scale_location = GetPosNegScale(runs_positive_location, runs_negative_location)
                pos_scale_combined, neg_scale_combined = GetPosNegScale(runs_positive_combined, runs_negative_combined)
                
                # Log to database
                pitch_db_data = [(
                    p[0], #ModelId
                    p[1], #GameId
                    p[2], #PitchId
                    p[3], #ModelRun
                    p[4], #AbsValue
                    (p[5] * pos_scale_stuff) + (p[8] * neg_scale_stuff), # Stuff
                    (p[6] * pos_scale_location) + (p[9] * neg_scale_location), # Location
                    (p[7] * pos_scale_combined) + (p[10] * neg_scale_combined), # Combined
                    ) for p in pitch_data]
                cursor.executemany(f"INSERT INTO Output_PitchValue VALUES(?,?,?,?,{year},?,?,?,?)", pitch_db_data)
                
            del dataset
            torch.cuda.empty_cache()
            gc.collect()
            
        del network
        torch.cuda.empty_cache()
        gc.collect()
        
    pitch_db.commit()
    
    
if __name__ == "__main__":
    if _SHOULD_PROFILE:
        with eval_profiler:
            main()
        eval_profiler.dump_stats("eval.lprof")
    else:
        main()
        