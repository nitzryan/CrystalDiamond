from DBTypes import *
from Stuff.DataPrep.PrepMap import Prep_Map
import torch
from sklearn.decomposition import PCA
from typing import TypeVar, Callable
from Constants import db, DTYPE
from Buckets import *
from Stuff.DataPrep.PitchDataset import PitchIO
from tqdm import tqdm
from Constants import profiler
        
_OVERVIEW_STRING = "overview"
_LOC_STRING = "loc"
_STUFF_STRING = "stuff"
_GAME_STRING = "game"
_AVG_STRING = "avg"
        
SHOULD_PROFILE = False
   
if SHOULD_PROFILE:
    profiler.enable()
        
_T = TypeVar('T')
class DataPrep:
    @profiler
    def __init__(self,
        prep_map : Prep_Map,
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
                        "ZoneTop", 
                        "ZoneBot"
                        ]
        self.conditional_statement = "WHERE "
        for v in vars_to_check:
            self.conditional_statement += f"{v} IS NOT NULL AND "
        
        self.conditional_statement = self.conditional_statement[:-4]
        
        pitches = DB_PitchStatcast.Select_From_DB(
            cursor=cursor,
            conditional=self.conditional_statement + "AND YEAR > 2016 AND Year<=? AND LevelId=1",
            values=(DataPrep.__CutoffYear,)
        )
        
        self.__Create_PCA_Norms(self.prep_map.pitch_overview_map, pitches, _OVERVIEW_STRING, self.prep_map.pitch_overview_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_loc_map, pitches, _LOC_STRING, self.prep_map.pitch_loc_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_stuff_map, pitches, _STUFF_STRING, self.prep_map.pitch_stuff_size)
       
        # Get average result for each bucket
        num_buckets = BUCKET_PITCHVALUE.size(0) + 1
        entries_in_bucket = torch.zeros(num_buckets, dtype=torch.long)
        value_in_bucket = torch.zeros_like(entries_in_bucket, dtype=torch.float64)
        buckets = torch.bucketize(torch.tensor([p.RunValueHitter for p in pitches]), BUCKET_PITCHVALUE)
        for i, pitch in enumerate(pitches):
            bucket = buckets[i].item()
            entries_in_bucket[bucket] += 1
            value_in_bucket[bucket] += pitch.RunValueHitter
        self.bucket_value = value_in_bucket / entries_in_bucket
       
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
       
    __CutoffYear = 2023
        
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
        
    def Transform_PitchStats(self, stats : list[DB_PitchStatcast]) -> torch.Tensor:
        overview_stats = torch.tensor([self.prep_map.pitch_overview_map(x) for x in stats], dtype=DTYPE)
        loc_stats = torch.tensor([self.prep_map.pitch_loc_map(x) for x in stats], dtype=DTYPE)
        stuff_stats = torch.tensor([self.prep_map.pitch_stuff_map(x) for x in stats], dtype=DTYPE)

        overview_pca = self.Get_PCA_Transform(overview_stats, _OVERVIEW_STRING)
        loc_pca = self.Get_PCA_Transform(loc_stats, _LOC_STRING)
        stuff_pca = self.Get_PCA_Transform(stuff_stats, _STUFF_STRING)
        
        return overview_pca, loc_pca, stuff_pca
    
    def Transform_PitchGameData(self, games_dict : dict[(int, int), DB_PitcherStatcastGame], pitch_data : list[DB_PitchStatcast]) -> torch.Tensor:
        pitch_gamedata = torch.tensor([self.prep_map.pitcher_game_map(games_dict[p.GameId, p.PitcherId]) for p in pitch_data], dtype=DTYPE)
        pitch_gamedata_pca = self.Get_PCA_Transform(pitch_gamedata, _GAME_STRING)
        return pitch_gamedata_pca
        
    def Transform_PitchAverage(self, avg : DB_PitchDateAverages) -> torch.Tensor:
        data_avg = torch.tensor(self.prep_map.league_baseline_map(avg)).unsqueeze(0)
        avg_pca = self.Get_PCA_Transform(data_avg, _AVG_STRING).squeeze()
        return avg_pca
    
    @profiler
    def GenerateIOPitches(self, start_year : int = 2017, end_year : int = 2023, end_month : int = 13, mlb_only : bool = True) -> list[list[PitchIO]]:
        cursor = db.cursor()
        pitcher_dict : dict[int, list[PitchIO]] = {}
        
        for year in tqdm(range(start_year, end_year + 1), desc="DataPrep Years", leave=False):
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
                
                if len(pitch_avg) == 0:
                    raise Exception(f"No data found for PitchDataAverages for {year}-{month}")
                pitch_avg = pitch_avg[0]
                data_pitch_averages = self.Transform_PitchAverage(pitch_avg)
                
                # Pitcher Games
                pitcher_games = DB_PitcherStatcastGame.Select_From_DB(
                    cursor=cursor,
                    conditional="WHERE Year=? AND Month=?",
                    values=(year, month)
                )
                
                pitcher_games_dict : dict[(int, int), DB_PitcherStatcastGame] = {}
                for pg in pitcher_games:
                    pitcher_games_dict[(pg.GameId, pg.MlbId)] = pg
                
                # Individual Pitches
                level_cond = "AND LevelId=1" if mlb_only else ""
                pitches = DB_PitchStatcast.Select_From_DB(
                    cursor=cursor,
                    conditional=self.conditional_statement + f"AND Year=? AND Month=? {level_cond}",
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
                    data_overview, data_loc, data_stuff = self.Transform_PitchStats(pitch_data)

                    # Iterate through pitches
                    io_list : list[PitchIO] = []
                    for i, pitch in enumerate(pitch_data):
                        io_list.append(PitchIO(
                            game_id=pitch.GameId,
                            pitch_num=pitch.PitchId,
                            data_overview=data_overview[i],
                            data_loc=data_loc[i],
                            data_stuff=data_stuff[i],
                            data_pitcher_game=data_pitcher_game[i],
                            data_league_avg=data_pitch_averages,
                            output_value=torch.bucketize(torch.tensor([pitch.RunValueHitter]), BUCKET_PITCHVALUE).item(),
                            output_runs=pitch.PaResultDirectRuns,
                            output_outs=min(pitch.PaResultOuts, 2),
                            output_swung=pitch.HadSwing,
                            output_contact=pitch.HadContact,
                            output_inplay=pitch.IsInPlay
                        ))
                    
                    if not id in pitcher_dict:
                        pitcher_dict[id] = io_list
                    else:
                        pitcher_dict[id] += io_list
        
        if SHOULD_PROFILE:
            profiler.disable()
            profiler.dump_stats("data_prep.lprof")
        
        return list(pitcher_dict.values())