from sklearn.decomposition import PCA # type: ignore
from typing import TypeVar, Optional, Callable
from DBTypes import *
from Constants import db, DTYPE
from Constants import HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS, HITTER_PEAK_WAR_BUCKETS, HITTER_TOTAL_WAR_BUCKETS
from Constants import PITCHER_LEVEL_BUCKETS, PITCHER_BF_BUCKETS, PITCHER_PEAK_WAR_BUCKETS, PITCHER_TOTAL_WAR_BUCKETS
import math
import torch
from tqdm import tqdm
import random

_T = TypeVar('T')
class Data_Prep:
    def __init__(self):
        # Mutators
        self.off_mutator_scale = 0.05
        self.bsr_mutator_scale = 0.05
        self.def_mutator_scale = 0.05
        
        self.hitbio_mutator_scale = 0.05
        self.pitbio_mutator_scale = 0.05
        
        self.hitpt_mutator_scale = 0.05
        self.pitpt_mutator_scale = 0.05
        
        self.hitlvl_mutator_scale = 0.05
        self.pitlvl_mutator_scale = 0.05
        
        self.pit_mutator_scale = 0.05
        
        cursor = db.cursor()
        # Bios
        hitters = DB_Model_Players.Select_From_DB(cursor, "WHERE isHitter=1 AND signingYear<=?", (Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(Data_Prep.__Map_Bio, hitters, "hitbio", Data_Prep.__Bio_Size)
        
        pitchers = DB_Model_Players.Select_From_DB(cursor, "WHERE isPitcher=1 AND signingYear<=?", (Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(Data_Prep.__Map_Bio, pitchers, "pitbio", Data_Prep.__Bio_Size)
        
        # Get all stats <=2024 to get means/stds to normalize
        # Restrict so new data doesn't change old conversions which would break model
        hitter_stats = DB_Model_HitterStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        pitcher_stats = DB_Model_PitcherStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        
        # Age and level information, keep stats individual
        self.__Create_PCA_Norms(Data_Prep.__Map_HitterLevel, hitter_stats, "hitlevel", Data_Prep.__HitterLevel_Size)
        self.__Create_PCA_Norms(Data_Prep.__Map_PitcherLevel, pitcher_stats, "pitlevel", Data_Prep.__PitcherLevel_Size)
        
        # Stats for playing time
        self.__Create_PCA_Norms(Data_Prep.__Map_HitterPT, hitter_stats, "hitpt", Data_Prep.__Hitter_PT_Size)
        self.__Create_PCA_Norms(Data_Prep.__Map_PitcherPT, pitcher_stats, "pitpt", Data_Prep.__Pitcher_PT_Size)
        
        # Stats for evaluating performance
        self.__Create_PCA_Norms(Data_Prep.__Map_OFF, hitter_stats, "off", Data_Prep.__OFF_Size)
        self.__Create_PCA_Norms(Data_Prep.__Map_BSR, hitter_stats, "bsr", Data_Prep.__BSR_Size)
        self.__Create_PCA_Norms(Data_Prep.__Map_DEF, hitter_stats, "def", Data_Prep.__DEF_Size)
        
        self.__Create_PCA_Norms(Data_Prep.__Map_Pit, pitcher_stats, "pit", Data_Prep.__Pit_Size)
        
    __Map_Bio : Callable[[DB_Model_Players], list[float]] = \
        lambda p : [p.ageAtSigningYear, math.log10(p.draftPick), math.log10(p.draftSignRank)]
        
    __Map_OFF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.ParkHRFactor, h.ParkRunFactor, h.AVGRatio, h.OBPRatio, h.ISORatio, h.wOBARatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
    
    __Map_BSR : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.SBRateRatio, h.SBPercRatio]
    
    __Map_DEF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH]
    
    __Map_HitterPT : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.PA, h.MonthFrac, h.InjStatus]
        
    __Map_PitcherPT : Callable[[DB_Model_PitcherStats], list[float]] = \
        lambda p : [p.BF, p.MonthFrac, p.InjStatus]
    
    __Map_HitterLevel : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.Age, h.LevelId, h.Month]
    
    __Map_PitcherLevel : Callable[[DB_Model_PitcherStats], list[float]] = \
        lambda p : [p.Age, p.LevelId, p.Month]
        
    __Map_Pit : Callable[[DB_Model_PitcherStats], list[float]] = \
        lambda p : [p.ParkRunFactor, p.ParkHRFactor, p.GBPercRatio, p.ERARatio, p.FIPRatio, p.wOBARatio, p.HRPercRatio, p.BBPercRatio, p.KPercRatio]
    
    __Map_FirstSeason_Hit : Callable[[DB_Model_HitterStats, float], list[float]] = \
        lambda h, y : 1 if h.Year == y else 0
        
    __Map_FirstSeason_Pit : Callable[[DB_Model_PitcherStats, float], list[float]] = \
        lambda p, y : 1 if p.Year == y else 0
    
    # Comments are explained variance ratios
    __Bio_Size = 2 # Hitter [0.711, 0.282, 0.007]
                    #Pitcher [0.702, 0.289, 0.009]
    
    __HitterLevel_Size = 3
    __Hitter_PT_Size = 3
    __OFF_Size = 7 # [0.393, 0.168, 0.117, 0.114, 0.104, 0.078, 0.021, 0.006, 0.001]
    __BSR_Size = 2 # [0.722, 0.278]
    __DEF_Size = 9 # [0.152, 0.138, 0.127, 0.113, 0.11, 0.105, 0.101, 0.095, 0.059]
    __FirstSeason_Size = 1
    
    __PitcherLevel_Size = 3
    __Pitcher_PT_Size = 3
    __Pit_Size = 8 # [0.309, 0.127, 0.114, 0.108, 0.103, 0.094, 0.063, 0.053, 0.028]
    
    __Cutoff_Year = 2024
    
    def Transform_HitterStats(self, stats : list[DB_Model_HitterStats], hitter : DB_Model_Players) -> torch.Tensor:
        level_stats = torch.tensor([Data_Prep.__Map_HitterLevel(x) for x in stats], dtype=DTYPE)
        pt_stats = torch.tensor([Data_Prep.__Map_HitterPT(x) for x in stats], dtype=DTYPE)
        off_stats = torch.tensor([Data_Prep.__Map_OFF(x) for x in stats], dtype=DTYPE)
        bsr_stats = torch.tensor([Data_Prep.__Map_BSR(x) for x in stats], dtype=DTYPE)
        def_stats = torch.tensor([Data_Prep.__Map_DEF(x) for x in stats], dtype=DTYPE)

        level_pca = torch.from_numpy(getattr(self, "__hitlevel_pca").transform(level_stats))
        pt_pca = torch.from_numpy(getattr(self, "__hitpt_pca").transform(pt_stats))
        off_pca = torch.from_numpy(getattr(self, "__off_pca").transform(off_stats))
        bsr_pca = torch.from_numpy(getattr(self, "__bsr_pca").transform(bsr_stats))
        def_pca = torch.from_numpy(getattr(self, "__def_pca").transform(def_stats))
        
        isSigningYear = torch.tensor([Data_Prep.__Map_FirstSeason_Hit(x, hitter.signingYear) for x in stats], dtype=DTYPE).unsqueeze(-1)
        
        return torch.cat((isSigningYear, level_pca, pt_pca, off_pca, bsr_pca, def_pca), dim=1)
    
    def Transform_PitcherStats(self, stats : list[DB_Model_PitcherStats], pitcher : DB_Model_Players) -> torch.Tensor:
        level_stats = torch.tensor([Data_Prep.__Map_PitcherLevel(x) for x in stats], dtype=DTYPE)
        pt_stats = torch.tensor([Data_Prep.__Map_PitcherPT(x) for x in stats], dtype=DTYPE)
        pit_stats = torch.tensor([Data_Prep.__Map_Pit(x) for x in stats], dtype=DTYPE)
        
        level_pca = torch.from_numpy(getattr(self, "__pitlevel_pca").transform(level_stats))
        pt_pca = torch.from_numpy(getattr(self, "__pitpt_pca").transform(pt_stats))
        pit_pca = torch.from_numpy(getattr(self, "__pit_pca").transform(pit_stats))
        
        isSigningYear = torch.tensor([Data_Prep.__Map_FirstSeason_Pit(x, pitcher.signingYear) for x in stats], dtype=DTYPE).unsqueeze(-1)
        
        return torch.cat((isSigningYear, level_pca, pt_pca, pit_pca), dim=1)
    
    @staticmethod
    def Get_Hitter_Size() -> int:
        return Data_Prep.__Bio_Size + Data_Prep.__HitterLevel_Size + Data_Prep.__OFF_Size + Data_Prep.__BSR_Size + Data_Prep.__DEF_Size + Data_Prep.__Hitter_PT_Size + Data_Prep.__FirstSeason_Size + 1
    
    @staticmethod
    def Get_Pitcher_Size() -> int:
        return Data_Prep.__Bio_Size + Data_Prep.__PitcherLevel_Size + Data_Prep.__Pit_Size + Data_Prep.__Pitcher_PT_Size + Data_Prep.__FirstSeason_Size + 1
    
    def __Transform_HitterData(self, hitter : DB_Model_Players) -> torch.Tensor:
        bio_stats = torch.tensor([Data_Prep.__Map_Bio(hitter)], dtype=DTYPE)
        bio_pca = torch.from_numpy(getattr(self, "__hitbio_pca").transform(bio_stats))
        return bio_pca
    
    def __Transform_PitcherData(self, pitcher : DB_Model_Players) -> torch.Tensor:
        bio_stats = torch.tensor([Data_Prep.__Map_Bio(pitcher)], dtype=DTYPE)
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
        
    def Generate_IO_Hitters(self, player_condition : str, player_values : tuple[any]) -> tuple[list['DB_Model_Players'], list[torch.Tensor], list[torch.Tensor], int, list[torch.Tensor]]:
        # Get Hitters
        cursor = db.cursor()
        hitters = DB_Model_Players.Select_From_DB(cursor, player_condition, player_values)
        
        inputs = []
        outputs = []
        dates = []
        max_length = 0
        
        for hitter in hitters:
            # Get Stats
            stats = DB_Model_HitterStats.Select_From_DB(cursor, '''
                WHERE mlbId=:mlbId AND 
                (
                    Year<:year OR
                    (Year=:year AND Month<=:month)
                )
                ORDER BY Year ASC, MONTH ASC''',
                {'mlbId':hitter.mlbId,"year":hitter.lastProspectYear,"month":hitter.lastProspectMonth})
            l = len(stats) + 1
            if l > max_length:
                max_length = l
                
            dates.append(
                torch.cat(
                    (torch.tensor([(hitter.mlbId, 0, 0)], dtype=torch.long),
                    torch.tensor([(x.mlbId, x.Year, x.Month) for x in stats], dtype=torch.long))))
            
            # Input
            input = torch.zeros(l, Data_Prep.Get_Hitter_Size())
            input[0,0] = 1 # Initialization step, no stats here
            input[:,1:Data_Prep.__Bio_Size + 1] = self.__Transform_HitterData(hitter) # Hitter Bio
            if l > 1:
                input[1:, Data_Prep.__Bio_Size + 1:] = self.Transform_HitterStats(stats, hitter) # Month Stats
            inputs.append(input)
            
            # Output
            output = torch.zeros(l, 4, dtype=torch.long)
            output[:,0] = torch.bucketize(torch.tensor(hitter.warHitter), HITTER_TOTAL_WAR_BUCKETS)
            output[:,1] = torch.bucketize(torch.tensor(hitter.peakWarHitter), HITTER_PEAK_WAR_BUCKETS)
            output[:,2] = torch.bucketize(torch.tensor(hitter.highestLevelHitter), HITTER_LEVEL_BUCKETS)
            output[:,3] = torch.bucketize(torch.tensor(hitter.totalPA), HITTER_PA_BUCKETS)
            outputs.append(output)
        
        return hitters, inputs, outputs, max_length, dates
       
    def Generate_IO_Pitchers(self, player_condition : str, player_values : tuple[any]) -> tuple[list['DB_Model_Players'], list[torch.Tensor], list[torch.Tensor], int, list[torch.Tensor]]:
        # Get Hitters
        cursor = db.cursor()
        pitchers = DB_Model_Players.Select_From_DB(cursor, player_condition, player_values)
        
        inputs = []
        outputs = []
        dates = []
        max_length = 0
        
        for pitcher in pitchers:
            # Get Stats
            stats = DB_Model_PitcherStats.Select_From_DB(cursor, '''
                WHERE mlbId=:mlbId AND 
                (
                    Year<:year OR
                    (Year=:year AND Month<=:month)
                )
                ORDER BY Year ASC, MONTH ASC''',
                {'mlbId':pitcher.mlbId,"year":pitcher.lastProspectYear,"month":pitcher.lastProspectMonth})
            l = len(stats) + 1
            if l > max_length:
                max_length = l
                
            dates.append(
                torch.cat(
                    (torch.tensor([(pitcher.mlbId, 0, 0)], dtype=torch.long),
                    torch.tensor([(x.mlbId, x.Year, x.Month) for x in stats], dtype=torch.long))))
            
            # Input
            input = torch.zeros(l, Data_Prep.Get_Pitcher_Size())
            input[0,0] = 1 # Initialization step, no stats here
            input[:,1:Data_Prep.__Bio_Size + 1] = self.__Transform_PitcherData(pitcher) # Pitcher Bio
            if l > 1:
                input[1:, Data_Prep.__Bio_Size + 1:] = self.Transform_PitcherStats(stats, pitcher) # Month Stats
            inputs.append(input)
            
            # Output
            output = torch.zeros(l, 4, dtype=torch.long)
            output[:,0] = torch.bucketize(torch.tensor(pitcher.warPitcher), PITCHER_TOTAL_WAR_BUCKETS)
            output[:,1] = torch.bucketize(torch.tensor(pitcher.peakWarPitcher), PITCHER_PEAK_WAR_BUCKETS)
            output[:,2] = torch.bucketize(torch.tensor(pitcher.highestLevelPitcher), PITCHER_LEVEL_BUCKETS)
            output[:,3] = torch.bucketize(torch.tensor(pitcher.totalOuts), PITCHER_BF_BUCKETS)
            outputs.append(output)
        
        return pitchers, inputs, outputs, max_length, dates 
        
    def Update_Mutators(self, *, off_dev : Optional[float] = None, bsr_dev : Optional[float] = None, def_dev : Optional[float] = None, hitlevel_dev : Optional[float] = None, hitpt_dev : Optional[float] = None, 
                        hitbio_dev : Optional[float] = None, pitbio_dev : Optional[float] = None,
                        pit_dev : Optional[float] = None, pitlevel_dev : Optional[float] = None, pitpt_dev : Optional[float] = None):
        
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
        
    def Generate_Hitting_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__hitlevel_devs")]
        pt_stds = [math.sqrt(x) for x in getattr(self, "__hitpt_devs")]
        off_stds = [math.sqrt(x) for x in getattr(self, "__off_devs")]
        bsr_stds =[math.sqrt(x) for x in getattr(self, "__bsr_devs")]
        def_stds = [math.sqrt(x) for x in getattr(self, "__def_devs")]
        bio_stds = [math.sqrt(x) for x in getattr(self, "__hitbio_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Hitter_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Hitter Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (Data_Prep.__Bio_Size + 1)
                
                firstyear_mutator = [0] * Data_Prep.__FirstSeason_Size
                level_mutator = [0] * Data_Prep.__HitterLevel_Size
                pt_mutator = [0] * Data_Prep.__Hitter_PT_Size
                off_mutator = [0] * Data_Prep.__OFF_Size
                bsr_mutator = [0] * Data_Prep.__BSR_Size
                def_mutator = [0] * Data_Prep.__DEF_Size
                
                for i in range(Data_Prep.__FirstSeason_Size):
                    firstyear_mutator[i] = 0
                for i in range(Data_Prep.__HitterLevel_Size):
                    level_mutator[i] = self.hitlvl_mutator_scale * random.gauss(0, level_stds[i])
                for i in range(Data_Prep.__Hitter_PT_Size):
                    pt_mutator[i] = self.hitpt_mutator_scale * random.gauss(0, pt_stds[i])
                for i in range(Data_Prep.__OFF_Size):
                    off_mutator[i] = self.off_mutator_scale * random.gauss(0, off_stds[i])
                for i in range(Data_Prep.__BSR_Size):
                    bsr_mutator[i] = self.bsr_mutator_scale * random.gauss(0, bsr_stds[i])
                for i in range(Data_Prep.__DEF_Size):
                    def_mutator[i] = self.def_mutator_scale * random.gauss(0, def_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + firstyear_mutator + level_mutator + pt_mutator + off_mutator + bsr_mutator + def_mutator)
            
            for i in range(Data_Prep.__Bio_Size):
                mutators[1+i,:] = self.hitbio_mutator_scale * random.gauss(0, bio_stds[i])
        
        return mutators
    
    def Generate_Pitching_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__pitlevel_devs")]
        pt_stds = [math.sqrt(x) for x in getattr(self, "__pitpt_devs")]
        pit_stds = [math.sqrt(x) for x in getattr(self, "__pit_devs")]
        bio_stds = [math.sqrt(x) for x in getattr(self, "__pitbio_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Pitcher_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Pitcher Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (Data_Prep.__Bio_Size + 1)
                
                firstyear_mutator = [0] * Data_Prep.__FirstSeason_Size
                level_mutator = [0] * Data_Prep.__PitcherLevel_Size
                pt_mutator = [0] * Data_Prep.__Pitcher_PT_Size
                pit_mutator = [0] * Data_Prep.__Pit_Size
                
                for i in range(Data_Prep.__FirstSeason_Size):
                    firstyear_mutator[i] = 0
                for i in range(Data_Prep.__PitcherLevel_Size):
                    level_mutator[i] = self.pitlvl_mutator_scale * random.gauss(0, level_stds[i])
                for i in range(Data_Prep.__Pitcher_PT_Size):
                    pt_mutator[i] = self.pitpt_mutator_scale * random.gauss(0, pt_stds[i])
                for i in range(Data_Prep.__Pit_Size):
                    pit_mutator[i] = self.pit_mutator_scale * random.gauss(0, pit_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + firstyear_mutator + level_mutator + pt_mutator + pit_mutator)
            
            for i in range(Data_Prep.__Bio_Size):
                mutators[1+i,:] = self.pitbio_mutator_scale * random.gauss(0, bio_stds[i])
        
        return mutators