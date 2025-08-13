from typing import Callable
from sklearn.decomposition import PCA # type: ignore
from typing import TypeVar 
from DBTypes import *
from Constants import db, DTYPE
import math
import torch

_T = TypeVar('T')
class Custom_PCA:
    def __init__(self):
        cursor = db.cursor()
        # Get Ids for hitters, pitchers
        hitters = torch.tensor(cursor.execute('''SELECT mp.ageAtSigningYear, mp.draftPick
                                    FROM Model_Players AS mp
                                    INNER JOIN Player AS p ON mp.mlbId = p.mlbId
                                    WHERE mp.isHitter=1 AND p.signingYear<=?''', (Custom_PCA.__Cutoff_Year,)).fetchall()).float()
        
        hitters[:,1] = torch.log10(hitters[:,1]) # Log of draft pick for more variation between high draft picks
        self.__hitter_means = torch.mean(hitters, dim=0, keepdim=False)
        self.__hitter_devs = torch.std(hitters, dim=0, keepdim=False)
        
        pitchers = torch.tensor(cursor.execute('''SELECT mp.ageAtSigningYear, mp.draftPick
                                    FROM Model_Players AS mp
                                    INNER JOIN Player AS p ON mp.mlbId = p.mlbId
                                    WHERE mp.isPitcher=1 AND p.signingYear<=?''', (Custom_PCA.__Cutoff_Year,)).fetchall()).float()
        
        pitchers[:,1] = torch.log10(pitchers[:,1])
        self.__pitcher_means = torch.mean(pitchers, dim=0, keepdim=False)
        self.__pitcher_devs = torch.std(pitchers, dim=0, keepdim=False)
        
        # Get all stats <=2024 to get means/stds to normalize
        # Restrict so new data doesn't change old conversions which would break model
        hitter_stats = DB_Model_HitterStats.Select_From_DB(cursor, "WHERE Year<=?", (Custom_PCA.__Cutoff_Year,))
        
        # Comments are explained variance ratios
        # [0.502, 0.217, 0.145, 0.102, 0.027, 0.007, 0.001]
        self.__Create_PCA_Norms(Custom_PCA.__Map_OFF, hitter_stats, "off", Custom_PCA.__OFF_Size)
        # [0.722, 0.278]
        self.__Create_PCA_Norms(Custom_PCA.__Map_BSR, hitter_stats, "bsr", Custom_PCA.__BSR_Size)
        # [0.152, 0.138, 0.127, 0.113, 0.11, 0.105, 0.101, 0.095, 0.059]
        self.__Create_PCA_Norms(Custom_PCA.__Map_DEF, hitter_stats, "def", Custom_PCA.__DEF_Size)
        
    __Map_OFF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.AVGRatio, h.OBPRatio, h.ISORatio, h.wOBARatio, h.HRPercRatio, h.BBPercRatio, h.kPercRatio]
    
    __Map_BSR : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.SBRateRatio, h.SBPercRatio]
    
    __Map_DEF : Callable[[DB_Model_HitterStats], list[float]] = \
        lambda h : [h.PercC, h.Perc1B, h.Perc2B, h.Perc3B, h.PercSS, h.PercLF, h.PercCF, h.PercRF, h.PercDH]
    
    __OFF_Size = 5
    __BSR_Size = 2
    __DEF_Size = 9
    
    __Cutoff_Year = 2024
    
    def Transform_HitterStats(self, stats : list[DB_Model_HitterStats]) -> torch.Tensor:
        off_stats = torch.tensor([Custom_PCA.__Map_OFF(x) for x in stats], dtype=DTYPE)
        bsr_stats = torch.tensor([Custom_PCA.__Map_BSR(x) for x in stats], dtype=DTYPE)
        def_stats = torch.tensor([Custom_PCA.__Map_DEF(x) for x in stats], dtype=DTYPE)


        off_pca = torch.from_numpy(getattr(self, "__off_pca").transform(off_stats))
        bsr_pca = torch.from_numpy(getattr(self, "__bsr_pca").transform(bsr_stats))
        def_pca = torch.from_numpy(getattr(self, "__def_pca").transform(def_stats))
        
        return torch.cat((off_pca, bsr_pca, def_pca), dim=1)
    
    def Get_Hitter_Size(self) -> int:
        return Custom_PCA.__OFF_Size + Custom_PCA.__BSR_Size + Custom_PCA.__DEF_Size
    
    def Normalize_HitterData(self, hitter : DB_Model_Players) -> torch.Tensor:
        m : Callable[[DB_Model_Players], list[float]] = lambda h : [h.ageAtSigningYear, math.log10(h.draftPick)]
        hitter_tensor = torch.tensor(list(m(hitter)))
        return (hitter_tensor - self.__hitter_means) / self.__hitter_devs
    
    def Normalize_PitcherData(self, pitcher : DB_Model_Players) -> torch.Tensor:
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
        setattr(self, "__" + name + "_pca", pca)