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