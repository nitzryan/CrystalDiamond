import gc
from dataclasses import dataclass

from PitchModel.PitchTrackingDBTypes import DB_PitchData
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.DataPrep.PitchIO import PitchIOData
from PitchModel.Stuff.DataPrep.PrepMap import PrepMapConfig, CreatePrepMap

@dataclass
class _PreparedData:
    config : PrepMapConfig
    data_prep : DataPrep
    io_data : PitchIOData

class TuningData:
    def __init__(self, start_year : int, end_year : int):
        self.start_year = start_year
        self.end_year = end_year
        self.pitches : list[DB_PitchData] = DataPrep.Select_Pitches(start_year, end_year, mlb_only=True)
        if len(self.pitches) == 0:
            raise ValueError(f"No MLB pitches found for years {start_year}-{end_year}")
        
        self._current : _PreparedData | None = None
        
    def Get(self, config : PrepMapConfig) -> tuple[DataPrep, PitchIOData]:
        if self._current is None or self._current.config != config:
            # Free the old data before building the new data so both are never in memory together
            self.Clear()
            data_prep = DataPrep.From_Pitches(CreatePrepMap(config), self.pitches)
            io_data = data_prep.GenerateIOFromPitches(self.pitches)
            self._current = _PreparedData(config=config, data_prep=data_prep, io_data=io_data)
        
        return self._current.data_prep, self._current.io_data
    
    def Clear(self) -> None:
        if self._current is not None:
            gc.collect()
            self._current = None
        
