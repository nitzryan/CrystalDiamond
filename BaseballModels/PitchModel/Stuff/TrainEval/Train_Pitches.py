from tqdm import tqdm
import torch
import gc

from PitchModel.Constants import device, pitch_db
from PitchModel.Shared import GetDataPrep
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from PitchModel.Stuff.Model.PitchModel import PitchModel, DEFAULT_ARGS_MAP
from PitchModel.Stuff.Model.ModelTrain import TrainAndGraph
from PitchModel.Stuff.Model.ModelOutputType import *

def Train_Pitches(num_models : int):
    if num_models < 0:
        exit(1)
        
    cursor = pitch_db.cursor()
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    cursor.execute("DELETE FROM ModelTrainingHistory_PitchValue")
    cursor.execute("DELETE FROM PlayersInTrainingData")
    pitch_db.commit()
    
    for model_id, model_name in tqdm(model_ids, desc="Training Pitch Architectures"):
        data_prep = GetDataPrep(model_id)
        pitch_io_data = data_prep.GenerateIOPitches()
        
        for i in tqdm(range(num_models), desc="Model Runs", leave=False):
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
                    model_name_pt = f"{model_name}_{i}"
                    network = PitchModel(args=args, data_prep=data_prep).to(device)
                    
                    datasets.SetOutputType(model_output_type)
                    
                    result = TrainAndGraph(
                        network=network,
                        datasets=datasets,
                        model_name=f'PitchModel/Models/{model_name_pt}',
                        should_output=False,
                    )
                    test_losses.append(result.test_loss)
                    val_seen_losses.append(result.val_seen_loss)
                    val_unseen_losses.append(result.val_unseen_loss)
            
            # Log Results
            cursor = pitch_db.cursor()
            cursor.execute("INSERT INTO ModelTrainingHistory_PitchValue VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
                (
                    model_id,
                    i,
                    *test_losses,
                    *val_seen_losses,
                    *val_unseen_losses
                )
            )
            cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{i},1)", [(x,) for x in datasets.train.ids])
            cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{i},0)", [(x,) for x in datasets.test.ids])
            pitch_db.commit()
            
            # Clear RAM/VRAM
            del network
            del datasets
            torch.cuda.empty_cache()
            gc.collect()