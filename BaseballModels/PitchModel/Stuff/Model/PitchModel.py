import torch
import torch.nn as nn
import torch.nn.functional as F

from Buckets import *
from DBTypes import *
from PitchDBTypes import *
from Stuff.Model.ResnetBlock import ResnetBlock
from Stuff.Model.Utilities import *
from Stuff.DataPrep.DataPrep import DataPrep
from Constants import pitch_db
import warnings
from Stuff.Model.ModelOutputType import *
        
def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters
        
class PitchModelArgs:
    def __init__(self,
                model_variant_type : ModelVariantType,
                model_output_type : ModelOutputType,
                learning_rate : float,
                block_size : int,
                num_blocks : int,
                dropout : float,
                weight_decay : float):
        
        self.model_variant_type = model_variant_type
        self.model_output_type = model_output_type
        
        self.learning_rate = learning_rate
        self.block_size = block_size
        self.num_blocks = num_blocks
        self.dropout = dropout
        self.weight_decay = weight_decay
        
DEFAULT_STUFF_RESULT_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.Result,
    learning_rate=0.001,
    block_size=64,
    num_blocks=8,
    dropout=0.2,
    weight_decay=1e-2
)
        
DEFAULT_STUFF_SWINGRESULTS_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.SwingResults,
    learning_rate=0.001,
    block_size=128,
    num_blocks=4,
    dropout=0.0,
    weight_decay=3e-3
)

DEFAULT_STUFF_INPLAY_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.InPlay,
    learning_rate=0.001,
    block_size=128,
    num_blocks=4,
    dropout=0.2,
    weight_decay=3e-3
)

DEFAULT_COMBINED_RESULT_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.Result,
    learning_rate=0.001,
    block_size=256,
    num_blocks=4,
    dropout=0.2,
    weight_decay=1e-5
)
        
DEFAULT_COMBINED_SWINGRESULTS_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.SwingResults,
    learning_rate=0.001,
    block_size=64,
    num_blocks=8,
    dropout=0.1,
    weight_decay=1e-4
)

DEFAULT_COMBINED_INPLAY_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.InPlay,
    learning_rate=0.001,
    block_size=64,
    num_blocks=8,
    dropout=0.3,
    weight_decay=1e-4
)
        
DEFAULT_ARGS_MAP = {
    (ModelVariantType.Stuff, ModelOutputType.Result) : DEFAULT_STUFF_RESULT_ARGS,
    (ModelVariantType.Stuff, ModelOutputType.SwingResults) : DEFAULT_STUFF_SWINGRESULTS_ARGS,
    (ModelVariantType.Stuff, ModelOutputType.InPlay) : DEFAULT_STUFF_INPLAY_ARGS,
    
    (ModelVariantType.Combined, ModelOutputType.Result) : DEFAULT_COMBINED_RESULT_ARGS,
    (ModelVariantType.Combined, ModelOutputType.SwingResults) : DEFAULT_COMBINED_SWINGRESULTS_ARGS,
    (ModelVariantType.Combined, ModelOutputType.InPlay) : DEFAULT_COMBINED_INPLAY_ARGS,
}
        
class PitchModel(nn.Module):
    def __init__(self,
                args : PitchModelArgs,
                data_prep : DataPrep,
    ):
        super().__init__()
        
        self.nonlin = F.leaky_relu
        self.model_variant_type = args.model_variant_type
        self.model_output_type = args.model_output_type
        
        match args.model_variant_type:
            case ModelVariantType.Stuff:
                input_size = data_prep.GetStuffInputSize()
            case ModelVariantType.Combined:
                input_size = data_prep.GetCombinedInputSize()
        
        match args.model_output_type:
            case ModelOutputType.Result:
                output_size = 4
            case ModelOutputType.SwingResults:
                output_size = 3
            case ModelOutputType.InPlay:
                output_size = BUCKET_INPLAY_VALUE.size(0) + 1
        
        self.layers = nn.ModuleList(
            [nn.Linear(input_size, args.block_size)] +
            [ResnetBlock(dim=args.block_size, dropout=args.dropout) for _ in range(args.num_blocks)] +
            [nn.Linear(args.block_size, output_size)]
        )
        
        self.optimizer = torch.optim.AdamW(params=self.parameters(), lr=args.learning_rate, weight_decay=args.weight_decay)
        
    def forward(self, data : torch.Tensor) -> torch.Tensor:
        for layer in self.layers:
            data = layer(data)
        
        return data
        
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