import sqlite3

class DB_Draft_Results:
	def __init__(self, values : tuple[any]):
		self.Year = values[0]
		self.Pick = values[1]
		self.Round = values[2]
		self.mlbId = values[3]
		self.Signed = values[4]
		self.Bonus = values[5]
		self.BonusRank = values[6]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.Pick,self.Round,self.mlbId,self.Signed,self.Bonus,self.BonusRank)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Draft_Results']:
		items = cursor.execute("SELECT * FROM Draft_Results " + conditional, values).fetchall()
		return [DB_Draft_Results(i) for i in items]


##############################################################################################
class DB_League_Factors:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
		self.Year = values[1]
		self.RunFactor = values[2]
		self.HRFactor = values[3]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.RunFactor,self.HRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_League_Factors']:
		items = cursor.execute("SELECT * FROM League_Factors " + conditional, values).fetchall()
		return [DB_League_Factors(i) for i in items]


##############################################################################################
class DB_Level_Factors:
	def __init__(self, values : tuple[any]):
		self.LevelId = values[0]
		self.Year = values[1]
		self.RunFactor = values[2]
		self.HRFactor = values[3]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LevelId,self.Year,self.RunFactor,self.HRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Level_Factors']:
		items = cursor.execute("SELECT * FROM Level_Factors " + conditional, values).fetchall()
		return [DB_Level_Factors(i) for i in items]


##############################################################################################
class DB_Level_HitterStats:
	def __init__(self, values : tuple[any]):
		self.LevelId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.AB = values[3]
		self.AVG = values[4]
		self.OBP = values[5]
		self.SLG = values[6]
		self.ISO = values[7]
		self.wOBA = values[8]
		self.HRPerc = values[9]
		self.BBPerc = values[10]
		self.KPerc = values[11]
		self.SBRate = values[12]
		self.SBPerc = values[13]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LevelId,self.Year,self.Month,self.AB,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Level_HitterStats']:
		items = cursor.execute("SELECT * FROM Level_HitterStats " + conditional, values).fetchall()
		return [DB_Level_HitterStats(i) for i in items]


##############################################################################################
class DB_Level_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.LevelId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.ERA = values[3]
		self.RA = values[4]
		self.FipConstant = values[5]
		self.wOBA = values[6]
		self.HRPerc = values[7]
		self.BBPerc = values[8]
		self.kPerc = values[9]
		self.GOPerc = values[10]
		self.avg = values[11]
		self.iso = values[12]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LevelId,self.Year,self.Month,self.ERA,self.RA,self.FipConstant,self.wOBA,self.HRPerc,self.BBPerc,self.kPerc,self.GOPerc,self.avg,self.iso)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Level_PitcherStats']:
		items = cursor.execute("SELECT * FROM Level_PitcherStats " + conditional, values).fetchall()
		return [DB_Level_PitcherStats(i) for i in items]


##############################################################################################
class DB_Model_HitterStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.Age = values[3]
		self.PA = values[4]
		self.InjStatus = values[5]
		self.TrainMask = values[6]
		self.MonthFrac = values[7]
		self.LevelId = values[8]
		self.ParkRunFactor = values[9]
		self.ParkHRFactor = values[10]
		self.AVGRatio = values[11]
		self.OBPRatio = values[12]
		self.ISORatio = values[13]
		self.wOBARatio = values[14]
		self.SBRateRatio = values[15]
		self.SBPercRatio = values[16]
		self.HRPercRatio = values[17]
		self.BBPercRatio = values[18]
		self.kPercRatio = values[19]
		self.PercC = values[20]
		self.Perc1B = values[21]
		self.Perc2B = values[22]
		self.Perc3B = values[23]
		self.PercSS = values[24]
		self.PercLF = values[25]
		self.PercCF = values[26]
		self.PercRF = values[27]
		self.PercDH = values[28]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.Age,self.PA,self.InjStatus,self.TrainMask,self.MonthFrac,self.LevelId,self.ParkRunFactor,self.ParkHRFactor,self.AVGRatio,self.OBPRatio,self.ISORatio,self.wOBARatio,self.SBRateRatio,self.SBPercRatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_HitterStats']:
		items = cursor.execute("SELECT * FROM Model_HitterStats " + conditional, values).fetchall()
		return [DB_Model_HitterStats(i) for i in items]


