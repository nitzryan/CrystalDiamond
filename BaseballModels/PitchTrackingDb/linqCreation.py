import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = [

                    ]

type_overrides = [
    TypeOverride("PitchData", "Result", "Db.DbEnums.PitchResult"),
    
    TypeOverride("PitchData", "PitchClass", "Db.DbEnums.PitchClass"),
    TypeOverride("PitchFlightpath", "PitchClass", "Db.DbEnums.PitchClass"),
    
    TypeOverride("PitchFlightpathGameDelta", "PitchType", "Db.DbEnums.PitchType"),
                ]

boolean_types = [
    BooleanTypes("PitchData", ["HadSwing", "HadContact", "IsInPlay", "PitIsR", "HitIsR"]),
                ]

linqCreation(
    db_name="PitchTracking.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="PitchTrackingDb",
    dbContextName='PitchTrackingDbContext'
)