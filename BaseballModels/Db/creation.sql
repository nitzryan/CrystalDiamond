CREATE TABLE "Draft_Results" (
	"Year" INTEGER NOT NULL,
	"Pick" INTEGER NOT NULL,
	"Round" TEXT NOT NULL,
	"mlbId" INTEGER NOT NULL,
	"Signed" INTEGER NOT NULL,
	"Bonus" INTEGER NOT NULL,
	"BonusRank" INTEGER NOT NULL,
	PRIMARY KEY ("Year", "Pick")
);

CREATE TABLE "League_HitterStats" (
	"LeagueId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"AB" INTEGER NOT NULL,
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
	PRIMARY KEY("LeagueId","Year","Month")
);

CREATE TABLE "League_PitcherStats" (
	"LeagueId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"ERA"	REAL NOT NULL,
	"RA"	REAL NOT NULL,
	"FipConstant"	REAL NOT NULL,
	"wOBA"	REAL NOT NULL,
	"HRPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"kPerc"	REAL NOT NULL,
	"GOPerc"	REAL NOT NULL,
	"avg"	REAL NOT NULL,
	"iso"	REAL NOT NULL,
	PRIMARY KEY("LeagueId","Year","Month")
);

CREATE TABLE "Level_GameCounts" (
	"LevelId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"MaxPA" INTEGER NOT NULL,
	PRIMARY KEY("LevelId", "Year", "Month")
);

CREATE TABLE "Model_HitterStats" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"Age"	REAL NOT NULL,
	"PA"	INTEGER NOT NULL,
	"InjStatus" INTEGER NOT NULL,
	"TrainMask" INTEGER NOT NULL,
	"MonthFrac" REAL NOT NULL,
	"LevelId"	REAL NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	"AVGRatio"	REAL NOT NULL,
	"OBPRatio"	REAL NOT NULL,
	"ISORatio"	REAL NOT NULL,
	"wRC"	REAL NOT NULL,
	"crWAR" REAL NOT NULL,
	"crOFF" REAL NOT NULL,
	"crBSR" REAL NOT NULL,
	"crDEF" REAL NOT NULL,
	"SBRateRatio"	REAL NOT NULL,
	"SBPercRatio"	REAL NOT NULL,
	"HRPercRatio"	REAL NOT NULL,
	"BBPercRatio"	REAL NOT NULL,
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
	"InjStatus" INTEGER NOT NULL,
	"TrainMask" INTEGER NOT NULL,
	"MonthFrac" REAL NOT NULL,
	"LevelId"	REAL NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	"SpPerc" REAL NOT NULL,
	"GBPercRatio"	REAL NOT NULL,
	"ERARatio"	REAL NOT NULL,
	"FIPRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"HRPercRatio"	REAL NOT NULL,
	"BBPercRatio"	REAL NOT NULL,
	"KPercRatio"	REAL NOT NULL,
	"crWAR" REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","Month")
);

CREATE TABLE "Model_HitterValue" (
	"mlbId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"War1Year" REAL NOT NULL,
	"War2Year" REAL NOT NULL,
	"War3Year" REAL NOT NULL,
	"Off1Year" REAL NOT NULL,
	"Off2Year" REAL NOT NULL,
	"Off3Year" REAL NOT NULL,
	"Bsr1Year" REAL NOT NULL,
	"Bsr2Year" REAL NOT NULL,
	"Bsr3Year" REAL NOT NULL,
	"Def1Year" REAL NOT NULL,
	"Def2Year" REAL NOT NULL,
	"Def3Year" REAL NOT NULL,
	"Rep1Year" REAL NOT NULL,
	"Rep2Year" REAL NOT NULL,
	"Rep3Year" REAL NOT NULL,
	"Pa1Year" INTEGER NOT NULL,
	"Pa2Year" INTEGER NOT NULL,
	"Pa3Year" INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "Year", "Month")
);

CREATE TABLE "Model_PitcherValue" (
	"mlbId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"WarSP1Year" REAL NOT NULL,
	"WarSP2Year" REAL NOT NULL,
	"WarSP3Year" REAL NOT NULL,
	"WarRP1Year" REAL NOT NULL,
	"WarRP2Year" REAL NOT NULL,
	"WarRP3Year" REAL NOT NULL,
	"IPSP1Year" REAL NOT NULL,
	"IPSP2Year" REAL NOT NULL,
	"IPSP3Year" REAL NOT NULL,
	"IPRP1Year" REAL NOT NULL,
	"IPRP2Year" REAL NOT NULL,
	"IPRP3Year" REAL NOT NULL,
	PRIMARY KEY("mlbId", "Year", "Month")
);

