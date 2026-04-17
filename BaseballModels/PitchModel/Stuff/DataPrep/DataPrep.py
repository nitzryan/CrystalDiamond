from DBTypes import *
from Stuff.DataPrep.PrepMap import Prep_Map
import torch
from sklearn.decomposition import PCA
from typing import TypeVar, Callable
from Constants import db, DTYPE
from Buckets import *
from Stuff.DataPrep.PitchDataset import PitchIO
        
_T = TypeVar('T')
class DataPrep:
    def __init__(self,
        prep_map : Prep_Map,
    ):
        
        self.prep_map = prep_map
        
        cursor = db.cursor()
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
        self.conditional_statement += "Year<=? AND LevelId=1"
        
        pitches = DB_PitchStatcast.Select_From_DB(
            cursor=cursor,
            conditional=self.conditional_statement,
            values=(DataPrep.__CutoffYear,)
        )
        
        self.__Create_PCA_Norms(self.prep_map.pitch_overview_map, pitches, "overview", self.prep_map.pitch_overview_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_loc_map, pitches, "loc", self.prep_map.pitch_loc_size)
        self.__Create_PCA_Norms(self.prep_map.pitch_stuff_map, pitches, "stuff", self.prep_map.pitch_stuff_size)
       
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

        overview_pca = self.Get_PCA_Transform(overview_stats, "overview")
        loc_pca = self.Get_PCA_Transform(loc_stats, "loc")
        stuff_pca = self.Get_PCA_Transform(stuff_stats, "stuff")
        
        return overview_pca, loc_pca, stuff_pca
    
    def GenerateIOPitches(self, use_cutoff : bool) -> list[PitchIO]:
        cursor = db.cursor()
        pitches = DB_PitchStatcast.Select_From_DB(
            cursor=cursor,
            conditional=self.conditional_statement,
            values=(DataPrep.__CutoffYear if use_cutoff else 100000,)
        )
        
        data_overview, data_loc, data_stuff = self.Transform_PitchStats(pitches)
        
        pitch_io_list : list[PitchIO] = []
        for i in range(len(pitches)):
            pitch_io_list.append(PitchIO(
                data_overview=data_overview[i],
                data_loc=data_loc[i],
                data_stuff=data_stuff[i],
                output_value=torch.bucketize(torch.tensor([pitches[i].RunValueHitter]), BUCKET_PITCHVALUE).item(),
                output_runs=pitches[i].PaResultDirectRuns,
                output_outs=min(pitches[i].PaResultOuts, 2),
                output_swung=pitches[i].HadSwing,
                output_contact=pitches[i].HadContact,
                output_inplay=pitches[i].IsInPlay
            ))
            
        return pitch_io_list