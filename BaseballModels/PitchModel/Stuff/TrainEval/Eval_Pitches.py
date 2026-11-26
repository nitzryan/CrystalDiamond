from tqdm import tqdm
import torch
import torch.nn.functional as F
import warnings
import gc
import sys
import copy
from typing import Callable

from PitchModel.Constants import device, pitch_db, tracking_db, BUCKET_INPLAY_VALUE
from PitchModel.Shared import GetDataPrep
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelOutputType import *
from PitchModel.PitchDBTypes import *


def GetPosNegScale(pos_sum : float, neg_sum : float) -> tuple[float, float]:
    s = pos_sum + neg_sum
    if abs(s) < 1:
        return 1,1
    
    d = (pos_sum * pos_sum) + (neg_sum * neg_sum)
    pos_scale = 1.0 - (pos_sum * s) / d
    neg_scale = 1.0 - (neg_sum * s) / d
    return pos_scale, neg_scale

DEFAULT_MODELS_DIR = "PitchModel/Models"

def GetModelFilePath(
        filedir : str,
        model_name : str,
        model_year : int,
        model_run : int,
        model_variant_type : ModelVariantType,
        model_output_type : ModelOutputType) -> str:
    return f"{filedir}/{model_name}_{model_year}_{model_run}_{model_variant_type.name}_{model_output_type.name}.pt"

def LoadNetwork(
        data_prep : DataPrep,
        model_path : str,
        model_variant_type : ModelVariantType,
        model_output_type : ModelOutputType,
        run_device : torch.device | str) -> PitchModel:
    
    args = copy.copy(DEFAULT_ARGS_MAP[(model_variant_type, model_output_type)])
    args.custom_init = False
    
    network = PitchModel(args=args, data_prep=data_prep)
    with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
        network.load_state_dict(torch.load(model_path, map_location=run_device))
    network.eval()
    return network.to(run_device)

def GetTrainPitchers(cursor, model_id : int, model_year : int, model_run : int) -> torch.Tensor:
    rows = DB_PlayersInTrainingData.Select_From_DB(
        cursor, "WHERE modelId=? AND Year=? AND modelRun=?",
        (model_id, model_year, model_run))
    return torch.tensor(sorted({r.mlbId for r in rows if r.isTrain}), dtype=torch.int64)

def GetModelYearRuns(
        cursor,
        model_id : int,
        model_year : int,
        get_train_pitchers : Callable[[int], torch.Tensor] | None = None) -> tuple[list[int], dict[int, torch.Tensor]]:
    mth_list = DB_ModelTrainingHistory_PitchValue.Select_From_DB(
        cursor, "WHERE ModelId=? AND Year=?", (model_id, model_year))
    runs = sorted(mth.ModelRun for mth in mth_list)
    if len(runs) == 0:
        raise ValueError(f"No trained runs for model {model_id} year {model_year}")
    
    if get_train_pitchers is None:
        get_train_pitchers = lambda run: GetTrainPitchers(cursor, model_id, model_year, run)
    train_pitchers_by_run : dict[int, torch.Tensor] = {run: get_train_pitchers(run) for run in runs}
    return runs, train_pitchers_by_run

def _ReleaseMemory() -> None:
    torch.cuda.empty_cache()
    gc.collect()

