from tqdm import tqdm
import torch
import gc

from PitchModel.Constants import device, pitch_db
from PitchModel.Shared import GetModelPrepMap, GetModelYears, GetBinaryName
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelTrain import TrainAndGraph
from PitchModel.Stuff.Model.ModelOutputType import *
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep

def Train_Pitches(num_variants : int, start_year : int, end_year : int):
    if num_variants < 0:
        exit(1)
        
    cursor = pitch_db.cursor()
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    cursor.execute("DELETE FROM ModelTrainingHistory_PitchValue")
    cursor.execute("DELETE FROM PlayersInTrainingData")
    pitch_db.commit()
    
    for model_id, model_name in tqdm(model_ids, desc="Training Pitch Architectures"):
        prep_map = GetModelPrepMap(model_id)
        num_years = GetModelYears(model_id)
        
        # Do by year
        for year in tqdm(range(start_year + num_years - 1, end_year + 1), desc="Model Years", leave=False):
            data_year_start = year - num_years + 1
            validation_year = year + 1 if year < end_year else None
            
            data_prep = DataPrep(
                prep_map=prep_map,
                start_year=data_year_start,
                end_year=year,
                save_name=GetBinaryName(model_id, year)
            )
            pitch_io_data = data_prep.GenerateIOPitches(
                start_year=data_year_start,
                end_year=year,
                validation_year=validation_year,
                mlb_only=True
            )
            
            for i in tqdm(range(num_variants), desc="Model Runs", leave=False):
                datasets = CreateTestTrainDatasets(
                    pitch_io_data, 
                    train_idx=i)
                
                test_losses = []
                val_seen_losses = []
                val_unseen_losses = []
                
                for model_variant_type in tqdm(MODEL_VARIANTS, desc="Model Variants", leave=False):
                    for model_output_type in tqdm(MODEL_OUTPUTS, desc="Model Outputs", leave=False):
                        
                        # Run Model
                        args = DEFAULT_ARGS_MAP[(model_variant_type, model_output_type)]
                        model_name_pt = f"{model_name}_{year}_{i}"
                        network = PitchModel(args=args, data_prep=data_prep).to(device)
                        
                        datasets.SetOutputType(model_output_type)
                        
                        result = TrainAndGraph(
                            network=network,
                            datasets=datasets,
                            model_name=f'PitchModel/Models/{model_name_pt}',
                            should_output=False,
                        )
                        test_losses.append(result.test_loss)
                        
                        if result.val_seen_loss is not None:
                            val_seen_losses.append(result.val_seen_loss)
                        else:
                            val_seen_losses.append(-1000)
                        if result.val_unseen_loss is not None:
                            val_unseen_losses.append(result.val_unseen_loss)
                        else:
                            val_unseen_losses.append(-1000)
                
                # Log Results
                cursor = pitch_db.cursor()
                cursor.execute("INSERT INTO ModelTrainingHistory_PitchValue VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
                    (
                        model_id,
                        year,
                        i,
                        *test_losses,
                        *val_seen_losses,
                        *val_unseen_losses
                    )
                )
                cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{year},{i},1)", [(x,) for x in datasets.train.ids])
                cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{year},{i},0)", [(x,) for x in datasets.test.ids])
                pitch_db.commit()
                
                # Clear RAM/VRAM
                del network
                del datasets
                torch.cuda.empty_cache()
                gc.collect()
            
            del pitch_io_data
            del data_prep
            gc.collect()