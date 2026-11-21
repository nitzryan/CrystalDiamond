import os
from collections import OrderedDict
from typing import Callable, Hashable, TypeVar

_T = TypeVar('_T')
class PitchModelCache:
    def __init__(self, max_groups : int = 2):
        self._max_groups = max_groups
        self._groups : OrderedDict[Hashable, dict[tuple[str, Hashable], tuple[tuple[int, int], object]]] = OrderedDict()
    
    def Get(self, group : Hashable, path : str, load : Callable[[], _T], discriminator : Hashable = None) -> _T:
        stat = os.stat(path) # Raises FileNotFoundError for a missing file, which is what loading it would do
        signature = (stat.st_mtime_ns, stat.st_size)
        
        entries = self._groups.get(group)
        if entries is None:
            entries = {}
            self._groups[group] = entries
            while len(self._groups) > self._max_groups:
                self._groups.popitem(last=False)
        self._groups.move_to_end(group)
        
        key = (os.path.abspath(path), discriminator)
        entry = entries.get(key)
        if entry is not None and entry[0] == signature:
            return entry[1]
        
        value = load()
        entries[key] = (signature, value)
        return value
    
    def Groups(self) -> list[Hashable]:
        """Cached pairings, least recently used first."""
        return list(self._groups.keys())
    
    def Clear(self) -> None:
        self._groups.clear()
    
    def __len__(self) -> int:
        return sum(len(entries) for entries in self._groups.values())

class TrainingDataCache:
    def __init__(self, max_entries : int = 100):
        self._max_entries = max_entries
        self._entries : OrderedDict[Hashable, tuple[Hashable, object]] = OrderedDict()
    
    def Get(self, key : Hashable, signature : Hashable, load : Callable[[], _T]) -> _T:
        entry = self._entries.get(key)
        if entry is not None and entry[0] == signature:
            self._entries.move_to_end(key)
            return entry[1]
        
        value = load()
        self._entries[key] = (signature, value)
        self._entries.move_to_end(key)
        while len(self._entries) > self._max_entries:
            self._entries.popitem(last=False)
        return value
    
    def Clear(self) -> None:
        self._entries.clear()
    
    def __len__(self) -> int:
        return len(self._entries)

def GetDatabaseSignature(cursor) -> tuple:
    path = cursor.execute("PRAGMA database_list").fetchall()[0][2]
    if not path:
        return ()
    stat = os.stat(path)
    return (stat.st_mtime_ns, stat.st_size)

# Shared by every PitchModel.GetPitchOutput call in the process
pitch_model_cache = PitchModelCache(max_groups=2)
training_data_cache = TrainingDataCache(max_entries=100)