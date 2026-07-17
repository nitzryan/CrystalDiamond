import torch.nn as nn
import torch
import torch.nn.functional as F
import warnings

from Model.Combined.DataPrep.Data_Prep import Combined_Data_Prep
from Model.DBTypes import *
from Model.ModelDBTypes import *
from Model.Constants import DRAFT_MEANS, TOTAL_WAR_BUCKETS, db, model_db

class Test_Model_Runner:
    def __init__(self,
                data_prep : Combined_Data_Prep,
                col_network : nn.Module,
                pro_network : nn.Module,
                model_id : int = 1,
                device : torch.device = torch.device("cpu")):

        self.data_prep = data_prep
        self.col_network = col_network
        self.pro_network = pro_network
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
        model_cursor = model_db.cursor()
        model_runs = model_cursor.execute("SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE modelIdx=? ORDER BY ModelRun ASC", (model_id,)).fetchall()
        self.model_runs = [mr[0] for mr in model_runs]
        
        self.model_name = DB_ModelIdx.Select_From_DB(model_cursor, "WHERE id=?", (model_id,))[0].modelName

        self.col_network.to(device)
        self.pro_network.to(device)
        
    @torch.no_grad()
    def Run_Single_Hitter(self,
            pro_player : DB_Model_Players | None,
            pro_stats : list[DB_Model_HitterStats] | None,
            pro_month_war : list[DB_Player_MonthlyWar] | None,

            col_player : DB_College_Player | None,
            col_stats : list[DB_Model_College_HitterYear] | None
            ) -> tuple[list[DB_Output_College_HitterAggregation], list[DB_Output_PlayerWarAggregation]]:

        self.col_network.eval()
        self.pro_network.eval()
        
        # Get model runs where player is not in training
        model_cursor = model_db.cursor()
        if pro_player is not None:
            model_runs = [x[0] for x in model_cursor.execute(f"SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE mlbId=? AND modelIdx=? AND isHitter=1 ORDER BY modelRun ASC", (pro_player.mlbId, self.model_id)).fetchall()]
        elif col_player is not None:
            model_runs = [x[0] for x in model_cursor.execute(f"SELECT DISTINCT(modelRun) FROM PlayersInTrainingData WHERE tbcId=? AND modelIdx=? AND isHitter=1 ORDER BY modelRun ASC", (col_player.TBCId, self.model_id)).fetchall()]
        else:
            raise Exception("Expected at least 1 of Pro and College Player to not be None")
        
        # If not in PITD, run on all models
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
        
        pro_results_list = [DB_Output_PlayerWarAggregation((
                pro_stats[0].mlbId,
                self.model_id,
                1,
                0,
                0,
                0,0,0,0,0,0,0,0
            ))] + [DB_Output_PlayerWarAggregation(
            (
            ps.mlbId,
            self.model_id,
            1,
            ps.Year,
            ps.Month,
            0,0,0,0,0,0,0,0 # War
            )
        ) for ps in pro_stats] if pro_stats is not None else [] 
        
        for run in model_runs:
            # Load Models
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                self.pro_network.load_state_dict(torch.load(f"../Models/pro_{self.model_name}_{run}_hit.pt"))
                self.col_network.load_state_dict(torch.load(f"../Models/col_{self.model_name}_{run}_hit.pt"))
            
            # College Model
            col_data = col_io.input.unsqueeze(0).to(self.device)
            col_length = torch.tensor([col_io.length])

            col_output_draft, col_output_war, col_output_off, col_output_def, col_output_pa, col_output_pos, h0 = \
                self.col_network(col_data, col_length)

            h0 = h0.transpose(0, 1).contiguous()

            college_results = self.__Build_College_Outputs(
                col_io, col_length,
                col_output_draft, col_output_war, col_output_off, col_output_def, col_output_pa, col_output_pos)

            # Pro Model
            pro_results : list[DB_Output_PlayerWar] = []
            pro_valid = all(v is not None for v in
                            (pro_player, pro_stats, pro_month_war))
            if pro_valid:
                pro_data = pro_io.input.unsqueeze(0).to(self.device)                  # (1, L, F)
                pro_length = torch.tensor([pro_io.length])                           # CPU
                pro_pt_levelYearGames = pro_io.pt_levelYearGames.unsqueeze(0).to(self.device)

                pro_output_war, pro_output_level, pro_output_pa, pro_output_stats, \
                pro_output_pos, pro_output_mlbValue, pro_output_pt, pro_output_mlbstat = \
                    self.pro_network(pro_data, pro_length, pro_pt_levelYearGames, h0)

                pro_results = self.__Build_Pro_Outputs(pro_io, pro_output_war)

            # Update Results Aggregation
            if col_player is not None:
                for time_step, col_result in enumerate(college_results):
                    _Update_College_Aggregation(col_results_list[time_step], col_result, 1 / num_runs)
    
            if pro_player is not None:
                for time_step, pro_result in enumerate(pro_results):
                    _Update_PlayerWar_Aggregation(pro_results_list[time_step], pro_result, 1 / num_runs)
                    
        return col_results_list, pro_results_list
    
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
        war = F.softmax(pro_output_war, dim=2)   # (1, L, 7)
        L = war.size(1)
        dtype = war.dtype

        warMean = torch.zeros(war.size(0), L, device=war.device)
        for i in range(1, len(self.war_bucket_averages)):
            warMean[:, :] += war[:, :, i] * self.war_bucket_averages[i]

        dates = pro_io.dates.to(war.device).unsqueeze(0)[:, :L, :].to(dtype)   # (1, L, 3) = (mlbId, year, month)
        mlbIds = dates[:, :, 0].unsqueeze(2)
        model_idxs = torch.zeros_like(mlbIds)
        year_month = dates[:, :, 1:]

        opw = torch.cat((mlbIds, model_idxs, year_month, war, warMean.unsqueeze(-1)), dim=2)
        db_data = torch.nn.utils.rnn.unpad_sequence(opw, torch.tensor([pro_io.length]), batch_first=True)

        results : list[DB_Output_PlayerWar] = []
        for d in db_data:
            for row in d.tolist():
                # (mlbId, model, isHitter, modelIdx, year, month, war0..war6, war)
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