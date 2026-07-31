import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = []

boolean_types = [
    BooleanTypes("DraftRank", ["IsHitter", "IsEligible", "TrainingBias"]),
    BooleanTypes("Player", ["IsHitter", "IsPitcher", "InTraining"]),
    BooleanTypes("PlayerModel", ["IsHitter", "TrainingBias"]),
    BooleanTypes("PlayerRank", ["IsHitter", "TrainingBias"]),
    BooleanTypes("PlayerYearPositions", ["IsHitter"])
                ]

type_overrides = [
    TypeOverride("DraftRank", "TimestepQuality", "DbEnums.TimestepQuality"), 
    TypeOverride("PlayerModel", "TimestepQuality", "DbEnums.TimestepQuality"), 
    TypeOverride("PlayerRank", "TimestepQuality", "DbEnums.TimestepQuality"), 
    TypeOverride("QualityCode", "Code", "DbEnums.TimestepQuality"), 
    TypeOverride("QualityCode", "Severity", "DbEnums.Severity"), 
                ]

linqCreation(
    db_name="Site.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="SiteDb",
    dbContextName='SiteDbContext'
)