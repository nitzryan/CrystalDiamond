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

	NUM_ELEMENTS = 7

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.Pick,self.Round,self.mlbId,self.Signed,self.Bonus,self.BonusRank)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Draft_Results']:
		items = cursor.execute("SELECT * FROM Draft_Results " + conditional, values).fetchall()
		return [DB_Draft_Results(i) for i in items]

class DB_League_HitterStats:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
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
		self.Hit1B = values[14]
		self.Hit2B = values[15]
		self.Hit3B = values[16]
		self.HitHR = values[17]
		self.BB = values[18]
		self.HBP = values[19]
		self.K = values[20]
		self.SB = values[21]
		self.CS = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.Month,self.AB,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_League_HitterStats']:
		items = cursor.execute("SELECT * FROM League_HitterStats " + conditional, values).fetchall()
		return [DB_League_HitterStats(i) for i in items]

class DB_League_HitterYearStats:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
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
		self.Hit1B = values[14]
		self.Hit2B = values[15]
		self.Hit3B = values[16]
		self.HitHR = values[17]
		self.BB = values[18]
		self.HBP = values[19]
		self.K = values[20]
		self.SB = values[21]
		self.CS = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.Month,self.AB,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_League_HitterYearStats']:
		items = cursor.execute("SELECT * FROM League_HitterYearStats " + conditional, values).fetchall()
		return [DB_League_HitterYearStats(i) for i in items]

class DB_League_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
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

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.Month,self.ERA,self.RA,self.FipConstant,self.wOBA,self.HRPerc,self.BBPerc,self.kPerc,self.GOPerc,self.avg,self.iso)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_League_PitcherStats']:
		items = cursor.execute("SELECT * FROM League_PitcherStats " + conditional, values).fetchall()
		return [DB_League_PitcherStats(i) for i in items]

class DB_League_PitcherYearStats:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
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

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.Month,self.ERA,self.RA,self.FipConstant,self.wOBA,self.HRPerc,self.BBPerc,self.kPerc,self.GOPerc,self.avg,self.iso)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_League_PitcherYearStats']:
		items = cursor.execute("SELECT * FROM League_PitcherYearStats " + conditional, values).fetchall()
		return [DB_League_PitcherYearStats(i) for i in items]

class DB_Level_GameCounts:
	def __init__(self, values : tuple[any]):
		self.LevelId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.MaxPA = values[3]

	NUM_ELEMENTS = 4

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LevelId,self.Year,self.Month,self.MaxPA)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Level_GameCounts']:
		items = cursor.execute("SELECT * FROM Level_GameCounts " + conditional, values).fetchall()
		return [DB_Level_GameCounts(i) for i in items]

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
		self.wRC = values[14]
		self.crWAR = values[15]
		self.crOFF = values[16]
		self.crBSR = values[17]
		self.crDEF = values[18]
		self.SBRateRatio = values[19]
		self.SBPercRatio = values[20]
		self.HRPercRatio = values[21]
		self.BBPercRatio = values[22]
		self.kPercRatio = values[23]
		self.PercC = values[24]
		self.Perc1B = values[25]
		self.Perc2B = values[26]
		self.Perc3B = values[27]
		self.PercSS = values[28]
		self.PercLF = values[29]
		self.PercCF = values[30]
		self.PercRF = values[31]
		self.PercDH = values[32]
		self.Hit1B = values[33]
		self.Hit2B = values[34]
		self.Hit3B = values[35]
		self.HitHR = values[36]
		self.BB = values[37]
		self.HBP = values[38]
		self.K = values[39]
		self.SB = values[40]
		self.CS = values[41]

	NUM_ELEMENTS = 42

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.Age,self.PA,self.InjStatus,self.TrainMask,self.MonthFrac,self.LevelId,self.ParkRunFactor,self.ParkHRFactor,self.AVGRatio,self.OBPRatio,self.ISORatio,self.wRC,self.crWAR,self.crOFF,self.crBSR,self.crDEF,self.SBRateRatio,self.SBPercRatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_HitterStats']:
		items = cursor.execute("SELECT * FROM Model_HitterStats " + conditional, values).fetchall()
		return [DB_Model_HitterStats(i) for i in items]

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
		self.SpPerc = values[11]
		self.GBPercRatio = values[12]
		self.ERARatio = values[13]
		self.FIPRatio = values[14]
		self.wOBARatio = values[15]
		self.HRPercRatio = values[16]
		self.BBPercRatio = values[17]
		self.KPercRatio = values[18]
		self.crWAR = values[19]

	NUM_ELEMENTS = 20

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.Age,self.BF,self.InjStatus,self.TrainMask,self.MonthFrac,self.LevelId,self.ParkRunFactor,self.ParkHRFactor,self.SpPerc,self.GBPercRatio,self.ERARatio,self.FIPRatio,self.wOBARatio,self.HRPercRatio,self.BBPercRatio,self.KPercRatio,self.crWAR)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PitcherStats']:
		items = cursor.execute("SELECT * FROM Model_PitcherStats " + conditional, values).fetchall()
		return [DB_Model_PitcherStats(i) for i in items]