##############################################################################################
class DB_Model_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.Age = values[3]
		self.BF = values[4]
		self.InjStatus = values[5]
		self.TrainMask = values[6]
		self.MonthFrac = values[7]
		self.LevelId = values[8]
		self.ParkRunFactor = values[9]
		self.ParkHRFactor = values[10]
		self.GBPercRatio = values[11]
		self.ERARatio = values[12]
		self.FIPRatio = values[13]
		self.wOBARatio = values[14]
		self.HRPercRatio = values[15]
		self.BBPercRatio = values[16]
		self.KPercRatio = values[17]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.Age,self.BF,self.InjStatus,self.TrainMask,self.MonthFrac,self.LevelId,self.ParkRunFactor,self.ParkHRFactor,self.GBPercRatio,self.ERARatio,self.FIPRatio,self.wOBARatio,self.HRPercRatio,self.BBPercRatio,self.KPercRatio)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PitcherStats']:
		items = cursor.execute("SELECT * FROM Model_PitcherStats " + conditional, values).fetchall()
		return [DB_Model_PitcherStats(i) for i in items]


##############################################################################################
class DB_Model_PlayerWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.isHitter = values[2]
		self.PA = values[3]
		self.WAR_h = values[4]
		self.WAR_s = values[5]
		self.WAR_r = values[6]
		self.OFF = values[7]
		self.DEF = values[8]
		self.BSR = values[9]
		self.REP = values[10]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.isHitter,self.PA,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DEF,self.BSR,self.REP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PlayerWar']:
		items = cursor.execute("SELECT * FROM Model_PlayerWar " + conditional, values).fetchall()
		return [DB_Model_PlayerWar(i) for i in items]


##############################################################################################
class DB_Model_Players:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.isHitter = values[1]
		self.isPitcher = values[2]
		self.signingYear = values[3]
		self.lastProspectYear = values[4]
		self.lastProspectMonth = values[5]
		self.lastMLBSeason = values[6]
		self.ageAtSigningYear = values[7]
		self.draftPick = values[8]
		self.draftSignRank = values[9]
		self.highestLevelHitter = values[10]
		self.highestLevelPitcher = values[11]
		self.warHitter = values[12]
		self.warPitcher = values[13]
		self.peakWarHitter = values[14]
		self.peakWarPitcher = values[15]
		self.valueHitter = values[16]
		self.valuePitcher = values[17]
		self.valueStarterPerc = values[18]
		self.totalPA = values[19]
		self.totalOuts = values[20]
		self.rateOff = values[21]
		self.rateBsr = values[22]
		self.rateDef = values[23]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.isHitter,self.isPitcher,self.signingYear,self.lastProspectYear,self.lastProspectMonth,self.lastMLBSeason,self.ageAtSigningYear,self.draftPick,self.draftSignRank,self.highestLevelHitter,self.highestLevelPitcher,self.warHitter,self.warPitcher,self.peakWarHitter,self.peakWarPitcher,self.valueHitter,self.valuePitcher,self.valueStarterPerc,self.totalPA,self.totalOuts,self.rateOff,self.rateBsr,self.rateDef)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_Players']:
		items = cursor.execute("SELECT * FROM Model_Players " + conditional, values).fetchall()
		return [DB_Model_Players(i) for i in items]


