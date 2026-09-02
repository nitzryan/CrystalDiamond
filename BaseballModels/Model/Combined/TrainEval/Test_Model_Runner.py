from dataclasses import dataclass
import torch
import torch.nn.functional as F
import warnings

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
from Model.DBTypes import *
from Model.ModelDBTypes import *
from Model.Pro.Model.Player_Model import Recurrent_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.Constants import DRAFT_MEANS, TOTAL_WAR_BUCKETS, db, model_db
from Model.EvalStats import getOutputHitterStats as getOutputStats

@dataclass
class ModelResults:
    combined_io : Combined_IO
    col_output : list[DB_Output_College_HitterAggregation]
    pro_war : list[DB_Output_PlayerWarAggregation]
    # hitter_stats : list[list[DB_Output_HitterStatsAggregation]] | None
    # pitcher_stats : list[list[DB_Output_PitcherStatsAggregation]] | None
    

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

        self.model_id = model_id

        # Get model runs for model
        
        model_runs = model_cursor.execute("SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE modelId=? ORDER BY ModelRun ASC", (model_id,)).fetchall()
        self.model_runs = [mr[0] for mr in model_runs]

        self.col_network.to(device)
        self.pro_network.to(device)
        
    @torch.no_grad()
    def Run_Single_Hitter(self,
            pro_player : DB_Model_Players | None,
            pro_stats : list[DB_Model_HitterStats] | None,
            pro_month_war : list[DB_Player_MonthlyWar] | None,

            col_player : DB_College_Player | None,
            col_stats : list[DB_Model_College_HitterYear] | None
            ) -> ModelResults:

        self.col_network.eval()
        self.pro_network.eval()
        
        
        # Ensure player has either college or pro data
        pro_valid = pro_player is not None and pro_stats is not None and pro_month_war is not None
        col_valid = col_player is not None and col_stats is not None and len(col_stats) > 0
        if not pro_valid and not col_valid:
            raise Exception("Expected at least 1 of Pro (player, stats, monthly war) and College (player, stats) to be fully provided")

        # Get test runs for the player, if they exist
        model_cursor = model_db.cursor()
        train_runs : set[int] = set()
        if pro_player is not None:
            id_col, id_val = "mlbId", pro_player.mlbId
        else:
            id_col, id_val = "tbcId", col_player.TBCId
        model_runs = [x[0] for x in model_cursor.execute(
            f"SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE {id_col}=? AND modelId=? AND isHitter=1 AND isTrain=0 ORDER BY modelRun ASC",
            (id_val, self.model_id)).fetchall()]

        # Player wasn't in any test runs, therefore they aren't in any train runs so should be evaluated on all runs
        if len(model_runs) == 0:
            model_runs = self.model_runs
        num_runs = len(model_runs)

        # Generate Data
        combined_io = self.data_prep.Generate_IO_Test_Hitter(
            pro_player, pro_stats, pro_month_war,
            col_player, col_stats)
        pro_io = combined_io.pro_io
        col_io = combined_io.college_io

        # Empty output lists that will be built run by run
        col_results_list = [DB_Output_College_HitterAggregation(
            (
            cs.TBCId,
            self.model_id,
            cs.Year,
            0,0,0,0,0,0,0,0, # Draft
            0,0,0,0,0,0,0,0, # War
            0,0,0,0,0,0,0,0, # Off
            0,0,0,0,0,0,0,0, # Def
            0,0,0,0,0,0,0,0, # Pa
            0,0,0,0,0,0,0,0 # Position
            )
        ) for cs in col_stats] if col_stats is not None else []
        
        pro_results_list : list[DB_Output_PlayerWarAggregation] = []
        if pro_valid:
            pro_results_list = [DB_Output_PlayerWarAggregation((
                    pro_player.mlbId,
                    self.model_id,
                    1,
                    0,
                    0,
                    0,0,0,0,0,0,0,0
                ))] + [DB_Output_PlayerWarAggregation(
                (
                pro_player.mlbId,
                self.model_id,
                1,
                ps.Year,
                ps.Month,
                0,0,0,0,0,0,0,0 # War
                )
            ) for ps in pro_stats]
        
        # Generate input tensors
        col_data = col_io.input.unsqueeze(0).to(self.device)
        col_length_cpu = torch.tensor([col_io.length], dtype=torch.long)
        col_length = col_length_cpu.to(self.device)
        if col_data.shape[1] == 0:
            # Create a 0-padded input tensor, will get masked out
            col_data = torch.zeros(1, 1, col_data.shape[2], dtype=col_data.dtype, device=self.device)

        if pro_valid:
            pro_data = pro_io.input.unsqueeze(0).to(self.device)
            pro_length = torch.tensor([pro_io.length], dtype=torch.long).to(self.device)
            pro_pt_levelYearGames = pro_io.pt_levelYearGames.unsqueeze(0).to(self.device)
            player_demo = torch.tensor([pro_io.player_demo], dtype=torch.long).to(self.device)
            player_bios = pro_io.player_bio.reshape(1, -1).to(self.device)
        
        for run in model_runs:
            # Load Models
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                self.pro_network.load_state_dict(torch.load(f"{self.model_dir}pro_{self.model_name}_{run}_hit.pt", map_location=self.device))
                self.col_network.load_state_dict(torch.load(f"{self.model_dir}col_{self.model_name}_{run}_hit.pt", map_location=self.device))
            self.pro_network.eval()
            self.col_network.eval()

            # College Model
            col_output_draft, col_output_war, col_output_off, col_output_def, col_output_pa, col_output_pos, i0 = \
                self.col_network(col_data, col_length)

            college_results = self.__Build_College_Outputs(
                col_io, col_length,
                col_output_draft, col_output_war, col_output_off, col_output_def, col_output_pa, col_output_pos)

            # Pro Model
            pro_results : list[DB_Output_PlayerWar] = []
            pro_hitter_stats : list[list[DB_Output_HitterStats]] = []
            if pro_valid:
                pro_output_war, pro_output_level, pro_output_pa, pro_output_stats, \
                pro_output_pos, pro_output_mlbValue, pro_output_pt, pro_output_mlbstat = \
                    self.pro_network(pro_data, pro_length, pro_pt_levelYearGames, i0, player_demo, player_bios)

                pro_results = self.__Build_Pro_Outputs(pro_io, pro_output_war)

            # Update Results Aggregation
            if col_player is not None:
                for time_step, col_result in enumerate(college_results):
                    _Update_College_Aggregation(col_results_list[time_step], col_result, 1 / num_runs)
    
            if pro_valid:
                for time_step, pro_result in enumerate(pro_results):
                    _Update_PlayerWar_Aggregation(pro_results_list[time_step], pro_result, 1 / num_runs)
                    
        return ModelResults(
            combined_io=combined_io,
            col_output=college_results,
            pro_war=pro_results,
        )
    
    def __Build_College_Outputs(self, col_io, col_length,
            draft, war, off, deff, pa, pos) -> list[DB_Output_College_Hitter]:

        mask = (col_length > 0)
        dmask = mask.to(draft.device)

        draft = F.softmax(draft[dmask], dim=-1)
        war   = F.softmax(war[dmask], dim=-1)
        off   = F.softmax(off[dmask], dim=-1)
        deff  = F.softmax(deff[dmask], dim=-1)
        pa    = F.softmax(pa[dmask], dim=-1)
        pos   = F.softmax(pos[dmask], dim=-1)

        if draft.size(0) == 0:
            return []

        L = draft.size(1)
        dtype = draft.dtype

        draftMean = torch.zeros(draft.size(0), L, device=draft.device)
        for i in range(len(DRAFT_MEANS)):
            draftMean[:, :] += draft[:, :, i] * DRAFT_MEANS[i]

        warMean = torch.zeros(war.size(0), L, device=war.device)
        for i in range(1, len(self.war_bucket_averages)):
            warMean[:, :] += war[:, :, i] * self.war_bucket_averages[i]

        dates = col_io.dates.to(draft.device).unsqueeze(0)[:, :L, :].to(dtype)   # (1, L, 2) = (id, year)
        ids = dates[:, :, 0].unsqueeze(2)
        years = dates[:, :, 1].unsqueeze(2)
        model_idxs = torch.zeros_like(years)

        db_input = torch.cat((ids, model_idxs, years,
                              draft, draftMean.unsqueeze(-1),
                              war, warMean.unsqueeze(-1),
                              off, deff, pa, pos), dim=2)
        db_input = torch.nn.utils.rnn.unpad_sequence(db_input, col_length[mask], batch_first=True)

        results : list[DB_Output_College_Hitter] = []
        for d in db_input:
            for row in d.tolist():
                # (id, model, modelIdx, year, ...buckets...)
                values = (int(row[0]), self.model_id, int(row[1]), int(row[2]), *row[3:])
                results.append(DB_Output_College_Hitter(values))
        return results
    
    def __Build_Pro_Outputs(self, pro_io, pro_output_war) -> list[DB_Output_PlayerWar]:
        war = F.softmax(pro_output_war, dim=2)
        L = war.size(1)
        dtype = war.dtype

        warMean = torch.zeros(war.size(0), L, device=war.device)
        for i in range(1, len(self.war_bucket_averages)):
            warMean[:, :] += war[:, :, i] * self.war_bucket_averages[i]

        dates = pro_io.dates.to(war.device).unsqueeze(0)[:, :L, :].to(dtype)
        mlbIds = dates[:, :, 0].unsqueeze(2)
        model_idxs = torch.zeros_like(mlbIds)
        year_month = dates[:, :, 1:]

        opw = torch.cat((mlbIds, model_idxs, year_month, war, warMean.unsqueeze(-1)), dim=2)
        db_data = torch.nn.utils.rnn.unpad_sequence(opw, torch.tensor([pro_io.length]), batch_first=True)

        results : list[DB_Output_PlayerWar] = []
        for d in db_data:
            for row in d.tolist():
                values = (int(row[0]), self.model_id, 1, int(row[1]), int(row[2]), int(row[3]), *row[4:])
                results.append(DB_Output_PlayerWar(values))
        return results
    
