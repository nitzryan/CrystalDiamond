from Stuff.DataPrep.PrepMap import *

def GetModelMaps(model_id : int) -> Prep_Map:
    if model_id == 1:
        return standard_prep_map
    raise ValueError("Invalid model_id for GetModelMaps")