CREATE TABLE "Model_PlayerWar" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"isHitter"	INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"WAR_h" REAL NOT NULL,
	"WAR_s" REAL NOT NULL,
	"WAR_r" REAL NOT NULL,
	"OFF"	REAL NOT NULL,
	"DEF"	REAL NOT NULL,
	"BSR"	REAL NOT NULL,
	"REP"   REAL NOT NULL,
	PRIMARY KEY("mlbId","Year","isHitter")
);

CREATE TABLE "Model_Players" (
	"mlbId"	INTEGER NOT NULL,
	"isHitter"	INTEGER NOT NULL,
	"isPitcher"	INTEGER NOT NULL,
	"signingYear" INTEGER NOT NULL,
	"lastProspectYear"	INTEGER NOT NULL,
	"lastProspectMonth"	INTEGER NOT NULL,
	"lastMLBSeason"	INTEGER NOT NULL,
	"ageAtSigningYear"	REAL NOT NULL,
	"draftPick" INTEGER NOT NULL,
	"draftSignRank" INTEGER NOT NULL,
	"prospectType" INTEGER NOT NULL,
	"highestLevelHitter" INTEGER NOT NULL,
	"highestLevelPitcher" INTEGER NOT NULL,
	"warHitter" REAL NOT NULL,
	"warPitcher" REAL NOT NULL,
	"peakWarHitter" REAL NOT NULL,
	"peakWarPitcher" REAL NOT NULL,
	"valueHitter" REAL NOT NULL,
	"valuePitcher" REAL NOT NULL,
	"valueStarterPerc" REAL NOT NULL,
	"totalPA" INTEGER NOT NULL,
	"totalOuts" INTEGER NOT NULL,
	"rateOff" REAL NOT NULL,
	"rateBsr" REAL NOT NULL,
	"rateDef" REAL NOT NULL,
	PRIMARY KEY("mlbId")
);

CREATE TABLE Model_OrgLeagueStatus (
	"OrgId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"MLB_PF" REAL NOT NULL,
	"AAA_PF" REAL NOT NULL,
	"AA_PF" REAL NOT NULL,
	"HA_PF" REAL NOT NULL,
	"A_PF" REAL NOT NULL,
	"LA_PF" REAL NOT NULL,
	"Rk_PF" REAL NOT NULL,
	"DSL_PF" REAL NOT NULL,
	"MLB_MonthsFrac" REAL NOT NULL,
	"AAA_MonthsFrac" REAL NOT NULL,
	"AA_MonthsFrac" REAL NOT NULL,
	"HA_MonthsFrac" REAL NOT NULL,
	"A_MonthsFrac" REAL NOT NULL,
	"LA_MonthsFrac" REAL NOT NULL,
	"Rk_MonthsFrac" REAL NOT NULL,
	"DSL_MonthsFrac" REAL NOT NULL,
	PRIMARY KEY("OrgId", "Year", "Month")
);

CREATE TABLE Model_HitterLevelStats (
	"MlbId" INTEGER NOT NULL,
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
	"ParkRunFactor" REAL NOT NULL,
	PRIMARY KEY("MlbId", "Year", "Month", "LevelId")
);

CREATE TABLE Model_LeagueHittingBaselines (
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"LevelId" INTEGER NOT NULL,
	"Hit1B" REAL NOT NULL,
	"Hit2B" REAL NOT NULL,
	"Hit3B" REAL NOT NULL,
	"HitHR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL, 
	"SB" REAL NOT NULL,
	"CS" REAL NOT NULL,
	PRIMARY KEY("Year", "Month", "LeagueId")
);

CREATE TABLE Model_PitcherLevelStats (
	"MlbId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"LevelId" INTEGER NOT NULL,
	"Outs_SP" INTEGER NOT NULL,
	"Outs_RP" INTEGER NOT NULL,
	"G" INTEGER NOT NULL,
	"GS" INTEGER NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL,
	"ParkRunFactor" REAL NOT NULL,
	PRIMARY KEY("MlbId", "Year", "Month", "LevelId")
);

CREATE TABLE Model_LeaguePitchingBaselines (
	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"LevelId" INTEGER NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"HBP" REAL NOT NULL,
	"K" REAL NOT NULL, 
	PRIMARY KEY("Year", "Month", "LeagueId")
);

