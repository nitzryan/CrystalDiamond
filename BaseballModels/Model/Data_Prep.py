from sklearn.decomposition import PCA # type: ignore
from typing import TypeVar, Optional, Callable
from DBTypes import *
from Constants import db, DTYPE
from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, HITTER_PEAK_WAR_BUCKETS
from Constants import PITCHER_LEVEL_BUCKETS, PITCHER_BF_BUCKETS, PITCHER_PEAK_WAR_BUCKETS
import math
import torch
from tqdm import tqdm
import random
from Prep_Map import Prep_Map
from Output_Map import Output_Map
from Output_StatAggregation import Aggregate_HitterStats, Aggregate_PitcherStats
import DB_AdvancedStatements

class Player_IO:
    def __init__(self, player : DB_Model_Players, 
                 input : torch.Tensor, 
                 output : torch.Tensor,
                 length : int,
                 dates : torch.Tensor, 
                 prospect_mask : torch.Tensor,
                 stat_level_mask : torch.Tensor,
                 year_level_mask : torch.Tensor,
                 year_stat_output : torch.Tensor,
                 year_pos_output : torch.Tensor,
                 mlb_value_mask : torch.Tensor,
                 mlb_value_stats : torch.Tensor):
        
        self.player = player
        self.input = input
        self.output = output
        self.length = length
        self.dates = dates
        self.prospect_mask = prospect_mask
        self.stat_level_mask = stat_level_mask
        self.year_level_mask = year_level_mask
        self.year_stat_output = year_stat_output
        self.year_pos_output = year_pos_output
        self.mlb_value_mask = mlb_value_mask
        self.mlb_value_stats = mlb_value_stats
        
    @staticmethod
    def GetMaxLength(io_list : list['Player_IO']) -> int:
        max_length = 0
        for io in io_list:
            max_length = max(max_length, io.length)
        return max_length
        

