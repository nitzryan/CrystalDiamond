import torch
import torch.nn as nn
import torch.nn.functional as F
from Stuff.DataPrep.DataPrep import DataPrep
from Buckets import *
from DBTypes import *
from PitchDBTypes import *
from Stuff.Model.PitcherPredLayers import PitcherPredLayers
from Stuff.Model.Utilities import *
from Constants import pitch_db
import warnings
        
def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters
        
class PitchModel(nn.Module):
    def __init__(self,
                data_prep : DataPrep,
                
                loss_backprop_weights : list[float] = [1, 1, 0.1, 1, 1, 0.1, 1, 1, 0.1],
                
                
                ########## PRED BLOCKS ##########
                ## Combined ##
                combined_pred_size_result : int = 64,
                combined_pred_blocks_result : int = 8,
                combined_pred_dropout_result : float = 0.1,
                combined_result_weight_decay : float = 1e-4,
                
                combined_pred_size_swing : int = 64,
                combined_pred_blocks_swing : int = 2,
                combined_pred_dropout_swing : float = 0.1,
                combined_swing_weight_decay : float = 1e-4,
                
                combined_pred_size_inplay : int = 64,
                combined_pred_blocks_inplay : int = 8,
                combined_pred_dropout_inplay : float = 0.3,
                combined_inplay_weight_decay : float = 1e-4,
                
                ## Location ##
                location_pred_size_result : int = 128,
                location_pred_blocks_result : int = 8,
                location_pred_dropout_result = 0.1,
                location_result_weight_decay : float = 1e-4,
                
                location_pred_size_swing : int = 128,
                location_pred_blocks_swing : int = 4,
                location_pred_dropout_swing : float = 0.1,
                location_swing_weight_decay : float = 1e-4,
                
                location_pred_size_inplay : int = 64,
                location_pred_blocks_inplay : int = 8,
                location_pred_dropout_inplay : float = 0.3,
                location_inplay_weight_decay : float = 1e-4,
                
                ## Stuff ##
                stuff_pred_size_result : int = 64,
                stuff_pred_blocks_result : int = 2,
                stuff_pred_dropout_result : float = 0.3,
                stuff_result_weight_decay : float = 1e-4,
                
                stuff_pred_size_swing : int = 64,
                stuff_pred_blocks_swing : int = 2,
                stuff_pred_dropout_swing : float = 0.1,
                stuff_swing_weight_decay : float = 1e-4,
                
                stuff_pred_size_inplay : int = 64,
                stuff_pred_blocks_inplay : int = 8,
                stuff_pred_dropout_inplay : float = 0.3,
                stuff_inplay_weight_decay : float = 1e-4,
    ):
        super().__init__()
        
        self.data_prep = data_prep
        prep_map = data_prep.prep_map
        self.loss_backprop_weights = loss_backprop_weights
        self.nonlin = F.leaky_relu
        
        # Prediction layers for Location only
        self.location_pred = PitcherPredLayers(
            input_size=prep_map.pitch_loc_size + prep_map.hitter_zone_size + prep_map.pitch_overview_size,
            
            block_size_result=location_pred_size_result,
            num_layers_result=location_pred_blocks_result,
            dropout_result=location_pred_dropout_result,
            
            block_size_swing=location_pred_size_swing,
            num_layers_swing=location_pred_blocks_swing,
            dropout_swing=location_pred_dropout_swing,
            
            block_size_inplay=location_pred_size_inplay,
            num_layers_inplay=location_pred_blocks_inplay,
            dropout_inplay=location_pred_dropout_inplay,
        )
            
        # Prediction layers for stuff only
        self.stuff_pred = PitcherPredLayers(
            input_size=prep_map.pitch_stuff_size + prep_map.pitch_overview_size + prep_map.league_baseline_size,
            
            block_size_result=stuff_pred_size_result,
            num_layers_result=stuff_pred_blocks_result,
            dropout_result=stuff_pred_dropout_result,
            
            block_size_swing=stuff_pred_size_swing,
            num_layers_swing=stuff_pred_blocks_swing,
            dropout_swing=stuff_pred_dropout_swing,
            
            block_size_inplay=stuff_pred_size_inplay,
            num_layers_inplay=stuff_pred_blocks_inplay,
            dropout_inplay=stuff_pred_dropout_inplay,
        )
        
        # Prediction layers for location + stuff
        self.combined_pred = PitcherPredLayers(
            input_size=prep_map.pitch_loc_size + prep_map.hitter_zone_size + prep_map.pitch_overview_size + \
                prep_map.pitch_stuff_size + prep_map.league_baseline_size + \
                    prep_map.pitch_combined_size,
            
            block_size_result=combined_pred_size_result,
            num_layers_result=combined_pred_blocks_result,
            dropout_result=combined_pred_dropout_result,
            
            block_size_swing=combined_pred_size_swing,
            num_layers_swing=combined_pred_blocks_swing,
            dropout_swing=combined_pred_dropout_swing,
            
            block_size_inplay=combined_pred_size_inplay,
            num_layers_inplay=combined_pred_blocks_inplay,
            dropout_inplay=combined_pred_dropout_inplay,
        )
        
        # Set parameter groups to allow for sub-parts of network to set learning rates independently
        stuff_result_parameters = GetParameters(self.stuff_pred.result_modules)
        location_result_parameters = GetParameters(self.location_pred.result_modules)
        combined_result_parameters = GetParameters(self.combined_pred.result_modules)
        
        stuff_swing_parameters = GetParameters(self.stuff_pred.swing_modules)
        location_swing_parameters = GetParameters(self.location_pred.swing_modules)
        combined_swing_parameters = GetParameters(self.combined_pred.swing_modules)
        
        stuff_inplay_parameters = GetParameters(self.stuff_pred.inplay_modules)
        location_inplay_parameters = GetParameters(self.location_pred.inplay_modules)
        combined_inplay_parameters = GetParameters(self.combined_pred.inplay_modules)
        
        self.optimizer = torch.optim.AdamW([
            {'params': location_result_parameters, 'lr': 0.005, 'weight_decay': location_result_weight_decay},
            {'params': location_swing_parameters, 'lr': 0.005, 'weight_decay': location_swing_weight_decay},
            {'params': location_inplay_parameters, 'lr': 0.001, 'weight_decay': location_inplay_weight_decay},
            
            {'params' : stuff_result_parameters, 'lr': 0.005, 'weight_decay': stuff_result_weight_decay},
            {'params' : stuff_swing_parameters, 'lr': 0.001, 'weight_decay': stuff_swing_weight_decay},
            {'params' : stuff_inplay_parameters, 'lr': 0.001, 'weight_decay': stuff_inplay_weight_decay},
            
            {'params': combined_result_parameters, 'lr': 0.005, 'weight_decay': combined_result_weight_decay},
            {'params': combined_swing_parameters, 'lr': 0.001, 'weight_decay': combined_swing_weight_decay},
            {'params': combined_inplay_parameters, 'lr': 0.001, 'weight_decay': combined_inplay_weight_decay}
        ])
        
    def forward(self, data : tuple[torch.Tensor, ...]) -> tuple[torch.Tensor, ...]:
        overview, location, stuff, combined, game, league_avg = data
        
        # Code for adding noise, commented out because it currently makes the model worse
        # if self.eval:
        #     with torch.no_grad():
        #         batch_size = location.size(0)
        #         location_noise = self.data_prep.Get_PCA_Noise_Location(batch_size).to(location.device, non_blocking=True)
        #         stuff_noise = self.data_prep.Get_PCA_Noise_Stuff(batch_size).to(stuff.device, non_blocking=True)
        #         print(stuff_noise[0])
        #         exit(1)
        #         location += location_noise
        #         stuff += stuff_noise
        
        # Location
        data_location = torch.cat((overview, location), dim=-1)
        output_location = self.location_pred(data_location)
        
        # Stuff
        data_stuff = torch.cat((overview, stuff, league_avg), dim=-1)
        output_stuff = self.stuff_pred(data_stuff)
        
        # Combined
        data_combined = torch.cat((overview, location, stuff, league_avg, combined), dim=-1)
        output_combined = self.combined_pred(data_combined)
        
        return output_location + output_stuff + output_combined
        
    def GetPitchOutput(self, filedir : str, pitches : list[DB_PitchStatcast]) -> list[DB_Output_PitchValueAggregation]:
        self.eval()
        if len(pitches) == 0:
            return []
        
        mlbId = pitches[0].PitcherId
        league = pitches[0].LeagueId
        year = pitches[0].Year
        month = pitches[0].Month
        for p in pitches:
            if p.PitcherId != mlbId:
                raise Exception("Not all pitches in GetPitchOutput have the same MlbId")
            if p.LeagueId != league:
                raise Exception("Not all pitches in GetPitchOutput have the same LeagueId")
            if p.Year != year:
                raise Exception("Not all pitches in GetPitchOutput have the same Year")
            if p.Month != month:
                raise Exception("Not all pitches in GetPitchOutput have the same Month")
            
        # Convert pitches to form required by model
        model_pitches_tuple = self.data_prep.DbPitchesToModelPitches(pitches)
        model_pitches_tuple = tuple(d.to(next(self.parameters()).device, non_blocking=True) for d in model_pitches_tuple)
        
        # Get Models that do not have that player
        pitch_cursor = pitch_db.cursor()
        model_runs = [x[0] for x in pitch_cursor.execute("SELECT modelRun FROM PlayersInTrainingData WHERE mlbId=? AND modelId=? AND isTrain=?", (mlbId, 1, 0)).fetchall()]
        if len(model_runs) == 0:
            model_runs = [x[0] for x in pitch_cursor.execute("SELECT DISTINCT modelRun FROM PlayersInTrainingData WHERE modelId=? AND isTrain=?", (1, 0)).fetchall()]
            
        model_name = pitch_cursor.execute("SELECT Name FROM Models_PitchValue WHERE Id=?", (1,)).fetchall()[0][0]
        
        # Get Output_PitchValue for each model/pitch
        opv_list : list[list[DB_Output_PitchValue]] = []
        for mr in model_runs:
            name = filedir + model_name + "_" + str(mr) + ".pt"
            with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                self.load_state_dict(torch.load(name))
            self.eval()
            
            outputs = self(model_pitches_tuple)
            
            result_location = F.softmax(outputs[0], dim=-1)
            swing_location = F.softmax(outputs[1], dim=-1)
            inplay_location = F.softmax(outputs[2], dim=-1)
            
            result_stuff = F.softmax(outputs[3], dim=-1)
            swing_stuff = F.softmax(outputs[3 + 1], dim=-1)
            inplay_stuff = F.softmax(outputs[3 + 2], dim=-1)
            
            result_combined = F.softmax(outputs[6], dim=-1)
            swing_combined = F.softmax(outputs[6 + 1], dim=-1)
            inplay_combined = F.softmax(outputs[6 + 2], dim=-1)
                
            # Get expected value of in-play
            inplay_expected = self.data_prep.ip_bucket_value.to(result_combined.device)
            inplay_expected_location = (inplay_location * inplay_expected).sum(dim=1, keepdim=True)
            inplay_expected_stuff = (inplay_stuff * inplay_expected).sum(dim=1, keepdim=True)
            inplay_expected_combined = (inplay_combined * inplay_expected).sum(dim=1, keepdim=True)
            
            single_run_data = [DB_Output_PitchValue(tuple(row.tolist())) for row in torch.cat((\
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                torch.zeros(len(pitches), 1),
                result_location.cpu(),
                swing_location.cpu(),
                inplay_expected_location.cpu(),
                result_stuff.cpu(),
                swing_stuff.cpu(),
                inplay_expected_stuff.cpu(),
                result_combined.cpu(),
                swing_combined.cpu(),
                inplay_expected_combined.cpu(),), dim=-1)]

            opv_list.append(single_run_data)
        
        opva_list : list[DB_Output_PitchValueAggregation] = []
        for i in range(len(pitches)):
            l = len(model_runs)
            agg = DB_Output_PitchValueAggregation((0,0,0,year,0,0,pitches[i].CountBalls,pitches[i].CountStrike,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0))
            for j in range(len(model_runs)):
                agg.locationCalledStrike += opv_list[j][i].locationCalledStrike / l
                agg.locationBall += opv_list[j][i].locationBall / l
                agg.locationHBP += opv_list[j][i].locationHBP / l
                agg.locationSwing += opv_list[j][i].locationSwing / l
                agg.locationWhiff += opv_list[j][i].locationWhiff / l
                agg.locationFoul += opv_list[j][i].locationFoul / l
                agg.locationInPlay += opv_list[j][i].locationInPlay / l
                agg.locationInPlayExpected += opv_list[j][i].locationInPlayExpected / l
                agg.stuffCalledStrike += opv_list[j][i].stuffCalledStrike / l
                agg.stuffBall += opv_list[j][i].stuffBall / l
                agg.stuffHBP += opv_list[j][i].stuffHBP / l
                agg.stuffSwing += opv_list[j][i].stuffSwing / l
                agg.stuffWhiff += opv_list[j][i].stuffWhiff / l
                agg.stuffFoul += opv_list[j][i].stuffFoul / l
                agg.stuffInPlay += opv_list[j][i].stuffInPlay / l
                agg.stuffInPlayExpected += opv_list[j][i].stuffInPlayExpected / l
                agg.combinedCalledStrike += opv_list[j][i].combinedCalledStrike / l
                agg.combinedBall += opv_list[j][i].combinedBall / l
                agg.combinedHBP += opv_list[j][i].combinedHBP / l
                agg.combinedSwing += opv_list[j][i].combinedSwing / l
                agg.combinedWhiff += opv_list[j][i].combinedWhiff / l
                agg.combinedFoul += opv_list[j][i].combinedFoul / l
                agg.combinedInPlay += opv_list[j][i].combinedInPlay / l
                agg.combinedInPlayExpected += opv_list[j][i].combinedInPlayExpected / l
            
            opva_list.append(agg)
        return opva_list