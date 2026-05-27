CREATE TABLE PitchStatcast
(
    "GameId" INTEGER NOT NULL,
    "PitchId" INTEGER NOT NULL,
    "PaId" INTEGER NOT NULL,
    "PitcherId" INTEGER NOT NULL,
    "HitterId" INTEGER NOT NULL,
    "LeagueId" INTEGER NOT NULL,
    "LevelId" INTEGER NOT NULL,

    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "PitcherPitchNum" INTEGER NOT NULL,

    "CountBalls" INTEGER NOT NULL,
    "CountStrike" INTEGER NOT NULL,
    "Outs" INTEGER NOT NULL,
    "BaseOccupancy" INTEGER NOT NULL,
    "PitchType" INTEGER NOT NULL,

    "PaResult" INTEGER NOT NULL,
    "PaResultOccupancy" INTEGER NOT NULL,
    "PaResultOuts" INTEGER NOT NULL,
    "PaResultDirectRuns" INTEGER NOT NULL,
    "RunsAfterPa" INTEGER NOT NULL, 

    "Result" INTEGER NOT NULL,
    "HadSwing" INTEGER NOT NULL,
    "HadContact" INTEGER NOT NULL,
    "IsInPlay" INTEGER NOT NULL,

    -- Handidness
    "HitIsR" INTEGER NOT NULL,
    "PitIsR" INTEGER NOT NULL,
    -- Velocity
    "vX" REAL,
    "vY" REAL,
    "vZ" REAL,
    "vStart" REAL,
    "vEnd" REAL,
    -- Accel
    "aX" REAL,
    "aY" REAL,
    "aZ" REAL,
    -- Break
    "pfxX" REAL,
    "pfxZ" REAL,
    "BreakAngle" REAL,
    "BreakVertical" REAL,
    "BreakInduced" REAL,
    "BreakHorizontal" REAL,
    "SpinRate" INTEGER,
    "SpinDirection" INTEGER,
    -- Location
    "pX" REAL,
    "pZ" REAL,
    -- Zone Location
    "ZoneTop" REAL,
    "ZoneBot" REAL,
    -- Release
    "Extension" REAL,
    "x0" REAL,
    "y0" REAL,
    "z0" REAL,
    "PlateTime" REAL,

    -- Hit Data, all or nothing null
    "LaunchSpeed" REAL,
    "LaunchAngle" REAL,
    "TotalDist" REAL,
    "HitCoordX" REAL,
    "HitCoordY" REAL,

    "RunValueHitter" REAL NOT NULL,
    "RunValueSmoothedHitter" REAL NOT NULL,

    "Scenario" INTEGER NOT NULL,
    "ModelStuff" REAL,
    "ModelLocation" REAL,
    "ModelPitch" REAL,

    PRIMARY KEY("GameId", "PitchId")
);

CREATE TABLE PitchNonStatcast
(
    "GameId" INTEGER NOT NULL,
    "PitchId" INTEGER NOT NULL,
    "PitcherId" INTEGER NOT NULL,
    "HitterId" INTEGER NOT NULL,
    "LeagueId" INTEGER NOT NULL,
    "LevelId" INTEGER NOT NULL,

    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "PitcherPitchNum" INTEGER NOT NULL,

    "CountBalls" INTEGER NOT NULL,
    "CountStrike" INTEGER NOT NULL,
    "Outs" INTEGER NOT NULL,
    "BaseOccupancy" INTEGER NOT NULL,

    "PaResult" INTEGER NOT NULL,
    "Result" INTEGER NOT NULL,
    "HadSwing" INTEGER NOT NULL,
    "HadContact" INTEGER NOT NULL,
    "IsInPlay" INTEGER NOT NULL,

    -- Handidness
    "HitIsR" INTEGER NOT NULL,
    "PitIsR" INTEGER NOT NULL,

    "RunValueHitter" REAL NOT NULL,

    "Scenario" INTEGER NOT NULL,

    PRIMARY KEY("GameId", "PitchId")
);

-- Indexes created in GetStatcastData.cs

CREATE TABLE PitcherStatcastGame
(
    "MlbId" INTEGER NOT NULL,
    "GameId" INTEGER NOT NULL,
    "LevelId" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "FastballVelo" REAL,
    "FastballBreakHoriz" REAL,
    "FastballBreakInduced" REAL,
    "FastballBreakVert" REAL,

    "SinkerVelo" REAL,
    "SinkerBreakHoriz" REAL,
    "SinkerBreakInduced" REAL,
    "SinkerBreakVert" REAL,

    PRIMARY KEY("MlbId", "GameId")
);

CREATE TABLE PitchDateAverages
(
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "Extension" REAL NOT NULL,

    "FastballVelo" REAL NOT NULL,
    "Fastball4SeamVert" REAL NOT NULL,
    "Fastball4SeamHoriz" REAL NOT NULL,
    "FastballCount" INTEGER NOT NULL,

    "SinkerVelo" REAL NOT NULL,
    "SinkerVert" REAL NOT NULL,
    "SinkerHoriz" REAL NOT NULL,
    "SinkerCount" INTEGER NOT NULL,

    "CurveballVelo" REAL NOT NULL,
    "CurveballHoriz" REAL NOT NULL,
    "CurveballVert" REAL NOT NULL,
    "CurveballCount" INTEGER NOT NULL,

    PRIMARY KEY ("Year", "Month")
);

CREATE TABLE PitcherStatcastMonth
(
    "MlbId" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "Stuff" REAL NOT NULL,
    "Pitch" REAL NOT NULL,
    "Actual" REAL NOT NULL,
    "NumPitches" INTEGER NOT NULL,

    "StuffFastball" REAL,
    "PitchFastball" REAL,
    "ActFastball" REAL,
    "NumFastballs" INTEGER NOT NULL,

    "StuffBreaking" REAL,
    "PitchBreaking" REAL,
    "ActBreaking" REAL,
    "NumBreaking" INTEGER NOT NULL,

    "StuffChangeup" REAL,
    "PitchChangeup" REAL,
    "ActChangeup" REAL,
    "NumChangeup" INTEGER NOT NULL,

    PRIMARY KEY("MlbId", "Year", "Month")
);

-- Perc/Exit velo are % of MLB average that month
CREATE TABLE HitterStatcastMonth
(
    "MlbId" INTEGER NOT NULL,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,

    "BattedBallEvents" INTEGER NOT NULL,
    "AvgExitVelo" REAL NOT NULL,
    "PeakExitVelo" REAL NOT NULL,

    "NumPitches" INTEGER NOT NULL,
    "NumSwings" INTEGER NOT NULL,
    "ChasePerc" REAL NOT NULL,
    "WhiffPerc" REAL NOT NULL,
    "ZoneSwingPerc" REAL NOT NULL,
    "ZoneContactPerc" REAL NOT NULL,

    "NumFastballs" INTEGER NOT NULL,
    "FastballContactPerc" REAL NOT NULL,
    "NumBreaking" INTEGER NOT NULL,
    "BreakingContactPerc" REAL NOT NULL,
    "NumChangeup" INTEGER NOT NULL,
    "ChangeupContactPerc" REAL NOT NULL,

    PRIMARY KEY("MlbId", "Year", "Month")
);