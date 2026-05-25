from tqdm import tqdm
import torch
import gc

from Constants import device, pitch_db
from Shared import GetDataPrep
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import TrainAndGraph

def Train_Pitches(num_models : int):
    if num_models < 0:
        exit(1)
        
    cursor = pitch_db.cursor()
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    cursor.execute("DELETE FROM ModelTrainingHistory_PitchValue")
    pitch_db.commit()
    
    for model_id, model_name in tqdm(model_ids, desc="Training Pitch Architectures"):
        data_prep = GetDataPrep(model_id)
        pitch_io_list = data_prep.GenerateIOPitches()
        
        for i in tqdm(range(num_models), desc="Model Runs", leave=False):
            train_dataset, test_dataset = CreateTestTrainDatasets(
                pitch_io_list, 
                train_idx=i)
            
            # Run Model
            model_name_pt = f"{model_name}_{i}"
            network = PitchModel(data_prep=data_prep).to(device)
            
            losses = TrainAndGraph(
                network=network,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                model_name=f'Models/{model_name_pt}',
                should_output=False,
            )
            
            # Log Results
            cursor = pitch_db.cursor()
            cursor.execute("INSERT INTO ModelTrainingHistory_PitchValue VALUES(?,?,?,?,?,?,?,?,?,?,?,?)",
                (
                    model_id,
                    i,
                    losses[0],
                    losses[1],
                    losses[2],
                    losses[3],
                    losses[4],
                    losses[5],
                    losses[6],
                    losses[7],
                    losses[8],
                    "TODO",
                )
            )
            cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{i},1)", [(x,) for x in train_dataset.ids])
            cursor.executemany(f"INSERT INTO PlayersInTrainingData VALUES (?,{model_id},{i},0)", [(x,) for x in test_dataset.ids])
            pitch_db.commit()
            
            # Clear RAM/VRAM
            del network
            del train_dataset
            del test_dataset
            torch.cuda.empty_cache()
            gc.collect()