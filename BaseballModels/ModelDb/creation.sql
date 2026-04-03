CREATE TABLE "Output_PlayerWar" (
	"mlbId"	INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"war0"	REAL NOT NULL,
	"war1"	REAL NOT NULL,
	"war2"	REAL NOT NULL,
	"war3"	REAL NOT NULL,
	"war4"	REAL NOT NULL,
	"war5"	REAL NOT NULL,
	"war6"	REAL NOT NULL,
	"war" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "isHitter", "ModelIdx","year","month")
);

CREATE INDEX "idx_Output_PlayerWar" ON "Output_PlayerWar" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month"
);

CREATE TABLE Output_HitterStats (
	"MlbId" INTEGER NOT NULL,
	"Model" INTEGER NOT NULL,
	"ModelIdx" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"LevelId" INTEGER NOT NULL,
	"Pa" INTEGER NOT NULL,
	"Hit1B" REAL NOT NULL,
	"Hit2B" REAL NOT NULL,
	"Hit3B" REAL NOT NULL,
	"HitHR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL, 
	"SB" REAL NOT NULL,
	"CS" REAL NOT NULL,
	"BSR" REAL NOT NULL,
	"DRAA" REAL NOT NULL,
	"ParkRunFactor" REAL NOT NULL,
	"PercC"	REAL NOT NULL,
	"Perc1B"	REAL NOT NULL,
	"Perc2B"	REAL NOT NULL,
	"Perc3B"	REAL NOT NULL,
	"PercSS"	REAL NOT NULL,
	"PercLF"	REAL NOT NULL,
	"PercCF"	REAL NOT NULL,
	"PercRF"	REAL NOT NULL,
	"PercDH"	REAL NOT NULL,
	PRIMARY KEY("MlbId", "Model", "ModelIdx", "Year", "Month", "LevelId")
);

CREATE INDEX "idx_Output_HitterStats" ON "Output_HitterStats" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month",
	"levelId"
);

CREATE TABLE "Output_PitcherStats" (
	"mlbId" INTEGER NOT NULL,
	"Model" INTEGER NOT NULL,
	"ModelIdx" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"Outs_SP" REAL NOT NULL,
	"Outs_RP" REAL NOT NULL,
	"GS" REAL NOT NULL,
	"GR" REAL NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL,
	"ParkRunFactor" REAL NOT NULL,
	"SP_Perc" REAL NOT NULL,
	"RP_Perc" REAL NOT NULL,
	PRIMARY KEY("MlbId", "Model", "ModelIdx", "Year", "Month", "LevelId")
);

CREATE INDEX "idx_Output_PitcherStats" ON "Output_PitcherStats" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month",
	"levelId"
);

CREATE TABLE "Output_College" (
	"tbcId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"draft0"	REAL NOT NULL,
	"draft1"	REAL NOT NULL,
	"draft2"	REAL NOT NULL,
	"draft3"	REAL NOT NULL,
	"draft4"	REAL NOT NULL,
	"draft5"	REAL NOT NULL,
	"draft6"	REAL NOT NULL,
	"draft" REAL NOT NULL,
	PRIMARY KEY("tbcId", "model", "isHitter", "ModelIdx","year")
);

CREATE INDEX "idx_Output_College" ON "Output_College" (
	"model",
	"modelIdx",
	"tbcId",
	"year"
);

-- Training data
CREATE TABLE "Model_TrainingHistory" (
	"ModelName"	TEXT NOT NULL,
	"IsHitter"	INTEGER NOT NULL,
	"TestLoss"	REAL NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	"NumLayers" INTEGER NOT NULL,
	"HiddenSize" INTEGER NOT NULL,
	PRIMARY KEY("ModelName","ModelIdx","IsHitter")
);

CREATE TABLE "PlayersInTrainingData" (
	"mlbId" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	PRIMARY KEY ("mlbId", "modelIdx")
);

CREATE TABLE "ModelIdx" (
	"id" INTEGER NOT NULL,
	"pitcherModelName" TEXT NOT NULL,
	"hitterModelName" TEXT NOT NULL,
	"modelName" TEXT NOT NULL,
	PRIMARY KEY("id")
);

CREATE TABLE "Model_TrainingHistory_College" (
	"ModelName"	TEXT NOT NULL,
	"IsHitter"	INTEGER NOT NULL,
	"TestLoss"	REAL NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	"NumLayers" INTEGER NOT NULL,
	"HiddenSize" INTEGER NOT NULL,
	PRIMARY KEY("ModelName","ModelIdx","IsHitter")
);

CREATE TABLE "PlayersInTrainingData_College" (
	"tbcId" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	PRIMARY KEY ("tbcId", "modelIdx")
);

CREATE TABLE "ModelIdx_College" (
	"id" INTEGER NOT NULL,
	"pitcherModelName" TEXT NOT NULL,
	"hitterModelName" TEXT NOT NULL,
	"modelName" TEXT NOT NULL,
	PRIMARY KEY("id")
);