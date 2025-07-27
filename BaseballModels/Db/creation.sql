CREATE TABLE "League_Factors" (
	"LeagueId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"RunFactor"	REAL NOT NULL,
	"HRFactor"	REAL NOT NULL,
	PRIMARY KEY("LeagueId","Year")
);

CREATE TABLE "Level_Factors" (
	"LevelId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"RunFactor"	REAL NOT NULL,
	"HRFactor"	REAL NOT NULL,
	PRIMARY KEY("LevelId","Year")
);

CREATE TABLE "Level_HitterStats" (
	"LevelId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"AVG"	REAL NOT NULL,
	"OBP"	REAL NOT NULL,
	"SLG"	REAL NOT NULL,
	"ISO"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	"HRPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"SBRate"	REAL NOT NULL,
	"SBPerc"	REAL NOT NULL,
	PRIMARY KEY("LevelId","Year","Month")
);

CREATE TABLE "Level_PitcherStats" (
	"Level"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"ERA"	REAL NOT NULL,
	"RA"	REAL NOT NULL,
	"FipConstant"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	"hrPerc"	REAL NOT NULL,
	"bbPerc"	REAL NOT NULL,
	"kPerc"	REAL NOT NULL,
	"goPerc"	REAL NOT NULL,
	"avg"	REAL NOT NULL,
	"iso"	REAL NOT NULL,
	PRIMARY KEY("Level","Year","Month")
);

CREATE TABLE "Model_HitterStats" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"Age"	REAL NOT NULL,
	"PA"	INTEGER NOT NULL,
	"Level"	INTEGER NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	"avgRatio"	REAL NOT NULL,
	"obpRatio"	REAL NOT NULL,
	"isoRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"sbRateRatio"	REAL NOT NULL,
	"sbPercRatio"	REAL NOT NULL,
	"hrPercRatio"	REAL NOT NULL,
	"bbPercRatio"	REAL NOT NULL,
	"kPercRatio"	REAL NOT NULL,
	"PercC"	REAL NOT NULL,
	"Perc1B"	REAL NOT NULL,
	"Perc2B"	REAL NOT NULL,
	"Perc3B"	REAL NOT NULL,
	"PercSS"	REAL NOT NULL,
	"PercLF"	REAL NOT NULL,
	"PercCF"	REAL NOT NULL,
	"PercRF"	REAL NOT NULL,
	"PercDH"	REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","Month")
);

CREATE TABLE "Model_PitcherStats" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"Age"	REAL NOT NULL,
	"BF"	INTEGER NOT NULL,
	"Level"	REAL NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	"GBPercRatio"	REAL NOT NULL,
	"ERARatio"	REAL NOT NULL,
	"FIPRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"hrPercRatio"	REAL NOT NULL,
	"bbPercRatio"	REAL NOT NULL,
	"kPercRatio"	REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","Month")
);

CREATE TABLE "Model_PlayerWar" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"isHitter"	INTEGER NOT NULL,
	"pa"	INTEGER NOT NULL,
	"war"	REAL NOT NULL,
	"off"	REAL NOT NULL,
	"def"	REAL NOT NULL,
	"bsr"	REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","isHitter")
);

CREATE TABLE "Model_Players" (
	"mlbId"	INTEGER NOT NULL,
	"isHitter"	INTEGER NOT NULL,
	"isPitcher"	INTEGER NOT NULL,
	"lastProspectYear"	INTEGER NOT NULL,
	"lastProspectMonth"	INTEGER NOT NULL,
	"lastMLBSeason"	INTEGER NOT NULL,
	"ageAtSigningYear"	REAL NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Model_TrainingHistory" (
	"ModelName"	TEXT NOT NULL,
	"Year"	INTEGER NOT NULL,
	"IsHitter"	INTEGER NOT NULL,
	"TestLoss"	REAL NOT NULL,
	"NumLayers"	INTEGER NOT NULL,
	"HiddenSize"	INTEGER NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	PRIMARY KEY("ModelName","Year","IsHitter")
);

CREATE TABLE "Output_PlayerWar" (
	"mlbId"	INTEGER NOT NULL,
	"modelIdx"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"prob0"	REAL NOT NULL,
	"prob1"	REAL NOT NULL,
	"prob2"	REAL NOT NULL,
	"prob3"	REAL NOT NULL,
	"prob4"	REAL NOT NULL,
	"prob5"	REAL NOT NULL,
	"prob6"	REAL NOT NULL,
	PRIMARY KEY("mlbId","modelIdx","year","month")
);

