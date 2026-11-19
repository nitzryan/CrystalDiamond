import optuna
import gc
import torch
import torch.nn.functional as F

from PitchModel.Stuff.Model.PitchModel import PitchModelArgs, PitchModel
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.DataPrep.PitchIO import PitchIOData
from PitchModel.Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from PitchModel.Stuff.DataPrep.PitchDataset import CreateTestTrainDatasets, GetDisjointTrainIndices
from PitchModel.Stuff.Model.ModelTrain import TrainAndGraph
from PitchModel.Constants import device

_ACTIVATION_FUNCTIONS = ["ReLU", "LeakyReLU", "GELU", "SiLU", "Tanh"]
_ACTIVATION_MAP = {
        "ReLU": F.relu,
        "LeakyReLU": F.leaky_relu,
        "GELU": F.gelu,
        "SiLU": F.silu,
        "Tanh": F.tanh,
    }

def RunEvaluation(
            trial : optuna.trial.Trial,
            io_data : PitchIOData,
            data_prep : DataPrep,
            model_variant_type : ModelVariantType,
            model_output_type : ModelOutputType,
            max_repeats : int,) -> float:
    
    learning_rate = trial.suggest_float('learning_rate', 1e-5, 1e-2, log=True)
    block_size = trial.suggest_int('block_size', 16, 300)
    num_blocks = trial.suggest_int('num_blocks', 6, 16)
    dropout = trial.suggest_float('dropout', 0, 0.4)
    weight_decay = trial.suggest_float('weight_decay', 1e-9, 1e-1, log=True)
    activation = _ACTIVATION_MAP[trial.suggest_categorical('activation', _ACTIVATION_FUNCTIONS)]
    num_epochs = trial.suggest_int('num_epochs', 50, 300)
    batch_size = trial.suggest_int('batch_size', 10000, 25000)
    
    args = PitchModelArgs(
        model_variant_type=model_variant_type,
        model_output_type=model_output_type,
        learning_rate=learning_rate, 
        block_size=block_size,
        num_blocks=num_blocks,
        dropout=dropout,
        weight_decay=weight_decay,
        activation_function=activation,
        num_epochs=num_epochs,
        batch_size=batch_size,
    )
    
    sum_loss = 0
    # Repeats use folds whose test sets share no pitchers, so they are independent samples
    for train_idx in GetDisjointTrainIndices(max_repeats):
        datasets = CreateTestTrainDatasets(
            io_data=io_data,
            train_idx=train_idx
        )
        datasets.SetOutputType(model_output_type)
        
        network = PitchModel(args, data_prep).to(device)
        result = TrainAndGraph(
            network=network,
            datasets=datasets,
            model_name="tuning",
            should_output=False,
            show_progress=False,
            num_epochs=num_epochs,
            batch_size=batch_size,
            save_model=False
        )
        sum_loss += result.test_loss
        
        del datasets
        del network
        torch.cuda.empty_cache()
        gc.collect()
        
    return sum_loss