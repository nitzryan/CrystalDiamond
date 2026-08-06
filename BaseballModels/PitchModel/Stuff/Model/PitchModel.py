import torch
import torch.nn as nn
import torch.nn.functional as F

from PitchModel.DBTypes import *
from PitchModel.PitchDBTypes import *
from PitchModel.Stuff.Model.ResnetBlock import ResnetBlock
from PitchModel.Stuff.Model.Utilities import *
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Constants import pitch_db, BUCKET_INPLAY_VALUE
import warnings
from PitchModel.Stuff.Model.ModelOutputType import *
        
def GetParameters(layers):
    parameters = []
    for l in layers:
        parameters.extend(l.parameters())
    return parameters
        
class PitchModelArgs:
    def __init__(self,
                model_variant_type : ModelVariantType,
                model_output_type : ModelOutputType,
                batch_size : int,
                num_epochs : int,
                learning_rate : float,
                block_size : int,
                num_blocks : int,
                dropout : float,
                weight_decay : float,
                activation_function : Callable[[torch.Tensor], torch.Tensor] = F.leaky_relu):
        
        self.model_variant_type = model_variant_type
        self.model_output_type = model_output_type
        
        self.batch_size = batch_size
        self.num_epochs = num_epochs
        
        self.learning_rate = learning_rate
        self.block_size = block_size
        self.num_blocks = num_blocks
        self.dropout = dropout
        self.weight_decay = weight_decay
        self.activation_function = activation_function
        
DEFAULT_STUFF_RESULT_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.Result,
    learning_rate=4.4e-4,
    block_size=90,
    num_blocks=12,
    dropout=0.288,
    weight_decay=2.6e-4,
    activation_function=F.gelu,
    num_epochs=228,
    batch_size=12000
)
        
DEFAULT_STUFF_SWINGRESULTS_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.SwingResults,
    learning_rate=1.0e-3,
    block_size=84,
    num_blocks=11,
    dropout=0.375,
    weight_decay=4.3e-7,
    activation_function=F.leaky_relu,
    num_epochs=78,
    batch_size=13500
)

DEFAULT_STUFF_INPLAY_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Stuff,
    model_output_type=ModelOutputType.InPlay,
    learning_rate=6.8e-5,
    block_size=220,
    num_blocks=6,
    dropout=0.32,
    weight_decay=4.0e-6,
    activation_function=F.relu,
    num_epochs=124,
    batch_size=15000
)

DEFAULT_COMBINED_RESULT_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.Result,
    learning_rate=3.2e-4,
    block_size=126,
    num_blocks=10,
    dropout=0.122,
    weight_decay=1.4e-8,
    activation_function=F.relu,
    num_epochs=141,
    batch_size=17000
)
        
DEFAULT_COMBINED_SWINGRESULTS_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.SwingResults,
    learning_rate=5.0e-4,
    block_size=93,
    num_blocks=8,
    dropout=0.264,
    weight_decay=1.4e-2,
    activation_function=F.tanh,
    num_epochs=243,
    batch_size=16500
)