CREATE TABLE "Park_Factors" (
	"TeamId"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"RunFactor"	INTEGER NOT NULL,
	"HRFactor"	INTEGER NOT NULL,
	PRIMARY KEY("TeamId","Year")
);

CREATE TABLE "Park_ScoringData" (
	"TeamId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"HomePa"	INTEGER NOT NULL,
	"HomeOuts"	INTEGER NOT NULL,
	"HomeRuns"	INTEGER NOT NULL,
	"HomeHRs"	INTEGER NOT NULL,
	"AwayPa"	INTEGER NOT NULL,
	"AwayOuts"	INTEGER NOT NULL,
	"AwayRuns"	INTEGER NOT NULL,
	"AwayHRs"	INTEGER NOT NULL,
	PRIMARY KEY("TeamId","Year","LeagueId")
);

CREATE TABLE "Player" (
	"mlbId"	INTEGER NOT NULL,
	"fangraphsId"	INTEGER,
	"birthYear"	INTEGER NOT NULL,
	"birthMonth"	INTEGER NOT NULL,
	"birthDate"	INTEGER NOT NULL,
	"draftPick"	INTEGER,
	"draftBonus"	INTEGER,
	"signingYear"	INTEGER,
	"signingMonth"	INTEGER,
	"signingDate"	INTEGER,
	"signingBonus"	INTEGER ,
	"bats"	TEXT NOT NULL,
	"throws"	TEXT NOT NULL,
	"isRetired"	INTEGER,
	"useFirstName"	TEXT NOT NULL,
	"useLastName"	TEXT NOT NULL, 
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Player_CareerStatus" (
	"mlbId"	INTEGER NOT NULL,
	"isPitcher"	INTEGER NOT NULL,
	"isHitter"	INTEGER NOT NULL,
	"isActive"	INTEGER NOT NULL,
	"serviceReached"	INTEGER,
	"mlbStartYear"	INTEGER,
	"mlbRookieYear"	INTEGER,
	"mlbRookieMonth"	INTEGER,
	"serviceEndYear"	INTEGER,
	"serviceLapseYear"	INTEGER,
	"careerStartYear"	INTEGER NOT NULL,
	"agedOut"	INTEGER,
	"ignorePlayer"	INTEGER,
	"highestLevel"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Player_Hitter_GameLog" (
	"gameLogId"	INTEGER NOT NULL,
	"gameId"	INTEGER NOT NULL,
	"mlbId"	INTEGER NOT NULL,
	"Day"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"AB"	INTEGER NOT NULL,
	"H"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"K"	INTEGER NOT NULL,
	"BB"	INTEGER NOT NULL,
	"SB"	INTEGER NOT NULL,
	"CS"	INTEGER NOT NULL,
	"HBP"	INTEGER NOT NULL,
	"Position"	INTEGER NOT NULL,
	"Level"	INTEGER NOT NULL,
	"HomeTeamId"	INTEGER NOT NULL,
	"TeamId"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
	PRIMARY KEY("gameLogId")
);

CREATE TABLE "Player_Hitter_MonthAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"AVG"	REAL NOT NULL,
	"OBP"	REAL NOT NULL,
	"SLG"	REAL NOT NULL,
	"ISO"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	"wRC"	REAL NOT NULL,
	"HRPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"SBRate"	REAL NOT NULL,
	"SBPerc"	REAL NOT NULL,
	PRIMARY KEY("mlbId","levelId","year","month","teamId","leagueId")
);

CREATE TABLE "Player_Hitter_MonthStats" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"AB"	INTEGER NOT NULL,
	"H"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"K"	INTEGER NOT NULL,
	"BB"	INTEGER NOT NULL,
	"SB"	INTEGER NOT NULL,
	"CS"	INTEGER NOT NULL,
	"HBP"	INTEGER NOT NULL,
	"ParkRunFactor"	INTEGER NOT NULL,
	"ParkHRFactor"	INTEGER NOT NULL,
	"GamesC"	INTEGER NOT NULL,
	"Games1B"	INTEGER NOT NULL,
	"Games2B"	INTEGER NOT NULL,
	"Games3B"	INTEGER NOT NULL,
	"GamesSS"	INTEGER NOT NULL,
	"GamesLF"	INTEGER NOT NULL,
	"GamesCF"	INTEGER NOT NULL,
	"GamesRF"	INTEGER NOT NULL,
	"GamesDH"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId","Year","Month","LevelId")
);