CREATE TABLE "Model_TrainingHistory" (
	"ModelName"	TEXT NOT NULL,
	"IsHitter"	INTEGER NOT NULL,
	"TestLoss"	REAL NOT NULL,
	"ModelIdx"	INTEGER NOT NULL,
	"NumLayers" INTEGER NOT NULL,
	"HiddenSize" INTEGER NOT NULL,
	PRIMARY KEY("ModelName","ModelIdx","IsHitter")
);

CREATE TABLE "Output_PlayerWar" (
	"mlbId"	INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"modelIdx"	INTEGER NOT NULL,
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
	PRIMARY KEY("mlbId", "model", "isHitter", "modelIdx","year","month")
);

CREATE TABLE "Output_HitterStats" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	"AVG" REAL NOT NULL,
	"OBP" REAL NOT NULL,
	"ISO" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"K" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "levelId", "modelIdx")
);

CREATE TABLE "Output_PitcherStats" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	"GB" REAL NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"K" REAL NOT NULL,
	"SP" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "levelId", "modelIdx")
);

CREATE TABLE "Output_PlayerWarAggregation" (
	"mlbId"	INTEGER NOT NULL,
	"model"	INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
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
	PRIMARY KEY("mlbId","model", "isHitter", "year","month")
);

CREATE TABLE "Output_HitterStatsAggregation" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"AVG" REAL NOT NULL,
	"OBP" REAL NOT NULL,
	"ISO" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"K" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "levelId")
);

CREATE TABLE "Output_PitcherStatsAggregation" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"GB" REAL NOT NULL,
	"ERA" REAL NOT NULL,
	"FIP" REAL NOT NULL,
	"HR" REAL NOT NULL,
	"BB" REAL NOT NULL,
	"K" REAL NOT NULL,
	"SP" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "levelId")
);

CREATE TABLE "Output_HitterValue" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"WAR1Year" REAL NOT NULL,
	"OFF1Year" REAL NOT NULL,
	"BSR1Year" REAL NOT NULL,
	"DEF1Year" REAL NOT NULL,
	"WAR2Year" REAL NOT NULL,
	"OFF2Year" REAL NOT NULL,
	"BSR2Year" REAL NOT NULL,
	"DEF2Year" REAL NOT NULL,
	"WAR3Year" REAL NOT NULL,
	"OFF3Year" REAL NOT NULL,
	"BSR3Year" REAL NOT NULL,
	"DEF3Year" REAL NOT NULL,
	"PA1Year" REAL NOT NULL,
	"PA2Year" REAL NOT NULL,
	"PA3Year" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "modelIdx", "month", "year")
);

CREATE TABLE "Output_HitterValueAggregation" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"WAR1Year" REAL NOT NULL,
	"OFF1Year" REAL NOT NULL,
	"BSR1Year" REAL NOT NULL,
	"DEF1Year" REAL NOT NULL,
	"WAR2Year" REAL NOT NULL,
	"OFF2Year" REAL NOT NULL,
	"BSR2Year" REAL NOT NULL,
	"DEF2Year" REAL NOT NULL,
	"WAR3Year" REAL NOT NULL,
	"OFF3Year" REAL NOT NULL,
	"BSR3Year" REAL NOT NULL,
	"DEF3Year" REAL NOT NULL,
	"PA1Year" REAL NOT NULL,
	"PA2Year" REAL NOT NULL,
	"PA3Year" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "month", "year")
);

CREATE TABLE "Output_PitcherValue" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"WarSP1Year" REAL NOT NULL,
	"WarRP1Year" REAL NOT NULL,
	"WarSP2Year" REAL NOT NULL,
	"WarRP2Year" REAL NOT NULL,
	"WarSP3Year" REAL NOT NULL,
	"WarRP3Year" REAL NOT NULL,
	"IPSP1Year" REAL NOT NULL,
	"IPRP1Year" REAL NOT NULL,
	"IPSP2Year" REAL NOT NULL,
	"IPRP2Year" REAL NOT NULL,
	"IPSP3Year" REAL NOT NULL,
	"IPRP3Year" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "modelIdx", "month", "year")
);

CREATE TABLE "Output_PitcherValueAggregation" (
	"mlbId" INTEGER NOT NULL,
	"model" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"WarSP1Year" REAL NOT NULL,
	"WarRP1Year" REAL NOT NULL,
	"WarSP2Year" REAL NOT NULL,
	"WarRP2Year" REAL NOT NULL,
	"WarSP3Year" REAL NOT NULL,
	"WarRP3Year" REAL NOT NULL,
	"IPSP1Year" REAL NOT NULL,
	"IPRP1Year" REAL NOT NULL,
	"IPSP2Year" REAL NOT NULL,
	"IPRP2Year" REAL NOT NULL,
	"IPSP3Year" REAL NOT NULL,
	"IPRP3Year" REAL NOT NULL,
	PRIMARY KEY("mlbId", "model", "month", "year")
);

