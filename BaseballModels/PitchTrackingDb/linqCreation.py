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
    
    TypeOverride("PitchFlightpath", "PitchType", "Db.DbEnums.PitchType"),
    TypeOverride("PitchFlightpathGameDelta", "FastballPitchType", "Db.DbEnums.PitchType"),
                ]

boolean_types = [
    BooleanTypes("PitchData", ["HadSwing", "HadContact", "IsInPlay", "PitIsR", "HitIsR"]),
    BooleanTypes("PitchFlightpath", ["PitIsR"]),
                ]

linqCreation(
    db_name="PitchTracking.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="PitchTrackingDb",
    dbContextName='PitchTrackingDbContext'
)