##############################################################################################
class DB_Model_TrainingHistory:
	def __init__(self, values : tuple[any]):
		self.ModelName = values[0]
		self.IsHitter = values[1]
		self.TestLoss = values[2]
		self.ModelIdx = values[3]
		self.NumLayers = values[4]
		self.HiddenSize = values[5]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelName,self.IsHitter,self.TestLoss,self.ModelIdx,self.NumLayers,self.HiddenSize)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_TrainingHistory']:
		items = cursor.execute("SELECT * FROM Model_TrainingHistory " + conditional, values).fetchall()
		return [DB_Model_TrainingHistory(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list['DB_Model_TrainingHistory']) -> None:
		cursor.executemany("INSERT INTO Model_TrainingHistory VALUES(?,?,?,?,?,?)", [i.To_Tuple() for i in items])

##############################################################################################
class DB_Output_PlayerWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.modelIdx = values[3]
		self.year = values[4]
		self.month = values[5]
		self.prob0 = values[6]
		self.prob1 = values[7]
		self.prob2 = values[8]
		self.prob3 = values[9]
		self.prob4 = values[10]
		self.prob5 = values[11]
		self.prob6 = values[12]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.isHitter,self.modelIdx,self.year,self.month,self.prob0,self.prob1,self.prob2,self.prob3,self.prob4,self.prob5,self.prob6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWar']:
		items = cursor.execute("SELECT * FROM Output_PlayerWar " + conditional, values).fetchall()
		return [DB_Output_PlayerWar(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list['DB_Output_PlayerWar']) -> None:
		cursor.executemany("INSERT INTO Output_PlayerWar VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

##############################################################################################
class DB_Output_PlayerWarAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.year = values[3]
		self.month = values[4]
		self.prob0 = values[5]
		self.prob1 = values[6]
		self.prob2 = values[7]
		self.prob3 = values[8]
		self.prob4 = values[9]
		self.prob5 = values[10]
		self.prob6 = values[11]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.isHitter,self.year,self.month,self.prob0,self.prob1,self.prob2,self.prob3,self.prob4,self.prob5,self.prob6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWarAggregation']:
		items = cursor.execute("SELECT * FROM Output_PlayerWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerWarAggregation(i) for i in items]


##############################################################################################
class DB_Park_Factors:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.LeagueId = values[1]
		self.LevelId = values[2]
		self.Year = values[3]
		self.RunFactor = values[4]
		self.HRFactor = values[5]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.LeagueId,self.LevelId,self.Year,self.RunFactor,self.HRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Park_Factors']:
		items = cursor.execute("SELECT * FROM Park_Factors " + conditional, values).fetchall()
		return [DB_Park_Factors(i) for i in items]


##############################################################################################
class DB_Park_ScoringData:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.Year = values[1]
		self.LeagueId = values[2]
		self.LevelId = values[3]
		self.HomePa = values[4]
		self.HomeOuts = values[5]
		self.HomeRuns = values[6]
		self.HomeHRs = values[7]
		self.AwayPa = values[8]
		self.AwayOuts = values[9]
		self.AwayRuns = values[10]
		self.AwayHRs = values[11]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.Year,self.LeagueId,self.LevelId,self.HomePa,self.HomeOuts,self.HomeRuns,self.HomeHRs,self.AwayPa,self.AwayOuts,self.AwayRuns,self.AwayHRs)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Park_ScoringData']:
		items = cursor.execute("SELECT * FROM Park_ScoringData " + conditional, values).fetchall()
		return [DB_Park_ScoringData(i) for i in items]


##############################################################################################
class DB_Player:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.fangraphsId = values[1]
		self.position = values[2]
		self.birthYear = values[3]
		self.birthMonth = values[4]
		self.birthDate = values[5]
		self.draftPick = values[6]
		self.signingYear = values[7]
		self.signingMonth = values[8]
		self.signingDate = values[9]
		self.signingBonus = values[10]
		self.bats = values[11]
		self.throws = values[12]
		self.isRetired = values[13]
		self.useFirstName = values[14]
		self.useLastName = values[15]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.fangraphsId,self.position,self.birthYear,self.birthMonth,self.birthDate,self.draftPick,self.signingYear,self.signingMonth,self.signingDate,self.signingBonus,self.bats,self.throws,self.isRetired,self.useFirstName,self.useLastName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player']:
		items = cursor.execute("SELECT * FROM Player " + conditional, values).fetchall()
		return [DB_Player(i) for i in items]


##############################################################################################
class DB_Player_CareerStatus:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.isPitcher = values[1]
		self.isHitter = values[2]
		self.isActive = values[3]
		self.serviceReached = values[4]
		self.mlbStartYear = values[5]
		self.mlbRookieYear = values[6]
		self.mlbRookieMonth = values[7]
		self.serviceEndYear = values[8]
		self.serviceLapseYear = values[9]
		self.agedOut = values[10]
		self.playingGap = values[11]
		self.ignorePlayer = values[12]
		self.highestLevelPitcher = values[13]
		self.highestLevelHitter = values[14]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.isPitcher,self.isHitter,self.isActive,self.serviceReached,self.mlbStartYear,self.mlbRookieYear,self.mlbRookieMonth,self.serviceEndYear,self.serviceLapseYear,self.agedOut,self.playingGap,self.ignorePlayer,self.highestLevelPitcher,self.highestLevelHitter)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_CareerStatus']:
		items = cursor.execute("SELECT * FROM Player_CareerStatus " + conditional, values).fetchall()
		return [DB_Player_CareerStatus(i) for i in items]


##############################################################################################
class DB_Player_Hitter_GameLog:
	def __init__(self, values : tuple[any]):
		self.gameLogId = values[0]
		self.gameId = values[1]
		self.mlbId = values[2]
		self.Day = values[3]
		self.Month = values[4]
		self.Year = values[5]
		self.AB = values[6]
		self.PA = values[7]
		self.H = values[8]
		self.hit2B = values[9]
		self.hit3B = values[10]
		self.HR = values[11]
		self.K = values[12]
		self.BB = values[13]
		self.SB = values[14]
		self.CS = values[15]
		self.HBP = values[16]
		self.Position = values[17]
		self.LevelId = values[18]
		self.HomeTeamId = values[19]
		self.TeamId = values[20]
		self.LeagueId = values[21]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.gameLogId,self.gameId,self.mlbId,self.Day,self.Month,self.Year,self.AB,self.PA,self.H,self.hit2B,self.hit3B,self.HR,self.K,self.BB,self.SB,self.CS,self.HBP,self.Position,self.LevelId,self.HomeTeamId,self.TeamId,self.LeagueId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_GameLog']:
		items = cursor.execute("SELECT * FROM Player_Hitter_GameLog " + conditional, values).fetchall()
		return [DB_Player_Hitter_GameLog(i) for i in items]


##############################################################################################
class DB_Player_Hitter_MonthAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.teamId = values[4]
		self.leagueId = values[5]
		self.PA = values[6]
		self.AVG = values[7]
		self.OBP = values[8]
		self.SLG = values[9]
		self.ISO = values[10]
		self.wOBA = values[11]
		self.wRC = values[12]
		self.HRPerc = values[13]
		self.BBPerc = values[14]
		self.KPerc = values[15]
		self.SBRate = values[16]
		self.SBPerc = values[17]
		self.SB = values[18]
		self.CS = values[19]
		self.HR = values[20]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.PA,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.wRC,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc,self.SB,self.CS,self.HR)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthAdvanced " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthAdvanced(i) for i in items]


##############################################################################################
class DB_Player_Hitter_MonthStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.AB = values[4]
		self.PA = values[5]
		self.H = values[6]
		self.hit2B = values[7]
		self.hit3B = values[8]
		self.HR = values[9]
		self.K = values[10]
		self.BB = values[11]
		self.SB = values[12]
		self.CS = values[13]
		self.HBP = values[14]
		self.ParkRunFactor = values[15]
		self.ParkHRFactor = values[16]
		self.GamesC = values[17]
		self.Games1B = values[18]
		self.Games2B = values[19]
		self.Games3B = values[20]
		self.GamesSS = values[21]
		self.GamesLF = values[22]
		self.GamesCF = values[23]
		self.GamesRF = values[24]
		self.GamesDH = values[25]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.AB,self.PA,self.H,self.hit2B,self.hit3B,self.HR,self.K,self.BB,self.SB,self.CS,self.HBP,self.ParkRunFactor,self.ParkHRFactor,self.GamesC,self.Games1B,self.Games2B,self.Games3B,self.GamesSS,self.GamesLF,self.GamesCF,self.GamesRF,self.GamesDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthStats']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthStats " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthStats(i) for i in items]


##############################################################################################
class DB_Player_Hitter_MonthlyRatios:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.AVGRatio = values[4]
		self.OBPRatio = values[5]
		self.ISORatio = values[6]
		self.wOBARatio = values[7]
		self.SBRateRatio = values[8]
		self.SBPercRatio = values[9]
		self.HRPercRatio = values[10]
		self.BBPercRatio = values[11]
		self.kPercRatio = values[12]
		self.PercC = values[13]
		self.Perc1B = values[14]
		self.Perc2B = values[15]
		self.Perc3B = values[16]
		self.PercSS = values[17]
		self.PercLF = values[18]
		self.PercCF = values[19]
		self.PercRF = values[20]
		self.PercDH = values[21]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.AVGRatio,self.OBPRatio,self.ISORatio,self.wOBARatio,self.SBRateRatio,self.SBPercRatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthlyRatios']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthlyRatios " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthlyRatios(i) for i in items]


##############################################################################################
class DB_Player_Hitter_YearAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.teamId = values[3]
		self.leagueId = values[4]
		self.PA = values[5]
		self.AVG = values[6]
		self.OBP = values[7]
		self.SLG = values[8]
		self.ISO = values[9]
		self.wOBA = values[10]
		self.wRC = values[11]
		self.HR = values[12]
		self.BBPerc = values[13]
		self.KPerc = values[14]
		self.SB = values[15]
		self.CS = values[16]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.teamId,self.leagueId,self.PA,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.wRC,self.HR,self.BBPerc,self.KPerc,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_YearAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Hitter_YearAdvanced " + conditional, values).fetchall()
		return [DB_Player_Hitter_YearAdvanced(i) for i in items]


##############################################################################################
class DB_Player_OrgMap:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.day = values[3]
		self.parentOrgId = values[4]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.day,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_OrgMap']:
		items = cursor.execute("SELECT * FROM Player_OrgMap " + conditional, values).fetchall()
		return [DB_Player_OrgMap(i) for i in items]


##############################################################################################
class DB_Transaction_Log:
	def __init__(self, values : tuple[any]):
		self.transactionId = values[0]
		self.mlbId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.day = values[4]
		self.toIL = values[5]
		self.parentOrgId = values[6]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.transactionId,self.mlbId,self.year,self.month,self.day,self.toIL,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Transaction_Log']:
		items = cursor.execute("SELECT * FROM Transaction_Log " + conditional, values).fetchall()
		return [DB_Transaction_Log(i) for i in items]


##############################################################################################
class DB_Player_Pitcher_GameLog:
	def __init__(self, values : tuple[any]):
		self.gameLogId = values[0]
		self.gameId = values[1]
		self.mlbId = values[2]
		self.day = values[3]
		self.month = values[4]
		self.year = values[5]
		self.battersFaced = values[6]
		self.outs = values[7]
		self.GO = values[8]
		self.AO = values[9]
		self.R = values[10]
		self.ER = values[11]
		self.h = values[12]
		self.k = values[13]
		self.BB = values[14]
		self.HBP = values[15]
		self.hit2B = values[16]
		self.hit3B = values[17]
		self.HR = values[18]
		self.levelId = values[19]
		self.homeTeamId = values[20]
		self.TeamId = values[21]
		self.LeagueId = values[22]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.gameLogId,self.gameId,self.mlbId,self.day,self.month,self.year,self.battersFaced,self.outs,self.GO,self.AO,self.R,self.ER,self.h,self.k,self.BB,self.HBP,self.hit2B,self.hit3B,self.HR,self.levelId,self.homeTeamId,self.TeamId,self.LeagueId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_GameLog']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_GameLog " + conditional, values).fetchall()
		return [DB_Player_Pitcher_GameLog(i) for i in items]


##############################################################################################
class DB_Player_Pitcher_MonthAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.teamId = values[4]
		self.leagueId = values[5]
		self.BF = values[6]
		self.Outs = values[7]
		self.GBRatio = values[8]
		self.ERA = values[9]
		self.FIP = values[10]
		self.KPerc = values[11]
		self.BBPerc = values[12]
		self.HRPerc = values[13]
		self.HR = values[14]
		self.wOBA = values[15]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.BF,self.Outs,self.GBRatio,self.ERA,self.FIP,self.KPerc,self.BBPerc,self.HRPerc,self.HR,self.wOBA)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthAdvanced " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthAdvanced(i) for i in items]


