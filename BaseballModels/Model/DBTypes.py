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
		self.crDRAA = values[18]
		self.crDPOS = values[19]
		self.SBRateRatio = values[20]
		self.SBPercRatio = values[21]
		self.HRPercRatio = values[22]
		self.BBPercRatio = values[23]
		self.kPercRatio = values[24]
		self.PercC = values[25]
		self.Perc1B = values[26]
		self.Perc2B = values[27]
		self.Perc3B = values[28]
		self.PercSS = values[29]
		self.PercLF = values[30]
		self.PercCF = values[31]
		self.PercRF = values[32]
		self.PercDH = values[33]
		self.Hit1B = values[34]
		self.Hit2B = values[35]
		self.Hit3B = values[36]
		self.HitHR = values[37]
		self.BB = values[38]
		self.HBP = values[39]
		self.K = values[40]
		self.SB = values[41]
		self.CS = values[42]

	NUM_ELEMENTS = 43

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.Age,self.PA,self.InjStatus,self.TrainMask,self.MonthFrac,self.LevelId,self.ParkRunFactor,self.ParkHRFactor,self.AVGRatio,self.OBPRatio,self.ISORatio,self.wRC,self.crWAR,self.crOFF,self.crBSR,self.crDRAA,self.crDPOS,self.SBRateRatio,self.SBPercRatio,self.HRPercRatio,self.BBPercRatio,self.kPercRatio,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS)
                        
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

class DB_Model_LevelYearGames:
	def __init__(self, values : tuple[any]):
		self.Year = values[0]
		self.Month = values[1]
		self.MLB_Games = values[2]
		self.AAA_Games = values[3]
		self.AA_Games = values[4]
		self.HA_Games = values[5]
		self.A_Games = values[6]
		self.LA_Games = values[7]
		self.Rk_Games = values[8]
		self.DSL_Games = values[9]

	NUM_ELEMENTS = 10

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.Month,self.MLB_Games,self.AAA_Games,self.AA_Games,self.HA_Games,self.A_Games,self.LA_Games,self.Rk_Games,self.DSL_Games)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_LevelYearGames']:
		items = cursor.execute("SELECT * FROM Model_LevelYearGames " + conditional, values).fetchall()
		return [DB_Model_LevelYearGames(i) for i in items]

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
		self.BSR = values[15]
		self.DRAA = values[16]
		self.DPOS = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.ParkRunFactor,self.BSR,self.DRAA,self.DPOS)
                        
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

class DB_Player_Fielder_GameLog:
	def __init__(self, values : tuple[any]):
		self.gameLogId = values[0]
		self.gameId = values[1]
		self.mlbId = values[2]
		self.LeagueId = values[3]
		self.TeamId = values[4]
		self.Day = values[5]
		self.Month = values[6]
		self.Year = values[7]
		self.Position = values[8]
		self.Outs = values[9]
		self.Chances = values[10]
		self.Errors = values[11]
		self.ThrowErrors = values[12]
		self.Started = values[13]
		self.IsHome = values[14]
		self.SB = values[15]
		self.CS = values[16]
		self.PassedBall = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.gameLogId,self.gameId,self.mlbId,self.LeagueId,self.TeamId,self.Day,self.Month,self.Year,self.Position,self.Outs,self.Chances,self.Errors,self.ThrowErrors,self.Started,self.IsHome,self.SB,self.CS,self.PassedBall)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Fielder_GameLog']:
		items = cursor.execute("SELECT * FROM Player_Fielder_GameLog " + conditional, values).fetchall()
		return [DB_Player_Fielder_GameLog(i) for i in items]

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
		self.crREP = values[23]
		self.crOFF = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.ParkFactor,self.PA,self.AVG,self.OBP,self.SLG,self.ISO,self.wOBA,self.wRC,self.HRPerc,self.BBPerc,self.KPerc,self.SBRate,self.SBPerc,self.SB,self.CS,self.HR,self.crWAR,self.crREP,self.crOFF)
                        
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

