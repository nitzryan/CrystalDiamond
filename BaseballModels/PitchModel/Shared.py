from PitchModel.Stuff.DataPrep.PrepMap import *
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Constants import DATA_PREP_BINARY_ALL_FILE

def GetModelPrepMap(model_id : int) -> Prep_Map:
    if model_id == 1 or model_id == 2:
        return standard_prep_map
    raise ValueError("Invalid model_id for GetModelPrepMap")

def GetModelYears(model_id : int) -> int:
    if model_id == 1:
        return 1
    if model_id == 2:
        return 2
    raise ValueError("Invalid Model")

def GetBinaryName(model_id : int, year : int) -> str:
    return f"PitchModel/Binaries/model{model_id}_year{year}.pkl"

def GetDataPrep(model_id : int, model_year : int) -> DataPrep:
    return DataPrep.Load_From_File(GetBinaryName(model_id, model_year))