DEFAULT_COMBINED_INPLAY_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.InPlay,
    learning_rate=1.7e-4,
    block_size=174,
    num_blocks=7,
    dropout=0.363,
    weight_decay=5.4e-4,
    activation_function=F.leaky_relu,
    num_epochs=150,
    batch_size=21000
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
            [ResnetBlock(dim=args.block_size, dropout=args.dropout, activation_function=args.activation_function) for _ in range(args.num_blocks)] +
            [nn.Linear(args.block_size, output_size)]
        )
        
        self.optimizer = torch.optim.AdamW(params=self.parameters(), lr=args.learning_rate, weight_decay=args.weight_decay)
        
    def forward(self, data : torch.Tensor) -> torch.Tensor:
        for layer in self.layers:
            data = layer(data)
        
        return data
        
    @staticmethod
    def GetPitchOutput(data_prep : DataPrep, filedir : str, pitches : list[DB_PitchStatcast], run_device : str = 'cuda') -> list[DB_Output_PitchValueAggregation]:
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
        model_pitches_tuple = data_prep.DbPitchesToModelPitches(pitches)
        data_ovr, data_loc, data_stuff, data_comb, data_game, data_league = tuple(d.to(run_device, non_blocking=True) for d in model_pitches_tuple)
        
        # Get Models that do not have that player
        pitch_cursor = pitch_db.cursor()
        model_runs = [x[0] for x in pitch_cursor.execute("SELECT modelRun FROM PlayersInTrainingData WHERE mlbId=? AND modelId=? AND isTrain=?", (mlbId, 1, 0)).fetchall()]
        if len(model_runs) == 0:
            model_runs = [x[0] for x in pitch_cursor.execute("SELECT DISTINCT modelRun FROM PlayersInTrainingData WHERE modelId=? AND isTrain=?", (1, 0)).fetchall()]
            
        model_name = pitch_cursor.execute("SELECT Name FROM Models_PitchValue WHERE Id=?", (1,)).fetchall()[0][0]
        
        # Get Output_PitchValue for each model/pitch
        opv_list : list[list[DB_Output_PitchValue]] = []
        for mr in model_runs:
            output_list = []
            for model_variant in [ModelVariantType.Stuff, ModelVariantType.Combined]:
                for model_output in [ModelOutputType.Result, ModelOutputType.SwingResults, ModelOutputType.InPlay]:
                    model = PitchModel(args=DEFAULT_ARGS_MAP[(model_variant, model_output)], data_prep=data_prep)
                    model = model.to(run_device)
                    
                    with warnings.catch_warnings(action='ignore', category=FutureWarning): # Warning about loading models, irrelevant here
                        model.load_state_dict(torch.load(f"{filedir}/{model_name}_{mr}_{model_variant.name}_{model_output.name}.pt"))
                    model.eval()
                    
                    match model_variant:
                        case ModelVariantType.Stuff:
                            model_data = torch.cat((data_ovr, data_stuff, data_league), dim=-1)
                        case ModelVariantType.Combined:
                            model_data = torch.cat((data_ovr, data_loc, data_stuff, data_comb, data_league), dim=-1)
                    
                    output = model(model_data)
                    result = F.softmax(output, dim=-1)
                    
                    if model_output == ModelOutputType.InPlay:
                        # Get expected value of in-play
                        inplay_expected = data_prep.ip_bucket_value.to(result.device)
                        inplay_expected_output = (result * inplay_expected).sum(dim=1, keepdim=True)
                        output_list.append(inplay_expected_output.cpu())
                    else:
                        output_list.append(result.cpu())
            
            
            run_data = [tuple(row.tolist()) for row in torch.cat((\
                torch.tensor([1 for _ in pitches]).unsqueeze(-1),
                torch.tensor([p.GameId for p in pitches]).unsqueeze(-1),
                torch.tensor([p.PitchId for p in pitches]).unsqueeze(-1),
                torch.tensor([mr for _ in pitches]).unsqueeze(-1),
                torch.tensor([p.Year for p in pitches]).unsqueeze(-1),
                torch.tensor([p.LevelId for p in pitches]).unsqueeze(-1),
                torch.tensor([p.PitcherId for p in pitches]).unsqueeze(-1),
                output_list[0],
                output_list[1],
                output_list[2],
                output_list[3],
                output_list[4],
                output_list[5]), dim=-1)]
            
            run_pitches = [DB_Output_PitchValue(rd) for rd in run_data]
            opv_list.append(run_pitches)
                
        opva_list : list[DB_Output_PitchValueAggregation] = []
        for i in range(len(pitches)):
            l = len(model_runs)
            agg = DB_Output_PitchValueAggregation((0,0,0,year,0,0,pitches[i].CountBalls,pitches[i].CountStrike,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0))
            for j in range(len(model_runs)):
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