def _Update_College_Aggregation(
    agg: DB_Output_College_HitterAggregation,
    non_agg: DB_Output_College_Hitter,
    weight: float
) -> None:
    fields_to_aggregate = [
        'draft0', 'draft1', 'draft2', 'draft3', 'draft4', 'draft5', 'draft6', 'draft',
        'war0', 'war1', 'war2', 'war3', 'war4', 'war5', 'war6', 'war',
        'off0', 'off1', 'off2', 'off3', 'off4', 'off5', 'off6', 'offNone',
        'def0', 'def1', 'def2', 'def3', 'def4', 'def5', 'def6', 'defNone',
        'pa0', 'pa1', 'pa2', 'pa3', 'pa4', 'pa5', 'pa6',
        'ProbC', 'Prob1B', 'Prob2B', 'Prob3B', 'ProbSS', 'ProbLF', 'ProbCF', 'ProbRF', 'ProbDH'
    ]

    for field in fields_to_aggregate:
        current = getattr(agg, field)
        addition = getattr(non_agg, field) * weight
        setattr(agg, field, current + addition)
        
def _Update_PlayerWar_Aggregation(
    agg: "DB_Output_PlayerWarAggregation",
    non_agg: "DB_Output_PlayerWar",
    weight: float
) -> None:
    fields_to_aggregate = [
        'war0', 'war1', 'war2', 'war3', 'war4', 'war5', 'war6', 'war'
    ]

    for field in fields_to_aggregate:
        current = getattr(agg, field)
        addition = getattr(non_agg, field) * weight
        setattr(agg, field, current + addition)