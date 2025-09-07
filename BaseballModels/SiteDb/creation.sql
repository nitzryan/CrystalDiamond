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
	PRIMARY KEY("mlbId")
);

CREATE TABLE "HitterStats" (
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

CREATE TABLE "PitcherStats" (
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

CREATE TABLE "PlayerModel" (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"modelName" TEXT NOT NULL,
	"probs" TEXT NOT NULL,
	"rank" INTEGER,
	PRIMARY KEY("mlbId","year","month","modelName")
);

CREATE TABLE "PlayerRank" (
	"mlbId" INTEGER NOT NULL,
	"modelName" TEXT NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"war" REAL NOT NULL,
	"teamId" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	"rank" INTEGER NOT NULL,
	"teamRank" INTEGER NOT NULL,
	"highestLevel" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year", "month", "modelName")
);

CREATE INDEX "idx_PlayerRankOverall" ON "PlayerRank" (
	"year", "month", "rank"
);

CREATE INDEX idx_PlayerRankTeam ON "PlayerRank" (
	"teamId", "year", "month", "teamRank"
);