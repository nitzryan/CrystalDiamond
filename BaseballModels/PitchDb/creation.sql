-- Individual pitch data from model
CREATE TABLE Output_PitchValue (
	"model" INTEGER NOT NULL,
	"gameId" INTEGER NOT NULL,
	"pitchId" INTEGER NOT NULL,
	"ModelRun"	INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	
	-- Location
	"locationCalledStrike" REAL NOT NULL,
	"locationBall" REAL NOT NULL,
	"locationHBP" REAL NOT NULL,
	"locationSwing" REAL NOT NULL,

	"locationWhiff" REAL NOT NULL,
	"locationFoul" REAL NOT NULL,
	"locationInPlay" REAL NOT NULL,

	"locationInPlayExpected" REAL NOT NULL,

	-- Stuff
	"stuffCalledStrike" REAL NOT NULL,
	"stuffBall" REAL NOT NULL,
	"stuffHBP" REAL NOT NULL,
	"stuffSwing" REAL NOT NULL,

	"stuffWhiff" REAL NOT NULL,
	"stuffFoul" REAL NOT NULL,
	"stuffInPlay" REAL NOT NULL,

	"stuffInPlayExpected" REAL NOT NULL,

	-- Combined
	"combinedCalledStrike" REAL NOT NULL,
	"combinedBall" REAL NOT NULL,
	"combinedHBP" REAL NOT NULL,
	"combinedSwing" REAL NOT NULL,

	"combinedWhiff" REAL NOT NULL,
	"combinedFoul" REAL NOT NULL,
	"combinedInPlay" REAL NOT NULL,

	"combinedInPlayExpected" REAL NOT NULL,

	PRIMARY KEY("model", "gameId", "pitchId", "ModelRun")
);

CREATE INDEX idx_Output_PitchValue ON Output_PitchValue
(
	"gameId", "pitchId", "model"
);

CREATE TABLE Output_PitchValueAggregation (
	"model" INTEGER NOT NULL,
	"gameId" INTEGER NOT NULL,
	"pitchId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	
	-- Location
	"locationCalledStrike" REAL NOT NULL,
	"locationBall" REAL NOT NULL,
	"locationHBP" REAL NOT NULL,
	"locationSwing" REAL NOT NULL,

	"locationWhiff" REAL NOT NULL,
	"locationFoul" REAL NOT NULL,
	"locationInPlay" REAL NOT NULL,

	"locationInPlayExpected" REAL NOT NULL,

	-- Stuff
	"stuffCalledStrike" REAL NOT NULL,
	"stuffBall" REAL NOT NULL,
	"stuffHBP" REAL NOT NULL,
	"stuffSwing" REAL NOT NULL,

	"stuffWhiff" REAL NOT NULL,
	"stuffFoul" REAL NOT NULL,
	"stuffInPlay" REAL NOT NULL,

	"stuffInPlayExpected" REAL NOT NULL,

	-- Combined
	"combinedCalledStrike" REAL NOT NULL,
	"combinedBall" REAL NOT NULL,
	"combinedHBP" REAL NOT NULL,
	"combinedSwing" REAL NOT NULL,

	"combinedWhiff" REAL NOT NULL,
	"combinedFoul" REAL NOT NULL,
	"combinedInPlay" REAL NOT NULL,

	"combinedInPlayExpected" REAL NOT NULL,

	"locationRuns" REAL NOT NULL,
	"stuffRuns" REAL NOT NULL,
	"combinedRuns" REAL NOT NULL,

	PRIMARY KEY("model", "gameId", "pitchId")
);

CREATE INDEX idx_Output_PitchValueAggregation ON Output_PitchValueAggregation
(
	"gameId", "pitchId", "model"
);

CREATE TABLE Models_PitchValue
(
	"Id" INTEGER NOT NULL,
	"Name" TEXT NOT NULL,

	PRIMARY KEY("Id")
);

CREATE TABLE ModelTrainingHistory_PitchValue
(
	"ModelId" INTEGER NOT NULL,
	"ModelRun" INTEGER NOT NULL,
	
	"PitchType" INTEGER NOT NULL,
	
	"LossLocationResult" REAL NOT NULL,
	"LossLocationSwing" REAL NOT NULL,
	"LossLocationInplay" REAL NOT NULL,
	"LossStuffResult" REAL NOT NULL,
	"LossStuffSwing" REAL NOT NULL,
	"LossStuffInplay" REAL NOT NULL,
	"LossCombinedResult" REAL NOT NULL,
	"LossCombinedSwing" REAL NOT NULL,
	"LossCombinedInplay" REAL NOT NULL,
	
	"Arch" TEXT NOT NULL,

	PRIMARY KEY("ModelId", "ModelRun", "PitchType")
);

CREATE TABLE YearLeagueDeviations
(
	"ModelId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,

	"ActDev" REAL NOT NULL,
	"StuffDev" REAL NOT NULL,
	"LocDev" REAL NOT NULL,
	"PitchDev" REAL NOT NULL,

	PRIMARY KEY("ModelId", "Year")
);

-- Pitcher/Game Model Runs
CREATE TABLE PitcherStuff
(
	"MlbId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"GameId" INTEGER NOT NULL,
	"PitchType" INTEGER NOT NULL,
	"Scenario" INTEGER NOT NULL,

	"NumPitches" INTEGER NOT NULL,
	"ValueStuff" REAL NOT NULL,
	"ValueLoc" REAL NOT NULL,
	"ValueCombined" REAL NOT NULL,

	"Vel" REAL NOT NULL,
	"BreakHoriz" REAL NOT NULL,
	"BreakVert" REAL NOT NULL,

	PRIMARY KEY("MlbId", "Year", "Month", "PitchType", "Scenario", "GameId")
);

CREATE INDEX idx_PitcherStuffGame ON PitcherStuff
(
	"MlbId", "GameId", "PitchType", "Scenario"
);