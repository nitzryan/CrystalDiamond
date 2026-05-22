from Stuff.DataPrep.PrepMap import *
from Stuff.DataPrep.DataPrep import PitchType

def GetModelMaps(model_id : int) -> tuple[Prep_Map, list[PitchType]]:
    if model_id == 2:
        return standard_prep_map, [PitchType.All]
    elif model_id == 1:
        return standard_prep_map, [PitchType.Fastball, PitchType.Changeup, PitchType.Curveball]
    raise ValueError("Invalid model_id for GetModelMaps")