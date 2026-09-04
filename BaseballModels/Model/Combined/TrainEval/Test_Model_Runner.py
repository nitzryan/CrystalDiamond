from dataclasses import dataclass
import torch
import torch.nn.functional as F
import warnings
import copy

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Pro.DataPrep.Data_Prep import Pro_Hitter_Data, Pro_Pitcher_Data, Player_IO
from Model.College.DataPrep.Data_Prep import College_Hitter_Data, College_Pitcher_Data, College_IO
from Model.DBTypes import *
from Model.ModelDBTypes import *
from Model.Pro.Model.Player_Model import Recurrent_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.Constants import DRAFT_MEANS, NUM_LEVELS, model_db
from Model.EvalStats import getOutputHitterStats as getOutputStats

VARIANT_BATCH_SIZE = 10000

@dataclass
class ModelResults:
    combined_io : Combined_IO
    col_output : list[DB_Output_College_HitterAggregation] | list[DB_Output_College_PitcherAggregation] | None
    pro_war : list[DB_Output_PlayerWarAggregation]
    pro_stats : list[list[DB_Output_HitterStatsAggregation]] | list[list[DB_Output_PitcherStatsAggregation]] | None
    
@dataclass
class HitterOverrides:
    # Pro overrides
    pro_player       : list[DB_Model_Players]                | None = None
    pro_stats        : list[list[DB_Model_HitterStats]]      | None = None
    pro_monthly_wars : list[list[DB_Player_MonthlyWar]]      | None = None
    pro_level_stats  : list[list[DB_Model_HitterLevelStats]] | None = None
    pro_mlb_values   : list[list[DB_Model_HitterValue]]      | None = None
    pro_player_wars  : list[list[DB_Model_PlayerWar]]        | None = None
    # College overrides
    col_player    : list[DB_College_Player]                   | None = None
    col_stats     : list[list[DB_Model_College_HitterYear]]   | None = None
    col_pro_stats : list[DB_Model_College_HitterProStats]     | None = None

@dataclass
class PitcherOverrides:
    # Pro overrides
    pro_player       : list[DB_Model_Players]                | None = None
    pro_stats        : list[list[DB_Model_PitcherStats]]      | None = None
    pro_monthly_wars : list[list[DB_Player_MonthlyWar]]      | None = None
    pro_level_stats  : list[list[DB_Model_PitcherLevelStats]] | None = None
    pro_mlb_values   : list[list[DB_Model_PitcherValue]]      | None = None
    pro_player_wars  : list[list[DB_Model_PlayerWar]]        | None = None
    # College overrides
    col_player    : list[DB_College_Player]                   | None = None
    col_stats     : list[list[DB_Model_College_PitcherYear]]   | None = None
    col_pro_stats : list[DB_Model_College_PitcherProStats]     | None = None