##############################################################################################
class DB_Player_Pitcher_MonthStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.levelId = values[3]
		self.battersFaced = values[4]
		self.outs = values[5]
		self.GO = values[6]
		self.AO = values[7]
		self.R = values[8]
		self.ER = values[9]
		self.h = values[10]
		self.k = values[11]
		self.BB = values[12]
		self.HBP = values[13]
		self.hit2B = values[14]
		self.hit3B = values[15]
		self.HR = values[16]
		self.ParkRunFactor = values[17]
		self.ParkHRFactor = values[18]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.levelId,self.battersFaced,self.outs,self.GO,self.AO,self.R,self.ER,self.h,self.k,self.BB,self.HBP,self.hit2B,self.hit3B,self.HR,self.ParkRunFactor,self.ParkHRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthStats']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthStats " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthStats(i) for i in items]


##############################################################################################
class DB_Player_Pitcher_MonthlyRatios:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.levelId = values[3]
		self.GBPercRatio = values[4]
		self.ERARatio = values[5]
		self.FIPRatio = values[6]
		self.wOBARatio = values[7]
		self.HRPercRatio = values[8]
		self.BBPercRatio = values[9]
		self.kPercRatio = values[10]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.levelId,self.GBPercRatio,self.ERARatio,self.FIPRatio,self.wOBARatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthlyRatios']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthlyRatios " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthlyRatios(i) for i in items]