class DB_Player_Fielder_MonthStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.LeagueId = values[4]
		self.TeamId = values[5]
		self.Position = values[6]
		self.Chances = values[7]
		self.Errors = values[8]
		self.ThrowErrors = values[9]
		self.Outs = values[10]
		self.r_ERR = values[11]
		self.r_PM = values[12]
		self.PosAdjust = values[13]
		self.d_RAA = values[14]
		self.scaledDRAA = values[15]
		self.r_GIDP = values[16]
		self.r_ARM = values[17]
		self.r_SB = values[18]
		self.SB = values[19]
		self.CS = values[20]
		self.r_PB = values[21]
		self.PB = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.LevelId,self.LeagueId,self.TeamId,self.Position,self.Chances,self.Errors,self.ThrowErrors,self.Outs,self.r_ERR,self.r_PM,self.PosAdjust,self.d_RAA,self.scaledDRAA,self.r_GIDP,self.r_ARM,self.r_SB,self.SB,self.CS,self.r_PB,self.PB)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Fielder_MonthStats']:
		items = cursor.execute("SELECT * FROM Player_Fielder_MonthStats " + conditional, values).fetchall()
		return [DB_Player_Fielder_MonthStats(i) for i in items]

class DB_Player_Fielder_YearStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.LevelId = values[2]
		self.LeagueId = values[3]
		self.TeamId = values[4]
		self.Position = values[5]
		self.Chances = values[6]
		self.Errors = values[7]
		self.ThrowErrors = values[8]
		self.Outs = values[9]
		self.r_ERR = values[10]
		self.r_PM = values[11]
		self.PosAdjust = values[12]
		self.d_RAA = values[13]
		self.scaledDRAA = values[14]
		self.r_GIDP = values[15]
		self.r_ARM = values[16]
		self.r_SB = values[17]
		self.SB = values[18]
		self.CS = values[19]
		self.r_PB = values[20]
		self.PB = values[21]

	NUM_ELEMENTS = 22

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.LevelId,self.LeagueId,self.TeamId,self.Position,self.Chances,self.Errors,self.ThrowErrors,self.Outs,self.r_ERR,self.r_PM,self.PosAdjust,self.d_RAA,self.scaledDRAA,self.r_GIDP,self.r_ARM,self.r_SB,self.SB,self.CS,self.r_PB,self.PB)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Fielder_YearStats']:
		items = cursor.execute("SELECT * FROM Player_Fielder_YearStats " + conditional, values).fetchall()
		return [DB_Player_Fielder_YearStats(i) for i in items]

class DB_Player_Hitter_MonthBaserunning:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.LevelId = values[3]
		self.LeagueId = values[4]
		self.TeamId = values[5]
		self.rSB = values[6]
		self.rSBNorm = values[7]
		self.rUBR = values[8]
		self.rGIDP = values[9]
		self.rBSR = values[10]
		self.TimesOnFirst = values[11]
		self.TimesOnBase = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.Month,self.LevelId,self.LeagueId,self.TeamId,self.rSB,self.rSBNorm,self.rUBR,self.rGIDP,self.rBSR,self.TimesOnFirst,self.TimesOnBase)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_MonthBaserunning']:
		items = cursor.execute("SELECT * FROM Player_Hitter_MonthBaserunning " + conditional, values).fetchall()
		return [DB_Player_Hitter_MonthBaserunning(i) for i in items]

