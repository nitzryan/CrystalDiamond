from typing import TypeVar 
import torch
from Constants import DTYPE

_T = TypeVar('T')
def Generate_Mean_Std(t: type[_T], items : list[_T]) -> tuple[_T,_T]:
    means = t(items[0].To_Tuple())
    devs = t(items[0].To_Tuple())
    for name, value in items[0].__dict__.items():
        if not name.startswith('_') and not callable(value):
            vals = torch.tensor(list(map(lambda x : getattr(x, name), items)), dtype=DTYPE).float()
            setattr(means, name, vals.mean())
            setattr(devs, name, vals.std())
    return means, devs

def Normalize(item: _T, means: _T, devs: _T) -> _T:
    for name, value in item.__dict__.items():
        if not name.startswith('_') and not callable(value):
            raw_value = getattr(item, name)
            norm_value = (raw_value - getattr(means, name)) / getattr(devs, name)
            setattr(item, name, norm_value)
    return item

import Output_Map
import Prep_Map
def GetModelMaps(model_id : int) -> tuple[Prep_Map.Prep_Map, Output_Map.Output_Map]:
    if model_id == 1:
        return Prep_Map.base_prep_map, Output_Map.base_output_map
    if model_id == 2:
        return Prep_Map.statsonly_prep_map, Output_Map.base_output_map
    if model_id == 3:
        return Prep_Map.meanrregression_prep_map, Output_Map.meanregression_output_map
    raise Exception(f"No mapping found for model_id={model_id}")