_T = TypeVar('T')
class Data_Prep:
    def __init__(self, prep_map : 'Prep_Map', output_map : 'Output_Map'):
        # Mutators
        self.off_mutator_scale = 0.25
        self.bsr_mutator_scale = 0.25
        self.def_mutator_scale = 0.25
        
        self.hitbio_mutator_scale = 0.25
        self.pitbio_mutator_scale = 0.25
        
        self.hitpt_mutator_scale = 0.25
        self.pitpt_mutator_scale = 0.25
        
        self.hitlvl_mutator_scale = 0.25
        self.pitlvl_mutator_scale = 0.25
        
        self.pit_mutator_scale = 0.25
        
        self.mlb_hit_value_mutator_scale = 0.25
        self.mlb_pit_value_mutator_scale = 0.25
        
        self.prep_map = prep_map
        self.output_map = output_map
        
        cursor = db.cursor()
        # Bios
        hitters = DB_Model_Players.Select_From_DB(cursor, "WHERE isHitter=1 AND signingYear<=?", (Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_bio, hitters, "hitbio", self.prep_map.bio_size)
        
        pitchers = DB_Model_Players.Select_From_DB(cursor, "WHERE isPitcher=1 AND signingYear<=?", (Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_bio, pitchers, "pitbio", self.prep_map.bio_size)
        
        # Get all stats <=2024 to get means/stds to normalize
        # Restrict so new data doesn't change old conversions which would break model
        hitter_stats = DB_Model_HitterStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        pitcher_stats = DB_Model_PitcherStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        
        # Normalize output stats
        self.__Create_Standard_Norms(self.output_map.map_hitter_output, hitter_stats, "hitoutput")
        self.__Create_Standard_Norms(self.output_map.map_pitcher_output, pitcher_stats, "pitoutput")
        
        # Age and level information, keep stats individual
        self.__Create_PCA_Norms(self.prep_map.map_hitterlvl, hitter_stats, "hitlevel", self.prep_map.hitterlvl_size)
        self.__Create_PCA_Norms(self.prep_map.map_pitcherlvl, pitcher_stats, "pitlevel", self.prep_map.pitcherlvl_size)
        
        # Stats for playing time
        self.__Create_PCA_Norms(self.prep_map.map_hitterpt, hitter_stats, "hitpt", self.prep_map.hitterpt_size)
        self.__Create_PCA_Norms(self.prep_map.map_pitcherpt, pitcher_stats, "pitpt", self.prep_map.pitcherpt_size)
        
        # Stats for evaluating performance
        self.__Create_PCA_Norms(self.prep_map.map_off, hitter_stats, "off", self.prep_map.off_size)
        self.__Create_PCA_Norms(self.prep_map.map_bsr, hitter_stats, "bsr", self.prep_map.bsr_size)
        self.__Create_PCA_Norms(self.prep_map.map_def, hitter_stats, "def", self.prep_map.def_size)
        
        self.__Create_PCA_Norms(self.prep_map.map_pit, pitcher_stats, "pit", self.prep_map.pit_size)
    
        # Get Player value
        hitter_values = DB_Model_HitterValue.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year - 2,)) # Need 3 years of data
        pitcher_values = DB_Model_PitcherValue.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year - 2,))
        self.__Create_Standard_Norms(self.output_map.map_mlb_hitter_values, hitter_values, "hittervalues")
        self.__Create_Standard_Norms(self.output_map.map_mlb_pitcher_values, pitcher_values, "pitchervalues")
        
        #Montly mlb input values
        hitter_month_values = DB_Player_YearlyWar.Select_From_DB(cursor, "WHERE Year<=? AND isHitter=1", (Data_Prep.__Cutoff_Year,))
        pitcher_month_values = DB_Player_YearlyWar.Select_From_DB(cursor, "WHERE Year<=? AND isHitter=0", (Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_mlb_hit_value, hitter_month_values, "hittermonthvalues", self.prep_map.mlb_hit_value_size) # Needed for input
        self.__Create_PCA_Norms(self.prep_map.map_mlb_pit_value, pitcher_month_values, "pitchermonthvalues", self.prep_map.mlb_pit_value_size)
        # self.__Create_Standard_Norms(self.prep_map.map_mlb_hit_value, hitter_month_values, "hittermonthvalues") # Needed for output
        # self.__Create_Standard_Norms(self.prep_map.map_mlb_pit_value, pitcher_month_values, "pitchermonthvalues")
    
    __Cutoff_Year = 2024
    
    def Transform_HitterStats(self, stats : list[DB_Model_HitterStats]) -> torch.Tensor:
        level_stats = torch.tensor([self.prep_map.map_hitterlvl(x) for x in stats], dtype=DTYPE)
        pt_stats = torch.tensor([self.prep_map.map_hitterpt(x) for x in stats], dtype=DTYPE)
        off_stats = torch.tensor([self.prep_map.map_off(x) for x in stats], dtype=DTYPE)
        bsr_stats = torch.tensor([self.prep_map.map_bsr(x) for x in stats], dtype=DTYPE)
        def_stats = torch.tensor([self.prep_map.map_def(x) for x in stats], dtype=DTYPE)

        level_pca = torch.from_numpy(getattr(self, "__hitlevel_pca").transform(level_stats))
        pt_pca = torch.from_numpy(getattr(self, "__hitpt_pca").transform(pt_stats))
        off_pca = torch.from_numpy(getattr(self, "__off_pca").transform(off_stats))
        bsr_pca = torch.from_numpy(getattr(self, "__bsr_pca").transform(bsr_stats))
        def_pca = torch.from_numpy(getattr(self, "__def_pca").transform(def_stats))
        
        return torch.cat((level_pca, pt_pca, off_pca, bsr_pca, def_pca), dim=1)
    
    def Transform_PitcherStats(self, stats : list[DB_Model_PitcherStats]) -> torch.Tensor:
        level_stats = torch.tensor([self.prep_map.map_pitcherlvl(x) for x in stats], dtype=DTYPE)
        pt_stats = torch.tensor([self.prep_map.map_pitcherpt(x) for x in stats], dtype=DTYPE)
        pit_stats = torch.tensor([self.prep_map.map_pit(x) for x in stats], dtype=DTYPE)
        
        level_pca = torch.from_numpy(getattr(self, "__pitlevel_pca").transform(level_stats))
        pt_pca = torch.from_numpy(getattr(self, "__pitpt_pca").transform(pt_stats))
        pit_pca = torch.from_numpy(getattr(self, "__pit_pca").transform(pit_stats))
        
        return torch.cat((level_pca, pt_pca, pit_pca), dim=1)
    
    def Transform_HitterMlbValues(self, values : list[DB_Player_MonthlyWar]) -> torch.Tensor:
        hitter_values = torch.tensor([self.prep_map.map_mlb_hit_value(v) for v in values], dtype=DTYPE)
        values_pca = torch.from_numpy(getattr(self, "__hittermonthvalues_pca").transform(hitter_values))
        return values_pca
    
    def Transform_PitcherMlbValues(self, values : list[DB_Player_MonthlyWar]) -> torch.Tensor:
        pitcher_values = torch.tensor([self.prep_map.map_mlb_pit_value(v) for v in values], dtype=DTYPE)
        values_pca = torch.from_numpy(getattr(self, "__pitchermonthvalues_pca").transform(pitcher_values))
        return values_pca
    
    def Transform_HitterOutputStats(self, stats : list[DB_Model_HitterStats]) -> torch.Tensor:
        output_stats = torch.tensor([self.output_map.map_hitter_output(x) for x in stats], dtype=DTYPE)
        hit_stats_means : torch.Tensor = getattr(self, "__hitoutput_means")
        hit_stats_devs : torch.Tensor = getattr(self, "__hitoutput_devs")
        return (output_stats - hit_stats_means) / hit_stats_devs
    
    def Transform_PitcherOutputStats(self, stats : list[DB_Model_PitcherStats]) -> torch.Tensor:
        output_stats = torch.tensor([self.output_map.map_pitcher_output(x) for x in stats], dtype=DTYPE)
        pit_stats_means : torch.Tensor = getattr(self, "__pitoutput_means")
        pit_stats_devs : torch.Tensor = getattr(self, "__pitoutput_devs")
        return (output_stats - pit_stats_means) / pit_stats_devs
    
    def Get_Hitter_Size(self) -> int:
        return self.prep_map.bio_size + self.prep_map.hitterlvl_size + self.prep_map.off_size + self.prep_map.bsr_size + self.prep_map.def_size + self.prep_map.hitterpt_size + self.prep_map.mlb_hit_value_size + 1
    
    def Get_Pitcher_Size(self) -> int:
        return self.prep_map.bio_size + self.prep_map.pitcherlvl_size + self.prep_map.pit_size + self.prep_map.pitcherpt_size + self.prep_map.mlb_pit_value_size + 1
    
    def __Transform_HitterData(self, hitter : DB_Model_Players) -> torch.Tensor:
        bio_stats = torch.tensor([self.prep_map.map_bio(hitter)], dtype=DTYPE)
        bio_pca = torch.from_numpy(getattr(self, "__hitbio_pca").transform(bio_stats))
        return bio_pca
    
    def __Transform_PitcherData(self, pitcher : DB_Model_Players) -> torch.Tensor:
        bio_stats = torch.tensor([self.prep_map.map_bio(pitcher)], dtype=DTYPE)
        bio_pca = torch.from_numpy(getattr(self, "__pitbio_pca").transform(bio_stats))
        return bio_pca
    
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
        
    def __Create_Standard_Norms(self, map : Callable[[_T], list[float]], stats : list[_T], name : str) -> None:
        total = torch.tensor([map(h) for h in stats], dtype=DTYPE).float()
        means = torch.mean(total, dim=0, keepdim=False)
        devs = torch.std(total, dim=0, keepdim=False)
        setattr(self, "__" + name + "_means", means)
        setattr(self, "__" + name + "_devs", devs)
        
    def Generate_IO_Hitters(self, player_condition : str, player_values : tuple[any], use_cutoff : bool) -> list[Player_IO]:
        # Get Hitters
        cursor = db.cursor()
        hitters = DB_Model_Players.Select_From_DB(cursor, player_condition, player_values)
        
        io : list[Player_IO] = []
        cutoff_year = Data_Prep.__Cutoff_Year if use_cutoff else 1000000
        hit_stats_means : torch.Tensor = getattr(self, "__hitoutput_means")
        hit_stats_devs : torch.Tensor = getattr(self, "__hitoutput_devs")
        
        mlb_value_means : torch.Tensor = getattr(self, "__hittervalues_means")
        mlb_value_devs : torch.Tensor = getattr(self, "__hittervalues_devs")
        for hitter in tqdm(hitters, desc="Generating Hitters", leave=False):
            # Get Stats
            stats_monthlywar = DB_AdvancedStatements.Select_LeftJoin(DB_Model_HitterStats, DB_Player_MonthlyWar, cursor,
                                                                     "SELECT * FROM Model_HitterStats AS mhs LEFT JOIN Player_MonthlyWar AS pmw ON mhs.mlbId=pmw.mlbId AND mhs.month=pmw.month AND mhs.year=pmw.year WHERE mhs.mlbId=? AND mhs.Year<=?",
                                                                     (hitter.mlbId, cutoff_year))
            l = len(stats_monthlywar) + 1
            stats = [mhs for mhs, pmw in stats_monthlywar]
                
            mlb_values = DB_Model_HitterValue.Select_From_DB(cursor, '''
                WHERE mlbId=:mlbId AND
                (
                    Year<=:year
                )
                ORDER BY Year ASC, MONTH ASC''',
                {'mlbId':hitter.mlbId,'year':cutoff_year})
                
            dates = torch.cat(
                    (torch.tensor([(hitter.mlbId, 0, 0)], dtype=torch.long),
                    torch.tensor([(x.mlbId, x.Year, x.Month) for x, _ in stats_monthlywar], dtype=torch.long)))
            
            # Input
            input = torch.zeros(l, self.Get_Hitter_Size())
            input[0,0] = 1 # Initialization step, no stats here
            input[:,1:self.prep_map.bio_size + 1] = self.__Transform_HitterData(hitter) # Hitter Bio
            if l > 1:
                input[1:, self.prep_map.bio_size + 1:-self.prep_map.mlb_hit_value_size] = self.Transform_HitterStats(stats) # Month Stats
                input[1:, -self.prep_map.mlb_hit_value_size:] = self.Transform_HitterMlbValues([pmw for mhs, pmw in stats_monthlywar])
            
            # Output
            output = torch.zeros(l, 5, dtype=torch.long)
            output[:,0] = torch.bucketize(torch.tensor(self.output_map.map_hitter_war(hitter)), self.output_map.buckets_hitter_war)
            output[:,1] = torch.bucketize(torch.tensor(self.output_map.map_hitter_value(hitter)), self.output_map.buckets_hitter_value)
            output[:,2] = torch.bucketize(torch.tensor(hitter.peakWarHitter), HITTER_PEAK_WAR_BUCKETS)
            output[:,3] = torch.bucketize(torch.tensor(hitter.highestLevelHitter), HITTER_LEVEL_BUCKETS)
            output[:,4] = torch.bucketize(torch.tensor(hitter.totalPA), HITTER_PA_BUCKETS)
        
            # Masks
            prospect_mask = torch.zeros(l, dtype=torch.float)
            # Determine if player should be ignored.  If yes, then initial mask should be 0, otherwise 1
            prospect_mask[0] = 1 if cursor.execute("SELECT ignorePlayer FROM Player_CareerStatus WHERE mlbId=?", (hitter.mlbId,)).fetchone()[0] == None else 0
            for i, stat in enumerate(stats):
                prospect_mask[i + 1] = Output_Map.GetProspectMask(stat)
            
            lvl_mask = torch.zeros(l, len(HITTER_LEVEL_BUCKETS), dtype=torch.float)
            for i, stat in enumerate(stats):
                lvl_mask[i,:] = torch.tensor(Output_Map.GetOutputMasks(stat))
            
            # 1 Year aggregation
            stat_year_output = torch.zeros(l, self.output_map.hitter_stats_size, dtype=torch.float)
            pos_year_output = torch.zeros(l, self.output_map.hitter_positions_size, dtype=torch.float)
            lvl_year_mask = torch.zeros(l, len(HITTER_LEVEL_BUCKETS), dtype=torch.float)
            if len(stats) > 1:
                start_month = stats[1].Month
                start_year = stats[1].Year
                for i, stat in enumerate(stats):
                    if i == 0:
                        (_l,_s,_p) = Aggregate_HitterStats(startMonth=start_month - 1, endMonth=start_month - 1, startYear=start_year, endYear=start_year + 1, output_map=self.output_map, stats=stats)
                    else:
                        (_l,_s,_p) = Aggregate_HitterStats(startMonth=stat.Month, endMonth=stat.Month, startYear=stat.Year, endYear=stat.Year + 1, output_map=self.output_map, stats=stats)
                    lvl_year_mask[i,:] = _l
                    stat_year_output[i,:] = (_s - hit_stats_means) / hit_stats_devs
                    pos_year_output[i,:] = _p
            
            # MLB Value stats and mask
            mlb_value_mask = torch.zeros(l, 3, 2, dtype=torch.float)
            mlb_value_stats = torch.zeros(l, self.output_map.mlb_hitter_values_size, dtype=DTYPE)
            for i, value in enumerate(mlb_values):
                mlb_value_stats[i+1] = (torch.tensor(self.output_map.map_mlb_hitter_values(value)) - mlb_value_means) / mlb_value_devs
                current_value_year = stats[i].Year
                # Mask on whether the PA count should be counted
                mlb_value_mask[i+1,0,0] = current_value_year < cutoff_year
                mlb_value_mask[i+1,1,0] = current_value_year < (cutoff_year - 1)
                mlb_value_mask[i+1,2,0] = current_value_year < (cutoff_year - 2)
                # Mask on whether the rate stats should be counted
                # Scale so don't take too much from small samples, but cap to prevent bias (players who perform poorly get cut/sent down, so uncapped scale would overestimate marginal players)
                mlb_value_mask[i+1,0,1] = min(value.Pa1Year / 100, 1)
                mlb_value_mask[i+1,1,1] = min(value.Pa2Year / 100, 1)
                mlb_value_mask[i+1,2,1] = min(value.Pa3Year / 100, 1)
            if len(mlb_values) > 0:
                mlb_value_mask[0] = mlb_value_mask[1]
                mlb_value_stats[0] = mlb_value_stats[1]
            
            io.append(Player_IO(player=hitter, input=input, output=output, length=l, dates=dates, prospect_mask=prospect_mask, stat_level_mask=lvl_mask, year_level_mask=lvl_year_mask, year_stat_output=stat_year_output, year_pos_output=pos_year_output, mlb_value_mask=mlb_value_mask, mlb_value_stats=mlb_value_stats))
        
        return io
       
    def Generate_IO_Pitchers(self, player_condition : str, player_values : tuple[any], use_cutoff : bool) -> list[Player_IO]:
        # Get Pitchers
        cursor = db.cursor()
        pitchers = DB_Model_Players.Select_From_DB(cursor, player_condition, player_values)
        
        io : list[Player_IO] = []
        pit_stats_means : torch.Tensor = getattr(self, "__pitoutput_means")
        pit_stats_devs : torch.Tensor = getattr(self, "__pitoutput_devs")
        
        cutoff_year = Data_Prep.__Cutoff_Year if use_cutoff else 1000000
        mlb_value_means : torch.Tensor = getattr(self, "__pitchervalues_means")
        mlb_value_devs : torch.Tensor = getattr(self, "__pitchervalues_devs")
        for pitcher in tqdm(pitchers, desc="Generating Pitchers", leave=False):
            # Get Stats
            stats_monthlywar = DB_AdvancedStatements.Select_LeftJoin(DB_Model_PitcherStats, DB_Player_MonthlyWar, cursor,
                                                                     "SELECT * FROM Model_PitcherStats AS mhs LEFT JOIN Player_MonthlyWar AS pmw ON mhs.mlbId=pmw.mlbId AND mhs.month=pmw.month AND mhs.year=pmw.year WHERE mhs.mlbId=? AND mhs.Year<=? ORDER BY mhs.Year ASC, mhs.Month ASC",
                                                                     (pitcher.mlbId, cutoff_year))
            l = len(stats_monthlywar) + 1
            stats = [mhs for mhs, pmw in stats_monthlywar]
                
            dates = torch.cat(
                    (torch.tensor([(pitcher.mlbId, 0, 0)], dtype=torch.long),
                    torch.tensor([(x.mlbId, x.Year, x.Month) for x in stats], dtype=torch.long)))
            
            mlb_values = DB_Model_PitcherValue.Select_From_DB(cursor, '''
                WHERE mlbId=:mlbId AND
                (
                    Year<=:year
                )
                ORDER BY Year ASC, MONTH ASC''',
                {'mlbId':pitcher.mlbId,'year':cutoff_year})
            
            # Input
            input = torch.zeros(l, self.Get_Pitcher_Size())
            input[0,0] = 1 # Initialization step, no stats here
            input[:,1:self.prep_map.bio_size + 1] = self.__Transform_PitcherData(pitcher) # Pitcher Bio
            if l > 1:
                input[1:, self.prep_map.bio_size + 1:-self.prep_map.mlb_pit_value_size] = self.Transform_PitcherStats(stats) # Month Stats
                input[1:, -self.prep_map.mlb_pit_value_size:] = self.Transform_PitcherMlbValues([pmw for mhs, pmw in stats_monthlywar])
            
            # Output
            output = torch.zeros(l, 5, dtype=torch.long)
            output[:,0] = torch.bucketize(torch.tensor(self.output_map.map_pitcher_war(pitcher)), self.output_map.buckets_pitcher_war)
            output[:,1] = torch.bucketize(torch.tensor(self.output_map.map_pitcher_value(pitcher)), self.output_map.buckets_pitcher_value)
            output[:,2] = torch.bucketize(torch.tensor(pitcher.peakWarPitcher), PITCHER_PEAK_WAR_BUCKETS)
            output[:,3] = torch.bucketize(torch.tensor(pitcher.highestLevelPitcher), PITCHER_LEVEL_BUCKETS)
            output[:,4] = torch.bucketize(torch.tensor(pitcher.totalOuts), PITCHER_BF_BUCKETS)
        
            # Masks
            prospect_mask = torch.zeros(l, dtype=torch.float)
            prospect_mask[0] = 1 if cursor.execute("SELECT ignorePlayer FROM Player_CareerStatus WHERE mlbId=?", (pitcher.mlbId,)).fetchone()[0] == None else 0
            for i, stat in enumerate(stats):
                prospect_mask[i + 1] = Output_Map.GetProspectMask(stat)
            
            lvl_mask = torch.zeros(l, len(HITTER_LEVEL_BUCKETS), dtype=torch.float)
            for i, stat in enumerate(stats):
                lvl_mask[i,:] = torch.tensor(Output_Map.GetOutputMasks(stat))
            
            # 1 Year aggregation
            stat_year_output = torch.zeros(l, self.output_map.pitcher_stats_size, dtype=torch.float)
            pos_year_output = torch.zeros(l, self.output_map.pitcher_positions_size, dtype=torch.float)
            lvl_year_mask = torch.zeros(l, len(HITTER_LEVEL_BUCKETS), dtype=torch.float)
            if len(stats) > 1:
                start_month = stats[1].Month
                start_year = stats[1].Year
                for i, stat in enumerate(stats):
                    if i == 0:
                        (_l,_s,_p) = Aggregate_PitcherStats(startMonth=start_month - 1, endMonth=start_month - 1, startYear=start_year, endYear=start_year + 1, output_map=self.output_map, stats=stats)
                    else:
                        (_l,_s,_p) = Aggregate_PitcherStats(startMonth=stat.Month, endMonth=stat.Month, startYear=stat.Year, endYear=stat.Year + 1, output_map=self.output_map, stats=stats)
                    lvl_year_mask[i,:] = _l
                    stat_year_output[i,:] = (_s - pit_stats_means) / pit_stats_devs
                    pos_year_output[i,:] = _p
            
            # MLB Value stats and mask
            mlb_value_mask = torch.zeros(l, 3, 3, dtype=torch.float)
            mlb_value_stats = torch.zeros(l, self.output_map.mlb_pitcher_values_size, dtype=DTYPE)
            for i, value in enumerate(mlb_values):
                mlb_value_stats[i+1] = (torch.tensor(self.output_map.map_mlb_pitcher_values(value)) - mlb_value_means) / mlb_value_devs
                current_value_year = stats[i].Year
                # Mask on whether the PA count should be counted
                mlb_value_mask[i+1,0,0] = current_value_year < cutoff_year
                mlb_value_mask[i+1,1,0] = current_value_year < (cutoff_year - 1)
                mlb_value_mask[i+1,2,0] = current_value_year < (cutoff_year - 2)
                # Mask on whether the rate stats should be counted
                # Scale so don't take too much from small samples, but cap to prevent bias (players who perform poorly get cut/sent down, so uncapped scale would overestimate marginal players)
                mlb_value_mask[i+1,0,1] = min(value.IPSP1Year / 25, 1)
                mlb_value_mask[i+1,0,2] = min(value.IPRP1Year / 15, 1)
                mlb_value_mask[i+1,1,1] = min(value.IPSP2Year / 25, 1)
                mlb_value_mask[i+1,1,2] = min(value.IPRP2Year / 15, 1)
                mlb_value_mask[i+1,2,1] = min(value.IPSP3Year / 25, 1)
                mlb_value_mask[i+1,2,2] = min(value.IPRP3Year / 15, 1)
            if len(mlb_values) > 0:
                mlb_value_mask[0] = mlb_value_mask[1]
                mlb_value_stats[0] = mlb_value_stats[1]
            
            io.append(Player_IO(player=pitcher, input=input, output=output, length=l, dates=dates, prospect_mask=prospect_mask, stat_level_mask=lvl_mask, year_level_mask=lvl_year_mask, year_stat_output=stat_year_output, year_pos_output=pos_year_output, mlb_value_mask=mlb_value_mask, mlb_value_stats=mlb_value_stats))
        
        return io
        
    def Update_Mutators(self, *, off_dev : Optional[float] = None, bsr_dev : Optional[float] = None, def_dev : Optional[float] = None, hitlevel_dev : Optional[float] = None, hitpt_dev : Optional[float] = None, 
                        hitbio_dev : Optional[float] = None, pitbio_dev : Optional[float] = None,
                        pit_dev : Optional[float] = None, pitlevel_dev : Optional[float] = None, pitpt_dev : Optional[float] = None,
                        mlb_hitstat_dev : Optional[float] = None, mlb_pitstat_dev : Optional[float] = None):
        
        if off_dev is not None:
            self.off_mutator_scale = off_dev
        if bsr_dev is not None:
            self.bsr_mutator_scale = bsr_dev
        if def_dev is not None:
            self.def_mutator_scale = def_dev
        if hitlevel_dev is not None:
            self.hitlvl_mutator_scale = hitlevel_dev
        if hitpt_dev is not None:
            self.hitpt_mutator_scale = hitpt_dev
        if hitbio_dev is not None:
            self.hitbio_mutator_scale = hitbio_dev
        if pitbio_dev is not None:
            self.pitbio_mutator_scale = pitbio_dev
        if pit_dev is not None:
            self.pit_mutator_scale = pit_dev
        if pitlevel_dev is not None:
            self.pitlvl_mutator_scale = pitlevel_dev
        if pitpt_dev is not None:
            self.pitpt_mutator_scale = pitpt_dev
        if mlb_hitstat_dev is not None:
            self.mlb_hit_value_mutator_scale = mlb_hitstat_dev
        if mlb_pitstat_dev is not None:
            self.mlb_pit_value_mutator_scale = mlb_pitstat_dev
        
    def Generate_Hitting_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__hitlevel_devs")]
        pt_stds = [math.sqrt(x) for x in getattr(self, "__hitpt_devs")]
        off_stds = [math.sqrt(x) for x in getattr(self, "__off_devs")]
        bsr_stds =[math.sqrt(x) for x in getattr(self, "__bsr_devs")]
        def_stds = [math.sqrt(x) for x in getattr(self, "__def_devs")]
        bio_stds = [math.sqrt(x) for x in getattr(self, "__hitbio_devs")]
        mlbstat_stds = [math.sqrt(x) for x in getattr(self, "__hittermonthvalues_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Hitter_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Hitter Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (self.prep_map.bio_size + 1)
                
                level_mutator = [0] * self.prep_map.hitterlvl_size
                pt_mutator = [0] * self.prep_map.hitterpt_size
                off_mutator = [0] * self.prep_map.off_size
                bsr_mutator = [0] * self.prep_map.bsr_size
                def_mutator = [0] * self.prep_map.def_size
                mlbstats_mutator = [0] * self.prep_map.mlb_hit_value_size
                
                for i in range(self.prep_map.hitterlvl_size):
                    level_mutator[i] = self.hitlvl_mutator_scale * random.gauss(0, level_stds[i])
                for i in range(self.prep_map.hitterpt_size):
                    pt_mutator[i] = self.hitpt_mutator_scale * random.gauss(0, pt_stds[i])
                for i in range(self.prep_map.off_size):
                    off_mutator[i] = self.off_mutator_scale * random.gauss(0, off_stds[i])
                for i in range(self.prep_map.bsr_size):
                    bsr_mutator[i] = self.bsr_mutator_scale * random.gauss(0, bsr_stds[i])
                for i in range(self.prep_map.def_size):
                    def_mutator[i] = self.def_mutator_scale * random.gauss(0, def_stds[i])
                for i in range(self.prep_map.mlb_hit_value_size):
                    mlbstats_mutator[i] = self.mlb_hit_value_mutator_scale * random.gauss(0, mlbstat_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + level_mutator + pt_mutator + off_mutator + bsr_mutator + def_mutator + mlbstats_mutator)
            
            for i in range(self.prep_map.bio_size):
                mutators[1+i,:] = self.hitbio_mutator_scale * random.gauss(0, bio_stds[i])
        
        return mutators
    
    def Generate_Pitching_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__pitlevel_devs")]
        pt_stds = [math.sqrt(x) for x in getattr(self, "__pitpt_devs")]
        pit_stds = [math.sqrt(x) for x in getattr(self, "__pit_devs")]
        bio_stds = [math.sqrt(x) for x in getattr(self, "__pitbio_devs")]
        mlbstat_stds = [math.sqrt(x) for x in getattr(self, "__pitchermonthvalues_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Pitcher_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Pitcher Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (self.prep_map.bio_size + 1)
                
                level_mutator = [0] * self.prep_map.pitcherlvl_size
                pt_mutator = [0] * self.prep_map.pitcherpt_size
                pit_mutator = [0] * self.prep_map.pit_size
                mlbstats_mutator = [0] * self.prep_map.mlb_pit_value_size
                
                for i in range(self.prep_map.pitcherlvl_size):
                    level_mutator[i] = self.pitlvl_mutator_scale * random.gauss(0, level_stds[i])
                for i in range(self.prep_map.pitcherpt_size):
                    pt_mutator[i] = self.pitpt_mutator_scale * random.gauss(0, pt_stds[i])
                for i in range(self.prep_map.pit_size):
                    pit_mutator[i] = self.pit_mutator_scale * random.gauss(0, pit_stds[i])
                for i in range(self.prep_map.mlb_pit_value_size):
                    mlbstats_mutator[i] = self.mlb_pit_value_mutator_scale * random.gauss(0, mlbstat_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + level_mutator + pt_mutator + pit_mutator + mlbstats_mutator)
            
            for i in range(self.prep_map.bio_size):
                mutators[1+i,:] = self.pitbio_mutator_scale * random.gauss(0, bio_stds[i])
        
        return mutators
    
    def Get_Pa_Offsets(self) -> tuple[float, float, float]:
        mlb_value_means : torch.Tensor = getattr(self, "__hittervalues_means")
        mlb_value_devs : torch.Tensor = getattr(self, "__hittervalues_devs")
        
        zero_offset = (-mlb_value_means) / mlb_value_devs
        return tuple(zero_offset[-3:].tolist())
    
    def Get_Ip_Offsets(self) -> torch.tensor:
        mlb_value_means : torch.Tensor = getattr(self, "__pitchervalues_means")
        mlb_value_devs : torch.Tensor = getattr(self, "__pitchervalues_devs")
        
        zero_offset = (-mlb_value_means) / mlb_value_devs
        return zero_offset[-6:]