##############################################################################################
class DB_Player_Pitcher_YearAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.teamId = values[3]
		self.leagueId = values[4]
		self.BF = values[5]
		self.Outs = values[6]
		self.GBRatio = values[7]
		self.ERA = values[8]
		self.FIP = values[9]
		self.KPerc = values[10]
		self.BBPerc = values[11]
		self.HR = values[12]
		self.wOBA = values[13]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.teamId,self.leagueId,self.BF,self.Outs,self.GBRatio,self.ERA,self.FIP,self.KPerc,self.BBPerc,self.HR,self.wOBA)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_YearAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_YearAdvanced " + conditional, values).fetchall()
		return [DB_Player_Pitcher_YearAdvanced(i) for i in items]


##############################################################################################
class DB_Player_ServiceLapse:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_ServiceLapse']:
		items = cursor.execute("SELECT * FROM Player_ServiceLapse " + conditional, values).fetchall()
		return [DB_Player_ServiceLapse(i) for i in items]


##############################################################################################
class DB_Player_ServiceTime:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.serviceYear = values[2]
		self.serviceDays = values[3]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.serviceYear,self.serviceDays)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_ServiceTime']:
		items = cursor.execute("SELECT * FROM Player_ServiceTime " + conditional, values).fetchall()
		return [DB_Player_ServiceTime(i) for i in items]


