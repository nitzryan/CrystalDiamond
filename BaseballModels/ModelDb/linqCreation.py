import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = []

boolean_types = [
    BooleanTypes("PlayersInTrainingData", ["IsHitter", "IsTrain"]),
    BooleanTypes("Model_TrainingHistory", ["IsHitter"]),
    BooleanTypes("Output_PlayerWar", ["IsHitter"]),
    BooleanTypes("Output_PlayerWarAggregation", ["IsHitter"]),
    BooleanTypes("Output_PlayerHighestLevel", ["IsHitter"]),
    BooleanTypes("Output_PlayerHighestLevelAggregation", ["IsHitter"]),
    BooleanTypes("WarBucketAverages", ["IsHitter"]),
                ]

type_overrides = []
    
linqCreation(
    db_name="Model.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="ModelDb",
    dbContextName='ModelDbContext'
)