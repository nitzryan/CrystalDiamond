import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from DbShared.linqCreation import *

autoincrement_pairs = [
    AutoincrementPair("Player_Hitter_GameLog", "GameLogId"), 
    AutoincrementPair("Player_Pitcher_GameLog", "GameLogId"), 
    AutoincrementPair("Transaction_Log", "TransactionId"), 
    AutoincrementPair("GamePlayByPlay", "EventId"),
    AutoincrementPair("Player_Fielder_GameLog", "GameLogId")
                    ]

type_overrides = [
    TypeOverride("GamePlayByPlay", "Result", "DbEnums.PBP_Events"), 
    
    TypeOverride("GamePlayByPlay", "HitTrajectory", "DbEnums.PBP_HitTrajectory"), 
    TypeOverride("GamePlayByPlay", "HitHardness", "DbEnums.PBP_HitHardness"), 
    
    TypeOverride("GamePlayByPlay", "StartBaseOccupancy", "DbEnums.BaseOccupancy"), 
    TypeOverride("GamePlayByPlay", "EndBaseOccupancy", "DbEnums.BaseOccupancy"),
    TypeOverride("PitchStatcast", "BaseOccupancy", "DbEnums.BaseOccupancy"),
    TypeOverride("PitchStatcast", "PaResultOccupancy", "DbEnums.BaseOccupancy"),
    TypeOverride("PitchNonStatcast", "BaseOccupancy", "DbEnums.BaseOccupancy"),
    
    TypeOverride("GamePlayByPlay", "EventFlag", "DbEnums.GameFlags"),
    
    TypeOverride("Player_Fielder_GameLog", "Position", "DbEnums.Position"),
    TypeOverride("Player_Fielder_MonthStats", "Position", "DbEnums.Position"),
    TypeOverride("Player_Fielder_YearStats", "Position", "DbEnums.Position"),
    
    TypeOverride("College_HitterStats", "Pos", "DbEnums.CollegePosition"),
    TypeOverride("Model_College_HitterYear", "Pos", "DbEnums.CollegePosition"),
    
    TypeOverride("PitchStatcast", "PitchType", "DbEnums.PitchType"),
    
    TypeOverride("PitchStatcast", "Result", "DbEnums.PitchResult"),
    TypeOverride("PitchNonStatcast", "Result", "DbEnums.PitchResult"),
    TypeOverride("RunExpectancyMatrix", "Result", "DbEnums.PitchResult"),
    
    TypeOverride("PitchStatcast", "PaResult", "DbEnums.PitchPaResult"),
    TypeOverride("PitchNonStatcast", "PaResult", "DbEnums.PitchPaResult"),
    
    TypeOverride("PitchStatcast", "Scenario", "DbEnums.PitchScenario"),
    TypeOverride("PitchNonStatcast", "Scenario", "DbEnums.PitchScenario"),
    
    TypeOverride("Model_Players", "ProspectType", "DbEnums.ProspectType"),
                ]

boolean_types = [
    BooleanTypes("Player_Fielder_GameLog", ["Started", "IsHome"]),
    BooleanTypes("GamePlayByPlay_GameFielders", ["IsHome"]),
    BooleanTypes("College_Player", ["IsHitter", "IsPitcher", "IsEligible"]),
    BooleanTypes("PitchStatcast", ["HadSwing", "HadContact", "IsInPlay", "HitIsR", "PitIsR"]),
    BooleanTypes("PitchNonStatcast", ["HadSwing", "HadContact", "IsInPlay", "HitIsR", "PitIsR"]),
    BooleanTypes("PitcherStatcastMonth", ["IsValid"]),
    BooleanTypes("HitterStatcastMonth", ["IsValid"]),
    BooleanTypes("Model_Players", ["IsEligible", "IsHitter", "IsPitcher"]),
    BooleanTypes("Player_YearlyWPA", ["IsHitter", "IsStarter"]),
    BooleanTypes("Player_CareerStatus", ["IsPitcher", "IsHitter", "isActive"])
                ]

linqCreation(
    db_name="BaseballStats.db",
    autoincrement_pairs=autoincrement_pairs,
    type_overrides=type_overrides,
    boolean_types=boolean_types,
    namespace="Db",
    dbContextName='SqliteDbContext'
)