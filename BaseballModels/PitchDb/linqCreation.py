import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = []

type_overrides = [
    TypeOverride("PitcherStuff", "Scenario", "Db.DbEnums.PitchScenario"), 
    TypeOverride("PitcherStuff", "PitchType", "Db.DbEnums.PitchType"), 
    TypeOverride("PitchModelResultBasis", "OutputType", "Db.DbEnums.PitchModelOutputType"),
                ]

boolean_types = [
    BooleanTypes("PlayersInTrainingData", ["IsTrain"]),
    BooleanTypes("PitcherStatcastMonth", ["IsValid"])
                ]

linqCreation(
    db_name="Pitch.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="PitchDb",
    dbContextName='PitchDbContext'
)

