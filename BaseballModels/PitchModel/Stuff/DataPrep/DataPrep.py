from __future__ import annotations
from DBTypes import *
from Stuff.DataPrep.PrepMap import Prep_Map, map_pitch_stuff
import torch
from sklearn.decomposition import PCA
from typing import TypeVar, Callable
from Constants import db, DTYPE
from Buckets import *
from Stuff.DataPrep.PitchDataset import PitchIO
from tqdm import tqdm
from Constants import profiler
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
        save_name : str | None = None
    ):
        
        self.prep_map = prep_map
        
        cursor = db.cursor()
        # Get valid pitches
        # Make sure that all nullable values are not null
        vars_to_check = ["vStart", 
                        "BreakAngle", 
                        "BreakInduced", 
                        "BreakHorizontal", 
                        "Extension", 
                        "pX", 
                        "pZ",
                        "x0",
                        "z0",
                        "ZoneTop", 
                        "ZoneBot",
                        "SpinRate",
                        "SpinDirection",
                        "PlateTime",
                        ]
        self.conditional_statement = "WHERE "
        for v in vars_to_check:
            self.conditional_statement += f"{v} IS NOT NULL AND "

        self.prep_map.pitch_stuff_map = map_pitch_stuff
        self.conditional_statement = self.conditional_statement[:-4]
        
        pitches = DB_PitchStatcast.Select_From_DB(
            cursor=cursor,
            conditional=self.conditional_statement + "AND YEAR > 2016 AND Year<=? AND LevelId=1",
            values=(DataPrep.__CutoffYear,)
        )
        
        self.__Create_PCA_Norms(self.prep_map.pitch_overview_map, pitches, _OVERVIEW_STRING, self.prep_map.pitch_overview_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_loc_map, pitches, _LOC_STRING, self.prep_map.pitch_loc_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_stuff_map, pitches, _STUFF_STRING, self.prep_map.pitch_stuff_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_combined_map, pitches, _COMB_STRING, self.prep_map.pitch_combined_size)
       
        hitterZones = DB_HitterYearZoneData.Select_From_DB(
            cursor=cursor,
            conditional="WHERE Year < ?",
            values=(DataPrep.__CutoffYear,)
        )
        self.__Create_PCA_Norms(self.prep_map.hitter_zone_map, hitterZones, _HITZONE_STRING, self.prep_map.hitter_zone_size)
       
        # Get average result for each bucket
        num_buckets = BUCKET_INPLAY_VALUE.size(0) + 1
        entries_in_bucket = torch.zeros(num_buckets, dtype=torch.long)
        value_in_bucket = torch.zeros_like(entries_in_bucket, dtype=torch.float64)
        for pitch in pitches:
            bucket, isinplay = DataPrep.GetInPlayBucket(pitch)
            if isinplay == 1:
                entries_in_bucket[bucket] += 1
                value_in_bucket[bucket] += pitch.RunValueSmoothedHitter
        self.ip_bucket_value = value_in_bucket / entries_in_bucket
       
        # Get pitcher game averages
        pitcher_games = DB_PitcherStatcastGame.Select_From_DB(
            cursor=cursor,
            conditional="WHERE Year > 2016 AND Year<=? AND LevelId=1",
            values=(DataPrep.__CutoffYear,)
        )
        self.__Create_PCA_Norms(self.prep_map.pitcher_game_map, pitcher_games, _GAME_STRING, self.prep_map.pitcher_game_size)
       
        # Get Month/Year MLB averages
        date_avg = DB_PitchDateAverages.Select_From_DB(
            cursor=cursor,
            conditional="WHERE Year > 2016 AND Year<=?",
            values=(DataPrep.__CutoffYear,)
        )
        self.__Create_PCA_Norms(self.prep_map.league_baseline_map, date_avg, _AVG_STRING, self.prep_map.league_baseline_size)
       
        if save_name is not None:
           with open(save_name, 'wb') as file:
               dill.dump(self, file)
       
    @staticmethod
    def Load_From_File(filename : str) -> DataPrep:
        with open(filename, 'rb') as file:
            return dill.load(file)
       
    __CutoffYear = 2024
        
    def Get_ZScore(self, stats : torch.Tensor, name : str) -> torch.Tensor:
        means : torch.Tensor = getattr(self, f"__{name}_means")
        devs : torch.Tensor = getattr(self, f"__{name}_devs")
        return (stats - means) / devs
    
    def Get_PCA_Transform(self, stats: torch.Tensor, name : str) -> torch.Tensor:
        z_score = self.Get_ZScore(stats, name)
        pca = getattr(self, f"__{name}_pca")
        
        return torch.from_numpy(pca.transform(z_score)).float()
    
    def Get_PCA_Noise_Stuff(self, length : int) -> torch.Tensor:
        return self.__Get_PCA_Noise(self.prep_map.noise_stuff, length, _STUFF_STRING)
    
    def Get_PCA_Noise_Location(self, length : int) -> torch.Tensor:
        return self.__Get_PCA_Noise(self.prep_map.noise_location, length, _LOC_STRING)
    
    def __Get_PCA_Noise(self, dev_tensor : torch.Tensor, length : int, name : str) -> torch.Tensor:
        devs : torch.Tensor = getattr(self, f"__{name}_devs")
        pca = getattr(self, f"__{name}_pca")
        
        noise = torch.randn(size=(length, dev_tensor.shape[0]), dtype=DTYPE) * dev_tensor
        stat_devs = noise / devs
        return torch.from_numpy(pca.transform(stat_devs)).float()
    
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
        
    def Transform_PitchStats(self, stats : list[DB_PitchStatcast]) -> torch.Tensor:
        overview_stats = torch.tensor([self.prep_map.pitch_overview_map(x) for x in stats], dtype=DTYPE)
        loc_stats = torch.tensor([self.prep_map.pitch_loc_map(x) for x in stats], dtype=DTYPE)
        stuff_stats = torch.tensor([self.prep_map.pitch_stuff_map(x) for x in stats], dtype=DTYPE)
        combined_stats = torch.tensor([self.prep_map.pitch_combined_map(x) for x in stats], dtype=DTYPE)

        overview_pca = self.Get_PCA_Transform(overview_stats, _OVERVIEW_STRING)
        loc_pca = self.Get_PCA_Transform(loc_stats, _LOC_STRING)
        stuff_pca = self.Get_PCA_Transform(stuff_stats, _STUFF_STRING)
        combined_pca = self.Get_PCA_Transform(combined_stats, _COMB_STRING)
        
        return overview_pca, loc_pca, stuff_pca, combined_pca
    
    def Transform_PitchGameData(self, games_dict : dict[(int, int), DB_PitcherStatcastGame], pitch_data : list[DB_PitchStatcast]) -> torch.Tensor:
        pitch_gamedata = torch.tensor([self.prep_map.pitcher_game_map(games_dict[p.GameId, p.PitcherId]) for p in pitch_data], dtype=DTYPE)
        pitch_gamedata_pca = self.Get_PCA_Transform(pitch_gamedata, _GAME_STRING)
        return pitch_gamedata_pca
        
    def Transform_PitchAverage(self, avg : DB_PitchDateAverages) -> torch.Tensor:
        data_avg = torch.tensor(self.prep_map.league_baseline_map(avg)).unsqueeze(0)
        avg_pca = self.Get_PCA_Transform(data_avg, _AVG_STRING).squeeze()
        return avg_pca
    
    def Transform_HitterZones(self, zones : list[DB_HitterYearZoneData]) -> torch.Tensor:
        hitter_zone_data = torch.tensor([self.prep_map.hitter_zone_map(hz) for hz in zones], dtype=DTYPE)
        hz_pca = self.Get_PCA_Transform(hitter_zone_data, _HITZONE_STRING)
        return hz_pca
    
    # Map to <Called Strike, Ball, HBP, Swung>
    @staticmethod
    def GetOutputType(pitch : DB_PitchStatcast) -> int:
        if pitch.Result == 1:
            return 0
        if pitch.Result == 4:
            return 1
        if pitch.Result == 6:
            return 2
        return 3
    
    # Map balls hit in play to expected runs bucket
    @staticmethod
    def GetInPlayBucket(pitch : DB_PitchStatcast) -> tuple[int, int]:
        if pitch.Result == 5:
            return torch.bucketize(torch.tensor([pitch.RunValueSmoothedHitter]), BUCKET_INPLAY_VALUE), 1
        return 0, 0
    
    # Map swung balls to <Whiff, Foul, InPlay>
    @staticmethod
    def GetSwingBucket(pitch : DB_PitchStatcast) -> tuple[int, int]:
        if pitch.Result == 2:
            return 0, 1
        if pitch.Result == 3:
            return 1, 1
        if pitch.Result == 5:
            return 2, 1
        return 0, 0
    
    @profiler
    def GenerateIOPitches(self, start_year : int = 2017, end_year : int = 2024, end_month : int = 13, mlb_only : bool = True) -> list[list[PitchIO]]:
        cursor = db.cursor()
        pitcher_dict : dict[int, list[PitchIO]] = {}
        
        for year in tqdm(range(start_year, end_year + 1), desc="DataPrep Years", leave=False):
            hitter_zone_dict : dict[int, list[float]] = {}
            hitter_zones = DB_HitterYearZoneData.Select_From_DB(
                cursor=cursor,
                conditional="WHERE Year=?",
                values=(year,)
            )
            hitter_zones_mapped = self.Transform_HitterZones(hitter_zones)
            for i in range(len(hitter_zones)):
                hitter_zone_dict[hitter_zones[i].MlbId] = hitter_zones_mapped[i]
            
            for month in tqdm([4,5,6,7,8,9], desc="Months", leave=False):
                if year == end_year and month > end_month:
                    continue
                if year == 2020 and month < 7: # COVID
                    continue
                # MLB average for the month
                pitch_avg = DB_PitchDateAverages.Select_From_DB(
                    cursor=cursor,
                    conditional="WHERE Year=? AND Month=?",
                    values=(year,month)
                )
                
                if len(pitch_avg) == 0: # Some years won't have March/Oct games
                    continue

                pitch_avg = pitch_avg[0]
                data_pitch_averages = self.Transform_PitchAverage(pitch_avg)
                
                # Pitcher Games
                month_cond_string = "Month=?"
                if month == 4:
                    month_cond_string = "Month<=?"
                elif month == 9:
                    month_cond_string = "Month>=?"
                pitcher_games = DB_PitcherStatcastGame.Select_From_DB(
                    cursor=cursor,
                    conditional=f"WHERE Year=? AND {month_cond_string}",
                    values=(year, month)
                )
                
                pitcher_games_dict : dict[(int, int), DB_PitcherStatcastGame] = {}
                for pg in pitcher_games:
                    pitcher_games_dict[(pg.GameId, pg.MlbId)] = pg
                
                # Individual Pitches
                level_cond = "AND LevelId=1" if mlb_only else ""
                pitches = DB_PitchStatcast.Select_From_DB(
                    cursor=cursor,
                    conditional=self.conditional_statement + f"AND Year=? AND {month_cond_string} {level_cond}",
                    values=(year, month)
                )
                
                player_pitch_dict : dict[int, list[DB_PitchStatcast]] = {}
                for pitch in pitches:
                    if not pitch.PitcherId in player_pitch_dict:
                        player_pitch_dict[pitch.PitcherId] = [pitch]
                    else:
                        player_pitch_dict[pitch.PitcherId].append(pitch)
        
                # Iterate through players
                for id, pitch_data in player_pitch_dict.items():
                    data_pitcher_game = self.Transform_PitchGameData(pitcher_games_dict, pitch_data)
                    data_overview, data_loc, data_stuff, data_combined = self.Transform_PitchStats(pitch_data)

                    # Iterate through pitches
                    io_list : list[PitchIO] = []
                    for i, pitch in enumerate(pitch_data):
                        inplay_bucket, inplay_mask = DataPrep.GetInPlayBucket(pitch)
                        swing_bucket, swing_mask = DataPrep.GetSwingBucket(pitch)
                        
                        io_list.append(PitchIO(
                            pitcher_id=id,
                            game_id=pitch.GameId,
                            pitch_num=pitch.PitchId,
                            level_id=pitch.LevelId,
                            data_overview=data_overview[i],
                            data_loc=torch.cat((data_loc[i], hitter_zone_dict[pitch.HitterId])),
                            data_stuff=data_stuff[i],
                            data_combined=data_combined[i],
                            data_pitcher_game=data_pitcher_game[i],
                            data_league_avg=data_pitch_averages,
                            output_type=DataPrep.GetOutputType(pitch),
                            output_swing=swing_bucket,
                            output_inplay=inplay_bucket,
                            mask_swing=swing_mask,
                            mask_inplay = inplay_mask
                        ))
                    
                    if not id in pitcher_dict:
                        pitcher_dict[id] = io_list
                    else:
                        pitcher_dict[id] += io_list
        
                # Ensure that garbage collector removes old data before new data is obtained
                del pitch_avg
                del data_pitch_averages
                del pitcher_games
                del pitcher_games_dict
                del pitches
                del player_pitch_dict
                gc.collect()
        
        if SHOULD_PROFILE:
            profiler.disable()
            profiler.dump_stats("data_prep.lprof")
        
        return list(pitcher_dict.values())
    
    def DbPitchesToModelPitches(self, pitches : list[DB_PitchStatcast]) -> tuple[torch.Tensor, ...]:
        data_overview, data_loc, data_stuff, data_combined = self.Transform_PitchStats(pitches)
        year = pitches[0].Year
        month = pitches[0].Month
        if month < 4:
            month = 4
        elif month > 9:
            month = 9
        
        # Hitter zone for the year
        cursor = db.cursor()
        hitter_zone = DB_HitterYearZoneData.Select_From_DB(
            cursor=cursor,
            conditional="WHERE Year=? AND MlbId=?",
            values=(year, pitches[0].HitterId)
        )[0]
        hitter_zone_mapped = self.Transform_HitterZones([hitter_zone])
        data_loc = torch.cat((data_loc, hitter_zone_mapped.repeat(data_loc.shape[0], 1)), dim=-1)
        # MLB average for the month
        
        pitch_avg = DB_PitchDateAverages.Select_From_DB(
            cursor=cursor,
            conditional="WHERE Year=? AND Month=?",
            values=(year,month)
        )[0]
        data_pitch_averages = self.Transform_PitchAverage(pitch_avg)
        return data_overview,\
            data_loc,\
            data_stuff,\
            data_combined,\
            torch.zeros(1, 1),\
            data_pitch_averages.repeat(data_loc.shape[0], 1)