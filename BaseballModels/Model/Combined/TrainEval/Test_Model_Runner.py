from dataclasses import dataclass
import torch
import torch.nn.functional as F
import warnings
import copy

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
from Model.Pro.DataPrep.Data_Prep import Pro_Hitter_Data
from Model.College.DataPrep.Data_Prep import College_Hitter_Data
from Model.DBTypes import *
from Model.ModelDBTypes import *
from Model.Pro.Model.Player_Model import Recurrent_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.Constants import DRAFT_MEANS, NUM_LEVELS, TOTAL_WAR_BUCKETS, db, model_db
from Model.EvalStats import getOutputHitterStats as getOutputStats

VARIANT_BATCH_SIZE = 10000

@dataclass
class ModelResults:
    combined_io : Combined_IO
    col_output : list[DB_Output_College_HitterAggregation]
    pro_war : list[DB_Output_PlayerWarAggregation]
    hitter_stats : list[list[DB_Output_HitterStatsAggregation]] | None
    pitcher_stats : list[list[DB_Output_PitcherStatsAggregation]] | None
    

class Test_Model_Runner:
    def __init__(self,
                data_prep : Combined_Data_Prep,
                model_dir : str,
                is_hitter : bool,
                model_id : int = 1,
                device : torch.device = torch.device("cpu")):

        pos_str = "hit" if is_hitter else "pit"
        model_cursor = model_db.cursor()
        self.model_name = DB_ModelId.Select_From_DB(model_cursor, "WHERE id=?", (model_id,))[0].modelName
        self.model_dir = model_dir

        self.data_prep = data_prep
        self.col_network = ColModel.LoadFromFile(model_dir + f"{self.model_name}_{pos_str}_col.json", data_prep.college_data_prep)
        self.pro_network = ProModel.LoadFromFile(model_dir + f"{self.model_name}_{pos_str}_pro.json", data_prep.pro_data_prep)
        self.device = device

        # Used to collapse the WAR bucket distribution into a single expected value
        # TODO : This should live in the DB to quickly query
        self.war_bucket_averages = [0]
        cursor = db.cursor()
        for i in range(1, len(TOTAL_WAR_BUCKETS)):
            bucket_min = TOTAL_WAR_BUCKETS[i - 1].item()
            bucket_max = min(TOTAL_WAR_BUCKETS[i].item(), 100)
            self.war_bucket_averages.append(cursor.execute(f"SELECT AVG(warHitter) FROM Model_Players WHERE IsEligible=1 AND IsHitter=1 AND warHitter>{bucket_min} AND warHitter<={bucket_max}").fetchone()[0])

        self.war_bucket_values = torch.tensor(self.war_bucket_averages, dtype=torch.float32)  # index 0 is already 0
        self.draft_means       = DRAFT_MEANS

        self.model_id = model_id

        # Get model runs for model
        
        model_runs = model_cursor.execute("SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE modelId=? ORDER BY ModelRun ASC", (model_id,)).fetchall()
        self.model_runs = [mr[0] for mr in model_runs]

        self.col_network.to(device)
        self.pro_network.to(device)
        
    # Wrapper of Run_Hitter_Variants for a single hitter
    def Run_Single_Hitter(self,
            mlbId : int | None,
            tbcId : int | None,
            *,
            # Pro overrides — anything given replaces what was loaded from the DB
            pro_player       : DB_Model_Players                | None = None,
            pro_stats        : list[DB_Model_HitterStats]      | None = None,
            pro_monthly_wars : list[DB_Player_MonthlyWar]      | None = None,
            pro_level_stats  : list[DB_Model_HitterLevelStats] | None = None,
            pro_mlb_values   : list[DB_Model_HitterValue]      | None = None,
            pro_player_wars  : list[DB_Model_PlayerWar]        | None = None,
            # College overrides
            col_player_over  : DB_College_Player                 | None = None,
            col_stats        : list[DB_Model_College_HitterYear] | None = None,
            col_pro_stats : list[DB_Model_College_HitterProStats]     | None = None,
            ) -> ModelResults:
        one = lambda x: None if x is None else [x]
        return self.Run_Hitter_Variants(mlbId, tbcId,
            pro_player=one(pro_player), pro_stats=one(pro_stats), pro_monthly_wars=one(pro_monthly_wars),
            pro_level_stats=one(pro_level_stats), pro_mlb_values=one(pro_mlb_values),
            pro_player_wars=one(pro_player_wars), col_player=one(col_player_over), col_stats=one(col_stats),
            col_pro_stats=one(col_pro_stats)
        )[0]
        
    @torch.no_grad()
    def Run_Hitter_Variants(self,
            mlbId : int | None,
            tbcId : int | None,
            *,
            # Pro overrides — each list has one entry per variant
            pro_player       : list[DB_Model_Players]                | None = None,
            pro_stats        : list[list[DB_Model_HitterStats]]      | None = None,
            pro_monthly_wars : list[list[DB_Player_MonthlyWar]]      | None = None,
            pro_level_stats  : list[list[DB_Model_HitterLevelStats]] | None = None,
            pro_mlb_values   : list[list[DB_Model_HitterValue]]      | None = None,
            pro_player_wars  : list[list[DB_Model_PlayerWar]]        | None = None,
            # College overrides
            col_player    : list[DB_College_Player]                   | None = None,
            col_stats     : list[list[DB_Model_College_HitterYear]]   | None = None,
            col_pro_stats : list[DB_Model_College_HitterProStats]     | None = None,
            ) -> list[ModelResults]:

        self.pro_network.eval()
        self.col_network.eval()

        # Validate Input
        if mlbId is None and tbcId is None:
            raise ValueError("At least one of mlbId / tbcId must be provided")
        # Variant count: every supplied override list must agree
        override_lens = {name: len(v) for name, v in {
            "pro_player": pro_player, "pro_stats": pro_stats, "pro_monthly_wars": pro_monthly_wars,
            "pro_level_stats": pro_level_stats, "pro_mlb_values": pro_mlb_values,
            "pro_player_wars": pro_player_wars, "col_player_over": col_player, "col_stats": col_stats,
            "col_pro_stats": col_pro_stats,
        }.items() if v is not None}
        if len(set(override_lens.values())) > 1:
            raise ValueError(f"All override lists must have the same number of variants, got {override_lens}")
        num_variants = next(iter(override_lens.values()), 1)

        
        # Apply overrides
        base_pro = self._Load_Pro(mlbId) if mlbId is not None else None
        base_col = self._Load_College(tbcId) if tbcId is not None else None
        pro_valid = base_pro is not None
        col_valid = base_col is not None

        def build_variant(i: int) -> Combined_IO:
            pd = None
            if base_pro is not None:
                pd = copy.copy(base_pro)   # shallow: we only swap list references
                if pro_player       is not None: pd.player       = pro_player[i]
                if pro_stats        is not None: pd.stats        = pro_stats[i]
                if pro_monthly_wars is not None: pd.monthly_wars = pro_monthly_wars[i]
                if pro_level_stats  is not None: pd.level_stats  = pro_level_stats[i]
                if pro_mlb_values   is not None: pd.mlb_values   = pro_mlb_values[i]
                if pro_player_wars  is not None: pd.player_wars  = pro_player_wars[i]
                if len(pd.stats) != len(pd.monthly_wars):
                    raise ValueError(
                        f"Variant {i}: stats ({len(pd.stats)}) and monthly_wars ({len(pd.monthly_wars)}) must be "
                        "parallel lists; if overriding one, override the other to match")
            cd = None
            if base_col is not None:
                cd = copy.copy(base_col)
                if col_player    is not None: cd.player    = col_player[i]
                if col_stats     is not None: cd.stats     = col_stats[i]
                if col_pro_stats is not None: cd.pro_stats = col_pro_stats[i]
            return self.data_prep.Generate_IO_Test_Hitter(pd, cd)

        # Build data and determine model runs
        ios = [build_variant(i) for i in range(num_variants)]

        model_runs = self._Runs_For_Player(mlbId, tbcId)
        num_runs = len(model_runs)

        dataset, _ = Create_Test_Train_Datasets(ios, is_hitter=True, device=self.device, eval_mode=True)
        batch_starts = range(0, num_variants, VARIANT_BATCH_SIZE)

        col_acc: list[torch.Tensor | None] = [None] * num_variants
        war_acc: list[torch.Tensor | None] = [None] * num_variants
        hit_acc: list[torch.Tensor | None] = [None] * num_variants

        for run in model_runs:
            with warnings.catch_warnings(action='ignore', category=FutureWarning):
                self.pro_network.load_state_dict(torch.load(f"{self.model_dir}pro_{self.model_name}_{run}_hit.pt", map_location=self.device))
                self.col_network.load_state_dict(torch.load(f"{self.model_dir}col_{self.model_name}_{run}_hit.pt", map_location=self.device))

            for start in batch_starts:
                end = min(start + VARIANT_BATCH_SIZE, num_variants)
                pro_input, _, _, col_input, _, _ = dataset.get_batch(slice(start, end))

                # College Model
                if col_valid:
                    draft, war, off, deff, pa, pos, i0 = self.col_network(*col_input)
                else:
                    # Need to create an empty i0, as pro network expects the tensor even though
                    # in this scenario it will just overwrite the values
                    i0 = torch.zeros((pro_input[0].size(0), self.pro_network.GetInitStateSize())).to(self.device)
                
                # Pro Model
                if pro_valid:
                    pro_data, pro_length, pro_pt_lyg, player_demo, player_bios = pro_input
                    pro_war, _, _, pro_stats_out, pro_pos, _, pro_pt, _ = self.pro_network(
                        pro_data, pro_length, pro_pt_lyg, i0, player_demo, player_bios)
                for b, v in enumerate(range(start, end)):
                    io = ios[v]
                    if col_valid:
                        col_acc[v] = _accumulate(col_acc[v],
                            self._College_Values(b, io.college_io.length, draft, war, off, deff, pa, pos))
                    if pro_valid:
                        L = io.pro_io.length
                        war_acc[v] = _accumulate(war_acc[v], self._Pro_War_Values(b, L, pro_war))
                        hit_acc[v] = _accumulate(hit_acc[v], self._Pro_HitterStat_Values(b, L, pro_stats_out, pro_pt, pro_pos))

        return [
            ModelResults(
                combined_io=io,
                col_output=self._Build_College_Rows(io.college_io, col_acc[i], num_runs),
                pro_war=self._Build_Pro_War_Rows(io.pro_io, war_acc[i], num_runs),
                hitter_stats=self._Build_Hitter_Stat_Rows(io.pro_io, hit_acc[i], num_runs),
                pitcher_stats=None)
            for i, io in enumerate(ios)
        ]
    
    def _Load_Pro(self, mlbId: int) -> Pro_Hitter_Data:
        # Load_Hitter_Data already raises if the Model_Players row is missing
        data = self.data_prep.pro_data_prep.Load_Hitter_Data(mlbId, use_cutoff=False)
        if len(data.stats) == 0:
            raise ValueError(f"mlbId={mlbId} exists but has no Model_HitterStats rows")
        return data

    def _Load_College(self, tbcId: int) -> College_Hitter_Data:
        data = self.data_prep.college_data_prep.Load_Hitter_Data(tbcId, use_cutoff=False)
        if len(data.stats) == 0:
            raise ValueError(f"tbcId={tbcId} exists but has no Model_College_HitterYear rows")
        return data
    
    def _Runs_For_Player(self, mlbId: int | None, tbcId: int | None) -> list[int]:
        if mlbId is not None:
            id_col, id_val = "mlbId", mlbId
        else:
            id_col, id_val = "tbcId", tbcId
        rows = model_db.cursor().execute(
            f"SELECT DISTINCT(modelRun) FROM PlayersInTrainingData "
            f"WHERE {id_col}=? AND modelId=? AND isHitter=1 AND isTrain=0 ORDER BY modelRun ASC",
            (id_val, self.model_id)).fetchall()
        return [r[0] for r in rows] or self.model_runs
    
    
    
    # Building tensors from output
    def _College_Values(self,
                b : int,
                length: int,
                draft : torch.Tensor, 
                war : torch.Tensor, 
                off : torch.Tensor, 
                deff : torch.Tensor, 
                pa : torch.Tensor, 
                pos : torch.Tensor) -> torch.Tensor | None:

        if length == 0:
            return None

        draft = F.softmax(draft[b, :length], dim=-1)
        war   = F.softmax(war[b, :length],   dim=-1)
        off   = F.softmax(off[b, :length],   dim=-1)
        deff  = F.softmax(deff[b, :length],  dim=-1)
        pa    = F.softmax(pa[b, :length],    dim=-1)
        pos   = F.softmax(pos[b, :length],   dim=-1)

        draft_mean = draft @ self.draft_means.to(draft)
        war_mean   = war   @ self.war_bucket_values.to(war)

        return torch.cat((draft, draft_mean.unsqueeze(-1),
                      war,   war_mean.unsqueeze(-1),
                      off, deff, pa, pos), dim=-1)
    
    def _Pro_War_Values(self, b : int, length: int, pro_output_war: torch.Tensor) -> torch.Tensor:
        war = F.softmax(pro_output_war[b, :length], dim=-1)
        war_mean = war @ self.war_bucket_values.to(war)
    
        return torch.cat((war, war_mean.unsqueeze(-1)), dim=-1)
    
    def _Pro_HitterStat_Values(self, b : int, length: int,
            stats: torch.Tensor, pt: torch.Tensor, pos: torch.Tensor) -> torch.Tensor:

        pro_prep = self.data_prep.pro_data_prep
        stat_means, stat_devs = pro_prep.GetHitStatMeans(), pro_prep.GetHitStatDevs()
        pt_means,   pt_devs   = pro_prep.GetHitPtMeans(),   pro_prep.GetHitPtDevs()
        num_stats = stat_means.size(0)

        assert stats.size(-1) == NUM_LEVELS * num_stats
        assert pt.size(-1)    == NUM_LEVELS
        assert pos.size(-1)   == NUM_LEVELS * 9

        stats = (stats[b, :length].reshape(length, NUM_LEVELS, num_stats).cpu() * stat_devs) + stat_means
        pt    = (pt[b, :length].reshape(length, NUM_LEVELS, 1).cpu()            * pt_devs)   + pt_means.unsqueeze(-1)
        pos   = F.softmax(pos[b, :length].reshape(length, NUM_LEVELS, 9).cpu(), dim=-1)

        return torch.cat((pt, stats, pos), dim=-1)
    
    
    # Building DB values
    def _Build_College_Rows(self, col_io, acc, num_runs) -> list[DB_Output_College_HitterAggregation]:
            if acc is None:
                return []
            dates = col_io.dates.tolist()                     # [(TBCId, year), ...]
            return [
                DB_Output_College_HitterAggregation((int(dates[t][0]), -self.model_id, int(dates[t][1]), *row))
                for t, row in enumerate((acc / num_runs).tolist())
            ]
    
    def _Build_Pro_War_Rows(self, pro_io, acc, num_runs) -> list[DB_Output_PlayerWarAggregation]:
        if acc is None:
            return []
        dates = pro_io.dates.tolist()                     # [(mlbId, year, month), ...]
        return [
            DB_Output_PlayerWarAggregation((int(dates[t][0]), -self.model_id, 1, int(dates[t][1]), int(dates[t][2]), *row))
            for t, row in enumerate((acc / num_runs).tolist())
        ]

    def _Build_Hitter_Stat_Rows(self, pro_io, acc, num_runs) -> list[list[DB_Output_HitterStatsAggregation]] | None:
        if acc is None:
            return None
        dates = pro_io.dates.tolist()
        mean = (acc / num_runs).tolist()                  # [L][NUM_LEVELS][1+S+9]
        return [
            [
                DB_Output_HitterStatsAggregation((int(dates[t][0]), -self.model_id, int(dates[t][1]), int(dates[t][2]), lvl, *mean[t][lvl]))
                for lvl in range(NUM_LEVELS)
            ]
            for t in range(len(mean))
        ]
    
def _accumulate(acc: torch.Tensor | None, val: torch.Tensor | None) -> torch.Tensor | None:
    if val is None:
        return acc
    return val.clone() if acc is None else acc + val