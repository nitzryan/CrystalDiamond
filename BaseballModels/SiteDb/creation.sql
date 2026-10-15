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
	"war" REAL NOT NULL,
	"rankWar" INTEGER,
	"trainingBias" INTEGER NOT NULL,
	"timestepQuality" INTEGER NOT NULL,
	PRIMARY KEY("mlbId","modelId", "isHitter", "year","month")
);

CREATE TABLE PostProspectPlayerModel (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"war" REAL NOT NULL,

	PRIMARY KEY("mlbId","modelId", "isHitter", "year","month")
)

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
	"trainingBias" INTEGER NOT NULL,
	"timestepQuality" INTEGER NOT NULL,
	"acqType" INTEGER NOT NULL,
	"draftPick" INTEGER,
	PRIMARY KEY("mlbId", "year", "month", "modelId", "isHitter")
);

CREATE TABLE "DraftRank" (
	"tbcId" INTEGER NOT NULL,
	"mlbId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"Name" TEXT,
	"Position" TEXT,
	"BirthYear" INTEGER NOT NULL,
	"BirthMonth" INTEGER NOT NULL,
	"BirthDate" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"isEligible" INTEGER NOT NULL,
	"rankEligible" INTEGER NOT NULL,
	"warPre" REAL,
	"warPost" REAL,
	"draftPick" INTEGER,
	"draftTeamid" INTEGER,
	"trainingBias" INTEGER NOT NULL,
	"timestepQuality" INTEGER NOT NULL,
	PRIMARY KEY("tbcId", "modelId", "year", "isHitter", "mlbId")
);

CREATE INDEX idx_DraftRankMLBID on DraftRank
(
	"mlbId", "modelId"
);

CREATE TABLE "TeamRank" (
	-- PK Data
	"teamId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,

	-- Rank data
	"highestRank" INTEGER NOT NULL,
	"top10" INTEGER NOT NULL,
	"top50" INTEGER NOT NULL,
	"top100" INTEGER NOT NULL,
	"top200" INTEGER NOT NULL,
	"top500" INTEGER NOT NULL,
	"rank" INTEGER NOT NULL,
	"war" REAL NOT NULL,

	-- Breakdown by demo
	"warHitter" REAL NOT NULL,
	"warPitcher" REAL NOT NULL,
	"warDraftHitter" REAL NOT NULL,
	"warDraftPitcher" REAL NOT NULL,
	"draftCapitalHitter" REAL NOT NULL,
	"draftCapitalPitcher" REAL NOT NULL,
	"warSignHitter" REAL NOT NULL,
	"warSignPitcher" REAL NOT NULL,
	"warTradeHitter" REAL NOT NULL,
	"warTradePitcher" REAL NOT NULL,

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

CREATE TABLE "QualityCode" (
    "category" TEXT    NOT NULL,
    "code"     INTEGER NOT NULL,
    "severity" INTEGER NOT NULL,
    "label"    TEXT    NOT NULL,
    "blurb"    TEXT    NOT NULL,
    PRIMARY KEY("category", "code")
);

CREATE TABLE TeamDraftOverview (
	"ModelId" INTEGER NOT NULL,
	"DraftYear" INTEGER NOT NULL,
	"EvaluationYear" INTEGER NOT NULL,
	"TeamId" INTEGER NOT NULL,

	"HitterCapital" REAL NOT NULL,
	"PitcherCapital" REAL NOT NULL,
	"HitterValue" REAL NOT NULL,
	"PitcherValue" REAL NOT NULL,

	PRIMARY KEY("ModelId", "DraftYear", "EvaluationYear", "TeamId")
);

CREATE TABLE TeamDraftPlayer (
    "modelId"      INTEGER NOT NULL,
    "mlbId"        INTEGER NOT NULL,
    "isHitter"     INTEGER NOT NULL,
    "draftYear"    INTEGER NOT NULL,
    "draftPick"    INTEGER NOT NULL,
    "draftTeamId"  INTEGER NOT NULL,
    "initialWar"   REAL    NOT NULL,
    "warYear1"     REAL,
    "warYear2"     REAL,
    "warYear3"     REAL,
    "warYear4"     REAL,
    "warYear5"     REAL,
    "warYear6"     REAL,
    "currentWar"   REAL    NOT NULL,
	"postEligibleYear" INTEGER,
    PRIMARY KEY("modelId", "mlbId", "isHitter")
);

CREATE TABLE "HitterMlbWarModel" (
    "mlbId" INTEGER NOT NULL,
    "modelId" INTEGER NOT NULL,
    "year" INTEGER NOT NULL,
    "month" INTEGER NOT NULL,
    "probsWar" BLOB NOT NULL,      -- 56 x uint16
	"ranksWar" BLOB NOT NULL,      -- 7 x uint16
    PRIMARY KEY("mlbId","modelId","year","month")
) WITHOUT ROWID;
CREATE TABLE "PitcherMlbWarModel" (
    "mlbId" INTEGER NOT NULL,
    "modelId" INTEGER NOT NULL,
    "year" INTEGER NOT NULL,
    "month" INTEGER NOT NULL,
    "probsWarSP" BLOB NOT NULL,    -- 56 x uint16
    "probsWarRP" BLOB NOT NULL,    -- 56 x uint16
	"ranksWarSP" BLOB NOT NULL,    -- 7 x uint16
	"ranksWarRP" BLOB NOT NULL,    -- 7 x uint16
    PRIMARY KEY("mlbId","modelId","year","month")
) WITHOUT ROWID;

CREATE TABLE "HitterMlbWarBucketAverages" (
    "month" INTEGER NOT NULL PRIMARY KEY,
    "war0" REAL NOT NULL, "war1" REAL NOT NULL, "war2" REAL NOT NULL, "war3" REAL NOT NULL,
    "war4" REAL NOT NULL, "war5" REAL NOT NULL, "war6" REAL NOT NULL, "war7" REAL NOT NULL
);
CREATE TABLE "PitcherMlbWarBucketAverages" (
    "month" INTEGER NOT NULL PRIMARY KEY,
    "warSP0" REAL NOT NULL, "warSP1" REAL NOT NULL, "warSP2" REAL NOT NULL, "warSP3" REAL NOT NULL,
    "warSP4" REAL NOT NULL, "warSP5" REAL NOT NULL, "warSP6" REAL NOT NULL, "warSP7" REAL NOT NULL,
    "warRP0" REAL NOT NULL, "warRP1" REAL NOT NULL, "warRP2" REAL NOT NULL, "warRP3" REAL NOT NULL,
    "warRP4" REAL NOT NULL, "warRP5" REAL NOT NULL, "warRP6" REAL NOT NULL, "warRP7" REAL NOT NULL
);

CREATE INDEX idx_TeamDraftPlayer_DraftYear ON TeamDraftPlayer
(
	"draftYear", "modelId"
);

CREATE TABLE ModelDraftPickValues
(
	"Pick" INTEGER NOT NULL,
	"WarHitter" REAL NOT NULL,
	"WarPitcher" REAL NOT NULL,

	PRIMARY KEY("Pick")
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