class DB_Player_Hitter_YearBaserunning:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.Year = values[1]
		self.LevelId = values[2]
		self.LeagueId = values[3]
		self.TeamId = values[4]
		self.rSB = values[5]
		self.rSBNorm = values[6]
		self.rUBR = values[7]
		self.rGIDP = values[8]
		self.rBSR = values[9]
		self.TimesOnFirst = values[10]
		self.TimesOnBase = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.Year,self.LevelId,self.LeagueId,self.TeamId,self.rSB,self.rSBNorm,self.rUBR,self.rGIDP,self.rBSR,self.TimesOnFirst,self.TimesOnBase)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Player_Hitter_YearBaserunning']:
		items = cursor.execute("SELECT * FROM Player_Hitter_YearBaserunning " + conditional, values).fetchall()
		return [DB_Player_Hitter_YearBaserunning(i) for i in items]

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
		self.ERAMinus = values[12]
		self.FIPMinus = values[13]
		self.KPerc = values[14]
		self.BBPerc = values[15]
		self.HRPerc = values[16]
		self.HR = values[17]
		self.wOBA = values[18]
		self.crWAR = values[19]

	NUM_ELEMENTS = 20

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.month,self.teamId,self.leagueId,self.BF,self.Outs,self.SPPerc,self.GBRatio,self.ERA,self.FIP,self.ERAMinus,self.FIPMinus,self.KPerc,self.BBPerc,self.HRPerc,self.HR,self.wOBA,self.crWAR)
                        
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
		self.ERAMinus = values[11]
		self.FIPMinus = values[12]
		self.KPerc = values[13]
		self.BBPerc = values[14]
		self.HR = values[15]
		self.wOBA = values[16]

	NUM_ELEMENTS = 17

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.levelId,self.year,self.teamId,self.leagueId,self.BF,self.Outs,self.SPPerc,self.GBRatio,self.ERA,self.FIP,self.ERAMinus,self.FIPMinus,self.KPerc,self.BBPerc,self.HR,self.wOBA)
                        
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
		self.DRAA = values[10]
		self.DEF = values[11]
		self.BSR = values[12]
		self.REP = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.isHitter,self.PA,self.IP_SP,self.IP_RP,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DRAA,self.DEF,self.BSR,self.REP)
                        
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
		self.DRAA = values[10]
		self.DEF = values[11]
		self.BSR = values[12]
		self.REP = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.year,self.month,self.PA,self.IP_SP,self.IP_RP,self.WAR_h,self.WAR_s,self.WAR_r,self.OFF,self.DRAA,self.DEF,self.BSR,self.REP)
                        
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
		self.runPB = values[16]
		self.PBPerOut = values[17]
		self.RPerPA = values[18]
		self.RPerWin = values[19]
		self.LeaguePA = values[20]
		self.LeagueGames = values[21]
		self.cFIP = values[22]
		self.FIPR9Adjustment = values[23]
		self.LeagueERA = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.avgWOBA,self.avgHitterWOBA,self.wOBAScale,self.wBB,self.wHBP,self.w1B,self.w2B,self.w3B,self.wHR,self.runSB,self.runCS,self.runErr,self.runGIDP,self.probGIDP,self.runPB,self.PBPerOut,self.RPerPA,self.RPerWin,self.LeaguePA,self.LeagueGames,self.cFIP,self.FIPR9Adjustment,self.LeagueERA)
                        
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
		self.doublePlayDict = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.LeagueId,self.Year,self.runExpDict,self.fieldOutcomeDict,self.bsrAdv1st3rdSingleDict,self.bsrAdv2ndHomeSingleDict,self.bsrAdv1stHomeDoubleDict,self.bsrAvoidForce2ndDict,self.bsrAdv2nd3rdGroundoutDict,self.bsrAdv1st2ndFlyoutDict,self.bsrAdv2nd3rdFlyoutDict,self.bsrAdv3rdHomeFlyoutDict,self.doublePlayDict)
                        
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
		self.EventFlag = values[31]

	NUM_ELEMENTS = 32

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.EventId,self.GameId,self.LeagueId,self.Year,self.Month,self.HitterId,self.PitcherId,self.FielderId,self.Run1stId,self.Run2ndId,self.Run3rdId,self.StartOuts,self.Inning,self.IsTop,self.StartBaseOccupancy,self.EndOuts,self.EndBaseOccupancy,self.RunsScored,self.RunsScoredInningAfterEvent,self.Result,self.HitZone,self.HitHardness,self.HitTrajectory,self.HitCoordX,self.HitCoordY,self.LaunchSpeed,self.LaunchAngle,self.LaunchDistance,self.Run1stOutcome,self.Run2ndOutcome,self.Run3rdOutcome,self.EventFlag)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_GamePlayByPlay']:
		items = cursor.execute("SELECT * FROM GamePlayByPlay " + conditional, values).fetchall()
		return [DB_GamePlayByPlay(i) for i in items]

class DB_GamePlayByPlay_GameFielders:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.IsHome = values[1]
		self.IdP = values[2]
		self.IdC = values[3]
		self.Id1B = values[4]
		self.Id2B = values[5]
		self.Id3B = values[6]
		self.IdSS = values[7]
		self.IdLF = values[8]
		self.IdCF = values[9]
		self.IdRF = values[10]
		self.SubList = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.IsHome,self.IdP,self.IdC,self.Id1B,self.Id2B,self.Id3B,self.IdSS,self.IdLF,self.IdCF,self.IdRF,self.SubList)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_GamePlayByPlay_GameFielders']:
		items = cursor.execute("SELECT * FROM GamePlayByPlay_GameFielders " + conditional, values).fetchall()
		return [DB_GamePlayByPlay_GameFielders(i) for i in items]

