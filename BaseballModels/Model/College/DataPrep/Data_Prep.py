from sklearn.decomposition import PCA # type: ignore
import torch
from DBTypes import *
from Constants import db, DTYPE, DRAFT_BUCKETS
from typing import TypeVar, Callable
from tqdm import tqdm
from College.DataPrep.Output_Map import College_Output_Map
from College.DataPrep.Prep_Map import College_Prep_Map

class College_IO:
    def __init__(
            self,
            player : DB_College_Player,
            input : torch.Tensor,
            output_draft : torch.Tensor,
            length : int,
            dates : torch.Tensor, 
    ):
        
        self.player = player
        self.input = input
        self.output_draft = output_draft
        self.length = length
        self.dates = dates
        
    @staticmethod
    def GetMaxLength(io_list : list['College_IO']) -> int:
        max_length = 0
        for io in io_list:
            max_length = max(max_length, io.length)
            
_T = TypeVar('T')
class College_Data_Prep:
    __Cutoff_Year = 2024
    
    def __init__(self, prep_map : 'College_Prep_Map', output_map : 'College_Output_Map'):
        self.prep_map = prep_map
        self.output_map = output_map
        
        cursor = db.cursor()
        
        # Bios
        hitters = DB_College_Player.Select_From_DB(cursor, "WHERE IsHitter=1 AND FirstYear>=2002 AND LastYear<=?", (College_Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_bio, hitters, "hitbio", self.prep_map.bio_size)
        
        pitchers = DB_College_Player.Select_From_DB(cursor, "WHERE isPitcher=1 AND FirstYear>=2002 AND LastYear<=?", (College_Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_bio, pitchers, "pitbio", self.prep_map.bio_size)
        
        # Stats
        hitter_stats = DB_Model_College_HitterYear.Select_From_DB(cursor, "WHERE Year<=?", (College_Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_hitstats, hitter_stats, "hitstat", self.prep_map.hitstats_size)
        self.__Create_PCA_Norms(self.prep_map.map_def, hitter_stats, "hitdef", self.prep_map.def_size)
        
        pitcher_stats = DB_Model_College_PitcherYear.Select_From_DB(cursor, "WHERE Year<=?", (College_Data_Prep.__Cutoff_Year,))
        self.__Create_PCA_Norms(self.prep_map.map_pitstats, pitcher_stats, "pitstat", self.prep_map.pitstats_size)
        
    def Get_ZScore(self, stats : torch.Tensor, name : str) -> torch.Tensor:
        means : torch.Tensor = getattr(self, f"__{name}_means")
        devs : torch.Tensor = getattr(self, f"__{name}_devs")
        return (stats - means) / devs
    
    def Get_PCA_Transform(self, stats: torch.Tensor, name : str) -> torch.Tensor:
        z_score = self.Get_ZScore(stats, name)
        pca = getattr(self, f"__{name}_pca")
        return torch.from_numpy(pca.transform(z_score))
        
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
        
    def __TransformHitterStats(self, stats : list[DB_Model_College_HitterYear]) -> torch.Tensor:
        off_stats = torch.tensor([self.prep_map.map_hitstats(x) for x in stats], dtype=DTYPE)
        def_stats = torch.tensor([self.prep_map.map_def(x) for x in stats], dtype=DTYPE)
        
        off_pca = self.Get_PCA_Transform(off_stats, "hitstat")
        def_pca = self.Get_PCA_Transform(def_stats, "hitdef")
        
        return torch.cat((off_pca, def_pca), dim=1)
    
    def __TransformPitcherStats(self, stats : list[DB_Model_College_PitcherYear]) -> torch.Tensor:
        pit_stats = torch.tensor([self.prep_map.map_pitstats(x) for x in stats], dtype=DTYPE)
        pit_pca = self.Get_PCA_Transform(pit_stats, "pitstat")
        return pit_pca
    
    def __Transform_HitterData(self, hitter : DB_College_Player) -> torch.Tensor:
        bio_stats = torch.tensor([self.prep_map.map_bio(hitter)], dtype=DTYPE)
        return self.Get_PCA_Transform(bio_stats, "hitbio")
    
    def __Transform_PitcherData(self, pitcher : DB_College_Player) -> torch.Tensor:
        bio_stats = torch.tensor([self.prep_map.map_bio(pitcher)], dtype=DTYPE)
        return self.Get_PCA_Transform(bio_stats, "pitbio")
    
    def Get_Hitter_Size(self) -> int:
        return self.prep_map.bio_size + self.prep_map.hitstats_size + self.prep_map.def_size
    
    def Get_Pitcher_Size(self) -> int:
        return self.prep_map.bio_size + self.prep_map.pitstats_size
    
    def Generate_IO_Hitters(self, player_condition : str, player_values : tuple[any], use_cutoff : bool) -> list[College_IO]:
        cursor = db.cursor()
        hitters = DB_College_Player.Select_From_DB(cursor, player_condition, player_values)
        
        io : list[College_IO] = []
        cutoff_year = College_Data_Prep.__Cutoff_Year if use_cutoff else 1000000
        
        for hitter in tqdm(hitters, desc="Generating Hitters", leave=False):
            stats = DB_Model_College_HitterYear.Select_From_DB(cursor, "WHERE tbcId=? AND year<=? ORDER BY Year ASC", (hitter.TBCId, cutoff_year))
            l = len(stats)
            if l == 0:
                continue
            
            # Dates
            dates = torch.tensor([(hitter.TBCId, x.Year,) for x in stats], dtype=torch.long)
            
            # Input
            input = torch.zeros(l, self.Get_Hitter_Size())
            input[:, :self.prep_map.bio_size] = self.__Transform_HitterData(hitter)
            input[:, self.prep_map.bio_size:] = self.__TransformHitterStats(stats)
            
            # Output
            output = torch.zeros(l, 1, dtype=torch.long)
            output[:] = torch.bucketize(torch.tensor(self.output_map.map_draft(hitter)), DRAFT_BUCKETS)
            
            io.append(College_IO(
                player=hitter,
                input=input,
                output_draft=output,
                length=l,
                dates=dates
            ))
        
        return io