-- Individual pitch data from model
CREATE TABLE Output_PitchValue (
	"model" INTEGER NOT NULL,
	"gameId" INTEGER NOT NULL,
	"pitchId" INTEGER NOT NULL,
	"ModelRun"	INTEGER NOT NULL,
	
	"absValue" REAL NOT NULL,
	"stuffOnly" REAL NOT NULL,
	"locationOnly" REAL NOT NULL,
	"combined" REAL NOT NULL,

	PRIMARY KEY("model", "gameId", "pitchId", "ModelRun")
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
	
	"LossLocation" REAL NOT NULL,
	"LossStuff" REAL NOT NULL,
	"LossCombined" REAL NOT NULL,
	
	"Arch" TEXT NOT NULL,

	PRIMARY KEY("ModelId", "ModelRun")
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