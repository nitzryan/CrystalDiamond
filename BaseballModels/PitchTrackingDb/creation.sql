-- Data to be used for Training/Evaluating Models
CREATE TABLE PitchData
(
    -- PK
    "GameId" INTEGER NOT NULL,
    "PitchId" INTEGER NOT NULL,
    
    -- Allows for easy query for finding specific pitches to train on
    "Year" INTEGER NOT NULL,
    "LevelId" INTEGER NOT NULL,

    -- Map pitch to a pitcher to compare to other pitches
    "PitcherId" INTEGER NOT NULL,

    -- Train 1 model per pitch class, fastball/breaking/offspeed
    "PitchClass" INTEGER NOT NULL,

    -- Situational Data
    "CountBalls" INTEGER NOT NULL,
    "CountStrike" INTEGER NOT NULL,
    "PitIsR" INTEGER NOT NULL,
    "HitIsR" INTEGER NOT NULL,

    -- Output Data
    "Result" INTEGER NOT NULL,
    "HadSwing" INTEGER NOT NULL,
    "HadContact" INTEGER NOT NULL,
    "IsInPlay" INTEGER NOT NULL,
    "RunValueInPlay" REAL NOT NULL, -- 0 (unused) for any event not in play

    -- Stuff Base
    "Vel" REAL NOT NULL,
    "Extension" REAL NOT NULL,
    "BreakInduced" REAL NOT NULL,
    "BreakHorizontal" REAL NOT NULL,

    -- Spin-Data
    "SpinRate" REAL NOT NULL,
    "SpinAxis" REAL NOT NULL,
    "ActiveSpin" REAL NOT NULL,

    -- Approach Angle
    "VaaAboveAverage" REAL NOT NULL,
    "HaaAboveAverage" REAL NOT NULL,

    -- Location Data
    "PlateX" REAL NOT NULL,
    "PlateZ" REAL NOT NULL,
    "ZoneTop" REAL NOT NULL,
    "ZoneBot" REAL NOT NULL,

    PRIMARY KEY("GameId", "PitchId")
);

CREATE INDEX idx_PitchData_Year ON PitchData
(
    "Year", "LevelId", "GameId", "PitchId"
);

CREATE INDEX idx_PitchData_PitcherId ON PitchData
(
    "PitcherId", "GameId", "PitchId"
);

-- Ball Flightpath
CREATE TABLE PitchFlightpath
(
    -- PK
    "GameId" INTEGER NOT NULL,
    "PitchId" INTEGER NOT NULL,

    -- Allows for easy query for finding specific pitches to train on
    "Year" INTEGER NOT NULL,

    -- Information to map to a specific player/pitch
    "PitcherId" INTEGER NOT NULL,
    "PitchType" INTEGER NOT NULL,
    "PitchClass" INTEGER NOT NULL,
    "PitIsR" INTEGER NOT NULL,

    -- Pitch Tracking Data (horiz/vert breaks at given timesteps)
    "BreakHoriz_05" REAL NOT NULL,
    "BreakVer_05" REAL NOT NULL,
    "BreakHoriz_10" REAL NOT NULL,
    "BreakVer_10" REAL NOT NULL,
    "BreakHoriz_15" REAL NOT NULL,
    "BreakVer_15" REAL NOT NULL,
    "BreakHoriz_20" REAL NOT NULL,
    "BreakVer_20" REAL NOT NULL,
    "BreakHoriz_25" REAL NOT NULL,
    "BreakVer_25" REAL NOT NULL,

    -- Attack Angles
    "HAA" REAL NOT NULL,
    "VAA" REAL NOT NULL,

    -- Final Break
    "HB" REAL NOT NULL,
    "IVB" REAL NOT NULL,
    "VB" REAL NOT NULL,
    "Vel" REAL NOT NULL,

    -- Error from Statcast plate position for validation
    "TrackingError" REAL NOT NULL,
    "PlateX" REAL NOT NULL,
    "PlateZ" REAL NOT NULL,

    PRIMARY KEY("GameId", "PitchId")
);

CREATE INDEX idx_PitchFlightpath_PitcherGamePitch ON PitchFlightpath
(
    "PitcherId", "GameId", "PitchClass"
);

CREATE INDEX idx_PitchFlightpath_Year ON PitchFlightpath
(
    "Year", "GameId", "PitchId"
);

-- Ball flightpath delta vs a given fastball for a game
CREATE TABLE PitchFlightpathGameDelta
(
    -- PK
    "GameId" INTEGER NOT NULL,
    "PitchId" INTEGER NOT NULL,
    
    -- Information to map to a specific player/pitch
    "PitcherId" INTEGER NOT NULL,
    "FastballPitchType" INTEGER NOT NULL, -- Only fastballs

    -- Pitch Tracking Deltas
    "BreakHoriz_05Delta" REAL NOT NULL,
    "BreakVer_05Delta" REAL NOT NULL,
    "BreakHoriz_10Delta" REAL NOT NULL,
    "BreakVer_10Delta" REAL NOT NULL,
    "BreakHoriz_15Delta" REAL NOT NULL,
    "BreakVer_15Delta" REAL NOT NULL,
    "BreakHoriz_20Delta" REAL NOT NULL,
    "BreakVer_20Delta" REAL NOT NULL,
    "BreakHoriz_25Delta" REAL NOT NULL,
    "BreakVer_25Delta" REAL NOT NULL,

    "BreakHoriz_Delta" REAL NOT NULL,
    "BreakVert_Delta" REAL NOT NULL,
    "BreakIVB_Delta" REAL NOT NULL,
    "Vel_Delta" REAL NOT NULL,

    PRIMARY KEY("GameId", "PitchId", "FastballPitchType")
);

CREATE INDEX idx_PitchFlightpathGameData_PitcherGamePitch ON PitchFlightpathGameDelta
(
    "PitcherId", "GameId", "FastballPitchType"
);