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

@eval_profiler
def main():
    BATCH_SIZE = 200000
    
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
        
        mth_list = DB_ModelTrainingHistory_PitchValue.Select_From_DB(cursor, "WHERE ModelId=?", (model_id,))
        network = PitchModel(data_prep=data_prep)
        
        for year in tqdm(range(2017, last_year + 1), desc="Years", leave=False):
            pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=13, mlb_only=True)
            dataset, _ = CreateTestTrainDatasets(pitch_io_list, 0, 0)
            
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
                
                # Iterate through pitches in batches
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
                    
                    # Get run values from results
                    exp_value_location = (value_location * bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    exp_value_stuff = (value_stuff * bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    exp_value_combined = (value_combined * bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    
                    # Get expected abs value (used for variance)
                    abs_value_location = (value_location * abs_bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    abs_value_stuff = (value_stuff * abs_bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    abs_value_combined = (value_combined * abs_bucket_value).sum(dim=1).to('cpu', non_blocking=True)
                    
                    
                    # Log Results
                    cursor = pitch_db.cursor()
                    pitch_data = []
                    for i in range(len(mapping_game_ids)):
                        pitch_data.append((
                            model_id, 
                            mapping_game_ids[i].item(), 
                            mapping_pitch_nums[i].item(), 
                            mth.ModelRun,
                            abs_value_combined[i].item(),
                            exp_value_stuff[i].item(),
                            exp_value_location[i].item(),
                            exp_value_combined[i].item()
                        ))
                        
                    cursor.executemany("INSERT INTO Output_PitchValue VALUES(?,?,?,?,?,?,?,?)", pitch_data)
                    
                    
                    # Clear Memory
                    del value_location
                    del value_stuff
                    del value_combined
                    del exp_value_location
                    del exp_value_stuff
                    del exp_value_combined
                    del abs_value_location
                    del abs_value_stuff
                    del abs_value_combined
                    torch.cuda.empty_cache()
                    gc.collect()
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
        