class DB_Model_HitterValue:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.War1Year = values[3]
		self.War2Year = values[4]
		self.War3Year = values[5]
		self.Off1Year = values[6]
		self.Off2Year = values[7]
		self.Off3Year = values[8]
		self.Bsr1Year = values[9]
		self.Bsr2Year = values[10]
		self.Bsr3Year = values[11]
		self.Def1Year = values[12]
		self.Def2Year = values[13]
		self.Def3Year = values[14]
		self.Rep1Year = values[15]
		self.Rep2Year = values[16]
		self.Rep3Year = values[17]
		self.Pa1Year = values[18]
		self.Pa2Year = values[19]
		self.Pa3Year = values[20]

	NUM_ELEMENTS = 21

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.War1Year,self.War2Year,self.War3Year,self.Off1Year,self.Off2Year,self.Off3Year,self.Bsr1Year,self.Bsr2Year,self.Bsr3Year,self.Def1Year,self.Def2Year,self.Def3Year,self.Rep1Year,self.Rep2Year,self.Rep3Year,self.Pa1Year,self.Pa2Year,self.Pa3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_HitterValue']:
		items = cursor.execute("SELECT * FROM Model_HitterValue " + conditional, values).fetchall()
		return [DB_Model_HitterValue(i) for i in items]

class DB_Model_PitcherValue:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.WarSP1Year = values[3]
		self.WarSP2Year = values[4]
		self.WarSP3Year = values[5]
		self.WarRP1Year = values[6]
		self.WarRP2Year = values[7]
		self.WarRP3Year = values[8]
		self.IPSP1Year = values[9]
		self.IPSP2Year = values[10]
		self.IPSP3Year = values[11]
		self.IPRP1Year = values[12]
		self.IPRP2Year = values[13]
		self.IPRP3Year = values[14]

	NUM_ELEMENTS = 15

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.WarSP1Year,self.WarSP2Year,self.WarSP3Year,self.WarRP1Year,self.WarRP2Year,self.WarRP3Year,self.IPSP1Year,self.IPSP2Year,self.IPSP3Year,self.IPRP1Year,self.IPRP2Year,self.IPRP3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PitcherValue']:
		items = cursor.execute("SELECT * FROM Model_PitcherValue " + conditional, values).fetchall()
		return [DB_Model_PitcherValue(i) for i in items]

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

	NUM_ELEMENTS = 11

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.isHitter,self.PA,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DEF,self.BSR,self.REP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PlayerWar']:
		items = cursor.execute("SELECT * FROM Model_PlayerWar " + conditional, values).fetchall()
		return [DB_Model_PlayerWar(i) for i in items]

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
		self.prospectType = values[10]
		self.highestLevelHitter = values[11]
		self.highestLevelPitcher = values[12]
		self.warHitter = values[13]
		self.warPitcher = values[14]
		self.peakWarHitter = values[15]
		self.peakWarPitcher = values[16]
		self.valueHitter = values[17]
		self.valuePitcher = values[18]
		self.valueStarterPerc = values[19]
		self.totalPA = values[20]
		self.totalOuts = values[21]
		self.rateOff = values[22]
		self.rateBsr = values[23]
		self.rateDef = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.isHitter,self.isPitcher,self.signingYear,self.lastProspectYear,self.lastProspectMonth,self.lastMLBSeason,self.ageAtSigningYear,self.draftPick,self.draftSignRank,self.prospectType,self.highestLevelHitter,self.highestLevelPitcher,self.warHitter,self.warPitcher,self.peakWarHitter,self.peakWarPitcher,self.valueHitter,self.valuePitcher,self.valueStarterPerc,self.totalPA,self.totalOuts,self.rateOff,self.rateBsr,self.rateDef)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_Players']:
		items = cursor.execute("SELECT * FROM Model_Players " + conditional, values).fetchall()
		return [DB_Model_Players(i) for i in items]

class DB_Model_OrgLeagueStatus:
	def __init__(self, values : tuple[any]):
		self.OrgId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.MLB_PF = values[3]
		self.AAA_PF = values[4]
		self.AA_PF = values[5]
		self.HA_PF = values[6]
		self.A_PF = values[7]
		self.LA_PF = values[8]
		self.Rk_PF = values[9]
		self.DSL_PF = values[10]
		self.MLB_MonthsFrac = values[11]
		self.AAA_MonthsFrac = values[12]
		self.AA_MonthsFrac = values[13]
		self.HA_MonthsFrac = values[14]
		self.A_MonthsFrac = values[15]
		self.LA_MonthsFrac = values[16]
		self.Rk_MonthsFrac = values[17]
		self.DSL_MonthsFrac = values[18]

	NUM_ELEMENTS = 19

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.OrgId,self.Year,self.Month,self.MLB_PF,self.AAA_PF,self.AA_PF,self.HA_PF,self.A_PF,self.LA_PF,self.Rk_PF,self.DSL_PF,self.MLB_MonthsFrac,self.AAA_MonthsFrac,self.AA_MonthsFrac,self.HA_MonthsFrac,self.A_MonthsFrac,self.LA_MonthsFrac,self.Rk_MonthsFrac,self.DSL_MonthsFrac)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_OrgLeagueStatus']:
		items = cursor.execute("SELECT * FROM Model_OrgLeagueStatus " + conditional, values).fetchall()
		return [DB_Model_OrgLeagueStatus(i) for i in items]

class DB_Model_HitterLevelStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.Pa = values[4]
		self.Hit1B = values[5]
		self.Hit2B = values[6]
		self.Hit3B = values[7]
		self.HitHR = values[8]
		self.BB = values[9]
		self.HBP = values[10]
		self.K = values[11]
		self.SB = values[12]
		self.CS = values[13]
		self.ParkRunFactor = values[14]

	NUM_ELEMENTS = 15

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.ParkRunFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_HitterLevelStats']:
		items = cursor.execute("SELECT * FROM Model_HitterLevelStats " + conditional, values).fetchall()
		return [DB_Model_HitterLevelStats(i) for i in items]

