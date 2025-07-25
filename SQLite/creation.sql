CREATE TABLE "League_Factors" (
	"LeagueId"	INTEGER,
	"Year"	INTEGER,
	"RunFactor"	INTEGER,
	"HRFactor"	INTEGER,
	PRIMARY KEY("LeagueId","Year")
);

CREATE TABLE "Level_Factors" (
	"LevelId"	INTEGER,
	"Year"	INTEGER,
	"RunFactor"	INTEGER,
	"HRFactor"	INTEGER,
	PRIMARY KEY("LevelId","Year")
);

CREATE TABLE "Level_HitterStats" (
	"LevelId"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"AVG"	REAL,
	"OBP"	REAL,
	"SLG"	REAL,
	"ISO"	REAL,
	"wOBA"	REAL,
	"HRPerc"	REAL,
	"BBPerc"	REAL,
	"KPerc"	REAL,
	"SBRate"	REAL,
	"SBPerc"	REAL,
	PRIMARY KEY("LevelId","Year","Month")
);

CREATE TABLE "Level_PitcherStats" (
	"Level"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"ERA"	REAL,
	"RA"	REAL,
	"FipConstant"	REAL,
	"wOBA"	REAL,
	"hrPerc"	REAL,
	"bbPerc"	REAL,
	"kPerc"	REAL,
	"goPerc"	REAL,
	"avg"	REAL,
	"iso"	REAL,
	PRIMARY KEY("Level","Year","Month")
);

CREATE TABLE "Model_HitterStats" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"Age"	REAL,
	"PA"	INTEGER,
	"Level"	INTEGER,
	"ParkRunFactor"	REAL,
	"ParkHRFactor"	REAL,
	"avgRatio"	REAL,
	"obpRatio"	REAL,
	"isoRatio"	REAL,
	"wOBARatio"	REAL,
	"sbRateRatio"	REAL,
	"sbPercRatio"	REAL,
	"hrPercRatio"	REAL,
	"bbPercRatio"	REAL,
	"kPercRatio"	REAL,
	"PercC"	REAL,
	"Perc1B"	REAL,
	"Perc2B"	REAL,
	"Perc3B"	REAL,
	"PercSS"	REAL,
	"PercLF"	REAL,
	"PercCF"	REAL,
	"PercRF"	REAL,
	"PercDH"	REAL,
	PRIMARY KEY("mlbId","Year","Month")
);

CREATE TABLE "Model_PitcherStats" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"Age"	REAL,
	"BF"	INTEGER,
	"Level"	REAL,
	"ParkRunFactor"	REAL,
	"ParkHRFactor"	REAL,
	"GBPercRatio"	REAL,
	"ERARatio"	REAL,
	"FIPRatio"	REAL,
	"wOBARatio"	REAL,
	"hrPercRatio"	REAL,
	"bbPercRatio"	REAL,
	"kPercRatio"	REAL,
	PRIMARY KEY("mlbId","Year","Month")
);

CREATE TABLE "Model_PlayerWar" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	"isHitter"	INTEGER,
	"pa"	INTEGER,
	"war"	REAL,
	"off"	REAL,
	"def"	REAL,
	"bsr"	REAL,
	PRIMARY KEY("mlbId","Year","isHitter")
);

CREATE TABLE "Model_Players" (
	"mlbId"	INTEGER,
	"isHitter"	INTEGER,
	"isPitcher"	INTEGER,
	"lastProspectYear"	INTEGER,
	"lastProspectMonth"	INTEGER,
	"lastMLBSeason"	INTEGER,
	"ageAtSigningYear"	REAL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Model_TrainingHistory" (
	"ModelName"	TEXT,
	"Year"	INTEGER,
	"IsHitter"	INTEGER,
	"TestLoss"	REAL,
	"NumLayers"	INTEGER,
	"HiddenSize"	INTEGER,
	"ModelIdx"	INTEGER,
	PRIMARY KEY("ModelName","Year","IsHitter")
);

CREATE TABLE "Output_PlayerWar" (
	"mlbId"	INTEGER,
	"modelIdx"	INTEGER,
	"year"	INTEGER,
	"month"	INTEGER,
	"prob0"	REAL,
	"prob1"	REAL,
	"prob2"	REAL,
	"prob3"	REAL,
	"prob4"	REAL,
	"prob5"	REAL,
	"prob6"	REAL,
	PRIMARY KEY("mlbId","modelIdx","year","month")
);

