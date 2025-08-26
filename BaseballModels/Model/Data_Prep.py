from typing import Callable
from sklearn.decomposition import PCA # type: ignore
from typing import TypeVar 
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
        self.off_mutator_scale = 0.2
        self.bsr_mutator_scale = 0.2
        self.def_mutator_scale = 0.3
        self.draft_mutator_scale = 0.2
        self.signing_age_scale = 0.2
        
        self.pit_mutator_scale = 0.2
        
        cursor = db.cursor()
        # Get Ids for hitters, pitchers
        hitters = torch.tensor(cursor.execute('''SELECT mp.ageAtSigningYear, mp.draftPick
                                    FROM Model_Players AS mp
                                    INNER JOIN Player AS p ON mp.mlbId = p.mlbId
                                    WHERE mp.isHitter=1 AND p.signingYear<=?''', (Data_Prep.__Cutoff_Year,)).fetchall()).float()
        
        hitters[:,1] = torch.log10(hitters[:,1]) # Log of draft pick for more variation between high draft picks
        self.__hitter_means = torch.mean(hitters, dim=0, keepdim=False)
        self.__hitter_devs = torch.std(hitters, dim=0, keepdim=False)
        
        pitchers = torch.tensor(cursor.execute('''SELECT mp.ageAtSigningYear, mp.draftPick
                                    FROM Model_Players AS mp
                                    INNER JOIN Player AS p ON mp.mlbId = p.mlbId
                                    WHERE mp.isPitcher=1 AND p.signingYear<=?''', (Data_Prep.__Cutoff_Year,)).fetchall()).float()
        
        pitchers[:,1] = torch.log10(pitchers[:,1])
        self.__pitcher_means = torch.mean(pitchers, dim=0, keepdim=False)
        self.__pitcher_devs = torch.std(pitchers, dim=0, keepdim=False)
        
        # Get all stats <=2024 to get means/stds to normalize
        # Restrict so new data doesn't change old conversions which would break model
        hitter_stats = DB_Model_HitterStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        pitcher_stats = DB_Model_PitcherStats.Select_From_DB(cursor, "WHERE Year<=?", (Data_Prep.__Cutoff_Year,))
        
        # Age and level information, keep stats individual
        self.__Create_PCA_Norms(Data_Prep.__Map_HitterLevel, hitter_stats, "hitlevel", Data_Prep.__HitterLevel_Size)
        self.__Create_PCA_Norms(Data_Prep.__Map_PitcherLevel, pitcher_stats, "pitlevel", Data_Prep.__PitcherLevel_Size)
        
        # Comments are explained variance ratios
        # [0.502, 0.217, 0.145, 0.102, 0.027, 0.007, 0.001]
        self.__Create_PCA_Norms(Data_Prep.__Map_OFF, hitter_stats, "off", Data_Prep.__OFF_Size)
        # [0.722, 0.278]
        self.__Create_PCA_Norms(Data_Prep.__Map_BSR, hitter_stats, "bsr", Data_Prep.__BSR_Size)
        # [0.152, 0.138, 0.127, 0.113, 0.11, 0.105, 0.101, 0.095, 0.059]
        self.__Create_PCA_Norms(Data_Prep.__Map_DEF, hitter_stats, "def", Data_Prep.__DEF_Size)
        
        # [0.309, 0.127, 0.114, 0.108, 0.103, 0.094, 0.063, 0.053, 0.028]
        self.__Create_PCA_Norms(Data_Prep.__Map_Pit, pitcher_stats, "pit", Data_Prep.__Pit_Size)
        
    __Map_OFF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.AVGRatio, h.OBPRatio, h.ISORatio, h.wOBARatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
    
    __Map_BSR : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.SBRateRatio, h.SBPercRatio]
    
    __Map_DEF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH]
    
    __Map_HitterLevel : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.Age, h.LevelId, h.InjStatus]
    
    __Map_PitcherLevel : Callable[[DB_Model_PitcherStats], list[float]] = \
        lambda p : [p.Age, p.LevelId, p.InjStatus]
        
    __Map_Pit : Callable[[DB_Model_PitcherStats], list[float]] = \
        lambda p : [p.ParkRunFactor, p.ParkHRFactor, p.GBPercRatio, p.ERARatio, p.FIPRatio, p.wOBARatio, p.HRPercRatio, p.BBPercRatio, p.KPercRatio]
    
    __Hitter_Size = 3
    __HitterLevel_Size = 3
    __OFF_Size = 5
    __BSR_Size = 2
    __DEF_Size = 9
    
    __Pitcher_Size = 3
    __PitcherLevel_Size = 3
    __Pit_Size = 8
    
    __Cutoff_Year = 2024
    
    def Transform_HitterStats(self, stats : list[DB_Model_HitterStats]) -> torch.Tensor:
        level_stats = torch.tensor([Data_Prep.__Map_HitterLevel(x) for x in stats], dtype=DTYPE)
        off_stats = torch.tensor([Data_Prep.__Map_OFF(x) for x in stats], dtype=DTYPE)
        bsr_stats = torch.tensor([Data_Prep.__Map_BSR(x) for x in stats], dtype=DTYPE)
        def_stats = torch.tensor([Data_Prep.__Map_DEF(x) for x in stats], dtype=DTYPE)

        level_pca = torch.from_numpy(getattr(self, "__hitlevel_pca").transform(level_stats))
        off_pca = torch.from_numpy(getattr(self, "__off_pca").transform(off_stats))
        bsr_pca = torch.from_numpy(getattr(self, "__bsr_pca").transform(bsr_stats))
        def_pca = torch.from_numpy(getattr(self, "__def_pca").transform(def_stats))
        
        return torch.cat((level_pca, off_pca, bsr_pca, def_pca), dim=1)
    
    def Transform_PitcherStats(self, stats : list[DB_Model_PitcherStats]) -> torch.Tensor:
        level_stats = torch.tensor([Data_Prep.__Map_PitcherLevel(x) for x in stats], dtype=DTYPE)
        pit_stats = torch.tensor([Data_Prep.__Map_Pit(x) for x in stats], dtype=DTYPE)
        
        level_pca = torch.from_numpy(getattr(self, "__pitlevel_pca").transform(level_stats))
        pit_pca = torch.from_numpy(getattr(self, "__pit_pca").transform(pit_stats))
        
        return torch.cat((level_pca, pit_pca), dim=1)
    
    @staticmethod
    def Get_Hitter_Size() -> int:
        return Data_Prep.__Hitter_Size + Data_Prep.__HitterLevel_Size + Data_Prep.__OFF_Size + Data_Prep.__BSR_Size + Data_Prep.__DEF_Size
    
    @staticmethod
    def Get_Pitcher_Size() -> int:
        return Data_Prep.__Pitcher_Size + Data_Prep.__PitcherLevel_Size + Data_Prep.__Pit_Size
    
    def __Normalize_HitterData(self, hitter : DB_Model_Players) -> torch.Tensor:
        m : Callable[[DB_Model_Players], list[float]] = lambda h : [h.ageAtSigningYear, math.log10(h.draftPick)]
        hitter_tensor = torch.tensor(list(m(hitter)))
        return (hitter_tensor - self.__hitter_means) / self.__hitter_devs
    
    def __Normalize_PitcherData(self, pitcher : DB_Model_Players) -> torch.Tensor:
        m : Callable[[DB_Model_Players], list[float]] = lambda h : [h.ageAtSigningYear, math.log10(h.draftPick)]
        pitcher_tensor = torch.tensor(list(m(pitcher)))
        return (pitcher_tensor - self.__pitcher_means) / self.__pitcher_devs
    
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
            input[:,1:Data_Prep.__Hitter_Size] = self.__Normalize_HitterData(hitter) # Hitter Bio
            if l > 1:
                input[1:, Data_Prep.__Hitter_Size:] = self.Transform_HitterStats(stats) # Month Stats
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
            input[:,1:Data_Prep.__Pitcher_Size] = self.__Normalize_PitcherData(pitcher) # Pitcher Bio
            if l > 1:
                input[1:, Data_Prep.__Pitcher_Size:] = self.Transform_PitcherStats(stats) # Month Stats
            inputs.append(input)
            
            # Output
            output = torch.zeros(l, 4, dtype=torch.long)
            output[:,0] = torch.bucketize(torch.tensor(pitcher.warPitcher), PITCHER_TOTAL_WAR_BUCKETS)
            output[:,1] = torch.bucketize(torch.tensor(pitcher.peakWarPitcher), PITCHER_PEAK_WAR_BUCKETS)
            output[:,2] = torch.bucketize(torch.tensor(pitcher.highestLevelPitcher), PITCHER_LEVEL_BUCKETS)
            output[:,3] = torch.bucketize(torch.tensor(pitcher.totalOuts), PITCHER_BF_BUCKETS)
            outputs.append(output)
        
        return pitchers, inputs, outputs, max_length, dates 
        
    def Generate_Hitting_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__hitlevel_devs")]
        off_stds = [math.sqrt(x) for x in getattr(self, "__off_devs")]
        bsr_stds =[math.sqrt(x) for x in getattr(self, "__bsr_devs")]
        def_stds = [math.sqrt(x) for x in getattr(self, "__def_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Hitter_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Hitter Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (Data_Prep.__Hitter_Size + Data_Prep.__HitterLevel_Size)
                
                off_mutator = [0] * Data_Prep.__OFF_Size
                bsr_mutator = [0] * Data_Prep.__BSR_Size
                def_mutator = [0] * Data_Prep.__DEF_Size
                
                for i in range(Data_Prep.__OFF_Size):
                    off_mutator[i] = self.off_mutator_scale * random.gauss(0, off_stds[i])
                for i in range(Data_Prep.__BSR_Size):
                    bsr_mutator[i] = self.bsr_mutator_scale * random.gauss(0, bsr_stds[i])
                for i in range(Data_Prep.__DEF_Size):
                    def_mutator[i] = self.def_mutator_scale * random.gauss(0, def_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + off_mutator + bsr_mutator + def_mutator)
            
            mutators[1,:] = random.gauss(0, self.signing_age_scale)
            mutators[2,:] = random.gauss(0, self.draft_mutator_scale)
            mutators[3,:] = random.gauss(0,0.2)
            # Only slightly modify level to avoid mid-month promotion overfitting
            mutators[4,:] = random.gauss(0, 0.01)
            # No mutator for injury, only has discrete values
        
        return mutators
    
    def Generate_Pitching_Mutators(self, batch_size : int, max_input_size : int) -> torch.Tensor:
        # Get std deviations from explained variance
        level_stds = [math.sqrt(x) for x in getattr(self, "__pitlevel_devs")]
        pit_stds = [math.sqrt(x) for x in getattr(self, "__pit_devs")]
        
        mutators = torch.zeros(size=(batch_size, max_input_size, self.Get_Pitcher_Size()))
        for n in tqdm(range(batch_size), leave=False, desc="Generating Pitcher Mutators"):
            for m in range(max_input_size):
                player_header = [0] * (Data_Prep.__Pitcher_Size + Data_Prep.__PitcherLevel_Size)
                
                pit_mutator = [0] * Data_Prep.__Pit_Size
                
                for i in range(Data_Prep.__Pit_Size):
                    pit_mutator[i] = self.pit_mutator_scale * random.gauss(0, pit_stds[i])
                    
                mutators[n,m] = torch.tensor(player_header + pit_mutator)
            
            mutators[1,:] = random.gauss(0, self.signing_age_scale)
            mutators[2,:] = random.gauss(0, self.draft_mutator_scale)
            mutators[3,:] = random.gauss(0,0.2)
            # Only slightly modify level to avoid mid-month promotion overfitting
            mutators[4,:] = random.gauss(0, 0.01)
            # No mutator for injury, only has discrete values
        
        return mutators