import torch
import torch.nn as nn
import torch.nn.functional as F

from PitchModel.PitchTrackingDBTypes import DB_PitchData
from PitchModel.PitchDBTypes import DB_Output_PitchValueAggregation
from PitchModel.Shared import GetBinaryName
from PitchModel.Stuff.Model.ResnetBlock import ResnetBlock
from PitchModel.Stuff.Model.Utilities import *
from PitchModel.Stuff.Model.PitchModelCache import pitch_model_cache, training_data_cache, GetDatabaseSignature
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.Model.Initialization import InitializeNetwork
from PitchModel.Constants import pitch_db, BUCKET_INPLAY_VALUE, device
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
                activation_function : Callable[[torch.Tensor], torch.Tensor] = F.leaky_relu,
                custom_init : bool = True):
        
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
        self.custom_init = custom_init
        
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
    learning_rate=1.24e-4,
    block_size=86,
    num_blocks=22,
    dropout=0.056,
    weight_decay=5.1e-7,
    activation_function=F.silu,
    num_epochs=217,
    batch_size=18700
)
        
DEFAULT_COMBINED_SWINGRESULTS_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.SwingResults,
    learning_rate=6.3e-4,
    block_size=84,
    num_blocks=7,
    dropout=0.07,
    weight_decay=1.8e-8,
    activation_function=F.silu,
    num_epochs=57,
    batch_size=17000
)

DEFAULT_COMBINED_INPLAY_ARGS = PitchModelArgs(
    model_variant_type=ModelVariantType.Combined,
    model_output_type=ModelOutputType.InPlay,
    learning_rate=1.7e-3,
    block_size=64,
    num_blocks=6,
    dropout=0.400,
    weight_decay=1.0e-4,
    activation_function=F.gelu,
    num_epochs=56,
    batch_size=12000
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
        
        if args.custom_init:
            InitializeNetwork(self.layers, args.activation_function, args.model_output_type)
        
        self.optimizer = torch.optim.AdamW(params=self.parameters(), lr=args.learning_rate, weight_decay=args.weight_decay)
        
    def forward(self, data : torch.Tensor) -> torch.Tensor:
        for layer in self.layers:
            data = layer(data)
        
        return data
        
    @staticmethod
    def GetPitchOutput(
            pitches : list[DB_PitchData],
            model_id : int,
            filedir : str,
            binaries_dir : str | None = None,
            model_year : int | None = None,
            run_device : torch.device | str | None = None) -> list[DB_Output_PitchValueAggregation]:
        
        # Deferred: Eval_Pitches imports this module
        from PitchModel.Stuff.TrainEval.Eval_Pitches import GetModelFilePath, LoadNetwork, GetTrainPitchers, GetModelYearRuns, GetModelYearOutput
        
        if len(pitches) == 0:
            return []
        if run_device is None:
            run_device = device
        
        cursor = pitch_db.cursor()
        name_rows = cursor.execute("SELECT Name FROM Models_PitchValue WHERE Id=?", (model_id,)).fetchall()
        if len(name_rows) == 0:
            raise ValueError(f"model_id {model_id} is not in Models_PitchValue")
        model_name = name_rows[0][0]
        database_signature = GetDatabaseSignature(cursor)
        
        # Different model years use different models and normalization, so each is scored together
        indices_by_model_year : dict[int, list[int]] = {}
        for i, pitch in enumerate(pitches):
            year_to_use = pitch.Year - 1 if model_year is None else model_year
            indices_by_model_year.setdefault(year_to_use, []).append(i)
        
        results : list[DB_Output_PitchValueAggregation | None] = [None] * len(pitches)
        for year_to_use, indices in indices_by_model_year.items():
            year_pitches = [pitches[i] for i in indices]
            
            def get_train_pitchers(run):
                return training_data_cache.Get(
                    (model_id, year_to_use, run),
                    database_signature,
                    lambda: GetTrainPitchers(cursor, model_id, year_to_use, run))
            runs, train_pitchers_by_run = GetModelYearRuns(cursor, model_id, year_to_use, get_train_pitchers)
            
            pairing = (model_id, year_to_use)
            
            data_prep_path = GetBinaryName(model_id, year_to_use) if binaries_dir is None \
                else GetBinaryName(model_id, year_to_use, binaries_dir)
            data_prep = pitch_model_cache.Get(data_prep_path, lambda: DataPrep.Load_From_File(data_prep_path))
            
            def get_network(run, model_variant_type, model_output_type):
                model_path = GetModelFilePath(filedir, model_name, year_to_use, run, model_variant_type, model_output_type)
                return pitch_model_cache.Get(
                    pairing,
                    model_path,
                    lambda: LoadNetwork(data_prep, model_path, model_variant_type, model_output_type, run_device),
                    discriminator=str(run_device))
            
            with torch.no_grad():
                data_stuff, data_comb = data_prep.Transform_PitchStats(year_pitches)
                year_output = GetModelYearOutput(
                    data_prep=data_prep,
                    runs=runs,
                    train_pitchers_by_run=train_pitchers_by_run,
                    data_stuff=data_stuff.to(run_device),
                    data_comb=data_comb.to(run_device),
                    pitcher_ids=torch.tensor([p.PitcherId for p in year_pitches], dtype=torch.int64),
                    get_network=get_network,
                    run_device=run_device,
                    model_description=f"Model {model_id} year {year_to_use}")
            
            for output_row, i in zip(year_output.tolist(), indices):
                pitch = pitches[i]
                results[i] = DB_Output_PitchValueAggregation((
                    model_id, pitch.GameId, pitch.PitchId, year_to_use,
                    pitch.Year, pitch.LevelId, pitch.PitcherId,
                    *output_row,
                    -1000, -1000, None, None))
        
        return results