CREATE TABLE "Park_Factors" (
	"TeamId"	INTEGER,
	"LeagueId"	INTEGER,
	"LevelId"	INTEGER,
	"Year"	INTEGER,
	"RunFactor"	INTEGER,
	"HRFactor"	INTEGER,
	PRIMARY KEY("TeamId","Year")
);

CREATE TABLE "Park_ScoringData" (
	"TeamId"	INTEGER,
	"Year"	INTEGER,
	"LeagueId"	INTEGER,
	"LevelId"	INTEGER,
	"HomePa"	INTEGER,
	"HomeOuts"	INTEGER,
	"HomeRuns"	INTEGER,
	"HomeHRs"	INTEGER,
	"AwayPa"	INTEGER,
	"AwayOuts"	INTEGER,
	"AwayRuns"	INTEGER,
	"AwayHRs"	INTEGER,
	PRIMARY KEY("TeamId","Year","LeagueId")
);

CREATE TABLE "Player" (
	"mlbId"	INTEGER,
	"position"	TEXT,
	"fangraphsId"	INTEGER,
	"birthYear"	INTEGER,
	"birthMonth"	INTEGER,
	"birthDate"	INTEGER,
	"draftPick"	INTEGER,
	"draftBonus"	INTEGER,
	"signingYear"	INTEGER,
	"signingMonth"	INTEGER,
	"signingDate"	INTEGER,
	"signingBonus"	INTEGER,
	"bats"	TEXT,
	"throws"	TEXT,
	"isRetired"	INTEGER,
	"useFirstName"	TEXT,
	"useLastName"	TEXT, parentOrgId INTEGER,
	PRIMARY KEY("mlbId","position")
);

CREATE TABLE "Player_CareerStatus" (
	"mlbId"	INTEGER,
	"isPitcher"	INTEGER,
	"isHitter"	INTEGER,
	"isActive"	INTEGER,
	"serviceReached"	INTEGER,
	"mlbStartYear"	INTEGER,
	"mlbRookieYear"	INTEGER,
	"mlbRookieMonth"	INTEGER,
	"serviceEndYear"	INTEGER,
	"serviceLapseYear"	INTEGER,
	"careerStartYear"	INTEGER,
	"agedOut"	INTEGER,
	"ignorePlayer"	INTEGER,
	"highestLevel"	INTEGER,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Player_Hitter_GameLog" (
	"gameLogId"	INTEGER,
	"gameId"	INTEGER,
	"mlbId"	INTEGER,
	"Day"	INTEGER,
	"Month"	INTEGER,
	"Year"	INTEGER,
	"AB"	INTEGER,
	"H"	INTEGER,
	"2B"	INTEGER,
	"3B"	INTEGER,
	"HR"	INTEGER,
	"K"	INTEGER,
	"BB"	INTEGER,
	"SB"	INTEGER,
	"CS"	INTEGER,
	"HBP"	INTEGER,
	"Position"	INTEGER,
	"Level"	INTEGER,
	"HomeTeamId"	INTEGER,
	"TeamId"	INTEGER,
	"LeagueId"	INTEGER,
	PRIMARY KEY("gameLogId")
);

CREATE TABLE "Player_Hitter_MonthAdvanced" (
	"mlbId"	INTEGER,
	"levelId"	INTEGER,
	"year"	INTEGER,
	"month"	INTEGER,
	"teamId"	INTEGER,
	"leagueId"	INTEGER,
	"PA"	INTEGER,
	"AVG"	REAL,
	"OBP"	REAL,
	"SLG"	REAL,
	"ISO"	REAL,
	"wOBA"	REAL,
	"wRC"	REAL,
	"HRPerc"	REAL,
	"BBPerc"	REAL,
	"KPerc"	REAL,
	"SBRate"	REAL,
	"SBPerc"	REAL,
	PRIMARY KEY("mlbId","levelId","year","month","teamId","leagueId")
);