class DB_Model_LeagueHittingBaselines:
	def __init__(self, values : tuple[any]):
		self.Year = values[0]
		self.Month = values[1]
		self.LeagueId = values[2]
		self.LevelId = values[3]
		self.Hit1B = values[4]
		self.Hit2B = values[5]
		self.Hit3B = values[6]
		self.HitHR = values[7]
		self.BB = values[8]
		self.HBP = values[9]
		self.K = values[10]
		self.SB = values[11]
		self.CS = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.Month,self.LeagueId,self.LevelId,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_LeagueHittingBaselines']:
		items = cursor.execute("SELECT * FROM Model_LeagueHittingBaselines " + conditional, values).fetchall()
		return [DB_Model_LeagueHittingBaselines(i) for i in items]

class DB_Model_PitcherLevelStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.Outs_SP = values[4]
		self.Outs_RP = values[5]
		self.G = values[6]
		self.GS = values[7]
		self.ERA = values[8]
		self.FIP = values[9]
		self.HR = values[10]
		self.BB = values[11]
		self.HBP = values[12]
		self.K = values[13]
		self.ParkRunFactor = values[14]

	NUM_ELEMENTS = 15

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.LevelId,self.Outs_SP,self.Outs_RP,self.G,self.GS,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K,self.ParkRunFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_PitcherLevelStats']:
		items = cursor.execute("SELECT * FROM Model_PitcherLevelStats " + conditional, values).fetchall()
		return [DB_Model_PitcherLevelStats(i) for i in items]

class DB_Model_LeaguePitchingBaselines:
	def __init__(self, values : tuple[any]):
		self.Year = values[0]
		self.Month = values[1]
		self.LeagueId = values[2]
		self.LevelId = values[3]
		self.ERA = values[4]
		self.FIP = values[5]
		self.HR = values[6]
		self.BB = values[7]
		self.HBP = values[8]
		self.K = values[9]

	NUM_ELEMENTS = 10

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.Month,self.LeagueId,self.LevelId,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_LeaguePitchingBaselines']:
		items = cursor.execute("SELECT * FROM Model_LeaguePitchingBaselines " + conditional, values).fetchall()
		return [DB_Model_LeaguePitchingBaselines(i) for i in items]

class DB_Model_TrainingHistory:
	def __init__(self, values : tuple[any]):
		self.ModelName = values[0]
		self.IsHitter = values[1]
		self.TestLoss = values[2]
		self.ModelIdx = values[3]
		self.NumLayers = values[4]
		self.HiddenSize = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelName,self.IsHitter,self.TestLoss,self.ModelIdx,self.NumLayers,self.HiddenSize)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_TrainingHistory']:
		items = cursor.execute("SELECT * FROM Model_TrainingHistory " + conditional, values).fetchall()
		return [DB_Model_TrainingHistory(i) for i in items]

class DB_Output_PlayerWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.ModelIdx = values[3]
		self.year = values[4]
		self.month = values[5]
		self.war0 = values[6]
		self.war1 = values[7]
		self.war2 = values[8]
		self.war3 = values[9]
		self.war4 = values[10]
		self.war5 = values[11]
		self.war6 = values[12]
		self.war = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.isHitter,self.ModelIdx,self.year,self.month,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWar']:
		items = cursor.execute("SELECT * FROM Output_PlayerWar " + conditional, values).fetchall()
		return [DB_Output_PlayerWar(i) for i in items]

class DB_Output_HitterStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Model = values[1]
		self.ModelIdx = values[2]
		self.Year = values[3]
		self.Month = values[4]
		self.LevelId = values[5]
		self.Pa = values[6]
		self.Hit1B = values[7]
		self.Hit2B = values[8]
		self.Hit3B = values[9]
		self.HitHR = values[10]
		self.BB = values[11]
		self.HBP = values[12]
		self.K = values[13]
		self.SB = values[14]
		self.CS = values[15]
		self.ParkRunFactor = values[16]
		self.PercC = values[17]
		self.Perc1B = values[18]
		self.Perc2B = values[19]
		self.Perc3B = values[20]
		self.PercSS = values[21]
		self.PercLF = values[22]
		self.PercCF = values[23]
		self.PercRF = values[24]
		self.PercDH = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Model,self.ModelIdx,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterStats']:
		items = cursor.execute("SELECT * FROM Output_HitterStats " + conditional, values).fetchall()
		return [DB_Output_HitterStats(i) for i in items]

class DB_Output_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Model = values[1]
		self.ModelIdx = values[2]
		self.Year = values[3]
		self.Month = values[4]
		self.levelId = values[5]
		self.Outs_SP = values[6]
		self.Outs_RP = values[7]
		self.GS = values[8]
		self.GR = values[9]
		self.ERA = values[10]
		self.FIP = values[11]
		self.HR = values[12]
		self.BB = values[13]
		self.HBP = values[14]
		self.K = values[15]
		self.ParkRunFactor = values[16]
		self.SP_Perc = values[17]
		self.RP_Perc = values[18]

	NUM_ELEMENTS = 19

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Model,self.ModelIdx,self.Year,self.Month,self.levelId,self.Outs_SP,self.Outs_RP,self.GS,self.GR,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K,self.ParkRunFactor,self.SP_Perc,self.RP_Perc)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherStats']:
		items = cursor.execute("SELECT * FROM Output_PitcherStats " + conditional, values).fetchall()
		return [DB_Output_PitcherStats(i) for i in items]

