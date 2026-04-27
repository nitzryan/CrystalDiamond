import sys
from tqdm import tqdm
import torch
import gc

from Constants import device, pitch_db
from Shared import GetModelMaps
from Stuff.DataPrep.DataPrep import DataPrep
from Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets
from Stuff.Model.PitchModel import PitchModel
from Stuff.Model.ModelTrain import TrainAndGraph, _MODEL_OUTPUTS

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
        
    cursor = pitch_db.cursor()
    model_ids = cursor.execute("SELECT Id, Name FROM Models_PitchValue ORDER BY id ASC").fetchall()
    
    for model_id, model_name in tqdm(model_ids, desc="Training Architectures"):
        # Generatate IO data
        prep_map = GetModelMaps(model_id)
        data_prep = DataPrep(prep_map)
        pitch_io_list = data_prep.GenerateIOPitches(start_year=2017, end_year=2023, end_month=13)
        
        for i in tqdm(range(num_models), desc="Model Runs", leave=False):
            train_dataset, test_dataset = CreateTestTrainDatasets(pitch_io_list, test_size=0.25, random_state=i + 1)
            
            # Run Model
            model_name_pt = f"{model_name}_{i}"
            network = PitchModel(data_prep=data_prep).to(device)
            
            losses = TrainAndGraph(
                network=network,
                train_dataset=train_dataset,
                test_dataset=test_dataset,
                model_name=f'Models/{model_name_pt}',
                should_output=False
            )
            
            # Log Results
            len_outputs = len(_MODEL_OUTPUTS)
            cursor = pitch_db.cursor()
            cursor.execute("INSERT INTO ModelTrainingHistory_PitchValue VALUES(?,?,?,?,?,?)",
                (
                    model_id,
                    i,
                    losses[0],
                    losses[len_outputs],
                    losses[2 * len_outputs],
                    "TODO",
                )
            )
            pitch_db.commit()
            
            # Clear RAM/VRAM
            del network
            del train_dataset
            del test_dataset
            torch.cuda.empty_cache()
            gc.collect()