CREATE TABLE "Player_Hitter_MonthStats" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"LevelId"	INTEGER,
	"AB"	INTEGER,
	"H"	INTEGER,
	"2B"	INTEGER,
	"3B"	INTEGER,
	"HR"	INTEGER,
	"K"	INTEGER,
	"BB"	INTEGER,
	"SB"	INTEGER,
	"CS"	INTEGER,
	"HBP"	INTEGER,
	"ParkRunFactor"	INTEGER,
	"ParkHRFactor"	INTEGER,
	"GamesC"	INTEGER,
	"Games1B"	INTEGER,
	"Games2B"	INTEGER,
	"Games3B"	INTEGER,
	"GamesSS"	INTEGER,
	"GamesLF"	INTEGER,
	"GamesCF"	INTEGER,
	"GamesRF"	INTEGER,
	"GamesDH"	INTEGER,
	PRIMARY KEY("mlbId","Year","Month","LevelId")
);

CREATE TABLE "Player_Hitter_MonthlyRatios" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	"Month"	INTEGER,
	"Level"	INTEGER,
	"avgRatio"	REAL,
	"obpRatio"	REAL,
	"isoRatio"	REAL,
	"wOBARatio"	REAL,
	"sbRateRatio"	REAL,
	"sbPercRatio"	REAL,
	"hrPercRatio"	REAL,
	"bbPercRatio"	REAL,
	"kPercRatio"	REAL,
	"PercC"	REAL,
	"Perc1B"	REAL,
	"Perc2B"	REAL,
	"Perc3B"	REAL,
	"PercSS"	REAL,
	"PercLF"	REAL,
	"PercCF"	REAL,
	"PercRF"	REAL,
	"PercDH"	REAL,
	PRIMARY KEY("mlbId","Year","Month","Level")
);

CREATE TABLE "Player_Hitter_YearAdvanced" (
	"mlbId"	INTEGER,
	"levelId"	INTEGER,
	"year"	INTEGER,
	"teamId"	INTEGER,
	"leagueId"	INTEGER,
	"PA"	INTEGER,
	"AVG"	REAL,
	"OBP"	REAL,
	"SLG"	REAL,
	"ISO"	REAL,
	"wOBA"	REAL,
	"wRC"	REAL,
	"HR"	INTEGER,
	"BBPerc"	REAL,
	"KPerc"	REAL,
	"SB"	INTEGER,
	"CS"	INTEGER,
	PRIMARY KEY("mlbId","levelId","year","teamId","leagueId")
);

CREATE TABLE "Player_OrgMap" (
	"mlbId"	INTEGER,
	"year"	INTEGER,
	"month"	INTEGER,
	"parentOrgId"	INTEGER,
	PRIMARY KEY("year","mlbId","month")
);

CREATE TABLE "Player_Pitcher_GameLog" (
	"gameLogId"	INTEGER,
	"gameId"	INTEGER,
	"mlbId"	INTEGER,
	"day"	INTEGER,
	"month"	INTEGER,
	"year"	INTEGER,
	"battersFaced"	INTEGER,
	"outs"	INTEGER,
	"go"	INTEGER,
	"ao"	INTEGER,
	"r"	INTEGER,
	"er"	INTEGER,
	"h"	INTEGER,
	"k"	INTEGER,
	"bb"	INTEGER,
	"hbp"	INTEGER,
	"2B"	INTEGER,
	"3B"	INTEGER,
	"HR"	INTEGER,
	"level"	INTEGER,
	"homeTeamId"	INTEGER,
	"TeamId"	INTEGER,
	"LeagueId"	INTEGER,
	PRIMARY KEY("gameLogId")
);

CREATE TABLE "Player_Pitcher_MonthAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"BF"	INTEGER NOT NULL,
	"Outs"	INTEGER NOT NULL,
	"GBRatio"	REAL NOT NULL,
	"ERA"	REAL NOT NULL,
	"FIP"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"HRPerc"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	PRIMARY KEY("leagueId","teamId","month","year","levelId","mlbId")
);

CREATE TABLE "Player_Pitcher_MonthStats" (
	"mlbId"	INTEGER,
	"year"	INTEGER,
	"month"	INTEGER,
	"level"	INTEGER,
	"battersFaced"	INTEGER,
	"outs"	INTEGER,
	"go"	INTEGER,
	"ao"	INTEGER,
	"r"	INTEGER,
	"er"	INTEGER,
	"h"	INTEGER,
	"k"	INTEGER,
	"bb"	INTEGER,
	"hbp"	INTEGER,
	"2B"	INTEGER,
	"3B"	INTEGER,
	"HR"	INTEGER,
	"RunFactor"	INTEGER,
	"HRFactor"	INTEGER,
	PRIMARY KEY("mlbId","year","month","level")
);