class DB_Output_HitterValue:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.ModelIdx = values[2]
		self.year = values[3]
		self.month = values[4]
		self.WAR1Year = values[5]
		self.OFF1Year = values[6]
		self.BSR1Year = values[7]
		self.DEF1Year = values[8]
		self.WAR2Year = values[9]
		self.OFF2Year = values[10]
		self.BSR2Year = values[11]
		self.DEF2Year = values[12]
		self.WAR3Year = values[13]
		self.OFF3Year = values[14]
		self.BSR3Year = values[15]
		self.DEF3Year = values[16]
		self.PA1Year = values[17]
		self.PA2Year = values[18]
		self.PA3Year = values[19]

	NUM_ELEMENTS = 20

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.ModelIdx,self.year,self.month,self.WAR1Year,self.OFF1Year,self.BSR1Year,self.DEF1Year,self.WAR2Year,self.OFF2Year,self.BSR2Year,self.DEF2Year,self.WAR3Year,self.OFF3Year,self.BSR3Year,self.DEF3Year,self.PA1Year,self.PA2Year,self.PA3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterValue']:
		items = cursor.execute("SELECT * FROM Output_HitterValue " + conditional, values).fetchall()
		return [DB_Output_HitterValue(i) for i in items]

class DB_Output_PitcherValue:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.ModelIdx = values[2]
		self.year = values[3]
		self.month = values[4]
		self.WarSP1Year = values[5]
		self.WarRP1Year = values[6]
		self.WarSP2Year = values[7]
		self.WarRP2Year = values[8]
		self.WarSP3Year = values[9]
		self.WarRP3Year = values[10]
		self.IPSP1Year = values[11]
		self.IPRP1Year = values[12]
		self.IPSP2Year = values[13]
		self.IPRP2Year = values[14]
		self.IPSP3Year = values[15]
		self.IPRP3Year = values[16]

	NUM_ELEMENTS = 17

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.ModelIdx,self.year,self.month,self.WarSP1Year,self.WarRP1Year,self.WarSP2Year,self.WarRP2Year,self.WarSP3Year,self.WarRP3Year,self.IPSP1Year,self.IPRP1Year,self.IPSP2Year,self.IPRP2Year,self.IPSP3Year,self.IPRP3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherValue']:
		items = cursor.execute("SELECT * FROM Output_PitcherValue " + conditional, values).fetchall()
		return [DB_Output_PitcherValue(i) for i in items]

class DB_Park_Factors:
	def __init__(self, values : tuple[any]):
		self.StadiumId = values[0]
		self.LeagueId = values[1]
		self.LevelId = values[2]
		self.Year = values[3]
		self.RunFactor = values[4]
		self.HRFactor = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.StadiumId,self.LeagueId,self.LevelId,self.Year,self.RunFactor,self.HRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Park_Factors']:
		items = cursor.execute("SELECT * FROM Park_Factors " + conditional, values).fetchall()
		return [DB_Park_Factors(i) for i in items]

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

	NUM_ELEMENTS = 16

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.fangraphsId,self.position,self.birthYear,self.birthMonth,self.birthDate,self.draftPick,self.signingYear,self.signingMonth,self.signingDate,self.signingBonus,self.bats,self.throws,self.isRetired,self.useFirstName,self.useLastName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player']:
		items = cursor.execute("SELECT * FROM Player " + conditional, values).fetchall()
		return [DB_Player(i) for i in items]

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

	NUM_ELEMENTS = 15

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.isPitcher,self.isHitter,self.isActive,self.serviceReached,self.mlbStartYear,self.mlbRookieYear,self.mlbRookieMonth,self.serviceEndYear,self.serviceLapseYear,self.agedOut,self.playingGap,self.ignorePlayer,self.highestLevelPitcher,self.highestLevelHitter)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_CareerStatus']:
		items = cursor.execute("SELECT * FROM Player_CareerStatus " + conditional, values).fetchall()
		return [DB_Player_CareerStatus(i) for i in items]

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
		self.StadiumId = values[19]
		self.IsHome = values[20]
		self.TeamId = values[21]
		self.oppTeamId = values[22]
		self.LeagueId = values[23]

	NUM_ELEMENTS = 24

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.gameLogId,self.gameId,self.mlbId,self.Day,self.Month,self.Year,self.AB,self.PA,self.H,self.hit2B,self.hit3B,self.HR,self.K,self.BB,self.SB,self.CS,self.HBP,self.Position,self.LevelId,self.StadiumId,self.IsHome,self.TeamId,self.oppTeamId,self.LeagueId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_GameLog']:
		items = cursor.execute("SELECT * FROM Player_Hitter_GameLog " + conditional, values).fetchall()
		return [DB_Player_Hitter_GameLog(i) for i in items]

class DB_Player_Hitter_MonthAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.teamId = values[4]
		self.leagueId = values[5]
		self.ParkFactor = values[6]
		self.PA = values[7]
		self.AVG = values[8]
		self.OBP = values[9]
		self.SLG = values[10]
		self.ISO = values[11]
		self.wOBA = values[12]
		self.wRC = values[13]
		self.HRPerc = values[14]
		self.BBPerc = values[15]
		self.KPerc = values[16]
		self.SBRate = values[17]
		self.SBPerc = values[18]
		self.SB = values[19]
		self.CS = values[20]
		self.HR = values[21]
		self.crWAR = values[22]
		self.crOFF = values[23]
		self.crBSR = values[24]
		self.crDEF = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.ParkFactor,self.PA,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.wRC,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc,self.SB,self.CS,self.HR,self.crWAR,self.crOFF,self.crBSR,self.crDEF)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthAdvanced " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthAdvanced(i) for i in items]