CREATE TABLE "Park_Factors" (
	"StadiumId"	INTEGER NOT NULL,
	"LeagueId"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"RunFactor"	REAL NOT NULL,
	"HRFactor"	REAL NOT NULL,
	PRIMARY KEY("StadiumId","Year")
);

CREATE TABLE "Player" (
	"mlbId"	INTEGER NOT NULL,
	"fangraphsId"	INTEGER,
	"position" TEXT NOT NULL,
	"birthYear"	INTEGER NOT NULL,
	"birthMonth"	INTEGER NOT NULL,
	"birthDate"	INTEGER NOT NULL,
	"draftPick"	INTEGER,
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
	"isActive"	INTEGER,
	"serviceReached"	INTEGER,
	"mlbStartYear"	INTEGER,
	"mlbRookieYear"	INTEGER,
	"mlbRookieMonth"	INTEGER,
	"serviceEndYear"	INTEGER,
	"serviceLapseYear"	INTEGER,
	"agedOut"	INTEGER,
	"playingGap" INTEGER,
	"ignorePlayer"	INTEGER,
	"highestLevelPitcher"	INTEGER,
	"highestLevelHitter"	INTEGER,
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
	"PA" 	INTEGER NOT NULL,
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
	"LevelId"	INTEGER NOT NULL,
	"StadiumId"	INTEGER NOT NULL,
	"IsHome" INTEGER NOT NULL,
	"TeamId"	INTEGER NOT NULL,
	"oppTeamId" INTEGER NOT NULL,
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
	"ParkFactor" REAL NOT NULL,
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
	"SB" INTEGER NOT NULL,
	"CS" INTEGER NOT NULL,
	"HR" INTEGER NOT NULL,
	"crWAR" REAL NOT NULL,
	"crOFF" REAL NOT NULL,
	"crBSR" REAL NOT NULL,
	"crDEF" REAL NOT NULL,
	PRIMARY KEY("mlbId","levelId","year","month","teamId","leagueId")
);

CREATE TABLE "Player_Hitter_MonthStats" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"AB"	INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"H"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"K"	INTEGER NOT NULL,
	"BB"	INTEGER NOT NULL,
	"SB"	INTEGER NOT NULL,
	"CS"	INTEGER NOT NULL,
	"HBP"	INTEGER NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	"GamesC"	INTEGER NOT NULL,
	"Games1B"	INTEGER NOT NULL,
	"Games2B"	INTEGER NOT NULL,
	"Games3B"	INTEGER NOT NULL,
	"GamesSS"	INTEGER NOT NULL,
	"GamesLF"	INTEGER NOT NULL,
	"GamesCF"	INTEGER NOT NULL,
	"GamesRF"	INTEGER NOT NULL,
	"GamesDH"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId","Year","Month","LevelId", "LeagueId")
);

CREATE TABLE "Player_Hitter_MonthlyRatios" (
	"mlbId"	INTEGER NOT NULL,
	"Year"	INTEGER NOT NULL,
	"Month"	INTEGER NOT NULL,
	"LevelId"	INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"AVGRatio"	REAL NOT NULL,
	"OBPRatio"	REAL NOT NULL,
	"ISORatio"	REAL NOT NULL,
	"wRC"	REAL NOT NULL,
	"SBRateRatio"	REAL NOT NULL,
	"SBPercRatio"	REAL NOT NULL,
	"HRPercRatio"	REAL NOT NULL,
	"BBPercRatio"	REAL NOT NULL,
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
	PRIMARY KEY("mlbId","Year","Month","LevelId", "LeagueId")
);

CREATE TABLE "Player_Hitter_YearAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"ParkFactor" REAL NOT NULL,
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
	"day" INTEGER NOT NULL,
	"parentOrgId"	INTEGER NOT NULL,
	PRIMARY KEY("mlbId", "year","month", "day")
);

CREATE TABLE "Transaction_Log" (
	"transactionId" INTEGER NOT NULL,
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"day" INTEGER NOT NULL,
	"toIL" INTEGER NOT NULL,
	"parentOrgId" INTEGER NOT NULL,
	PRIMARY KEY("transactionId")
);

