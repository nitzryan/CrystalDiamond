from typing import TypeVar
from Model.DBTypes import *
import torch
from Model.Pro.DataPrep.Prep_Map import Prep_Map
from Model.Pro.DataPrep.Output_Map import Output_Map

from Model.College.DataPrep.Data_Prep import College_Data_Prep, College_IO
from Model.Pro.DataPrep.Data_Prep import Data_Prep, Player_IO
from Model.College.DataPrep.Prep_Map import College_Prep_Map
from Model.College.DataPrep.Output_Map import College_Output_Map
from Model.Pro.DataPrep.Prep_Map import Prep_Map
from Model.Pro.DataPrep.Output_Map import Output_Map

import dill

NUM_VARIANTS = 10

class Combined_IO:
    def __init__(self, 
                pro_io : Player_IO | None,
                college_io : College_IO | None):
        
        self.pro_io = pro_io
        self.college_io = college_io
        
_T = TypeVar('T')
class Combined_Data_Prep:
    def __init__(self, 
                prep_map : Prep_Map, 
                output_map : Output_Map, 
                college_prep_map : College_Prep_Map,
                college_output_map : College_Output_Map,
                save_name : str | None = None):
        
        self.pro_data_prep = Data_Prep(prep_map=prep_map, output_map=output_map)
        self.college_data_prep = College_Data_Prep(prep_map=college_prep_map, output_map=college_output_map)
        
        if save_name is not None:
           with open(save_name, 'wb') as file:
               dill.dump(self, file)
        
    @staticmethod
    def Load_From_File(filename : str) -> 'Combined_Data_Prep':
        with open(filename, 'rb') as file:
            return dill.load(file)
        
    def GetProIOSize(self, is_hitter : bool) -> int:
        if is_hitter:
            return self.pro_data_prep.Get_Hitter_Size()
        else:
            return self.pro_data_prep.Get_Pitcher_Size()
    
    def GetColIOSize(self, is_hitter : bool) -> int:
        if is_hitter:
            return self.college_data_prep.Get_Hitter_Size()
        else:
            return self.college_data_prep.Get_Pitcher_Size()
        
    def GetEmptyProIO(self, is_hitter : bool) -> Player_IO:
        if is_hitter:
            player_template = self.pro_data_prep.Generate_IO_Hitters("WHERE MlbId=?", (596146,), False)[0]
        else:
            player_template = self.pro_data_prep.Generate_IO_Pitchers("WHERE MlbId=?", (446155,), False)[0]
        
        return Player_IO(
            player=None,
            input=torch.zeros(0, *player_template.input.shape[1:]),
            output=torch.tensor([0,0,0], dtype=torch.long),
            prospect_value=torch.tensor([0]),
            output_war_class_variants=torch.zeros(0, *player_template.output_war_class_variants.shape[1:]),
            output_war_regression_variants=torch.zeros(0, *player_template.output_war_regression_variants.shape[1:]),
            length = 0,
            player_demo=2, # 1 will fetch college data, so don't use
            player_bio=player_template.player_bio,
            dates=torch.zeros(0, *player_template.dates.shape[1:]), 
            prospect_mask=torch.zeros(0, *player_template.prospect_mask.shape[1:]),
            stat_level_mask=torch.zeros(0, *player_template.stat_level_mask.shape[1:]),
            year_level_mask=torch.zeros(0, *player_template.year_level_mask.shape[1:]),
            year_stat_output=torch.zeros(0, *player_template.year_stat_output.shape[1:]),
            year_pos_output=torch.zeros(0, *player_template.year_pos_output.shape[1:]),
            mlb_value_mask=torch.zeros(0, *player_template.mlb_value_mask.shape[1:]),
            mlb_value_stats=torch.zeros(0, *player_template.mlb_value_stats.shape[1:]),
            pt_year_output=torch.zeros(0, *player_template.pt_year_output.shape[1:]),
            pt_levelYearGames=torch.zeros(0, *player_template.pt_levelYearGames.shape[1:]),
            mlb_stat_buckets=torch.zeros(0, *player_template.mlb_stat_buckets.shape[1:], dtype=torch.long),
            mlb_stat_mask=torch.zeros(0, *player_template.mlb_stat_mask.shape[1:]),
        )
    
    def GetEmptyCollegeIO(self, is_hitter : bool) -> College_IO:
        if is_hitter:
            player_template = self.college_data_prep.Generate_IO_Hitters("WHERE TBCId=?", (297375,), False)[0]
        else:
            player_template = self.college_data_prep.Generate_IO_Pitchers("WHERE TBCId=?", (103,), False)[0]

        return College_IO(
            player=None,
            input=torch.zeros(0, *player_template.input.shape[1:]),
            output_draft=torch.tensor(0),
            output_war=torch.tensor(0),
            output_pos=torch.ones_like(player_template.output_pos) / player_template.output_pos.shape[0],
            mask_pos=0,
            output_pa=torch.tensor(0) if player_template.output_pa is not None else None,
            output_off=torch.tensor(0) if player_template.output_off is not None else None,
            output_def=torch.tensor(0) if player_template.output_def is not None else None,
            length=0,
            dates=torch.zeros(0, *player_template.dates.shape[1:]),
        )
        
    def Generate_IO_Hitters(self, is_training : bool) -> list[Combined_IO]:
        conditions = "WHERE IsEligible=1 AND IsHitter=1" if is_training else "WHERE IsHitter=1"
        
        pro_io = self.pro_data_prep.Generate_IO_Hitters(conditions, (), is_training)
        college_io = self.college_data_prep.Generate_IO_Hitters(conditions, (), is_training)
        
        return self._Load_IO_Hitters(pro_io, college_io)
        
    def Generate_IO_Hitters_Update(self, year : int, month : int, college_year : int) -> list[Combined_IO]:
        pro_conditions = '''WHERE mlbId IN (
                SELECT DISTINCT(mlbId)
                FROM Model_HitterStats
                WHERE Year=? AND Month=?
            ) AND IsHitter=1;'''
            
        pro_io = self.pro_data_prep.Generate_IO_Hitters(pro_conditions, (year, month), False)
        
        # Need college io for pro_io
        college_io = self.college_data_prep.Generate_IO_Hitters(pro_conditions, (year, month), False)
        
        if college_year == year:
            player_in_col : set[int] = {o.player.MlbId for o in college_io}
            col_conditions = "WHERE LastYear>=? AND IsHitter=1"
            college_stat_io = self.college_data_prep.Generate_IO_Hitters(col_conditions, (year,), False)
            for cs in college_stat_io:
                if not cs.player.MlbId in player_in_col:
                    college_io.append(cs)
        
        return self._Load_IO_Hitters(pro_io, college_io)
        
    def Generate_IO_Test_Hitter(self,
            pro_player : DB_Model_Players | None,
            pro_stats : list[DB_Model_HitterStats] | None,
            pro_month_war : list[DB_Player_MonthlyWar] | None,
            
            col_player : DB_College_Player | None,
            col_stats : list[DB_Model_College_HitterYear] | None
            ) -> Combined_IO:
        
        if pro_player is not None and pro_stats is not None and pro_month_war is not None:
            pro_io = self.pro_data_prep.Generate_IO_Single_Hitter(pro_player, pro_stats, pro_month_war)
        else:
            pro_io = self.GetEmptyProIO(True)
            
        if col_player is not None and col_stats is not None:
            col_io = self.college_data_prep.Generate_IO_Single_Hitter(col_player, col_stats)
        else:
            col_io = self.GetEmptyCollegeIO(is_hitter=True)
            
        return Combined_IO(pro_io, col_io)
        
    def _Load_IO_Hitters(self, pro_io : list[Player_IO], college_io : list[College_IO]) -> list[Combined_IO]:
        empty_pro_io = self.GetEmptyProIO(is_hitter=True)
        empty_college_io = self.GetEmptyCollegeIO(is_hitter=True)
        
        io : list[Combined_IO] = []
        
        # Need to map pro <based on mlbId> and college <based on tbcId>
        mlbid_idx_dict : dict[int, int] = {}
        for i, p in enumerate(pro_io):
            mlbid_idx_dict[p.player.mlbId] = i
            
        # Add all college hitters
        for c in college_io:
            id = c.player.MlbId
            pro = pro_io[mlbid_idx_dict[id]] if id in mlbid_idx_dict else empty_pro_io
            io.append(Combined_IO(pro, c))
            
        # Need to map college hitters by mlbId to create shared for player with no college
        # Won't need an actual value, just that it is in the dict
        col_idx_dict : dict[int, None] = {}
        for c in college_io:
            if c.player.MlbId != 0:
                col_idx_dict[c.player.MlbId] = None
                
        for p in pro_io:
            if not p.player.mlbId in col_idx_dict:
                io.append(Combined_IO(p, empty_college_io))
        
        return io
       
    def Generate_IO_Pitchers(self, is_training : bool) -> list[Combined_IO]:
        conditions = "WHERE IsEligible=1 AND IsPitcher=1" if is_training else "WHERE IsPitcher=1"
        
        pro_io = self.pro_data_prep.Generate_IO_Pitchers(conditions, (), is_training)
        college_io = self.college_data_prep.Generate_IO_Pitchers(conditions, (), is_training)
        
        return self._Load_IO_Pitchers(pro_io, college_io)
        
    def Generate_IO_Pitchers_Update(self, year : int, month : int, college_year : int) -> list[Combined_IO]:
        pro_conditions = '''WHERE mlbId IN (
                SELECT DISTINCT(mlbId)
                FROM Model_PitcherStats
                WHERE Year=? AND Month=?
            ) AND IsPitcher=1;'''
            
        pro_io = pro_io = self.pro_data_prep.Generate_IO_Pitchers(pro_conditions, (year, month), False)
        
        # Need college io for pro_io
        college_io = self.college_data_prep.Generate_IO_Pitchers(pro_conditions, (year, month), False)
        
        if college_year == year:
            player_in_col : set[int] = {o.player.MlbId for o in college_io}
            col_conditions = "WHERE LastYear>=? AND IsPitcher=1"
            college_stat_io = self.college_data_prep.Generate_IO_Pitchers(col_conditions, (year,), False)
            for cs in college_stat_io:
                if not cs.player.MlbId in player_in_col:
                    college_io.append(cs)
        
        return self._Load_IO_Pitchers(pro_io, college_io)
        
    def _Load_IO_Pitchers(self, pro_io : list[Player_IO], college_io : list[College_IO]) -> list[Combined_IO]:
        empty_pro_io = self.GetEmptyProIO(is_hitter=False)
        empty_college_io = self.GetEmptyCollegeIO(is_hitter=False)
        
        io : list[Combined_IO] = []
        
        # Need to map pro <based on mlbId> and college <based on tbcId>
        mlbid_idx_dict : dict[int, int] = {}
        for i, p in enumerate(pro_io):
            mlbid_idx_dict[p.player.mlbId] = i
            
        # Add all college pitchers
        for c in college_io:
            id = c.player.MlbId
            pro = pro_io[mlbid_idx_dict[id]] if id in mlbid_idx_dict else empty_pro_io
            io.append(Combined_IO(pro, c))
            
        # Need to map college pitchers by mlbId to create shared for player with no college
        # Won't need an actual value, just that it is in the dict
        col_idx_dict : dict[int, None] = {}
        for c in college_io:
            if c.player.MlbId != 0:
                col_idx_dict[c.player.MlbId] = None
                
        for p in pro_io:
            if not p.player.mlbId in col_idx_dict:
                io.append(Combined_IO(p, empty_college_io))
        
        return io