class DB_Player_Hitter_MonthStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.LeagueId = values[4]
		self.AB = values[5]
		self.PA = values[6]
		self.H = values[7]
		self.hit2B = values[8]
		self.hit3B = values[9]
		self.HR = values[10]
		self.K = values[11]
		self.BB = values[12]
		self.SB = values[13]
		self.CS = values[14]
		self.HBP = values[15]
		self.ParkRunFactor = values[16]
		self.ParkHRFactor = values[17]
		self.GamesC = values[18]
		self.Games1B = values[19]
		self.Games2B = values[20]
		self.Games3B = values[21]
		self.GamesSS = values[22]
		self.GamesLF = values[23]
		self.GamesCF = values[24]
		self.GamesRF = values[25]
		self.GamesDH = values[26]

	NUM_ELEMENTS = 27

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.LeagueId,self.AB,self.PA,self.H,self.hit2B,self.hit3B,self.HR,self.K,self.BB,self.SB,self.CS,self.HBP,self.ParkRunFactor,self.ParkHRFactor,self.GamesC,self.Games1B,self.Games2B,self.Games3B,self.GamesSS,self.GamesLF,self.GamesCF,self.GamesRF,self.GamesDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthStats']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthStats " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthStats(i) for i in items]

class DB_Player_Hitter_MonthBaserunning:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.LeagueId = values[4]
		self.rSB = values[5]
		self.rUBR = values[6]
		self.rGIDP = values[7]
		self.TimesOnFirst = values[8]
		self.TimesOnBase = values[9]

	NUM_ELEMENTS = 10

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.LeagueId,self.rSB,self.rUBR,self.rGIDP,self.TimesOnFirst,self.TimesOnBase)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthBaserunning']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthBaserunning " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthBaserunning(i) for i in items]

class DB_Player_Hitter_MonthlyRatios:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.LeagueId = values[4]
		self.AVGRatio = values[5]
		self.OBPRatio = values[6]
		self.ISORatio = values[7]
		self.wRC = values[8]
		self.SBRateRatio = values[9]
		self.SBPercRatio = values[10]
		self.HRPercRatio = values[11]
		self.BBPercRatio = values[12]
		self.kPercRatio = values[13]
		self.PercC = values[14]
		self.Perc1B = values[15]
		self.Perc2B = values[16]
		self.Perc3B = values[17]
		self.PercSS = values[18]
		self.PercLF = values[19]
		self.PercCF = values[20]
		self.PercRF = values[21]
		self.PercDH = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.LeagueId,self.AVGRatio,self.OBPRatio,self.ISORatio,self.wRC,self.SBRateRatio,self.SBPercRatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthlyRatios']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthlyRatios " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthlyRatios(i) for i in items]

class DB_Player_Hitter_YearAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.teamId = values[3]
		self.leagueId = values[4]
		self.ParkFactor = values[5]
		self.PA = values[6]
		self.AVG = values[7]
		self.OBP = values[8]
		self.SLG = values[9]
		self.ISO = values[10]
		self.wOBA = values[11]
		self.wRC = values[12]
		self.HR = values[13]
		self.BBPerc = values[14]
		self.KPerc = values[15]
		self.SB = values[16]
		self.CS = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.teamId,self.leagueId,self.ParkFactor,self.PA,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.wRC,self.HR,self.BBPerc,self.KPerc,self.SB,self.CS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_YearAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Hitter_YearAdvanced " + conditional, values).fetchall()
		return [DB_Player_Hitter_YearAdvanced(i) for i in items]

class DB_Player_OrgMap:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.day = values[3]
		self.parentOrgId = values[4]

	NUM_ELEMENTS = 5

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.day,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_OrgMap']:
		items = cursor.execute("SELECT * FROM Player_OrgMap " + conditional, values).fetchall()
		return [DB_Player_OrgMap(i) for i in items]

class DB_Transaction_Log:
	def __init__(self, values : tuple[any]):
		self.transactionId = values[0]
		self.mlbId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.day = values[4]
		self.toIL = values[5]
		self.parentOrgId = values[6]

	NUM_ELEMENTS = 7

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.transactionId,self.mlbId,self.year,self.month,self.day,self.toIL,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Transaction_Log']:
		items = cursor.execute("SELECT * FROM Transaction_Log " + conditional, values).fetchall()
		return [DB_Transaction_Log(i) for i in items]

class DB_Player_Pitcher_GameLog:
	def __init__(self, values : tuple[any]):
		self.gameLogId = values[0]
		self.gameId = values[1]
		self.mlbId = values[2]
		self.day = values[3]
		self.month = values[4]
		self.year = values[5]
		self.started = values[6]
		self.battersFaced = values[7]
		self.outs = values[8]
		self.GO = values[9]
		self.AO = values[10]
		self.R = values[11]
		self.ER = values[12]
		self.h = values[13]
		self.k = values[14]
		self.BB = values[15]
		self.HBP = values[16]
		self.hit2B = values[17]
		self.hit3B = values[18]
		self.HR = values[19]
		self.levelId = values[20]
		self.stadiumId = values[21]
		self.isHome = values[22]
		self.TeamId = values[23]
		self.oppTeamId = values[24]
		self.LeagueId = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.gameLogId,self.gameId,self.mlbId,self.day,self.month,self.year,self.started,self.battersFaced,self.outs,self.GO,self.AO,self.R,self.ER,self.h,self.k,self.BB,self.HBP,self.hit2B,self.hit3B,self.HR,self.levelId,self.stadiumId,self.isHome,self.TeamId,self.oppTeamId,self.LeagueId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_GameLog']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_GameLog " + conditional, values).fetchall()
		return [DB_Player_Pitcher_GameLog(i) for i in items]

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
		self.SPPerc = values[8]
		self.GBRatio = values[9]
		self.ERA = values[10]
		self.FIP = values[11]
		self.KPerc = values[12]
		self.BBPerc = values[13]
		self.HRPerc = values[14]
		self.HR = values[15]
		self.wOBA = values[16]
		self.crWAR = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.BF,self.Outs,self.SPPerc,self.GBRatio,self.ERA,self.FIP,self.KPerc,self.BBPerc,self.HRPerc,self.HR,self.wOBA,self.crWAR)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthAdvanced " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthAdvanced(i) for i in items]