CREATE TABLE "Player_Pitcher_GameLog" (
	"gameLogId"	INTEGER NOT NULL,
	"gameId"	INTEGER NOT NULL,
	"mlbId"	INTEGER NOT NULL,
	"day"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"started" INTEGER NOT NULL,
	"battersFaced"	INTEGER NOT NULL,
	"outs"	INTEGER NOT NULL,
	"GO"	INTEGER NOT NULL,
	"AO"	INTEGER NOT NULL,
	"R"	INTEGER NOT NULL,
	"ER"	INTEGER NOT NULL,
	"h"	INTEGER NOT NULL,
	"k"	INTEGER NOT NULL,
	"BB"	INTEGER NOT NULL,
	"HBP"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"stadiumId"	INTEGER NOT NULL,
	"isHome" INTEGER NOT NULL,
	"TeamId"	INTEGER NOT NULL,
	"oppTeamId" INTEGER NOT NULL,
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
	"SPPerc"	REAL NOT NULL,
	"GBRatio"	REAL NOT NULL,
	"ERA"	REAL NOT NULL,
	"FIP"	REAL NOT NULL,
	"KPerc"	REAL NOT NULL,
	"BBPerc"	REAL NOT NULL,
	"HRPerc"	REAL NOT NULL,
	"HR" INTEGER NOT NULL,
	"wOBA"	REAL NOT NULL,
	"crWAR" REAL NOT NULL,
	PRIMARY KEY("leagueId","teamId","month","year","levelId","mlbId")
);

CREATE TABLE "Player_Pitcher_MonthStats" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"battersFaced"	INTEGER NOT NULL,
	"Outs"	INTEGER NOT NULL,
	"SPPerc"	REAL NOT NULL,
	"GO"	INTEGER NOT NULL,
	"AO"	INTEGER NOT NULL,
	"R"	INTEGER NOT NULL,
	"ER"	INTEGER NOT NULL,
	"h"	INTEGER NOT NULL,
	"k"	INTEGER NOT NULL,
	"BB"	INTEGER NOT NULL,
	"HBP"	INTEGER NOT NULL,
	"hit2B"	INTEGER NOT NULL,
	"hit3B"	INTEGER NOT NULL,
	"HR"	INTEGER NOT NULL,
	"ParkRunFactor"	REAL NOT NULL,
	"ParkHRFactor"	REAL NOT NULL,
	PRIMARY KEY("mlbId","year","month","levelId", "leagueId")
);

CREATE TABLE "Player_Pitcher_MonthlyRatios" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"LeagueId" INTEGER NOT NULL,
	"SPPerc" REAL NOT NULL,
	"GBPercRatio"	REAL NOT NULL,
	"ERARatio"	REAL NOT NULL,
	"FIPRatio"	REAL NOT NULL,
	"wOBARatio"	REAL NOT NULL,
	"HRPercRatio"	REAL NOT NULL,
	"BBPercRatio"	REAL NOT NULL,
	"kPercRatio"	REAL NOT NULL,
	PRIMARY KEY("mlbId","year","month","levelId", "LeagueId")
);

CREATE TABLE "Player_Pitcher_YearAdvanced" (
	"mlbId"	INTEGER NOT NULL,
	"levelId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"teamId"	INTEGER NOT NULL,
	"leagueId"	INTEGER NOT NULL,
	"BF"	INTEGER NOT NULL,
	"Outs"	INTEGER NOT NULL,
	"SPPerc" REAL NOT NULL,
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
	"isHitter"	INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"IP_SP" REAL NOT NULL,
	"IP_RP" REAL NOT NULL,
	"WAR_h"	REAL NOT NULL,
	"WAR_s" REAL NOT NULL,
	"WAR_r" REAL NOT NULL,
	"OFF"	REAL NOT NULL,
	"DEF"	REAL NOT NULL,
	"BSR"	REAL NOT NULL,
	"REP"   REAL NOT NULL,
	PRIMARY KEY("mlbId","year","isHitter")
);

