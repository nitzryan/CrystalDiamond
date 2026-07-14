from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc

from PitchModel.Constants import device, pitch_db, db
from PitchModel.Shared import GetDataPrep
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelOutputType import *
from PitchModel.PitchDBTypes import *

from line_profiler import LineProfiler
eval_profiler = LineProfiler()
_SHOULD_PROFILE = True

from PitchModel.Buckets import *

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
    BATCH_SIZE = 200000
    
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
        
        
        for year in tqdm(range(2017, last_year + 1), desc="Years", leave=False):
            if year == last_year:
                last_month = db_cursor.execute("SELECT Max(Month) FROM PitchStatcast WHERE Year=?", (last_year,)).fetchone()[0]
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=last_month, mlb_only=False)
            else:
                pitch_io_list = data_prep.GenerateIOPitches(start_year=year, end_year=year, end_month=13, mlb_only=False)
            dataset, _ = CreateTestTrainDatasets(pitch_io_list, 
                eval_mode=True)
            
            n_samples = len(dataset)
            num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
            indices = torch.arange(n_samples)
            
            # TODO : Create the model through the arch stored in mth.  Will be the same for all runs for a model
            for mth in tqdm(mth_list, desc="Evaluating Model Copies", leave=False):
                # Iterate through pitches in batches
                with torch.no_grad():
                    for batch_i in tqdm(range(num_batches), desc="Batches", leave=False):
                        # Get data from dataset
                        start = batch_i * BATCH_SIZE
                        end = min(start + BATCH_SIZE, n_samples)
                        batch_indices = indices[start:end]
                        mappings, data, _ = dataset.GetEntries(batch_indices, eval_mode=True)
                        mappings = tuple(m.to('cpu', non_blocking=True) for m in mappings)
                        mapping_game_ids, mapping_pitch_nums, mapping_pitcher_ids, mapping_level_ids = mappings
                        data_ovr, data_loc, data_stuff, data_comb, data_game, data_league = tuple(d.to(device, non_blocking=True) for d in data)
                        
                        output_list = []
                        for model_variant_type in tqdm(MODEL_VARIANTS, desc="Model Variants", leave=False):
                            for model_output_type in tqdm(MODEL_OUTPUTS, desc="Model Outputs", leave=False):
                                args = DEFAULT_ARGS_MAP[(model_variant_type, model_output_type)]
                                dataset.SetOutputType(model_output_type)
                                
                                network = PitchModel(args=args, data_prep=data_prep)
                                with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                                    network.load_state_dict(torch.load(f"PitchModel/Models/{model_name}_{mth.ModelRun}_{model_variant_type.name}_{model_output_type.name}.pt"))
                                network.eval()
                                network = network.to(device)
                                
                                match model_variant_type:
                                    case ModelVariantType.Stuff:
                                        model_data = torch.cat((data_ovr, data_stuff, data_league), dim=-1)
                                    case ModelVariantType.Combined:
                                        model_data = torch.cat((data_ovr, data_loc, data_stuff, data_comb, data_league), dim=-1)
                                
                                # Run through model
                                output = network(model_data)
                                result = F.softmax(output, dim=-1)
                                    
                                if model_output_type == ModelOutputType.InPlay:
                                    # Get expected value of in-play
                                    inplay_expected = data_prep.ip_bucket_value.to(result.device)
                                    inplay_expected_output = (result * inplay_expected).sum(dim=1, keepdim=True)
                                    output_list.append(inplay_expected_output.cpu())
                                else:
                                    output_list.append(result.cpu())
                                
                        db_data = [tuple(row.tolist()) for row in torch.cat((\
                            mapping_game_ids.unsqueeze(-1),
                            mapping_pitch_nums.unsqueeze(-1),
                            mapping_level_ids.unsqueeze(-1),
                            mapping_pitcher_ids.unsqueeze(-1),
                            output_list[0],
                            output_list[1],
                            output_list[2],
                            output_list[3],
                            output_list[4],
                            output_list[5]), dim=-1)]
                                
                                
                        cursor.executemany(f"INSERT INTO Output_PitchValue VALUES({model_id},?,?,{mth.ModelRun},{year},?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", db_data)
                        
                        # Clear Memory
                        del output_list
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
    
if __name__ == "__main__":
    Eval_Pitches()