CREATE TABLE "Player_Hitter_MonthlyRatios" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"Level"	INTEGER NOT NULL,
	"avgRatio"	REAL NOT NULL,
	"obpRatio"	REAL NOT NULL,
	"isoRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"sbRateRatio"	REAL NOT NULL,
	"sbPercRatio"	REAL NOT NULL,
	"hrPercRatio"	REAL NOT NULL,
	"bbPercRatio"	REAL NOT NULL,
	"kPercRatio"	REAL NOT NULL,
	"PercC"	REAL NOT NULL,
	"Perc1B"	REAL NOT NULL,
	"Perc2B"	REAL NOT NULL,
	"Perc3B"	REAL NOT NULL,
	"PercSS"	REAL NOT NULL,
	"PercLF"	REAL NOT NULL,
	"PercCF"	REAL NOT NULL,
	"PercRF"	REAL NOT NULL,
	"PercDH"	REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","Month","Level")
);

CREATE TABLE "Player_Hitter_YearAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"AVG"	REAL NOT NULL,
	"OBP"	REAL NOT NULL,
	"SLG"	REAL NOT NULL,
	"ISO"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	"wRC"	REAL NOT NULL,
	"HR"	INTEGER NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"SB"	INTEGER NOT NULL,
	"CS"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId","levelId","year","teamId","leagueId")
);

CREATE TABLE "Player_OrgMap" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"parentOrgId"	INTEGER NOT NULL,
	PRIMARY KEY("year","mlbId","month")
);

CREATE TABLE "Player_Pitcher_GameLog" (
	"gameLogId"	INTEGER NOT NULL,
	"gameId"	INTEGER NOT NULL,
	"mlbId"	INTEGER NOT NULL,
	"day"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"battersFaced"	INTEGER NOT NULL,
	"outs"	INTEGER NOT NULL,
	"go"	INTEGER NOT NULL,
	"ao"	INTEGER NOT NULL,
	"r"	INTEGER NOT NULL,
	"er"	INTEGER NOT NULL,
	"h"	INTEGER NOT NULL,
	"k"	INTEGER NOT NULL,
	"bb"	INTEGER NOT NULL,
	"hbp"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"level"	INTEGER NOT NULL,
	"homeTeamId"	INTEGER NOT NULL,
	"TeamId"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
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
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"level"	INTEGER NOT NULL,
	"battersFaced"	INTEGER NOT NULL,
	"outs"	INTEGER NOT NULL,
	"go"	INTEGER NOT NULL,
	"ao"	INTEGER NOT NULL,
	"r"	INTEGER NOT NULL,
	"er"	INTEGER NOT NULL,
	"h"	INTEGER NOT NULL,
	"k"	INTEGER NOT NULL,
	"bb"	INTEGER NOT NULL,
	"hbp"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"RunFactor"	INTEGER NOT NULL,
	"HRFactor"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId","year","month","level")
);

CREATE TABLE "Player_Pitcher_MonthlyRatios" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"level"	INTEGER NOT NULL,
	"GBPercRatio"	REAL NOT NULL,
	"ERARatio"	REAL NOT NULL,
	"FIPRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"hrPercRatio"	REAL NOT NULL,
	"bbPercRatio"	REAL NOT NULL,
	"kPercRatio"	REAL NOT NULL,
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
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Player_ServiceTime" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"serviceYear"	INTEGER NOT NULL,
	"serviceDays"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId","year")
);

CREATE TABLE "Player_YearlyWar" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"position"	INTEGER NOT NULL,
	"pa"	INTEGER NOT NULL,
	"war"	REAL NOT NULL,
	"off"	REAL NOT NULL,
	"def"	REAL NOT NULL,
	"bsr"	REAL NOT NULL,
	PRIMARY KEY("mlbId","year","position")
);

CREATE TABLE "Pre05_Players" (
	"mlbId"	INTEGER NOT NULL,
	"careerStartYear"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE "Team_League_Map" (
	"TeamId"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	PRIMARY KEY("TeamId","LeagueId","Year")
);

CREATE TABLE "Team_OrganizationMap" (
	"teamId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"parentOrgId"	INTEGER NOT NULL,
	PRIMARY KEY("teamId","year")
);

CREATE TABLE "Team_Parents" (
	"id"	INTEGER NOT NULL,
	"abbr"	TEXT NOT NULL,
	"name"	TEXT NOT NULL,
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