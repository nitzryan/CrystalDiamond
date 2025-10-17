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
	"HR9" REAL NOT NULL,
	"BBPerc" REAL NOT NULL,
	"KPerc" REAL NOT NULL,
	"GOPerc" REAL NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "teamId", "leagueId")
);

CREATE TABLE "PlayerModel" (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"probsWar" TEXT NOT NULL,
	"probsValue" TEXT NOT NULL,
	"rankWar" INTEGER,
	"rankValue" INTEGER,
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
	"value" REAL NOT NULL,
	"rankValue" INTEGER NOT NULL,
	"teamRankValue" INTEGER NOT NULL,
	"highestLevel" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "modelId", "isHitter")
);

CREATE TABLE "HitterWarRank" (
	"mlbId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	"war" REAL NOT NULL,
	"rankWar" INTEGER NOT NULL,
	"pa" REAL NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "modelId")
);

CREATE TABLE "PitcherWarRank" (
	"mlbId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"teamId" INTEGER NOT NULL,
	"spWar" REAL NOT NULL,
	"spIP" REAL NOT NULL,
	"rpWar" REAL NOT NULL,
	"rpIP" REAL NOT NULL,
	"spRank" INTEGER,
	"rpRank" INTEGER,
	PRIMARY KEY("mlbId", "year", "month", "modelId")
);

CREATE TABLE "TeamRank" (
	"teamId" INTEGER NOT NULL,
	"modelId" INTEGER NOT NULL,
	"isWar" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"value" REAL NOT NULL,
	"highestRank" INTEGER NOT NULL,
	"top10" INTEGER NOT NULL,
	"top50" INTEGER NOT NULL,
	"top100" INTEGER NOT NULL,
	"top200" INTEGER NOT NULL,
	"top500" INTEGER NOT NULL,
	"rank" INTEGER NOT NULL,
	PRIMARY KEY("teamId", "year", "month", "modelId", "isWar")
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

CREATE INDEX "idx_PlayerRankOverallValue" ON "PlayerRank" (
	"year", "month", "rankValue"
);

CREATE INDEX "idx_PlayerRankTeamWar" ON "PlayerRank" (
	"teamId", "year", "month", "teamRankWar"
);

CREATE INDEX "idx_PlayerRankTeamValue" ON "PlayerRank" (
	"teamId", "year", "month", "teamRankValue"
);