class DB_Player_Pitcher_MonthStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.levelId = values[3]
		self.LeagueId = values[4]
		self.battersFaced = values[5]
		self.Outs = values[6]
		self.SPPerc = values[7]
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
		self.ParkRunFactor = values[19]
		self.ParkHRFactor = values[20]

	NUM_ELEMENTS = 21

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.levelId,self.LeagueId,self.battersFaced,self.Outs,self.SPPerc,self.GO,self.AO,self.R,self.ER,self.h,self.k,self.BB,self.HBP,self.hit2B,self.hit3B,self.HR,self.ParkRunFactor,self.ParkHRFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthStats']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthStats " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthStats(i) for i in items]

class DB_Player_Pitcher_MonthlyRatios:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.levelId = values[3]
		self.LeagueId = values[4]
		self.SPPerc = values[5]
		self.GBPercRatio = values[6]
		self.ERARatio = values[7]
		self.FIPRatio = values[8]
		self.wOBARatio = values[9]
		self.HRPercRatio = values[10]
		self.BBPercRatio = values[11]
		self.kPercRatio = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.levelId,self.LeagueId,self.SPPerc,self.GBPercRatio,self.ERARatio,self.FIPRatio,self.wOBARatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_MonthlyRatios']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_MonthlyRatios " + conditional, values).fetchall()
		return [DB_Player_Pitcher_MonthlyRatios(i) for i in items]

class DB_Player_Pitcher_YearAdvanced:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.levelId = values[1]
		self.year = values[2]
		self.teamId = values[3]
		self.leagueId = values[4]
		self.BF = values[5]
		self.Outs = values[6]
		self.SPPerc = values[7]
		self.GBRatio = values[8]
		self.ERA = values[9]
		self.FIP = values[10]
		self.KPerc = values[11]
		self.BBPerc = values[12]
		self.HR = values[13]
		self.wOBA = values[14]

	NUM_ELEMENTS = 15

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.teamId,self.leagueId,self.BF,self.Outs,self.SPPerc,self.GBRatio,self.ERA,self.FIP,self.KPerc,self.BBPerc,self.HR,self.wOBA)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Pitcher_YearAdvanced']:
		items = cursor.execute("SELECT * FROM Player_Pitcher_YearAdvanced " + conditional, values).fetchall()
		return [DB_Player_Pitcher_YearAdvanced(i) for i in items]

class DB_Player_ServiceLapse:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_ServiceLapse']:
		items = cursor.execute("SELECT * FROM Player_ServiceLapse " + conditional, values).fetchall()
		return [DB_Player_ServiceLapse(i) for i in items]

class DB_Player_ServiceTime:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.serviceYear = values[2]
		self.serviceDays = values[3]

	NUM_ELEMENTS = 4

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.serviceYear,self.serviceDays)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_ServiceTime']:
		items = cursor.execute("SELECT * FROM Player_ServiceTime " + conditional, values).fetchall()
		return [DB_Player_ServiceTime(i) for i in items]

class DB_Player_YearlyWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.isHitter = values[2]
		self.PA = values[3]
		self.IP_SP = values[4]
		self.IP_RP = values[5]
		self.WAR_h = values[6]
		self.WAR_s = values[7]
		self.WAR_r = values[8]
		self.OFF = values[9]
		self.DEF = values[10]
		self.BSR = values[11]
		self.REP = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.isHitter,self.PA,self.IP_SP,self.IP_RP,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DEF,self.BSR,self.REP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_YearlyWar']:
		items = cursor.execute("SELECT * FROM Player_YearlyWar " + conditional, values).fetchall()
		return [DB_Player_YearlyWar(i) for i in items]

class DB_Player_MonthlyWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.PA = values[3]
		self.IP_SP = values[4]
		self.IP_RP = values[5]
		self.WAR_h = values[6]
		self.WAR_s = values[7]
		self.WAR_r = values[8]
		self.OFF = values[9]
		self.DEF = values[10]
		self.BSR = values[11]
		self.REP = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.PA,self.IP_SP,self.IP_RP,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DEF,self.BSR,self.REP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_MonthlyWar']:
		items = cursor.execute("SELECT * FROM Player_MonthlyWar " + conditional, values).fetchall()
		return [DB_Player_MonthlyWar(i) for i in items]

class DB_Pre05_Players:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.careerStartYear = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.careerStartYear)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Pre05_Players']:
		items = cursor.execute("SELECT * FROM Pre05_Players " + conditional, values).fetchall()
		return [DB_Pre05_Players(i) for i in items]

class DB_Team_League_Map:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.LeagueId = values[1]
		self.Year = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.LeagueId,self.Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_League_Map']:
		items = cursor.execute("SELECT * FROM Team_League_Map " + conditional, values).fetchall()
		return [DB_Team_League_Map(i) for i in items]

class DB_Team_OrganizationMap:
	def __init__(self, values : tuple[any]):
		self.teamId = values[0]
		self.year = values[1]
		self.parentOrgId = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.teamId,self.year,self.parentOrgId)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_OrganizationMap']:
		items = cursor.execute("SELECT * FROM Team_OrganizationMap " + conditional, values).fetchall()
		return [DB_Team_OrganizationMap(i) for i in items]

class DB_Team_Parents:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.abbr = values[1]
		self.name = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.abbr,self.name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Team_Parents']:
		items = cursor.execute("SELECT * FROM Team_Parents " + conditional, values).fetchall()
		return [DB_Team_Parents(i) for i in items]

