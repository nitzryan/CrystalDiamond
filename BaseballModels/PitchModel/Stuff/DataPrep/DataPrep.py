from __future__ import annotations
from PitchModel.PitchTrackingDBTypes import *
from PitchModel.Stuff.DataPrep.PrepMap import Prep_Map
import torch
from sklearn.decomposition import PCA
from typing import TypeVar, Callable
from PitchModel.Constants import tracking_db, DTYPE, BUCKET_INPLAY_VALUE
from PitchModel.Stuff.DataPrep.PitchDataset import PitchIO
from tqdm import tqdm
from PitchModel.Constants import profiler
import gc
import dill
        
_OVERVIEW_STRING = "overview"
_LOC_STRING = "loc"
_HITZONE_STRING = "hitzone"
_STUFF_STRING = "stuff"
_GAME_STRING = "game"
_AVG_STRING = "avg"
_COMB_STRING = "combined"
        
SHOULD_PROFILE = False
   
if SHOULD_PROFILE:
    profiler.enable()
        
_T = TypeVar('T')
class DataPrep:
    @profiler
    def __init__(self,
        prep_map : Prep_Map,
        start_year : int,
        end_year : int,
        save_name : str | None = None
    ):
        
        self.prep_map = prep_map
    
        cursor = tracking_db.cursor()
        pitches = DB_PitchData.Select_From_DB(
            cursor=cursor,
            conditional=f"WHERE YEAR>={start_year} AND Year<={end_year} AND LevelId=1",
            values=()
        )
        
        self.__Create_PCA_Norms(self.prep_map.pitch_stuff_map, pitches, _STUFF_STRING, self.prep_map.pitch_stuff_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_combined_map, pitches, _COMB_STRING, self.prep_map.pitch_combined_size)
       
        # Get average result for each bucket
        num_buckets = BUCKET_INPLAY_VALUE.size(0) + 1
        entries_in_bucket = torch.zeros(num_buckets, dtype=torch.long)
        value_in_bucket = torch.zeros_like(entries_in_bucket, dtype=torch.float64)
        for pitch in pitches:
            bucket, isinplay = DataPrep.GetInPlayBucket(pitch)
            if isinplay == 1:
                entries_in_bucket[bucket] += 1
                value_in_bucket[bucket] += pitch.RunValueInPlay
        self.ip_bucket_value = value_in_bucket / entries_in_bucket
       
        if save_name is not None:
           with open(save_name, 'wb') as file:
               dill.dump(self, file)
       
    @staticmethod
    def Load_From_File(filename : str) -> DataPrep:
        with open(filename, 'rb') as file:
            return dill.load(file)
        
    def GetStuffInputSize(self) -> int:
        return self.prep_map.pitch_stuff_size
    
    def GetCombinedInputSize(self) -> int:
        return self.prep_map.pitch_combined_size
        
    def Get_ZScore(self, stats : torch.Tensor, name : str) -> torch.Tensor:
        means : torch.Tensor = getattr(self, f"__{name}_means")
        devs : torch.Tensor = getattr(self, f"__{name}_devs")
        return (stats - means) / devs
    
    def Get_PCA_Transform(self, stats: torch.Tensor, name : str) -> torch.Tensor:
        z_score = self.Get_ZScore(stats, name)
        pca = getattr(self, f"__{name}_pca")
        
        return torch.from_numpy(pca.transform(z_score)).float()
    
    def __Create_PCA_Norms(self, map : Callable[[_T], list[float]], stats : list[_T], name : str, num_pca : int) -> None:
        # Get means, deviation of stats
        total = torch.tensor([map(h) for h in stats], dtype=DTYPE).float()
        means = torch.mean(total, dim=0, keepdim=False)
        devs = torch.std(total, dim=0, keepdim=False)
        setattr(self, "__" + name + "_means", means)
        setattr(self, "__" + name + "_devs", devs)
        
        # Normalize, use to fit PCA
        normalized = (total - means) / devs
        pca = PCA(num_pca)
        pca.fit(normalized)
        #print([round(x, 3) for x in pca.explained_variance_ratio_])
        setattr(self, "__" + name + "_pca", pca)
        
    def Transform_PitchStats(self, stats : list[DB_PitchData]) -> tuple[torch.Tensor, torch.Tensor]:
        stuff_stats = torch.tensor([self.prep_map.pitch_stuff_map(x) for x in stats], dtype=DTYPE)
        combined_stats = torch.tensor([self.prep_map.pitch_combined_map(x) for x in stats], dtype=DTYPE)

        stuff_pca = self.Get_PCA_Transform(stuff_stats, _STUFF_STRING)
        combined_pca = self.Get_PCA_Transform(combined_stats, _COMB_STRING)
        
        return stuff_pca, combined_pca
    
    # Map to <Called Strike, Ball, HBP, Swung>
    @staticmethod
    def GetOutputType(pitch : DB_PitchData) -> int:
        if pitch.Result == 1:
            return 0
        if pitch.Result == 4:
            return 1
        if pitch.Result == 6:
            return 2
        return 3
    
    # Map balls hit in play to expected runs bucket
    @staticmethod
    def GetInPlayBucket(pitch : DB_PitchData) -> tuple[int, int]:
        if pitch.Result == 5:
            return torch.bucketize(torch.tensor([pitch.RunValueInPlay]), BUCKET_INPLAY_VALUE), 1
        return 0, 0
    
    # Map swung balls to <Whiff, Foul, InPlay>
    @staticmethod
    def GetSwingBucket(pitch : DB_PitchData) -> tuple[int, int]:
        if pitch.Result == 2:
            return 0, 1
        if pitch.Result == 3:
            return 1, 1
        if pitch.Result == 5:
            return 2, 1
        return 0, 0
    
    @profiler
    def GenerateIOPitches(self, start_year : int = 2017, end_year : int = 2024, mlb_only : bool = True) -> list[list[PitchIO]]:
        cursor = tracking_db.cursor()
        pitcher_dict : dict[int, list[PitchIO]] = {}
        
        level_cond = "AND LevelId=1" if mlb_only else ""
        pitches = DB_PitchData.Select_From_DB(
            cursor=cursor,
            conditional=f"WHERE Year>=? AND Year<=? {level_cond}",
            values=(start_year, end_year)
        )
        
        data_stuff, data_combined = self.Transform_PitchStats(pitches)
        for i in range(len(pitches)):
            pitch = pitches[i]
            inplay_bucket, inplay_mask = DataPrep.GetInPlayBucket(pitch)
            swing_bucket, swing_mask = DataPrep.GetSwingBucket(pitch)
            io = PitchIO(
                pitcher_id=pitch.PitcherId,
                game_id=pitch.GameId,
                pitch_num=pitch.PitchId,
                level_id=pitch.LevelId,
                data_stuff=data_stuff[i],
                data_combined=data_combined[i],
                output_type=DataPrep.GetOutputType(pitch),
                output_swing=swing_bucket,
                output_inplay=inplay_bucket,
                mask_swing=swing_mask,
                mask_inplay = inplay_mask
            )
            
            if not pitch.PitcherId in pitcher_dict:
                pitcher_dict[pitch.PitcherId] = [io]
            else:
                pitcher_dict[pitch.PitcherId].append(io)
            
        
        if SHOULD_PROFILE:
            profiler.disable()
            profiler.dump_stats("data_prep.lprof")
        
        return list(pitcher_dict.values())
    
    # TODO This needs to be rewritten
    # def DbPitchesToModelPitches(self, pitches : list[DB_PitchStatcast]) -> tuple[torch.Tensor, ...]:
    #     data_overview, data_loc, data_stuff, data_combined = self.Transform_PitchStats(pitches)
    #     year = pitches[0].Year
    #     month = pitches[0].Month
    #     if month < 4:
    #         month = 4
    #     elif month > 9:
    #         month = 9
        
    #     # Hitter zone for the year
    #     cursor = db.cursor()
    #     hitter_zone = DB_HitterYearZoneData.Select_From_DB(
    #         cursor=cursor,
    #         conditional="WHERE Year=? AND MlbId=?",
    #         values=(year, pitches[0].HitterId)
    #     )[0]
    #     hitter_zone_mapped = self.Transform_HitterZones([hitter_zone])
    #     data_loc = torch.cat((data_loc, hitter_zone_mapped.repeat(data_loc.shape[0], 1)), dim=-1)
    #     # MLB average for the month
        
    #     pitch_avg = DB_PitchDateAverages.Select_From_DB(
    #         cursor=cursor,
    #         conditional="WHERE Year=? AND Month=?",
    #         values=(year,month)
    #     )[0]
    #     data_pitch_averages = self.Transform_PitchAverage(pitch_avg)
    #     return data_overview,\
    #         data_loc,\
    #         data_stuff,\
    #         data_combined,\
    #         torch.zeros(1, 1),\
    #         data_pitch_averages.repeat(data_loc.shape[0], 1)