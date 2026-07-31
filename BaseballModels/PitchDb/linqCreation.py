import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = []

type_overrides = [
    TypeOverride("PitcherStuff", "Scenario", "Db.DbEnums.PitchScenario"), 
    TypeOverride("PitcherStuff", "PitchType", "Db.DbEnums.PitchType"), 
                ]

boolean_types = [
    BooleanTypes("PlayersInTrainingData", ["IsTrain"]),
                ]

linqCreation(
    db_name="Pitch.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="PitchDb",
    dbContextName='PitchDbContext'
)