class DB_College_Player:
	def __init__(self, values : tuple[any]):
		self.TBCId = values[0]
		self.MlbId = values[1]
		self.FirstName = values[2]
		self.LastName = values[3]
		self.BirthYear = values[4]
		self.BirthMonth = values[5]
		self.BirthDay = values[6]
		self.DraftOvr = values[7]
		self.FirstYear = values[8]
		self.LastYear = values[9]
		self.Bats = values[10]
		self.Throws = values[11]
		self.IsPitcher = values[12]
		self.IsHitter = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TBCId,self.MlbId,self.FirstName,self.LastName,self.BirthYear,self.BirthMonth,self.BirthDay,self.DraftOvr,self.FirstYear,self.LastYear,self.Bats,self.Throws,self.IsPitcher,self.IsHitter)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_Player']:
		items = cursor.execute("SELECT * FROM College_Player " + conditional, values).fetchall()
		return [DB_College_Player(i) for i in items]

class DB_College_TeamMap:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.Name = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.Name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_TeamMap']:
		items = cursor.execute("SELECT * FROM College_TeamMap " + conditional, values).fetchall()
		return [DB_College_TeamMap(i) for i in items]

class DB_College_ParkFactors:
	def __init__(self, values : tuple[any]):
		self.TeamId = values[0]
		self.Year = values[1]
		self.RunFactor = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TeamId,self.Year,self.RunFactor)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_ParkFactors']:
		items = cursor.execute("SELECT * FROM College_ParkFactors " + conditional, values).fetchall()
		return [DB_College_ParkFactors(i) for i in items]

class DB_College_ConfMap:
	def __init__(self, values : tuple[any]):
		self.ConfId = values[0]
		self.Name = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ConfId,self.Name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_ConfMap']:
		items = cursor.execute("SELECT * FROM College_ConfMap " + conditional, values).fetchall()
		return [DB_College_ConfMap(i) for i in items]

class DB_College_ConferenceRank:
	def __init__(self, values : tuple[any]):
		self.ConfId = values[0]
		self.Year = values[1]
		self.AvgRPI = values[2]

	NUM_ELEMENTS = 3

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ConfId,self.Year,self.AvgRPI)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_ConferenceRank']:
		items = cursor.execute("SELECT * FROM College_ConferenceRank " + conditional, values).fetchall()
		return [DB_College_ConferenceRank(i) for i in items]

class DB_College_HitterStats:
	def __init__(self, values : tuple[any]):
		self.TBCId = values[0]
		self.Year = values[1]
		self.Level = values[2]
		self.TeamId = values[3]
		self.ConfId = values[4]
		self.ExpYears = values[5]
		self.AB = values[6]
		self.PA = values[7]
		self.H = values[8]
		self.H2B = values[9]
		self.H3B = values[10]
		self.HR = values[11]
		self.SB = values[12]
		self.CS = values[13]
		self.BB = values[14]
		self.IBB = values[15]
		self.K = values[16]
		self.HBP = values[17]
		self.AVG = values[18]
		self.OBP = values[19]
		self.SLG = values[20]
		self.OPS = values[21]
		self.Age = values[22]
		self.Pos = values[23]
		self.Height = values[24]
		self.Weight = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TBCId,self.Year,self.Level,self.TeamId,self.ConfId,self.ExpYears,self.AB,self.PA,self.H,self.H2B,self.H3B,self.HR,self.SB,self.CS,self.BB,self.IBB,self.K,self.HBP,self.AVG,self.OBP,self.SLG,self.OPS,self.Age,self.Pos,self.Height,self.Weight)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_HitterStats']:
		items = cursor.execute("SELECT * FROM College_HitterStats " + conditional, values).fetchall()
		return [DB_College_HitterStats(i) for i in items]

class DB_College_ConfHitterAvg:
	def __init__(self, values : tuple[any]):
		self.ConfId = values[0]
		self.Year = values[1]
		self.H = values[2]
		self.H2B = values[3]
		self.H3B = values[4]
		self.HR = values[5]
		self.SB = values[6]
		self.CS = values[7]
		self.BB = values[8]
		self.IBB = values[9]
		self.K = values[10]
		self.HBP = values[11]
		self.AVG = values[12]
		self.OBP = values[13]
		self.SLG = values[14]
		self.OPS = values[15]

	NUM_ELEMENTS = 16

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ConfId,self.Year,self.H,self.H2B,self.H3B,self.HR,self.SB,self.CS,self.BB,self.IBB,self.K,self.HBP,self.AVG,self.OBP,self.SLG,self.OPS)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_ConfHitterAvg']:
		items = cursor.execute("SELECT * FROM College_ConfHitterAvg " + conditional, values).fetchall()
		return [DB_College_ConfHitterAvg(i) for i in items]