CREATE TABLE "Player_Pitcher_MonthlyRatios" (
	"mlbId"	INTEGER,
	"year"	INTEGER,
	"month"	INTEGER,
	"level"	INTEGER,
	"GBPercRatio"	REAL,
	"ERARatio"	REAL,
	"FIPRatio"	REAL,
	"wOBARatio"	REAL,
	"hrPercRatio"	REAL,
	"bbPercRatio"	REAL,
	"kPercRatio"	REAL,
	PRIMARY KEY("mlbId","year","month","level")
);

CREATE TABLE "Player_Pitcher_YearAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"BF"	INTEGER NOT NULL,
	"Outs"	INTEGER NOT NULL,
	"GBRatio"	REAL NOT NULL,
	"ERA"	REAL NOT NULL,
	"FIP"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"HR"	INTEGER NOT NULL,
	"wOBA"	REAL NOT NULL,
	PRIMARY KEY("leagueId","teamId","year","levelId","mlbId")
);

CREATE TABLE "Player_ServiceLapse" (
	"mlbId"	INTEGER,
	"Year"	INTEGER,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Player_ServiceTime" (
	"mlbId"	INTEGER,
	"year"	INTEGER,
	"serviceYear"	INTEGER,
	"serviceDays"	INTEGER,
	PRIMARY KEY("mlbId","year")
);

CREATE TABLE "Player_YearlyWar" (
	"mlbId"	INTEGER,
	"year"	INTEGER,
	"position"	INTEGER,
	"pa"	INTEGER,
	"war"	REAL,
	"off"	REAL,
	"def"	REAL,
	"bsr"	REAL,
	PRIMARY KEY("mlbId","year","position")
);

CREATE TABLE "Pre05_Players" (
	"mlbId"	INTEGER,
	"careerStartYear"	INTEGER,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Team_League_Map" (
	"TeamId"	INTEGER,
	"LeagueId"	INTEGER,
	"Year"	INTEGER,
	PRIMARY KEY("TeamId","LeagueId","Year")
);

CREATE TABLE "Team_OrganizationMap" (
	"teamId"	INTEGER,
	"year"	INTEGER,
	"parentOrgId"	INTEGER,
	PRIMARY KEY("teamId","year")
);

CREATE TABLE "Team_Parents" (
	"id"	INTEGER,
	"abbr"	TEXT,
	"name"	TEXT,
	PRIMARY KEY("id")
);

CREATE INDEX "idx_HitterGameLog_Date" ON "Player_Hitter_GameLog" (
	"Year",
	"Month",
	"Day"
);

CREATE INDEX "idx_HitterGameLog_GamePlayer" ON "Player_Hitter_GameLog" (
	"gameId",
	"mlbId"
);

CREATE INDEX "idx_HitterGameLog_mlbIdDate" ON "Player_Hitter_GameLog" (
	"mlbId",
	"Year",
	"Month",
	"Day"
);

CREATE INDEX "idx_HitterMonthStats_LevelDate" ON "Player_Hitter_MonthStats" (
	"LevelId",
	"Year",
	"Month"
);

CREATE INDEX "idx_HitterMonthStats_PlayerDate" ON "Player_Hitter_MonthStats" (
	"mlbId",
	"Year",
	"Month",
	"LevelId"
);

CREATE INDEX "idx_PitcherAdvanced" ON "Player_Pitcher_MonthAdvanced" (
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_PitcherAdvancedYear" ON "Player_Pitcher_YearAdvanced" (
	"mlbId",
	"year"
);

CREATE INDEX "idx_PitcherGameLog_Date" ON "Player_Pitcher_GameLog" (
	"year",
	"month",
	"day"
);

CREATE INDEX "idx_PitcherGameLog_GamePlayer" ON "Player_Pitcher_GameLog" (
	"gameId",
	"mlbId"
);

CREATE INDEX "idx_PitcherGameLog_mlbIdDate" ON "Player_Pitcher_GameLog" (
	"mlbId",
	"year",
	"month",
	"day"
);

CREATE INDEX "idx_PitcherMonthStats_LevelDate" ON "Player_Pitcher_MonthStats" (
	"level",
	"year",
	"month"
);

CREATE INDEX "idx_PitcherMonthStats_PlayerDate" ON "Player_Pitcher_MonthStats" (
	"mlbId",
	"year",
	"month",
	"level"
);