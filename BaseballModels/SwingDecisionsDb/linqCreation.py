import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = []

type_overrides = [
    TypeOverride("SwingDecision", "PitchType", "Db.DbEnums.PitchType"),
    TypeOverride("SwingDecision", "BaseOccupancy", "Db.DbEnums.BaseOccupancy"),
    TypeOverride("SwingResultAggregation", "PitchGroup", "SwingDbEnums.PitchGroup")
                ]

boolean_types = [
    BooleanTypes("SwingDecision", ["DidSwing"])
    ]

linqCreation(
    db_name="SwingDecisions.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="SwingDecisionsDb",
    dbContextName='SwingDecisionsDbContext'
)