class DB_Model_College_HitterYear:
	def __init__(self, values : tuple[any]):
		self.TBCId = values[0]
		self.Level = values[1]
		self.Year = values[2]
		self.ExpYears = values[3]
		self.ParkRunFactor = values[4]
		self.ConfScore = values[5]
		self.PA = values[6]
		self.H = values[7]
		self.H2B = values[8]
		self.H3B = values[9]
		self.HR = values[10]
		self.SB = values[11]
		self.CS = values[12]
		self.BB = values[13]
		self.K = values[14]
		self.HBP = values[15]
		self.AVG = values[16]
		self.OBP = values[17]
		self.SLG = values[18]
		self.OPS = values[19]
		self.Age = values[20]
		self.Pos = values[21]
		self.Height = values[22]
		self.Weight = values[23]

	NUM_ELEMENTS = 24

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TBCId,self.Level,self.Year,self.ExpYears,self.ParkRunFactor,self.ConfScore,self.PA,self.H,self.H2B,self.H3B,self.HR,self.SB,self.CS,self.BB,self.K,self.HBP,self.AVG,self.OBP,self.SLG,self.OPS,self.Age,self.Pos,self.Height,self.Weight)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_College_HitterYear']:
		items = cursor.execute("SELECT * FROM Model_College_HitterYear " + conditional, values).fetchall()
		return [DB_Model_College_HitterYear(i) for i in items]

class DB_College_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.TBCId = values[0]
		self.Year = values[1]
		self.Level = values[2]
		self.TeamId = values[3]
		self.ConfId = values[4]
		self.ExpYears = values[5]
		self.G = values[6]
		self.GS = values[7]
		self.Outs = values[8]
		self.H = values[9]
		self.R = values[10]
		self.ER = values[11]
		self.HR = values[12]
		self.BB = values[13]
		self.K = values[14]
		self.HBP = values[15]
		self.ERA = values[16]
		self.H9 = values[17]
		self.HR9 = values[18]
		self.BB9 = values[19]
		self.K9 = values[20]
		self.WHIP = values[21]
		self.Age = values[22]
		self.Height = values[23]
		self.Weight = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TBCId,self.Year,self.Level,self.TeamId,self.ConfId,self.ExpYears,self.G,self.GS,self.Outs,self.H,self.R,self.ER,self.HR,self.BB,self.K,self.HBP,self.ERA,self.H9,self.HR9,self.BB9,self.K9,self.WHIP,self.Age,self.Height,self.Weight)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_PitcherStats']:
		items = cursor.execute("SELECT * FROM College_PitcherStats " + conditional, values).fetchall()
		return [DB_College_PitcherStats(i) for i in items]

class DB_College_ConfPitcherAvg:
	def __init__(self, values : tuple[any]):
		self.ConfId = values[0]
		self.Year = values[1]
		self.ERA = values[2]
		self.H9 = values[3]
		self.HR9 = values[4]
		self.BB9 = values[5]
		self.K9 = values[6]
		self.WHIP = values[7]

	NUM_ELEMENTS = 8

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ConfId,self.Year,self.ERA,self.H9,self.HR9,self.BB9,self.K9,self.WHIP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_College_ConfPitcherAvg']:
		items = cursor.execute("SELECT * FROM College_ConfPitcherAvg " + conditional, values).fetchall()
		return [DB_College_ConfPitcherAvg(i) for i in items]

class DB_Model_College_PitcherYear:
	def __init__(self, values : tuple[any]):
		self.TBCId = values[0]
		self.Level = values[1]
		self.Year = values[2]
		self.ExpYears = values[3]
		self.ParkRunFactor = values[4]
		self.ConfScore = values[5]
		self.G = values[6]
		self.GS = values[7]
		self.Outs = values[8]
		self.ERA = values[9]
		self.H9 = values[10]
		self.HR9 = values[11]
		self.BB9 = values[12]
		self.K9 = values[13]
		self.WHIP = values[14]
		self.Age = values[15]
		self.Height = values[16]
		self.Weight = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.TBCId,self.Level,self.Year,self.ExpYears,self.ParkRunFactor,self.ConfScore,self.G,self.GS,self.Outs,self.ERA,self.H9,self.HR9,self.BB9,self.K9,self.WHIP,self.Age,self.Height,self.Weight)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_College_PitcherYear']:
		items = cursor.execute("SELECT * FROM Model_College_PitcherYear " + conditional, values).fetchall()
		return [DB_Model_College_PitcherYear(i) for i in items]


##############################################################################################