class Test_Model_Runner:
    def __init__(self,
                data_prep : Combined_Data_Prep,
                model_dir : str,
                device : torch.device = torch.device("cpu")):

        
        self.model_dir = model_dir
        self.data_prep = data_prep
        self.device = device
        
    @torch.no_grad()
    def Run_Variants(self,
            mlbId : int | None,
            tbcId : int | None,
            modelId : int,
            is_hitter : bool,
            overrides : HitterOverrides | PitcherOverrides
            ) -> list[ModelResults]:

        # Validate Input
        if mlbId is None and tbcId is None:
            raise ValueError("At least one of mlbId / tbcId must be provided")
        if (is_hitter and not isinstance(overrides, HitterOverrides)) or (not is_hitter and not isinstance(overrides, PitcherOverrides)):
            raise ValueError(f"overrides was wrong type for isHitter={is_hitter}")
        
        # Variant count: every supplied override list must agree
        override_lens = {name: len(v) for name, v in {
            "pro_player": overrides.pro_player, "pro_stats": overrides.pro_stats, "pro_monthly_wars": overrides.pro_monthly_wars,
            "pro_level_stats": overrides.pro_level_stats, "pro_mlb_values": overrides.pro_mlb_values,
            "pro_player_wars": overrides.pro_player_wars, "col_player_over": overrides.col_player, "col_stats": overrides.col_stats,
            "col_pro_stats": overrides.col_pro_stats,
        }.items() if v is not None}
        if len(set(override_lens.values())) > 1:
            raise ValueError(f"All override lists must have the same number of variants, got {override_lens}")
        num_variants = next(iter(override_lens.values()), 1)

        # Retrieve Model
        pos_str = "hit" if is_hitter else "pit"
        model_cursor = model_db.cursor()
        model_name = DB_ModelId.Select_From_DB(model_cursor, "WHERE id=?", (modelId,))[0].modelName
        col_network = ColModel.LoadFromFile(self.model_dir + f"{model_name}_{pos_str}_col.json", self.data_prep.college_data_prep).to(self.device)
        pro_network = ProModel.LoadFromFile(self.model_dir + f"{model_name}_{pos_str}_pro.json", self.data_prep.pro_data_prep).to(self.device)
        
        col_network.eval()
        pro_network.eval()
        
        wba = DB_WarBucketAverages.Select_From_DB(model_cursor, "WHERE isHitter=?", (1 if is_hitter else 0,))[0]
        war_bucket_averages = torch.tensor([0, wba.war1, wba.war2, wba.war3, wba.war4, wba.war5, wba.war6]).to(self.device)
        
        # Get data
        base_pro = None if mlbId is None \
            else self._Load_Pro_Hitter(mlbId) if is_hitter \
            else self._Load_Pro_Pitcher(mlbId)
        base_col = None if tbcId is None\
            else self._Load_College_Hitter(tbcId) if is_hitter \
            else self._Load_College_Pitcher(tbcId)
        pro_valid = base_pro is not None
        col_valid = base_col is not None

        # Build data from overrides and determine model runs
        ios = [self._BuildVariants(is_hitter, i, base_pro, base_col, overrides) for i in range(num_variants)]

        model_runs = self._Runs_For_Player(mlbId, tbcId, modelId)
        num_runs = len(model_runs)

        dataset, _ = Create_Test_Train_Datasets(ios, is_hitter=is_hitter, device=self.device, eval_mode=True)
        batch_starts = range(0, num_variants, VARIANT_BATCH_SIZE)

        col_acc: list[torch.Tensor | None] = [None] * num_variants
        war_acc: list[torch.Tensor | None] = [None] * num_variants
        stat_acc: list[torch.Tensor | None] = [None] * num_variants

        for run in model_runs:
            with warnings.catch_warnings(action='ignore', category=FutureWarning):
                pro_network.load_state_dict(torch.load(f"{self.model_dir}pro_{model_name}_{run}_{pos_str}.pt", map_location=self.device))
                col_network.load_state_dict(torch.load(f"{self.model_dir}col_{model_name}_{run}_{pos_str}.pt", map_location=self.device))

            for start in batch_starts:
                end = min(start + VARIANT_BATCH_SIZE, num_variants)
                pro_input, _, _, col_input, _, _ = dataset.get_batch(slice(start, end))

                # College Model
                if col_valid:
                    if is_hitter:
                        draft, war, off, deff, pa, pos, i0 = col_network(*col_input)
                        col_values = (draft, war, off, deff, pa, pos)
                    else:
                        draft, war, pos, i0 = col_network(*col_input)
                        col_values = (draft, war, pos)
                else:
                    # Need to create an empty i0, as pro network expects the tensor even though
                    # in this scenario it will just overwrite the values
                    i0 = torch.zeros((pro_input[0].size(0), pro_network.GetInitStateSize())).to(self.device)
                
                # Pro Model
                if pro_valid:
                    pro_data, pro_length, pro_pt_lyg, player_demo, player_bios = pro_input
                    pro_war, _, _, pro_stats_out, pro_pos, _, pro_pt, _ = pro_network(
                        pro_data, pro_length, pro_pt_lyg, i0, player_demo, player_bios)
                for b, v in enumerate(range(start, end)):
                    io = ios[v]
                    if col_valid:
                        col_acc[v] = _accumulate(col_acc[v],
                            self._College_Values(is_hitter, b, io.college_io.length, col_values, war_bucket_averages))
                    if pro_valid:
                        L = io.pro_io.length
                        war_acc[v] = _accumulate(war_acc[v], self._Pro_War_Values(b, L, pro_war, war_bucket_averages))
                        stat_acc[v] = _accumulate(stat_acc[v], self._Pro_Stat_Values(is_hitter, b, L, pro_stats_out, pro_pt, pro_pos))

        return [
            ModelResults(
                combined_io=io,
                col_output=self._Build_College_Rows(is_hitter, io.college_io, col_acc[i], num_runs, modelId),
                pro_war=self._Build_Pro_War_Rows(is_hitter, io.pro_io, war_acc[i], num_runs, modelId),
                pro_stats=self._Build_Pro_Stat_Rows(is_hitter, io.pro_io, stat_acc[i], num_runs, modelId)
            )
            for i, io in enumerate(ios)
        ]
    
    def _BuildVariants(self, is_hitter : bool, i: int, base_pro : DB_Player, base_col : DB_College_Player, overrides : HitterOverrides | PitcherOverrides) -> Combined_IO:
        pd = None
        if base_pro is not None:
            pd = copy.copy(base_pro)   # shallow: we only swap list references
            if overrides.pro_player       is not None: pd.player       = overrides.pro_player[i]
            if overrides.pro_stats        is not None: pd.stats        = overrides.pro_stats[i]
            if overrides.pro_monthly_wars is not None: pd.monthly_wars = overrides.pro_monthly_wars[i]
            if overrides.pro_level_stats  is not None: pd.level_stats  = overrides.pro_level_stats[i]
            if overrides.pro_mlb_values   is not None: pd.mlb_values   = overrides.pro_mlb_values[i]
            if overrides.pro_player_wars  is not None: pd.player_wars  = overrides.pro_player_wars[i]
            if len(pd.stats) != len(pd.monthly_wars):
                raise ValueError(
                    f"Variant {i}: stats ({len(pd.stats)}) and monthly_wars ({len(pd.monthly_wars)}) must be "
                    "parallel lists; if overriding one, override the other to match")
        cd = None
        if base_col is not None:
            cd = copy.copy(base_col)
            if overrides.col_player    is not None: cd.player    = overrides.col_player[i]
            if overrides.col_stats     is not None: cd.stats     = overrides.col_stats[i]
            if overrides.col_pro_stats is not None: cd.pro_stats = overrides.col_pro_stats[i]
        
        if is_hitter:
            return self.data_prep.Generate_IO_Test_Hitter(pd, cd)
        else:
            return self.data_prep.Generate_IO_Test_Pitcher(pd, cd)
    
    def _Load_Pro_Hitter(self, mlbId: int) -> Pro_Hitter_Data:
        data = self.data_prep.pro_data_prep.Load_Hitter_Data(mlbId, use_cutoff=False, player=None)
        if len(data.stats) == 0:
            raise ValueError(f"mlbId={mlbId} exists but has no Model_HitterStats rows")
        return data
    
    def _Load_Pro_Pitcher(self, mlbId : int) -> Pro_Pitcher_Data:
        data = self.data_prep.pro_data_prep.Load_Pitcher_Data(mlbId, use_cutoff=False, player=None)
        if len(data.stats) == 0:
            raise ValueError(f"mlbId={mlbId} exists but has no Model_PitcherStats rows")
        return data

    def _Load_College_Hitter(self, tbcId: int) -> College_Hitter_Data:
        data = self.data_prep.college_data_prep.Load_Hitter_Data(tbcId, use_cutoff=False, player=None)
        if len(data.stats) == 0:
            raise ValueError(f"tbcId={tbcId} exists but has no Model_College_HitterYear rows")
        return data
    
    def _Load_College_Pitcher(self, tbcId: int) -> College_Pitcher_Data:
        data = self.data_prep.college_data_prep.Load_Pitcher_Data(tbcId, use_cutoff=False, player=None)
        if len(data.stats) == 0:
            raise ValueError(f"tbcId={tbcId} exists but has no Model_College_PitcherYear rows")
        return data
    
    def _Runs_For_Player(self, mlbId: int | None, tbcId: int | None, modelId : int) -> list[int]:
        if mlbId is not None:
            id_col, id_val = "mlbId", mlbId
        else:
            id_col, id_val = "tbcId", tbcId
        
        model_cursor = model_db.cursor()
        rows = model_cursor.execute(
            f"SELECT DISTINCT(modelRun) FROM PlayersInTrainingData "
            f"WHERE {id_col}=? AND modelId=? AND isHitter=1 AND isTrain=0 ORDER BY modelRun ASC",
            (id_val, modelId)).fetchall()
        
        if len(rows) > 0:
            return [r[0] for r in rows]
        
        model_runs = model_cursor.execute("SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE modelId=? ORDER BY ModelRun ASC", (modelId,)).fetchall()
        return [mr[0] for mr in model_runs]
    
    
    
    # Building tensors from output
    def _College_Values(self,
                is_hitter : bool,
                b : int,
                length: int,
                values : tuple[torch.Tensor, ...],
                war_bucket_averages : torch.Tensor) -> torch.Tensor | None:

        if length == 0:
            return None

        if is_hitter:
            draft, war, off, deff, pa, pos = values
            off   = F.softmax(off[b, :length],   dim=-1)
            deff  = F.softmax(deff[b, :length],  dim=-1)
            pa    = F.softmax(pa[b, :length],    dim=-1)
        else:
            draft, war, pos = values
            
        draft = F.softmax(draft[b, :length], dim=-1)
        war   = F.softmax(war[b, :length],   dim=-1)
        pos   = F.softmax(pos[b, :length],   dim=-1)

        draft_mean = draft @ DRAFT_MEANS.to(draft)
        war_mean   = war   @ war_bucket_averages.to(war)

        if is_hitter:
            return torch.cat((draft, draft_mean.unsqueeze(-1),
                        war,   war_mean.unsqueeze(-1),
                        off, deff, pa, pos), dim=-1)
        else:
            return torch.cat((draft, draft_mean.unsqueeze(-1),
                        war,   war_mean.unsqueeze(-1),
                        pos), dim=-1)
    
    def _Pro_War_Values(self, b : int, length: int, 
                pro_output_war: torch.Tensor,
                war_bucket_averages : torch.Tensor) -> torch.Tensor:
        war = F.softmax(pro_output_war[b, :length], dim=-1)
        war_mean = war @ war_bucket_averages.to(war)
    
        return torch.cat((war, war_mean.unsqueeze(-1)), dim=-1)
    
    def _Pro_Stat_Values(self, is_hitter : bool, b : int, length: int,
            stats: torch.Tensor, pt: torch.Tensor, pos: torch.Tensor) -> torch.Tensor:

        pro_prep = self.data_prep.pro_data_prep
        
        if is_hitter:
            stat_means, stat_devs = pro_prep.GetHitStatMeans(), pro_prep.GetHitStatDevs()
            pt_means,   pt_devs   = pro_prep.GetHitPtMeans(),   pro_prep.GetHitPtDevs()
        else:
            stat_means, stat_devs = pro_prep.GetPitStatMeans(), pro_prep.GetPitStatDevs()
            pt_means,   pt_devs   = pro_prep.GetPitPtMeans(),   pro_prep.GetPitPtDevs()
        num_stats = stat_means.size(0)
        num_positions = 9 if is_hitter else 2
        pt_count = 1 if is_hitter else 4

        assert stats.size(-1) == NUM_LEVELS * num_stats
        assert pt.size(-1)    == NUM_LEVELS * pt_count
        assert pos.size(-1)   == NUM_LEVELS * num_positions

        stats = (stats[b, :length].reshape(length, NUM_LEVELS, num_stats).cpu() * stat_devs) + stat_means
        pt    = (pt[b, :length].reshape(length, NUM_LEVELS, pt_count).cpu()            * pt_devs)   + pt_means
        pos   = F.softmax(pos[b, :length].reshape(length, NUM_LEVELS, num_positions).cpu(), dim=-1)

        return torch.cat((pt, stats, pos), dim=-1)
    
    
    # Building DB values
    def _Build_College_Rows(self, is_hitter : bool, col_io : College_IO | None, acc : torch.Tensor, num_runs : torch.Tensor, modelId : int) \
                -> list[DB_Output_College_HitterAggregation] | list[DB_Output_College_PitcherAggregation]:
        if acc is None:
            return []
        dates = col_io.dates.tolist()                     # [(TBCId, year), ...]
        
        if is_hitter:
            return [
                DB_Output_College_HitterAggregation((int(dates[t][0]), -modelId, int(dates[t][1]), *row))
                for t, row in enumerate((acc / num_runs).tolist())
            ]
        else:
            return [
                DB_Output_College_PitcherAggregation((int(dates[t][0]), -modelId, int(dates[t][1]), *row))
                for t, row in enumerate((acc / num_runs).tolist())
            ]
    
    def _Build_Pro_War_Rows(self, is_hitter : bool, pro_io, acc, num_runs, modelId) -> list[DB_Output_PlayerWarAggregation]:
        if acc is None:
            return []
        dates = pro_io.dates.tolist()                     # [(mlbId, year, month), ...]
        return [
            DB_Output_PlayerWarAggregation((int(dates[t][0]), -modelId, 1 if is_hitter else 0, int(dates[t][1]), int(dates[t][2]), *row))
            for t, row in enumerate((acc / num_runs).tolist())
        ]

    def _Build_Pro_Stat_Rows(self, is_hitter : bool, pro_io : Player_IO, acc : torch.Tensor, num_runs : torch.Tensor, modelId : int) \
                -> list[list[DB_Output_HitterStatsAggregation]] | list[list[DB_Output_PitcherStatsAggregation]] | None:
        if acc is None:
            return None
        dates = pro_io.dates.tolist()
        mean = (acc / num_runs).tolist()
        
        if is_hitter:
            return [
                [
                    DB_Output_HitterStatsAggregation((int(dates[t][0]), -modelId, int(dates[t][1]), int(dates[t][2]), lvl, *mean[t][lvl]))
                    for lvl in range(NUM_LEVELS)
                ]
                for t in range(len(mean))
            ]
        else:
            return [
                [
                    DB_Output_PitcherStatsAggregation((int(dates[t][0]), -modelId, int(dates[t][1]), int(dates[t][2]), lvl, *mean[t][lvl]))
                    for lvl in range(NUM_LEVELS)
                ]
                for t in range(len(mean))
            ]
    
def _accumulate(acc: torch.Tensor | None, val: torch.Tensor | None) -> torch.Tensor | None:
    if val is None:
        return acc
    return val.clone() if acc is None else acc + val