##############################################################################################
class DB_Player_YearlyWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.isHitter = values[2]
		self.PA = values[3]
		self.WAR_h = values[4]
		self.WAR_s = values[5]
		self.WAR_r = values[6]
		self.OFF = values[7]
		self.DEF = values[8]
		self.BSR = values[9]
		self.REP = values[10]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.isHitter,self.PA,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DEF,self.BSR,self.REP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_YearlyWar']:
		items = cursor.execute("SELECT * FROM Player_YearlyWar " + conditional, values).fetchall()
		return [DB_Player_YearlyWar(i) for i in items]


##############################################################################################
class DB_Pre05_Players:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.careerStartYear = values[1]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.careerStartYear)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Pre05_Players']:
		items = cursor.execute("SELECT * FROM Pre05_Players " + conditional, values).fetchall()
		return [DB_Pre05_Players(i) for i in items]


##############################################################################################
class DB_Team_League_Map:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.LeagueId = values[1]
		self.Year = values[2]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.LeagueId,self.Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_League_Map']:
		items = cursor.execute("SELECT * FROM Team_League_Map " + conditional, values).fetchall()
		return [DB_Team_League_Map(i) for i in items]


##############################################################################################
class DB_Team_OrganizationMap:
	def __init__(self, values : tuple[any]):
		self.teamId = values[0]
		self.year = values[1]
		self.parentOrgId = values[2]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.teamId,self.year,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_OrganizationMap']:
		items = cursor.execute("SELECT * FROM Team_OrganizationMap " + conditional, values).fetchall()
		return [DB_Team_OrganizationMap(i) for i in items]