class DB_Leagues:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.abbr = values[1]
		self.name = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.abbr,self.name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Leagues']:
		items = cursor.execute("SELECT * FROM Leagues " + conditional, values).fetchall()
		return [DB_Leagues(i) for i in items]

class DB_Ranking_Prospect:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.year = values[1]
		self.month = values[2]
		self.modelIdx = values[3]
		self.isHitter = values[4]
		self.rank = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.modelIdx,self.isHitter,self.rank)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Ranking_Prospect']:
		items = cursor.execute("SELECT * FROM Ranking_Prospect " + conditional, values).fetchall()
		return [DB_Ranking_Prospect(i) for i in items]

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

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.position,self.isPitcher,self.isHitter,self.hasModel,self.parentId,self.levelId,self.status,self.draftPick,self.draftRound,self.draftBonus,self.signingYear)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Site_PlayerBio']:
		items = cursor.execute("SELECT * FROM Site_PlayerBio " + conditional, values).fetchall()
		return [DB_Site_PlayerBio(i) for i in items]

class DB_ModelIdx:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.pitcherModelName = values[1]
		self.hitterModelName = values[2]
		self.modelName = values[3]

	NUM_ELEMENTS = 4

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.pitcherModelName,self.hitterModelName,self.modelName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelIdx']:
		items = cursor.execute("SELECT * FROM ModelIdx " + conditional, values).fetchall()
		return [DB_ModelIdx(i) for i in items]

class DB_PlayersInTrainingData:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.modelIdx = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.modelIdx)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PlayersInTrainingData']:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData(i) for i in items]

class DB_LeagueStats:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
		self.Year = values[1]
		self.avgWOBA = values[2]
		self.avgHitterWOBA = values[3]
		self.wOBAScale = values[4]
		self.wBB = values[5]
		self.wHBP = values[6]
		self.w1B = values[7]
		self.w2B = values[8]
		self.w3B = values[9]
		self.wHR = values[10]
		self.runSB = values[11]
		self.runCS = values[12]
		self.runErr = values[13]
		self.runGIDP = values[14]
		self.probGIDP = values[15]
		self.RPerPA = values[16]
		self.RPerWin = values[17]
		self.LeaguePA = values[18]
		self.LeagueGames = values[19]
		self.cFIP = values[20]
		self.FIPR9Adjustment = values[21]
		self.LeagueERA = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.avgWOBA,self.avgHitterWOBA,self.wOBAScale,self.wBB,self.wHBP,self.w1B,self.w2B,self.w3B,self.wHR,self.runSB,self.runCS,self.runErr,self.runGIDP,self.probGIDP,self.RPerPA,self.RPerWin,self.LeaguePA,self.LeagueGames,self.cFIP,self.FIPR9Adjustment,self.LeagueERA)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_LeagueStats']:
		items = cursor.execute("SELECT * FROM LeagueStats " + conditional, values).fetchall()
		return [DB_LeagueStats(i) for i in items]

class DB_LeagueRunMatrix:
	def __init__(self, values : tuple[any]):
		self.LeagueId = values[0]
		self.Year = values[1]
		self.runExpDict = values[2]
		self.fieldOutcomeDict = values[3]
		self.bsrAdv1st3rdSingleDict = values[4]
		self.bsrAdv2ndHomeSingleDict = values[5]
		self.bsrAdv1stHomeDoubleDict = values[6]
		self.bsrAvoidForce2ndDict = values[7]
		self.bsrAdv2nd3rdGroundoutDict = values[8]
		self.bsrAdv1st2ndFlyoutDict = values[9]
		self.bsrAdv2nd3rdFlyoutDict = values[10]
		self.bsrAdv3rdHomeFlyoutDict = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.runExpDict,self.fieldOutcomeDict,self.bsrAdv1st3rdSingleDict,self.bsrAdv2ndHomeSingleDict,self.bsrAdv1stHomeDoubleDict,self.bsrAvoidForce2ndDict,self.bsrAdv2nd3rdGroundoutDict,self.bsrAdv1st2ndFlyoutDict,self.bsrAdv2nd3rdFlyoutDict,self.bsrAdv3rdHomeFlyoutDict)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_LeagueRunMatrix']:
		items = cursor.execute("SELECT * FROM LeagueRunMatrix " + conditional, values).fetchall()
		return [DB_LeagueRunMatrix(i) for i in items]

class DB_GamePlayByPlay:
	def __init__(self, values : tuple[any]):
		self.EventId = values[0]
		self.GameId = values[1]
		self.LeagueId = values[2]
		self.Year = values[3]
		self.Month = values[4]
		self.HitterId = values[5]
		self.PitcherId = values[6]
		self.FielderId = values[7]
		self.Run1stId = values[8]
		self.Run2ndId = values[9]
		self.Run3rdId = values[10]
		self.StartOuts = values[11]
		self.Inning = values[12]
		self.IsTop = values[13]
		self.StartBaseOccupancy = values[14]
		self.EndOuts = values[15]
		self.EndBaseOccupancy = values[16]
		self.RunsScored = values[17]
		self.RunsScoredInningAfterEvent = values[18]
		self.Result = values[19]
		self.HitZone = values[20]
		self.HitHardness = values[21]
		self.HitTrajectory = values[22]
		self.HitCoordX = values[23]
		self.HitCoordY = values[24]
		self.LaunchSpeed = values[25]
		self.LaunchAngle = values[26]
		self.LaunchDistance = values[27]
		self.Run1stOutcome = values[28]
		self.Run2ndOutcome = values[29]
		self.Run3rdOutcome = values[30]

	NUM_ELEMENTS = 31

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.EventId,self.GameId,self.LeagueId,self.Year,self.Month,self.HitterId,self.PitcherId,self.FielderId,self.Run1stId,self.Run2ndId,self.Run3rdId,self.StartOuts,self.Inning,self.IsTop,self.StartBaseOccupancy,self.EndOuts,self.EndBaseOccupancy,self.RunsScored,self.RunsScoredInningAfterEvent,self.Result,self.HitZone,self.HitHardness,self.HitTrajectory,self.HitCoordX,self.HitCoordY,self.LaunchSpeed,self.LaunchAngle,self.LaunchDistance,self.Run1stOutcome,self.Run2ndOutcome,self.Run3rdOutcome)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_GamePlayByPlay']:
		items = cursor.execute("SELECT * FROM GamePlayByPlay " + conditional, values).fetchall()
		return [DB_GamePlayByPlay(i) for i in items]

