from PitchModel.Stuff.DataPrep.PrepMap import *
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Constants import DATA_PREP_BINARY_ALL_FILE

def GetModelMaps(model_id : int) -> Prep_Map:
    if model_id == 1:
        return standard_prep_map
    raise ValueError("Invalid model_id for GetModelMaps")

def GetDataPrep(model_id : int) -> DataPrep:
    if model_id == 1:
        return DataPrep.Load_From_File(DATA_PREP_BINARY_ALL_FILE)
    raise ValueError("Invalid model_id for GetDataPrep")