##############################################################################################
class DB_Team_Parents:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.abbr = values[1]
		self.name = values[2]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.abbr,self.name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_Parents']:
		items = cursor.execute("SELECT * FROM Team_Parents " + conditional, values).fetchall()
		return [DB_Team_Parents(i) for i in items]


##############################################################################################
class DB_Leagues:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.abbr = values[1]
		self.name = values[2]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.abbr,self.name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Leagues']:
		items = cursor.execute("SELECT * FROM Leagues " + conditional, values).fetchall()
		return [DB_Leagues(i) for i in items]


##############################################################################################
class DB_Ranking_Prospect:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.modelIdx = values[3]
		self.isHitter = values[4]
		self.rank = values[5]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.modelIdx,self.isHitter,self.rank)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Ranking_Prospect']:
		items = cursor.execute("SELECT * FROM Ranking_Prospect " + conditional, values).fetchall()
		return [DB_Ranking_Prospect(i) for i in items]


##############################################################################################
class DB_Site_PlayerBio:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.position = values[1]
		self.isPitcher = values[2]
		self.isHitter = values[3]
		self.hasModel = values[4]
		self.parentId = values[5]
		self.levelId = values[6]
		self.status = values[7]
		self.draftPick = values[8]
		self.draftRound = values[9]
		self.draftBonus = values[10]
		self.signingYear = values[11]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.position,self.isPitcher,self.isHitter,self.hasModel,self.parentId,self.levelId,self.status,self.draftPick,self.draftRound,self.draftBonus,self.signingYear)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Site_PlayerBio']:
		items = cursor.execute("SELECT * FROM Site_PlayerBio " + conditional, values).fetchall()
		return [DB_Site_PlayerBio(i) for i in items]


##############################################################################################
class DB_ModelIdx:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.pitcherModelName = values[1]
		self.hitterModelName = values[2]
		self.modelName = values[3]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.pitcherModelName,self.hitterModelName,self.modelName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelIdx']:
		items = cursor.execute("SELECT * FROM ModelIdx " + conditional, values).fetchall()
		return [DB_ModelIdx(i) for i in items]


##############################################################################################
class DB_PlayersInTrainingData:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.modelIdx = values[1]

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.modelIdx)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PlayersInTrainingData']:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData(i) for i in items]


##############################################################################################