class DB_Output_HitterStatsAggregation:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Model = values[1]
		self.Year = values[2]
		self.Month = values[3]
		self.LevelId = values[4]
		self.Pa = values[5]
		self.Hit1B = values[6]
		self.Hit2B = values[7]
		self.Hit3B = values[8]
		self.HitHR = values[9]
		self.BB = values[10]
		self.HBP = values[11]
		self.K = values[12]
		self.SB = values[13]
		self.CS = values[14]
		self.ParkRunFactor = values[15]
		self.PercC = values[16]
		self.Perc1B = values[17]
		self.Perc2B = values[18]
		self.Perc3B = values[19]
		self.PercSS = values[20]
		self.PercLF = values[21]
		self.PercCF = values[22]
		self.PercRF = values[23]
		self.PercDH = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Model,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterStatsAggregation']:
		items = cursor.execute("SELECT * FROM Output_HitterStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterStatsAggregation(i) for i in items]

class DB_Output_PitcherStatsAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Model = values[1]
		self.Year = values[2]
		self.Month = values[3]
		self.levelId = values[4]
		self.Outs_SP = values[5]
		self.Outs_RP = values[6]
		self.GS = values[7]
		self.GR = values[8]
		self.ERA = values[9]
		self.FIP = values[10]
		self.HR = values[11]
		self.BB = values[12]
		self.HBP = values[13]
		self.K = values[14]
		self.ParkRunFactor = values[15]
		self.SP_Perc = values[16]
		self.RP_Perc = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Model,self.Year,self.Month,self.levelId,self.Outs_SP,self.Outs_RP,self.GS,self.GR,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K,self.ParkRunFactor,self.SP_Perc,self.RP_Perc)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherStatsAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitcherStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherStatsAggregation(i) for i in items]

class DB_Output_HitterValueAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.year = values[2]
		self.month = values[3]
		self.WAR1Year = values[4]
		self.OFF1Year = values[5]
		self.BSR1Year = values[6]
		self.DEF1Year = values[7]
		self.WAR2Year = values[8]
		self.OFF2Year = values[9]
		self.BSR2Year = values[10]
		self.DEF2Year = values[11]
		self.WAR3Year = values[12]
		self.OFF3Year = values[13]
		self.BSR3Year = values[14]
		self.DEF3Year = values[15]
		self.PA1Year = values[16]
		self.PA2Year = values[17]
		self.PA3Year = values[18]

	NUM_ELEMENTS = 19

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.year,self.month,self.WAR1Year,self.OFF1Year,self.BSR1Year,self.DEF1Year,self.WAR2Year,self.OFF2Year,self.BSR2Year,self.DEF2Year,self.WAR3Year,self.OFF3Year,self.BSR3Year,self.DEF3Year,self.PA1Year,self.PA2Year,self.PA3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_HitterValueAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterValueAggregation(i) for i in items]

class DB_Output_PitcherValueAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.year = values[2]
		self.month = values[3]
		self.WarSP1Year = values[4]
		self.WarRP1Year = values[5]
		self.WarSP2Year = values[6]
		self.WarRP2Year = values[7]
		self.WarSP3Year = values[8]
		self.WarRP3Year = values[9]
		self.IPSP1Year = values[10]
		self.IPRP1Year = values[11]
		self.IPSP2Year = values[12]
		self.IPRP2Year = values[13]
		self.IPSP3Year = values[14]
		self.IPRP3Year = values[15]

	NUM_ELEMENTS = 16

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.year,self.month,self.WarSP1Year,self.WarRP1Year,self.WarSP2Year,self.WarRP2Year,self.WarSP3Year,self.WarRP3Year,self.IPSP1Year,self.IPRP1Year,self.IPSP2Year,self.IPRP2Year,self.IPSP3Year,self.IPRP3Year)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitcherValueAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherValueAggregation(i) for i in items]

class DB_Output_PlayerWarAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.year = values[3]
		self.month = values[4]
		self.war0 = values[5]
		self.war1 = values[6]
		self.war2 = values[7]
		self.war3 = values[8]
		self.war4 = values[9]
		self.war5 = values[10]
		self.war6 = values[11]
		self.war = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.model,self.isHitter,self.year,self.month,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWarAggregation']:
		items = cursor.execute("SELECT * FROM Output_PlayerWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerWarAggregation(i) for i in items]

class DB_GamePlayByPlayFlags:
	def __init__(self, values : tuple[any]):
		self.EventId = values[0]
		self.Flag = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.EventId,self.Flag)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_GamePlayByPlayFlags']:
		items = cursor.execute("SELECT * FROM GamePlayByPlayFlags " + conditional, values).fetchall()
		return [DB_GamePlayByPlayFlags(i) for i in items]


##############################################################################################
