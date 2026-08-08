from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc
import sys

from PitchModel.Constants import device, pitch_db, tracking_db, BUCKET_INPLAY_VALUE
from PitchModel.Shared import GetDataPrep
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelOutputType import *
from PitchModel.PitchDBTypes import *

from line_profiler import LineProfiler
eval_profiler = LineProfiler()
_SHOULD_PROFILE = True

def GetPosNegScale(pos_sum : float, neg_sum : float) -> tuple[float, float]:
    s = pos_sum + neg_sum
    if abs(s) < 1:
        return 1,1
    
    d = (pos_sum * pos_sum) + (neg_sum * neg_sum)
    pos_scale = 1.0 - (pos_sum * s) / d
    neg_scale = 1.0 - (neg_sum * s) / d
    return pos_scale, neg_scale

@eval_profiler
def Eval_Pitches(end_year : int):
    BATCH_SIZE = 2000000
    
    # Delete old data
    cursor = pitch_db.cursor()
    cursor.execute("DELETE FROM Output_PitchValue")
    pitch_db.commit()
    
    # Get Models
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    
    for model_id, model_name in tqdm(model_ids, desc="Evaluating Pitch Architectures"):
        model_years = [row[0] for row in cursor.execute(
            "SELECT DISTINCT Year FROM ModelTrainingHistory_PitchValue WHERE ModelId=? ORDER BY Year ASC",
            (model_id,)).fetchall()]
        
        for model_year in tqdm(model_years[:-1], desc="Model Years", leave=False):
            # DataPrep was saved during training as GetBinaryName(model_id, model_year)
            data_prep = GetDataPrep(model_id, model_year)
            mth_list = DB_ModelTrainingHistory_PitchValue.Select_From_DB(
                cursor, "WHERE ModelId=? AND Year=?", (model_id, model_year))
            
            year = model_year + 1
            if year > end_year:
                continue
            
            pitch_io_data = data_prep.GenerateIOPitches(
                start_year=year,
                end_year=year,
                validation_year=None,
                mlb_only=False
            )
            eval_datasets = CreateTestTrainDatasets(pitch_io_data, eval_mode=True)
            dataset = eval_datasets.train
        
            n_samples = len(dataset)
            num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
            indices = torch.arange(n_samples)
            
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
                        data_stuff, data_comb = tuple(d.to(device, non_blocking=True) for d in data)
                        
                        output_list = []
                        for model_variant_type in tqdm(MODEL_VARIANTS, desc="Model Variants", leave=False):
                            for model_output_type in tqdm(MODEL_OUTPUTS, desc="Model Outputs", leave=False):
                                args = DEFAULT_ARGS_MAP[(model_variant_type, model_output_type)]
                                dataset.SetOutputType(model_output_type)
                                
                                network = PitchModel(args=args, data_prep=data_prep)
                                with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                                    network.load_state_dict(torch.load(f"PitchModel/Models/{model_name}_{model_year}_{mth.ModelRun}_{model_variant_type.name}_{model_output_type.name}.pt"))
                                network.eval()
                                network = network.to(device)
                                
                                match model_variant_type:
                                    case ModelVariantType.Stuff:
                                        model_data = data_stuff
                                    case ModelVariantType.Combined:
                                        model_data = data_comb
                                
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
                                
                                del network
                                torch.cuda.empty_cache()
                                gc.collect()
                                
                        db_data = [tuple(row.tolist()) for row in torch.cat((\
                            mapping_game_ids.unsqueeze(-1),
                            mapping_pitch_nums.unsqueeze(-1),
                            mapping_level_ids.unsqueeze(-1),
                            mapping_pitcher_ids.unsqueeze(-1),
                            *output_list), dim=-1)]
                                
                                
                        cursor.executemany(f"INSERT INTO Output_PitchValue VALUES({model_id},?,?,{model_year},{mth.ModelRun},{year},?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", db_data)
                        
                        # Clear Memory
                        del output_list
                        del data
                        torch.cuda.empty_cache()
                        gc.collect()
            
            del dataset
            del data_prep
            torch.cuda.empty_cache()
            gc.collect()
        
    pitch_db.commit()
    
if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise Exception(f"Expected 1 argument, {len(sys.argv)} provided")
    
    Eval_Pitches(int(sys.argv[1]))