CREATE TABLE "Player_MonthlyWar" (
	"mlbId"	INTEGER NOT NULL,
	"year"	INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"PA"	INTEGER NOT NULL,
	"IP_SP" REAL NOT NULL,
	"IP_RP" REAL NOT NULL,
	"WAR_h"	REAL NOT NULL,
	"WAR_s" REAL NOT NULL,
	"WAR_r" REAL NOT NULL,
	"OFF"	REAL NOT NULL,
	"DEF"	REAL NOT NULL,
	"BSR"	REAL NOT NULL,
	"REP"   REAL NOT NULL,
	PRIMARY KEY("mlbId","year","month")
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

CREATE TABLE "Leagues" (
	"id" INTEGER NOT NULL,
	"abbr" TEXT NOT NULL,
	"name" TEXT NOT NULL,
	PRIMARY KEY ("id")
);

CREATE TABLE "Ranking_Prospect" (
	"mlbId" INTEGER NOT NULL,
	"year" INTEGER NOT NULL,
	"month" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"rank" INTEGER NOT NULL,
	PRIMARY KEY ("mlbId", "year", "month", "modelIdx", "isHitter")
);

CREATE TABLE "Site_PlayerBio" (
	"id" INTEGER NOT NULL,
	"position" TEXT NOT NULL,
	"isPitcher" INTEGER NOT NULL,
	"isHitter" INTEGER NOT NULL,
	"hasModel" INTEGER NOT NULL,
	"parentId" INTEGER NOT NULL,
	"levelId" INTEGER NOT NULL,
	"status" TEXT NOT NULL,
	"draftPick" INTEGER NOT NULL,
	"draftRound" TEXT NOT NULL,
	"draftBonus" INTEGER NOT NULL,
	"signingYear" INTEGER NOT NULL,
	PRIMARY KEY ("id")
);

CREATE TABLE "ModelIdx" (
	"id" INTEGER NOT NULL,
	"pitcherModelName" TEXT NOT NULL,
	"hitterModelName" TEXT NOT NULL,
	"modelName" TEXT NOT NULL,
	PRIMARY KEY("id")
);

CREATE TABLE "PlayersInTrainingData" (
	"mlbId" INTEGER NOT NULL,
	"modelIdx" INTEGER NOT NULL,
	PRIMARY KEY ("mlbId", "modelIdx")
);

CREATE TABLE "LeagueStats" (
	"LeagueId" INTEGER NOT NULL,
	"Year" INTEGER NOT NULL,
	"avgWOBA" REAL NOT NULL,
	"wOBAScale" REAL NOT NULL,
	"wBB" REAL NOT NULL,
	"wHBP" REAL NOT NULL,
	"w1B" REAL NOT NULL,
	"w2B" REAL NOT NULL,
	"w3B" REAL NOT NULL,
	"wHR" REAL NOT NULL,
	"runSB" REAL NOT NULL,
	"runCS" REAL NOT NULL,
	"RPerPA" REAL NOT NULL,
	"RPerWin" REAL NOT NULL,
	"LeaguePA" INTEGER NOT NULL,
	"LeagueGames" INTEGER NOT NULL,
	"cFIP" REAL NOT NULL,
	"FIPR9Adjustment" REAL NOT NULL,
	"LeagueERA" REAL NOT NULL,
	PRIMARY KEY ("LeagueId", "Year")
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
	"levelId",
	"year",
	"month"
);

CREATE INDEX "idx_PitcherMonthStats_PlayerDate" ON "Player_Pitcher_MonthStats" (
	"mlbId",
	"year",
	"month",
	"levelId"
);

CREATE INDEX "idx_TransactionLog" ON "Transaction_Log" (
	"mlbId",
	"year",
	"month",
	"day"
);

CREATE INDEX "idx_Output_PlayerWar" ON "Output_PlayerWar" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_Output_PlayerWarAggregation" ON "Output_PlayerWarAggregation" (
	"model",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_Output_PitcherValue" ON "Output_PitcherValue" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_Output_PitcherValueAggregation" ON "Output_PitcherValueAggregation" (
	"model",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_Output_HitterValue" ON "Output_HitterValue" (
	"model",
	"modelIdx",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_Output_HitterValueAggregation" ON "Output_HitterValueAggregation" (
	"model",
	"mlbId",
	"year",
	"month"
);

CREATE INDEX "idx_HitterGameLog_Player" ON "Player_Hitter_GameLog" (
	"mlbId"
);

CREATE INDEX "idx_PitcherGameLog_Player" ON "Player_Pitcher_GameLog" (
	"mlbId"
);

CREATE INDEX "idx_HitterGameLogForYearStats" ON "Player_Hitter_GameLog" (
	"mlbId",
	"LevelId",
	"TeamId",
	"LeagueId",
	"Year"
);

CREATE INDEX "idx_PitcherGameLogForYearStats" ON "Player_Pitcher_GameLog" (
	"mlbId",
	"LevelId",
	"TeamId",
	"LeagueId",
	"Year"
);