def GetModelYearOutput(
        data_prep : DataPrep,
        runs : list[int],
        train_pitchers_by_run : dict[int, torch.Tensor],
        data_stuff : torch.Tensor,
        data_comb : torch.Tensor,
        pitcher_ids : torch.Tensor,
        get_network : Callable[[int, ModelVariantType, ModelOutputType], PitchModel],
        run_device : torch.device | str,
        model_description : str,
        progress : bool = False,
        after_network : Callable[[], None] | None = None) -> torch.Tensor:
    with torch.no_grad():
        # Running sum of outputs across model runs
        output_sum = None
        run_counts = torch.zeros(len(pitcher_ids), dtype=torch.int64)
        
        run_iterable = tqdm(runs, desc="Evaluating Model Copies", leave=False) if progress else runs
        for run in run_iterable:
            # Only evaluate pitches whose pitcher was NOT in this run's training split
            eligible = ~torch.isin(
                pitcher_ids,
                train_pitchers_by_run[run].to(pitcher_ids.dtype))
            eligible_idx = eligible.nonzero(as_tuple=True)[0]
            if eligible_idx.numel() == 0:
                continue
            eligible_idx_dev = eligible_idx.to(run_device, non_blocking=True)
            run_counts[eligible_idx] += 1
            
            output_list = []
            variant_iterable = tqdm(MODEL_VARIANTS, desc="Model Variants", leave=False) if progress else MODEL_VARIANTS
            for model_variant_type in variant_iterable:
                output_iterable = tqdm(MODEL_OUTPUTS, desc="Model Outputs", leave=False) if progress else MODEL_OUTPUTS
                for model_output_type in output_iterable:
                    network = get_network(run, model_variant_type, model_output_type)
                    
                    match model_variant_type:
                        case ModelVariantType.Stuff:
                            model_data = data_stuff[eligible_idx_dev]
                        case ModelVariantType.Combined:
                            model_data = data_comb[eligible_idx_dev]
                    
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
                    if after_network is not None:
                        after_network()
            
            # Add this run's outputs to the running sum
            run_output = torch.cat(output_list, dim=-1)
            if output_sum is None:
                output_sum = torch.zeros(len(pitcher_ids), run_output.shape[-1],
                                         dtype=run_output.dtype)
            output_sum[eligible_idx] += run_output
            del output_list
        
        # Average across the runs each pitch was actually eligible for
        if output_sum is None or (run_counts == 0).any():
            bad_pitchers = pitcher_ids[run_counts == 0].unique().tolist()
            raise RuntimeError(
                f"{model_description}: pitchers {bad_pitchers} were in the "
                f"training split of every run and cannot be evaluated.")
        return output_sum / run_counts.unsqueeze(-1).to(output_sum.dtype)

def Eval_Pitches(end_year : int):
    BATCH_SIZE = 2000000
    
    # Delete old data
    cursor = pitch_db.cursor()
    cursor.execute("DELETE FROM Output_PitchValueAggregation")
    pitch_db.commit()
    
    # Get Models
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    
    for model_id, model_name in tqdm(model_ids, desc="Evaluating Pitch Architectures"):
        model_years = [row[0] for row in cursor.execute(
            "SELECT DISTINCT Year FROM ModelTrainingHistory_PitchValue WHERE ModelId=? ORDER BY Year ASC",
            (model_id,)).fetchall()]
        
        for model_year in tqdm(model_years, desc="Model Years", leave=False):
            # DataPrep was saved during training as GetBinaryName(model_id, model_year)
            data_prep = GetDataPrep(model_id, model_year)
            
            # Pitchers with isTrain=0 (test split) or with no row at all stay eligible.
            runs, train_pitchers_by_run = GetModelYearRuns(cursor, model_id, model_year)
            
            def load_network(run, model_variant_type, model_output_type):
                return LoadNetwork(
                    data_prep,
                    GetModelFilePath(DEFAULT_MODELS_DIR, model_name, model_year, run, model_variant_type, model_output_type),
                    model_variant_type, model_output_type, device)
            
            year = model_year + 1
            if year > end_year:
                continue
            
            pitch_io_data = data_prep.GenerateIOPitches(
                start_year=year,
                end_year=year,
                validation_year=None,
                mlb_only=False,
            )
            eval_datasets = CreateTestTrainDatasets(pitch_io_data, eval_mode=True)
            dataset = eval_datasets.train
        
            n_samples = len(dataset)
            num_batches = (n_samples + BATCH_SIZE - 1) // BATCH_SIZE
            indices = torch.arange(n_samples)
            
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
                        
                    output_avg = GetModelYearOutput(
                        data_prep=data_prep,
                        runs=runs,
                        train_pitchers_by_run=train_pitchers_by_run,
                        data_stuff=data_stuff,
                        data_comb=data_comb,
                        pitcher_ids=mapping_pitcher_ids,
                        get_network=load_network,
                        run_device=device,
                        model_description=f"Model {model_id} year {model_year}",
                        progress=True,
                        after_network=_ReleaseMemory)
                                
                    id_rows = torch.stack((
                        mapping_game_ids,
                        mapping_pitch_nums,
                        mapping_level_ids,
                        mapping_pitcher_ids), dim=-1).tolist()
                    db_data = [tuple(ids) + tuple(outputs) for ids, outputs in zip(id_rows, output_avg.tolist())]
                            
                    cursor.executemany(f"INSERT INTO Output_PitchValueAggregation VALUES({model_id},?,?,{model_year},{year},?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,-1000,-1000,NULL,NULL,NULL)", db_data)
                        
                    # Clear Memory
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