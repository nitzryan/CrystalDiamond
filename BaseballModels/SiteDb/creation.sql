CREATE TABLE "Player" (
	"mlbId" INTEGER NOT NULL,
	"firstName" TEXT NOT NULL,
	"lastName" TEXT NOT NULL,
	"birthYear" INTEGER NOT NULL,
	"birthMonth" INTEGER NOT NULL,
	"birthDate" INTEGER NOT NULL,
	"startYear" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	"status" TEXT NOT NULL,
	"orgId" INTEGER NOT NULL,
	"draftPick" INTEGER,
	"draftRound" TEXT,
	"draftBonus" INTEGER,
	"isHitter" INTEGER NOT NULL,
	"isPitcher" INTEGER NOT NULL,
	"inTraining" INTEGER NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "HitterYearStats" (
	"mlbId" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"leagueId" INTEGER NOT NULL,
	"PA" INTEGER NOT NULL,
	"AVG" REAL NOT NULL,
	"OBP" REAL NOT NULL,
	"SLG" REAL NOT NULL,
	"ISO" REAL NOT NULL,
	"WRC" INTEGER NOT NULL,
	"HR" INTEGER NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"SB" INTEGER NOT NULL,
	"CS" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year", "teamId", "leagueId")
);

CREATE TABLE "HitterMonthStats" (
	"mlbId" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"leagueId" INTEGER NOT NULL,
	"PA" INTEGER NOT NULL,
	"AVG" REAL NOT NULL,
	"OBP" REAL NOT NULL,
	"SLG" REAL NOT NULL,
	"ISO" REAL NOT NULL,
	"WRC" INTEGER NOT NULL,
	"HR" INTEGER NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"SB" INTEGER NOT NULL,
	"CS" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "teamId", "leagueId")
);

CREATE TABLE "PitcherYearStats" (
	"mlbId" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"leagueId" INTEGER NOT NULL,
	"IP" TEXT NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"ERAMinus" REAL NOT NULL,
	"FIPMinus" REAL NOT NULL,
	"HR9" REAL NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"GOPerc" REAL NOT NULL,
	PRIMARY KEY("mlbId", "year", "teamId", "leagueId")
);

CREATE TABLE "PitcherMonthStats" (
	"mlbId" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"leagueId" INTEGER NOT NULL,
	"IP" TEXT NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"ERAMinus" REAL NOT NULL,
	"FIPMinus" REAL NOT NULL,
	"HR9" REAL NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"GOPerc" REAL NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "teamId", "leagueId")
);

CREATE TABLE Prediction_HitterStats (
	"MlbId" INTEGER NOT NULL,
	"Model" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"LevelId" INTEGER NOT NULL,
	"Pa" REAL NOT NULL,
	"Hit1B" REAL NOT NULL,
	"Hit2B" REAL NOT NULL,
	"Hit3B" REAL NOT NULL,
	"HitHR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL, 
	"SB" REAL NOT NULL,
	"CS" REAL NOT NULL,
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
	"AVG" REAL NOT NULL,
	"OBP" REAL NOT NULL,
	"SLG" REAL NOT NULL,
	"ISO" REAL NOT NULL,
	"wRC" REAL NOT NULL,
	"crOFF" REAL NOT NULL,
	"crBSR" REAL NOT NULL,
	"crDEF" REAL NOT NULL,
	"crDPOS" REAL NOT NULL,
	"crDRAA" REAL NOT NULL,
	"crWAR" REAL NOT NULL,
	PRIMARY KEY("MlbId", "Model", "Year", "Month", "LevelId")
);

CREATE TABLE "Prediction_PitcherStats" (
	"mlbId" INTEGER NOT NULL,
	"Model" INTEGER NOT NULL,
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
	"HR9" REAL NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"ERAMinus" REAL NOT NULL,
	"FIPMinus" REAL NOT NULL,
	"ParkRunFactor" REAL NOT NULL,
	"SP_Perc" REAL NOT NULL,
	"RP_Perc" REAL NOT NULL,
	"crRAA" REAL NOT NULL,
	"crWAR" REAL NOT NULL,
	PRIMARY KEY("MlbId", "Model", "Year", "Month", "LevelId")
);

CREATE TABLE "PlayerModel" (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"probsWar" TEXT NOT NULL,
	"rankWar" INTEGER,
	PRIMARY KEY("mlbId","year","month","modelId", "isHitter")
);

CREATE TABLE "PlayerRank" (
	"mlbId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	"war" REAL NOT NULL,
	"rankWar" INTEGER NOT NULL,
	"teamRankWar" INTEGER NOT NULL,
	"highestLevel" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "modelId", "isHitter")
);

CREATE TABLE "TeamRank" (
	"teamId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"highestRank" INTEGER NOT NULL,
	"top10" INTEGER NOT NULL,
	"top50" INTEGER NOT NULL,
	"top100" INTEGER NOT NULL,
	"top200" INTEGER NOT NULL,
	"top500" INTEGER NOT NULL,
	"rank" INTEGER NOT NULL,
	"war" REAL NOT NULL,
	PRIMARY KEY("teamId", "year", "month", "modelId")
);

CREATE TABLE "Models" (
	"modelId" INTEGER NOT NULL,
	"name" TEXT NOT NULL,
	PRIMARY KEY("modelId")
);

CREATE TABLE "PlayerYearPositions" (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	PRIMARY KEY("mlbId", "isHitter", "year")
);

CREATE TABLE "HomeData" (
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"rankType" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isWar" INTEGER NOT NULL,
	"mlbId" INTEGER NOT NULL,
	"data" TEXT NOT NULL,
	"rank" INTEGER NOT NULL,
	PRIMARY KEY("year", "month", "modelId", "isWar", "rankType", "rank")
);

CREATE TABLE "HomeDataType" (
	"type" INTEGER NOT NULL,
	"name" TEXT NOT NULL,
	PRIMARY KEY("type")
);

CREATE INDEX "idx_PlayerRankOverallWar" ON "PlayerRank" (
	"year", "month", "rankWar"
);

CREATE INDEX "idx_PlayerRankTeamWar" ON "PlayerRank" (
	"teamId", "year", "month", "teamRankWar"
);

CREATE INDEX "idx_Prediction_HitterStats_PlayerDate" ON "Prediction_HitterStats" (
	"mlbId", "year", "month"
);

CREATE INDEX "idx_Prediction_PitcherStats_PlayerDate" ON "Prediction_PitcherStats" (
	"mlbId", "year", "month"
);

CREATE INDEX "idx_Prediction_HitterStats_ModelDateLevel" ON "Prediction_HitterStats" (
	"model", "year", "month", "levelId", "crWar"
);

CREATE INDEX "idx_Prediction_PitcherStats_ModelDateLevel" ON "Prediction_PitcherStats" (
	"model", "year", "month", "levelId", "crWar"
);