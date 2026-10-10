import sqlite3

class DB_Output_PlayerWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.isHitter = values[2]
		self.ModelRun = values[3]
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
		return (self.mlbId,self.ModelId,self.isHitter,self.ModelRun,self.year,self.month,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWar']:
		items = cursor.execute("SELECT * FROM Output_PlayerWar " + conditional, values).fetchall()
		return [DB_Output_PlayerWar(i) for i in items]

class DB_WarBucketAverages:
	def __init__(self, values : tuple[any]):
		self.isHitter = values[0]
		self.war1 = values[1]
		self.war2 = values[2]
		self.war3 = values[3]
		self.war4 = values[4]
		self.war5 = values[5]
		self.war6 = values[6]

	NUM_ELEMENTS = 7

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.isHitter,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_WarBucketAverages']:
		items = cursor.execute("SELECT * FROM WarBucketAverages " + conditional, values).fetchall()
		return [DB_WarBucketAverages(i) for i in items]

class DB_Output_PlayerHighestLevel:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.isHitter = values[2]
		self.ModelRun = values[3]
		self.year = values[4]
		self.month = values[5]
		self.DSL = values[6]
		self.CPX = values[7]
		self.A_LOW = values[8]
		self.A = values[9]
		self.A_HIGH = values[10]
		self.AA = values[11]
		self.AAA = values[12]
		self.MLB = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.isHitter,self.ModelRun,self.year,self.month,self.DSL,self.CPX,self.A_LOW,self.A,self.A_HIGH,self.AA,self.AAA,self.MLB)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerHighestLevel']:
		items = cursor.execute("SELECT * FROM Output_PlayerHighestLevel " + conditional, values).fetchall()
		return [DB_Output_PlayerHighestLevel(i) for i in items]

class DB_Output_HitterStats:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
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
		self.BSR = values[16]
		self.DRAA = values[17]
		self.ParkRunFactor = values[18]
		self.PercC = values[19]
		self.Perc1B = values[20]
		self.Perc2B = values[21]
		self.Perc3B = values[22]
		self.PercSS = values[23]
		self.PercLF = values[24]
		self.PercCF = values[25]
		self.PercRF = values[26]
		self.PercDH = values[27]

	NUM_ELEMENTS = 28

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.ModelId,self.ModelRun,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.BSR,self.DRAA,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterStats']:
		items = cursor.execute("SELECT * FROM Output_HitterStats " + conditional, values).fetchall()
		return [DB_Output_HitterStats(i) for i in items]

class DB_Output_PitcherStats:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
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
		return (self.mlbId,self.ModelId,self.ModelRun,self.Year,self.Month,self.levelId,self.Outs_SP,self.Outs_RP,self.GS,self.GR,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K,self.ParkRunFactor,self.SP_Perc,self.RP_Perc)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherStats']:
		items = cursor.execute("SELECT * FROM Output_PitcherStats " + conditional, values).fetchall()
		return [DB_Output_PitcherStats(i) for i in items]

class DB_Output_College_Hitter:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
		self.year = values[3]
		self.draft0 = values[4]
		self.draft1 = values[5]
		self.draft2 = values[6]
		self.draft3 = values[7]
		self.draft4 = values[8]
		self.draft5 = values[9]
		self.draft6 = values[10]
		self.draft = values[11]
		self.war0 = values[12]
		self.war1 = values[13]
		self.war2 = values[14]
		self.war3 = values[15]
		self.war4 = values[16]
		self.war5 = values[17]
		self.war6 = values[18]
		self.war = values[19]
		self.off0 = values[20]
		self.off1 = values[21]
		self.off2 = values[22]
		self.off3 = values[23]
		self.off4 = values[24]
		self.off5 = values[25]
		self.off6 = values[26]
		self.offNone = values[27]
		self.def0 = values[28]
		self.def1 = values[29]
		self.def2 = values[30]
		self.def3 = values[31]
		self.def4 = values[32]
		self.def5 = values[33]
		self.def6 = values[34]
		self.defNone = values[35]
		self.pa0 = values[36]
		self.pa1 = values[37]
		self.pa2 = values[38]
		self.pa3 = values[39]
		self.pa4 = values[40]
		self.pa5 = values[41]
		self.pa6 = values[42]
		self.ProbC = values[43]
		self.Prob1B = values[44]
		self.Prob2B = values[45]
		self.Prob3B = values[46]
		self.ProbSS = values[47]
		self.ProbLF = values[48]
		self.ProbCF = values[49]
		self.ProbRF = values[50]
		self.ProbDH = values[51]

	NUM_ELEMENTS = 52

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.ModelId,self.ModelRun,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war,self.off0,self.off1,self.off2,self.off3,self.off4,self.off5,self.off6,self.offNone,self.def0,self.def1,self.def2,self.def3,self.def4,self.def5,self.def6,self.defNone,self.pa0,self.pa1,self.pa2,self.pa3,self.pa4,self.pa5,self.pa6,self.ProbC,self.Prob1B,self.Prob2B,self.Prob3B,self.ProbSS,self.ProbLF,self.ProbCF,self.ProbRF,self.ProbDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_College_Hitter']:
		items = cursor.execute("SELECT * FROM Output_College_Hitter " + conditional, values).fetchall()
		return [DB_Output_College_Hitter(i) for i in items]

class DB_Output_College_Pitcher:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
		self.year = values[3]
		self.draft0 = values[4]
		self.draft1 = values[5]
		self.draft2 = values[6]
		self.draft3 = values[7]
		self.draft4 = values[8]
		self.draft5 = values[9]
		self.draft6 = values[10]
		self.draft = values[11]
		self.war0 = values[12]
		self.war1 = values[13]
		self.war2 = values[14]
		self.war3 = values[15]
		self.war4 = values[16]
		self.war5 = values[17]
		self.war6 = values[18]
		self.war = values[19]
		self.ProbSP = values[20]
		self.ProbRP = values[21]

	NUM_ELEMENTS = 22

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.ModelId,self.ModelRun,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war,self.ProbSP,self.ProbRP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_College_Pitcher']:
		items = cursor.execute("SELECT * FROM Output_College_Pitcher " + conditional, values).fetchall()
		return [DB_Output_College_Pitcher(i) for i in items]

class DB_Model_TrainingHistory:
	def __init__(self, values : tuple[any]):
		self.ModelName = values[0]
		self.IsHitter = values[1]
		self.TestLoss = values[2]
		self.TestLossCollege = values[3]
		self.ModelRun = values[4]

	NUM_ELEMENTS = 5

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelName,self.IsHitter,self.TestLoss,self.TestLossCollege,self.ModelRun)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_TrainingHistory']:
		items = cursor.execute("SELECT * FROM Model_TrainingHistory " + conditional, values).fetchall()
		return [DB_Model_TrainingHistory(i) for i in items]

class DB_PlayersInTrainingData:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.tbcId = values[1]
		self.modelId = values[2]
		self.modelRun = values[3]
		self.isHitter = values[4]
		self.isTrain = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.tbcId,self.modelId,self.modelRun,self.isHitter,self.isTrain)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PlayersInTrainingData']:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData(i) for i in items]

class DB_ModelId:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.modelName = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.modelName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelId']:
		items = cursor.execute("SELECT * FROM ModelId " + conditional, values).fetchall()
		return [DB_ModelId(i) for i in items]

class DB_Output_HitterMlbWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
		self.year = values[3]
		self.month = values[4]
		self.war_Yr0_Bkt0 = values[5]
		self.war_Yr0_Bkt1 = values[6]
		self.war_Yr0_Bkt2 = values[7]
		self.war_Yr0_Bkt3 = values[8]
		self.war_Yr0_Bkt4 = values[9]
		self.war_Yr0_Bkt5 = values[10]
		self.war_Yr0_Bkt6 = values[11]
		self.war_Yr0_Bkt7 = values[12]
		self.war0 = values[13]
		self.pa_Yr0_Bkt0 = values[14]
		self.pa_Yr0_Bkt1 = values[15]
		self.pa_Yr0_Bkt2 = values[16]
		self.pa_Yr0_Bkt3 = values[17]
		self.pa_Yr0_Bkt4 = values[18]
		self.pa_Yr0_Bkt5 = values[19]
		self.pa_Yr0_Bkt6 = values[20]
		self.pa0 = values[21]
		self.war_Yr1_Bkt0 = values[22]
		self.war_Yr1_Bkt1 = values[23]
		self.war_Yr1_Bkt2 = values[24]
		self.war_Yr1_Bkt3 = values[25]
		self.war_Yr1_Bkt4 = values[26]
		self.war_Yr1_Bkt5 = values[27]
		self.war_Yr1_Bkt6 = values[28]
		self.war_Yr1_Bkt7 = values[29]
		self.war1 = values[30]
		self.pa_Yr1_Bkt0 = values[31]
		self.pa_Yr1_Bkt1 = values[32]
		self.pa_Yr1_Bkt2 = values[33]
		self.pa_Yr1_Bkt3 = values[34]
		self.pa_Yr1_Bkt4 = values[35]
		self.pa_Yr1_Bkt5 = values[36]
		self.pa_Yr1_Bkt6 = values[37]
		self.pa1 = values[38]
		self.war_Yr2_Bkt0 = values[39]
		self.war_Yr2_Bkt1 = values[40]
		self.war_Yr2_Bkt2 = values[41]
		self.war_Yr2_Bkt3 = values[42]
		self.war_Yr2_Bkt4 = values[43]
		self.war_Yr2_Bkt5 = values[44]
		self.war_Yr2_Bkt6 = values[45]
		self.war_Yr2_Bkt7 = values[46]
		self.war2 = values[47]
		self.pa_Yr2_Bkt0 = values[48]
		self.pa_Yr2_Bkt1 = values[49]
		self.pa_Yr2_Bkt2 = values[50]
		self.pa_Yr2_Bkt3 = values[51]
		self.pa_Yr2_Bkt4 = values[52]
		self.pa_Yr2_Bkt5 = values[53]
		self.pa_Yr2_Bkt6 = values[54]
		self.pa2 = values[55]
		self.war_Yr3_Bkt0 = values[56]
		self.war_Yr3_Bkt1 = values[57]
		self.war_Yr3_Bkt2 = values[58]
		self.war_Yr3_Bkt3 = values[59]
		self.war_Yr3_Bkt4 = values[60]
		self.war_Yr3_Bkt5 = values[61]
		self.war_Yr3_Bkt6 = values[62]
		self.war_Yr3_Bkt7 = values[63]
		self.war3 = values[64]
		self.pa_Yr3_Bkt0 = values[65]
		self.pa_Yr3_Bkt1 = values[66]
		self.pa_Yr3_Bkt2 = values[67]
		self.pa_Yr3_Bkt3 = values[68]
		self.pa_Yr3_Bkt4 = values[69]
		self.pa_Yr3_Bkt5 = values[70]
		self.pa_Yr3_Bkt6 = values[71]
		self.pa3 = values[72]
		self.war_Yr4_Bkt0 = values[73]
		self.war_Yr4_Bkt1 = values[74]
		self.war_Yr4_Bkt2 = values[75]
		self.war_Yr4_Bkt3 = values[76]
		self.war_Yr4_Bkt4 = values[77]
		self.war_Yr4_Bkt5 = values[78]
		self.war_Yr4_Bkt6 = values[79]
		self.war_Yr4_Bkt7 = values[80]
		self.war4 = values[81]
		self.pa_Yr4_Bkt0 = values[82]
		self.pa_Yr4_Bkt1 = values[83]
		self.pa_Yr4_Bkt2 = values[84]
		self.pa_Yr4_Bkt3 = values[85]
		self.pa_Yr4_Bkt4 = values[86]
		self.pa_Yr4_Bkt5 = values[87]
		self.pa_Yr4_Bkt6 = values[88]
		self.pa4 = values[89]
		self.war_Yr5_Bkt0 = values[90]
		self.war_Yr5_Bkt1 = values[91]
		self.war_Yr5_Bkt2 = values[92]
		self.war_Yr5_Bkt3 = values[93]
		self.war_Yr5_Bkt4 = values[94]
		self.war_Yr5_Bkt5 = values[95]
		self.war_Yr5_Bkt6 = values[96]
		self.war_Yr5_Bkt7 = values[97]
		self.war5 = values[98]
		self.pa_Yr5_Bkt0 = values[99]
		self.pa_Yr5_Bkt1 = values[100]
		self.pa_Yr5_Bkt2 = values[101]
		self.pa_Yr5_Bkt3 = values[102]
		self.pa_Yr5_Bkt4 = values[103]
		self.pa_Yr5_Bkt5 = values[104]
		self.pa_Yr5_Bkt6 = values[105]
		self.pa5 = values[106]
		self.war_Yr6_Bkt0 = values[107]
		self.war_Yr6_Bkt1 = values[108]
		self.war_Yr6_Bkt2 = values[109]
		self.war_Yr6_Bkt3 = values[110]
		self.war_Yr6_Bkt4 = values[111]
		self.war_Yr6_Bkt5 = values[112]
		self.war_Yr6_Bkt6 = values[113]
		self.war_Yr6_Bkt7 = values[114]
		self.war6 = values[115]
		self.pa_Yr6_Bkt0 = values[116]
		self.pa_Yr6_Bkt1 = values[117]
		self.pa_Yr6_Bkt2 = values[118]
		self.pa_Yr6_Bkt3 = values[119]
		self.pa_Yr6_Bkt4 = values[120]
		self.pa_Yr6_Bkt5 = values[121]
		self.pa_Yr6_Bkt6 = values[122]
		self.pa6 = values[123]

	NUM_ELEMENTS = 124

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.ModelRun,self.year,self.month,self.war_Yr0_Bkt0,self.war_Yr0_Bkt1,self.war_Yr0_Bkt2,self.war_Yr0_Bkt3,self.war_Yr0_Bkt4,self.war_Yr0_Bkt5,self.war_Yr0_Bkt6,self.war_Yr0_Bkt7,self.war0,self.pa_Yr0_Bkt0,self.pa_Yr0_Bkt1,self.pa_Yr0_Bkt2,self.pa_Yr0_Bkt3,self.pa_Yr0_Bkt4,self.pa_Yr0_Bkt5,self.pa_Yr0_Bkt6,self.pa0,self.war_Yr1_Bkt0,self.war_Yr1_Bkt1,self.war_Yr1_Bkt2,self.war_Yr1_Bkt3,self.war_Yr1_Bkt4,self.war_Yr1_Bkt5,self.war_Yr1_Bkt6,self.war_Yr1_Bkt7,self.war1,self.pa_Yr1_Bkt0,self.pa_Yr1_Bkt1,self.pa_Yr1_Bkt2,self.pa_Yr1_Bkt3,self.pa_Yr1_Bkt4,self.pa_Yr1_Bkt5,self.pa_Yr1_Bkt6,self.pa1,self.war_Yr2_Bkt0,self.war_Yr2_Bkt1,self.war_Yr2_Bkt2,self.war_Yr2_Bkt3,self.war_Yr2_Bkt4,self.war_Yr2_Bkt5,self.war_Yr2_Bkt6,self.war_Yr2_Bkt7,self.war2,self.pa_Yr2_Bkt0,self.pa_Yr2_Bkt1,self.pa_Yr2_Bkt2,self.pa_Yr2_Bkt3,self.pa_Yr2_Bkt4,self.pa_Yr2_Bkt5,self.pa_Yr2_Bkt6,self.pa2,self.war_Yr3_Bkt0,self.war_Yr3_Bkt1,self.war_Yr3_Bkt2,self.war_Yr3_Bkt3,self.war_Yr3_Bkt4,self.war_Yr3_Bkt5,self.war_Yr3_Bkt6,self.war_Yr3_Bkt7,self.war3,self.pa_Yr3_Bkt0,self.pa_Yr3_Bkt1,self.pa_Yr3_Bkt2,self.pa_Yr3_Bkt3,self.pa_Yr3_Bkt4,self.pa_Yr3_Bkt5,self.pa_Yr3_Bkt6,self.pa3,self.war_Yr4_Bkt0,self.war_Yr4_Bkt1,self.war_Yr4_Bkt2,self.war_Yr4_Bkt3,self.war_Yr4_Bkt4,self.war_Yr4_Bkt5,self.war_Yr4_Bkt6,self.war_Yr4_Bkt7,self.war4,self.pa_Yr4_Bkt0,self.pa_Yr4_Bkt1,self.pa_Yr4_Bkt2,self.pa_Yr4_Bkt3,self.pa_Yr4_Bkt4,self.pa_Yr4_Bkt5,self.pa_Yr4_Bkt6,self.pa4,self.war_Yr5_Bkt0,self.war_Yr5_Bkt1,self.war_Yr5_Bkt2,self.war_Yr5_Bkt3,self.war_Yr5_Bkt4,self.war_Yr5_Bkt5,self.war_Yr5_Bkt6,self.war_Yr5_Bkt7,self.war5,self.pa_Yr5_Bkt0,self.pa_Yr5_Bkt1,self.pa_Yr5_Bkt2,self.pa_Yr5_Bkt3,self.pa_Yr5_Bkt4,self.pa_Yr5_Bkt5,self.pa_Yr5_Bkt6,self.pa5,self.war_Yr6_Bkt0,self.war_Yr6_Bkt1,self.war_Yr6_Bkt2,self.war_Yr6_Bkt3,self.war_Yr6_Bkt4,self.war_Yr6_Bkt5,self.war_Yr6_Bkt6,self.war_Yr6_Bkt7,self.war6,self.pa_Yr6_Bkt0,self.pa_Yr6_Bkt1,self.pa_Yr6_Bkt2,self.pa_Yr6_Bkt3,self.pa_Yr6_Bkt4,self.pa_Yr6_Bkt5,self.pa_Yr6_Bkt6,self.pa6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterMlbWar']:
		items = cursor.execute("SELECT * FROM Output_HitterMlbWar " + conditional, values).fetchall()
		return [DB_Output_HitterMlbWar(i) for i in items]

class DB_Output_PitcherMlbWar:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.ModelRun = values[2]
		self.year = values[3]
		self.month = values[4]
		self.outsSP_Yr0_Bkt0 = values[5]
		self.outsSP_Yr0_Bkt1 = values[6]
		self.outsSP_Yr0_Bkt2 = values[7]
		self.outsSP_Yr0_Bkt3 = values[8]
		self.outsSP_Yr0_Bkt4 = values[9]
		self.outsSP_Yr0_Bkt5 = values[10]
		self.outsSP_Yr0_Bkt6 = values[11]
		self.outsSP_Yr0_Bkt7 = values[12]
		self.outsSP0 = values[13]
		self.warSP_Yr0_Bkt0 = values[14]
		self.warSP_Yr0_Bkt1 = values[15]
		self.warSP_Yr0_Bkt2 = values[16]
		self.warSP_Yr0_Bkt3 = values[17]
		self.warSP_Yr0_Bkt4 = values[18]
		self.warSP_Yr0_Bkt5 = values[19]
		self.warSP_Yr0_Bkt6 = values[20]
		self.warSP_Yr0_Bkt7 = values[21]
		self.warSP0 = values[22]
		self.outsRP_Yr0_Bkt0 = values[23]
		self.outsRP_Yr0_Bkt1 = values[24]
		self.outsRP_Yr0_Bkt2 = values[25]
		self.outsRP_Yr0_Bkt3 = values[26]
		self.outsRP_Yr0_Bkt4 = values[27]
		self.outsRP_Yr0_Bkt5 = values[28]
		self.outsRP_Yr0_Bkt6 = values[29]
		self.outsRP_Yr0_Bkt7 = values[30]
		self.outsRP0 = values[31]
		self.warRP_Yr0_Bkt0 = values[32]
		self.warRP_Yr0_Bkt1 = values[33]
		self.warRP_Yr0_Bkt2 = values[34]
		self.warRP_Yr0_Bkt3 = values[35]
		self.warRP_Yr0_Bkt4 = values[36]
		self.warRP_Yr0_Bkt5 = values[37]
		self.warRP_Yr0_Bkt6 = values[38]
		self.warRP_Yr0_Bkt7 = values[39]
		self.warRP0 = values[40]
		self.outsSP_Yr1_Bkt0 = values[41]
		self.outsSP_Yr1_Bkt1 = values[42]
		self.outsSP_Yr1_Bkt2 = values[43]
		self.outsSP_Yr1_Bkt3 = values[44]
		self.outsSP_Yr1_Bkt4 = values[45]
		self.outsSP_Yr1_Bkt5 = values[46]
		self.outsSP_Yr1_Bkt6 = values[47]
		self.outsSP_Yr1_Bkt7 = values[48]
		self.outsSP1 = values[49]
		self.warSP_Yr1_Bkt0 = values[50]
		self.warSP_Yr1_Bkt1 = values[51]
		self.warSP_Yr1_Bkt2 = values[52]
		self.warSP_Yr1_Bkt3 = values[53]
		self.warSP_Yr1_Bkt4 = values[54]
		self.warSP_Yr1_Bkt5 = values[55]
		self.warSP_Yr1_Bkt6 = values[56]
		self.warSP_Yr1_Bkt7 = values[57]
		self.warSP1 = values[58]
		self.outsRP_Yr1_Bkt0 = values[59]
		self.outsRP_Yr1_Bkt1 = values[60]
		self.outsRP_Yr1_Bkt2 = values[61]
		self.outsRP_Yr1_Bkt3 = values[62]
		self.outsRP_Yr1_Bkt4 = values[63]
		self.outsRP_Yr1_Bkt5 = values[64]
		self.outsRP_Yr1_Bkt6 = values[65]
		self.outsRP_Yr1_Bkt7 = values[66]
		self.outsRP1 = values[67]
		self.warRP_Yr1_Bkt0 = values[68]
		self.warRP_Yr1_Bkt1 = values[69]
		self.warRP_Yr1_Bkt2 = values[70]
		self.warRP_Yr1_Bkt3 = values[71]
		self.warRP_Yr1_Bkt4 = values[72]
		self.warRP_Yr1_Bkt5 = values[73]
		self.warRP_Yr1_Bkt6 = values[74]
		self.warRP_Yr1_Bkt7 = values[75]
		self.warRP1 = values[76]
		self.outsSP_Yr2_Bkt0 = values[77]
		self.outsSP_Yr2_Bkt1 = values[78]
		self.outsSP_Yr2_Bkt2 = values[79]
		self.outsSP_Yr2_Bkt3 = values[80]
		self.outsSP_Yr2_Bkt4 = values[81]
		self.outsSP_Yr2_Bkt5 = values[82]
		self.outsSP_Yr2_Bkt6 = values[83]
		self.outsSP_Yr2_Bkt7 = values[84]
		self.outsSP2 = values[85]
		self.warSP_Yr2_Bkt0 = values[86]
		self.warSP_Yr2_Bkt1 = values[87]
		self.warSP_Yr2_Bkt2 = values[88]
		self.warSP_Yr2_Bkt3 = values[89]
		self.warSP_Yr2_Bkt4 = values[90]
		self.warSP_Yr2_Bkt5 = values[91]
		self.warSP_Yr2_Bkt6 = values[92]
		self.warSP_Yr2_Bkt7 = values[93]
		self.warSP2 = values[94]
		self.outsRP_Yr2_Bkt0 = values[95]
		self.outsRP_Yr2_Bkt1 = values[96]
		self.outsRP_Yr2_Bkt2 = values[97]
		self.outsRP_Yr2_Bkt3 = values[98]
		self.outsRP_Yr2_Bkt4 = values[99]
		self.outsRP_Yr2_Bkt5 = values[100]
		self.outsRP_Yr2_Bkt6 = values[101]
		self.outsRP_Yr2_Bkt7 = values[102]
		self.outsRP2 = values[103]
		self.warRP_Yr2_Bkt0 = values[104]
		self.warRP_Yr2_Bkt1 = values[105]
		self.warRP_Yr2_Bkt2 = values[106]
		self.warRP_Yr2_Bkt3 = values[107]
		self.warRP_Yr2_Bkt4 = values[108]
		self.warRP_Yr2_Bkt5 = values[109]
		self.warRP_Yr2_Bkt6 = values[110]
		self.warRP_Yr2_Bkt7 = values[111]
		self.warRP2 = values[112]
		self.outsSP_Yr3_Bkt0 = values[113]
		self.outsSP_Yr3_Bkt1 = values[114]
		self.outsSP_Yr3_Bkt2 = values[115]
		self.outsSP_Yr3_Bkt3 = values[116]
		self.outsSP_Yr3_Bkt4 = values[117]
		self.outsSP_Yr3_Bkt5 = values[118]
		self.outsSP_Yr3_Bkt6 = values[119]
		self.outsSP_Yr3_Bkt7 = values[120]
		self.outsSP3 = values[121]
		self.warSP_Yr3_Bkt0 = values[122]
		self.warSP_Yr3_Bkt1 = values[123]
		self.warSP_Yr3_Bkt2 = values[124]
		self.warSP_Yr3_Bkt3 = values[125]
		self.warSP_Yr3_Bkt4 = values[126]
		self.warSP_Yr3_Bkt5 = values[127]
		self.warSP_Yr3_Bkt6 = values[128]
		self.warSP_Yr3_Bkt7 = values[129]
		self.warSP3 = values[130]
		self.outsRP_Yr3_Bkt0 = values[131]
		self.outsRP_Yr3_Bkt1 = values[132]
		self.outsRP_Yr3_Bkt2 = values[133]
		self.outsRP_Yr3_Bkt3 = values[134]
		self.outsRP_Yr3_Bkt4 = values[135]
		self.outsRP_Yr3_Bkt5 = values[136]
		self.outsRP_Yr3_Bkt6 = values[137]
		self.outsRP_Yr3_Bkt7 = values[138]
		self.outsRP3 = values[139]
		self.warRP_Yr3_Bkt0 = values[140]
		self.warRP_Yr3_Bkt1 = values[141]
		self.warRP_Yr3_Bkt2 = values[142]
		self.warRP_Yr3_Bkt3 = values[143]
		self.warRP_Yr3_Bkt4 = values[144]
		self.warRP_Yr3_Bkt5 = values[145]
		self.warRP_Yr3_Bkt6 = values[146]
		self.warRP_Yr3_Bkt7 = values[147]
		self.warRP3 = values[148]
		self.outsSP_Yr4_Bkt0 = values[149]
		self.outsSP_Yr4_Bkt1 = values[150]
		self.outsSP_Yr4_Bkt2 = values[151]
		self.outsSP_Yr4_Bkt3 = values[152]
		self.outsSP_Yr4_Bkt4 = values[153]
		self.outsSP_Yr4_Bkt5 = values[154]
		self.outsSP_Yr4_Bkt6 = values[155]
		self.outsSP_Yr4_Bkt7 = values[156]
		self.outsSP4 = values[157]
		self.warSP_Yr4_Bkt0 = values[158]
		self.warSP_Yr4_Bkt1 = values[159]
		self.warSP_Yr4_Bkt2 = values[160]
		self.warSP_Yr4_Bkt3 = values[161]
		self.warSP_Yr4_Bkt4 = values[162]
		self.warSP_Yr4_Bkt5 = values[163]
		self.warSP_Yr4_Bkt6 = values[164]
		self.warSP_Yr4_Bkt7 = values[165]
		self.warSP4 = values[166]
		self.outsRP_Yr4_Bkt0 = values[167]
		self.outsRP_Yr4_Bkt1 = values[168]
		self.outsRP_Yr4_Bkt2 = values[169]
		self.outsRP_Yr4_Bkt3 = values[170]
		self.outsRP_Yr4_Bkt4 = values[171]
		self.outsRP_Yr4_Bkt5 = values[172]
		self.outsRP_Yr4_Bkt6 = values[173]
		self.outsRP_Yr4_Bkt7 = values[174]
		self.outsRP4 = values[175]
		self.warRP_Yr4_Bkt0 = values[176]
		self.warRP_Yr4_Bkt1 = values[177]
		self.warRP_Yr4_Bkt2 = values[178]
		self.warRP_Yr4_Bkt3 = values[179]
		self.warRP_Yr4_Bkt4 = values[180]
		self.warRP_Yr4_Bkt5 = values[181]
		self.warRP_Yr4_Bkt6 = values[182]
		self.warRP_Yr4_Bkt7 = values[183]
		self.warRP4 = values[184]
		self.outsSP_Yr5_Bkt0 = values[185]
		self.outsSP_Yr5_Bkt1 = values[186]
		self.outsSP_Yr5_Bkt2 = values[187]
		self.outsSP_Yr5_Bkt3 = values[188]
		self.outsSP_Yr5_Bkt4 = values[189]
		self.outsSP_Yr5_Bkt5 = values[190]
		self.outsSP_Yr5_Bkt6 = values[191]
		self.outsSP_Yr5_Bkt7 = values[192]
		self.outsSP5 = values[193]
		self.warSP_Yr5_Bkt0 = values[194]
		self.warSP_Yr5_Bkt1 = values[195]
		self.warSP_Yr5_Bkt2 = values[196]
		self.warSP_Yr5_Bkt3 = values[197]
		self.warSP_Yr5_Bkt4 = values[198]
		self.warSP_Yr5_Bkt5 = values[199]
		self.warSP_Yr5_Bkt6 = values[200]
		self.warSP_Yr5_Bkt7 = values[201]
		self.warSP5 = values[202]
		self.outsRP_Yr5_Bkt0 = values[203]
		self.outsRP_Yr5_Bkt1 = values[204]
		self.outsRP_Yr5_Bkt2 = values[205]
		self.outsRP_Yr5_Bkt3 = values[206]
		self.outsRP_Yr5_Bkt4 = values[207]
		self.outsRP_Yr5_Bkt5 = values[208]
		self.outsRP_Yr5_Bkt6 = values[209]
		self.outsRP_Yr5_Bkt7 = values[210]
		self.outsRP5 = values[211]
		self.warRP_Yr5_Bkt0 = values[212]
		self.warRP_Yr5_Bkt1 = values[213]
		self.warRP_Yr5_Bkt2 = values[214]
		self.warRP_Yr5_Bkt3 = values[215]
		self.warRP_Yr5_Bkt4 = values[216]
		self.warRP_Yr5_Bkt5 = values[217]
		self.warRP_Yr5_Bkt6 = values[218]
		self.warRP_Yr5_Bkt7 = values[219]
		self.warRP5 = values[220]
		self.outsSP_Yr6_Bkt0 = values[221]
		self.outsSP_Yr6_Bkt1 = values[222]
		self.outsSP_Yr6_Bkt2 = values[223]
		self.outsSP_Yr6_Bkt3 = values[224]
		self.outsSP_Yr6_Bkt4 = values[225]
		self.outsSP_Yr6_Bkt5 = values[226]
		self.outsSP_Yr6_Bkt6 = values[227]
		self.outsSP_Yr6_Bkt7 = values[228]
		self.outsSP6 = values[229]
		self.warSP_Yr6_Bkt0 = values[230]
		self.warSP_Yr6_Bkt1 = values[231]
		self.warSP_Yr6_Bkt2 = values[232]
		self.warSP_Yr6_Bkt3 = values[233]
		self.warSP_Yr6_Bkt4 = values[234]
		self.warSP_Yr6_Bkt5 = values[235]
		self.warSP_Yr6_Bkt6 = values[236]
		self.warSP_Yr6_Bkt7 = values[237]
		self.warSP6 = values[238]
		self.outsRP_Yr6_Bkt0 = values[239]
		self.outsRP_Yr6_Bkt1 = values[240]
		self.outsRP_Yr6_Bkt2 = values[241]
		self.outsRP_Yr6_Bkt3 = values[242]
		self.outsRP_Yr6_Bkt4 = values[243]
		self.outsRP_Yr6_Bkt5 = values[244]
		self.outsRP_Yr6_Bkt6 = values[245]
		self.outsRP_Yr6_Bkt7 = values[246]
		self.outsRP6 = values[247]
		self.warRP_Yr6_Bkt0 = values[248]
		self.warRP_Yr6_Bkt1 = values[249]
		self.warRP_Yr6_Bkt2 = values[250]
		self.warRP_Yr6_Bkt3 = values[251]
		self.warRP_Yr6_Bkt4 = values[252]
		self.warRP_Yr6_Bkt5 = values[253]
		self.warRP_Yr6_Bkt6 = values[254]
		self.warRP_Yr6_Bkt7 = values[255]
		self.warRP6 = values[256]

	NUM_ELEMENTS = 257

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.ModelRun,self.year,self.month,self.outsSP_Yr0_Bkt0,self.outsSP_Yr0_Bkt1,self.outsSP_Yr0_Bkt2,self.outsSP_Yr0_Bkt3,self.outsSP_Yr0_Bkt4,self.outsSP_Yr0_Bkt5,self.outsSP_Yr0_Bkt6,self.outsSP_Yr0_Bkt7,self.outsSP0,self.warSP_Yr0_Bkt0,self.warSP_Yr0_Bkt1,self.warSP_Yr0_Bkt2,self.warSP_Yr0_Bkt3,self.warSP_Yr0_Bkt4,self.warSP_Yr0_Bkt5,self.warSP_Yr0_Bkt6,self.warSP_Yr0_Bkt7,self.warSP0,self.outsRP_Yr0_Bkt0,self.outsRP_Yr0_Bkt1,self.outsRP_Yr0_Bkt2,self.outsRP_Yr0_Bkt3,self.outsRP_Yr0_Bkt4,self.outsRP_Yr0_Bkt5,self.outsRP_Yr0_Bkt6,self.outsRP_Yr0_Bkt7,self.outsRP0,self.warRP_Yr0_Bkt0,self.warRP_Yr0_Bkt1,self.warRP_Yr0_Bkt2,self.warRP_Yr0_Bkt3,self.warRP_Yr0_Bkt4,self.warRP_Yr0_Bkt5,self.warRP_Yr0_Bkt6,self.warRP_Yr0_Bkt7,self.warRP0,self.outsSP_Yr1_Bkt0,self.outsSP_Yr1_Bkt1,self.outsSP_Yr1_Bkt2,self.outsSP_Yr1_Bkt3,self.outsSP_Yr1_Bkt4,self.outsSP_Yr1_Bkt5,self.outsSP_Yr1_Bkt6,self.outsSP_Yr1_Bkt7,self.outsSP1,self.warSP_Yr1_Bkt0,self.warSP_Yr1_Bkt1,self.warSP_Yr1_Bkt2,self.warSP_Yr1_Bkt3,self.warSP_Yr1_Bkt4,self.warSP_Yr1_Bkt5,self.warSP_Yr1_Bkt6,self.warSP_Yr1_Bkt7,self.warSP1,self.outsRP_Yr1_Bkt0,self.outsRP_Yr1_Bkt1,self.outsRP_Yr1_Bkt2,self.outsRP_Yr1_Bkt3,self.outsRP_Yr1_Bkt4,self.outsRP_Yr1_Bkt5,self.outsRP_Yr1_Bkt6,self.outsRP_Yr1_Bkt7,self.outsRP1,self.warRP_Yr1_Bkt0,self.warRP_Yr1_Bkt1,self.warRP_Yr1_Bkt2,self.warRP_Yr1_Bkt3,self.warRP_Yr1_Bkt4,self.warRP_Yr1_Bkt5,self.warRP_Yr1_Bkt6,self.warRP_Yr1_Bkt7,self.warRP1,self.outsSP_Yr2_Bkt0,self.outsSP_Yr2_Bkt1,self.outsSP_Yr2_Bkt2,self.outsSP_Yr2_Bkt3,self.outsSP_Yr2_Bkt4,self.outsSP_Yr2_Bkt5,self.outsSP_Yr2_Bkt6,self.outsSP_Yr2_Bkt7,self.outsSP2,self.warSP_Yr2_Bkt0,self.warSP_Yr2_Bkt1,self.warSP_Yr2_Bkt2,self.warSP_Yr2_Bkt3,self.warSP_Yr2_Bkt4,self.warSP_Yr2_Bkt5,self.warSP_Yr2_Bkt6,self.warSP_Yr2_Bkt7,self.warSP2,self.outsRP_Yr2_Bkt0,self.outsRP_Yr2_Bkt1,self.outsRP_Yr2_Bkt2,self.outsRP_Yr2_Bkt3,self.outsRP_Yr2_Bkt4,self.outsRP_Yr2_Bkt5,self.outsRP_Yr2_Bkt6,self.outsRP_Yr2_Bkt7,self.outsRP2,self.warRP_Yr2_Bkt0,self.warRP_Yr2_Bkt1,self.warRP_Yr2_Bkt2,self.warRP_Yr2_Bkt3,self.warRP_Yr2_Bkt4,self.warRP_Yr2_Bkt5,self.warRP_Yr2_Bkt6,self.warRP_Yr2_Bkt7,self.warRP2,self.outsSP_Yr3_Bkt0,self.outsSP_Yr3_Bkt1,self.outsSP_Yr3_Bkt2,self.outsSP_Yr3_Bkt3,self.outsSP_Yr3_Bkt4,self.outsSP_Yr3_Bkt5,self.outsSP_Yr3_Bkt6,self.outsSP_Yr3_Bkt7,self.outsSP3,self.warSP_Yr3_Bkt0,self.warSP_Yr3_Bkt1,self.warSP_Yr3_Bkt2,self.warSP_Yr3_Bkt3,self.warSP_Yr3_Bkt4,self.warSP_Yr3_Bkt5,self.warSP_Yr3_Bkt6,self.warSP_Yr3_Bkt7,self.warSP3,self.outsRP_Yr3_Bkt0,self.outsRP_Yr3_Bkt1,self.outsRP_Yr3_Bkt2,self.outsRP_Yr3_Bkt3,self.outsRP_Yr3_Bkt4,self.outsRP_Yr3_Bkt5,self.outsRP_Yr3_Bkt6,self.outsRP_Yr3_Bkt7,self.outsRP3,self.warRP_Yr3_Bkt0,self.warRP_Yr3_Bkt1,self.warRP_Yr3_Bkt2,self.warRP_Yr3_Bkt3,self.warRP_Yr3_Bkt4,self.warRP_Yr3_Bkt5,self.warRP_Yr3_Bkt6,self.warRP_Yr3_Bkt7,self.warRP3,self.outsSP_Yr4_Bkt0,self.outsSP_Yr4_Bkt1,self.outsSP_Yr4_Bkt2,self.outsSP_Yr4_Bkt3,self.outsSP_Yr4_Bkt4,self.outsSP_Yr4_Bkt5,self.outsSP_Yr4_Bkt6,self.outsSP_Yr4_Bkt7,self.outsSP4,self.warSP_Yr4_Bkt0,self.warSP_Yr4_Bkt1,self.warSP_Yr4_Bkt2,self.warSP_Yr4_Bkt3,self.warSP_Yr4_Bkt4,self.warSP_Yr4_Bkt5,self.warSP_Yr4_Bkt6,self.warSP_Yr4_Bkt7,self.warSP4,self.outsRP_Yr4_Bkt0,self.outsRP_Yr4_Bkt1,self.outsRP_Yr4_Bkt2,self.outsRP_Yr4_Bkt3,self.outsRP_Yr4_Bkt4,self.outsRP_Yr4_Bkt5,self.outsRP_Yr4_Bkt6,self.outsRP_Yr4_Bkt7,self.outsRP4,self.warRP_Yr4_Bkt0,self.warRP_Yr4_Bkt1,self.warRP_Yr4_Bkt2,self.warRP_Yr4_Bkt3,self.warRP_Yr4_Bkt4,self.warRP_Yr4_Bkt5,self.warRP_Yr4_Bkt6,self.warRP_Yr4_Bkt7,self.warRP4,self.outsSP_Yr5_Bkt0,self.outsSP_Yr5_Bkt1,self.outsSP_Yr5_Bkt2,self.outsSP_Yr5_Bkt3,self.outsSP_Yr5_Bkt4,self.outsSP_Yr5_Bkt5,self.outsSP_Yr5_Bkt6,self.outsSP_Yr5_Bkt7,self.outsSP5,self.warSP_Yr5_Bkt0,self.warSP_Yr5_Bkt1,self.warSP_Yr5_Bkt2,self.warSP_Yr5_Bkt3,self.warSP_Yr5_Bkt4,self.warSP_Yr5_Bkt5,self.warSP_Yr5_Bkt6,self.warSP_Yr5_Bkt7,self.warSP5,self.outsRP_Yr5_Bkt0,self.outsRP_Yr5_Bkt1,self.outsRP_Yr5_Bkt2,self.outsRP_Yr5_Bkt3,self.outsRP_Yr5_Bkt4,self.outsRP_Yr5_Bkt5,self.outsRP_Yr5_Bkt6,self.outsRP_Yr5_Bkt7,self.outsRP5,self.warRP_Yr5_Bkt0,self.warRP_Yr5_Bkt1,self.warRP_Yr5_Bkt2,self.warRP_Yr5_Bkt3,self.warRP_Yr5_Bkt4,self.warRP_Yr5_Bkt5,self.warRP_Yr5_Bkt6,self.warRP_Yr5_Bkt7,self.warRP5,self.outsSP_Yr6_Bkt0,self.outsSP_Yr6_Bkt1,self.outsSP_Yr6_Bkt2,self.outsSP_Yr6_Bkt3,self.outsSP_Yr6_Bkt4,self.outsSP_Yr6_Bkt5,self.outsSP_Yr6_Bkt6,self.outsSP_Yr6_Bkt7,self.outsSP6,self.warSP_Yr6_Bkt0,self.warSP_Yr6_Bkt1,self.warSP_Yr6_Bkt2,self.warSP_Yr6_Bkt3,self.warSP_Yr6_Bkt4,self.warSP_Yr6_Bkt5,self.warSP_Yr6_Bkt6,self.warSP_Yr6_Bkt7,self.warSP6,self.outsRP_Yr6_Bkt0,self.outsRP_Yr6_Bkt1,self.outsRP_Yr6_Bkt2,self.outsRP_Yr6_Bkt3,self.outsRP_Yr6_Bkt4,self.outsRP_Yr6_Bkt5,self.outsRP_Yr6_Bkt6,self.outsRP_Yr6_Bkt7,self.outsRP6,self.warRP_Yr6_Bkt0,self.warRP_Yr6_Bkt1,self.warRP_Yr6_Bkt2,self.warRP_Yr6_Bkt3,self.warRP_Yr6_Bkt4,self.warRP_Yr6_Bkt5,self.warRP_Yr6_Bkt6,self.warRP_Yr6_Bkt7,self.warRP6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherMlbWar']:
		items = cursor.execute("SELECT * FROM Output_PitcherMlbWar " + conditional, values).fetchall()
		return [DB_Output_PitcherMlbWar(i) for i in items]

class DB_Output_HitterStatsAggregation:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.ModelId = values[1]
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
		self.BSR = values[15]
		self.DRAA = values[16]
		self.ParkRunFactor = values[17]
		self.PercC = values[18]
		self.Perc1B = values[19]
		self.Perc2B = values[20]
		self.Perc3B = values[21]
		self.PercSS = values[22]
		self.PercLF = values[23]
		self.PercCF = values[24]
		self.PercRF = values[25]
		self.PercDH = values[26]

	NUM_ELEMENTS = 27

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.ModelId,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.BSR,self.DRAA,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterStatsAggregation']:
		items = cursor.execute("SELECT * FROM Output_HitterStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterStatsAggregation(i) for i in items]

class DB_Output_PitcherStatsAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
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
		return (self.mlbId,self.ModelId,self.Year,self.Month,self.levelId,self.Outs_SP,self.Outs_RP,self.GS,self.GR,self.ERA,self.FIP,self.HR,self.BB,self.HBP,self.K,self.ParkRunFactor,self.SP_Perc,self.RP_Perc)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherStatsAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitcherStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherStatsAggregation(i) for i in items]

class DB_Output_PlayerWarAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
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
		return (self.mlbId,self.ModelId,self.isHitter,self.year,self.month,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerWarAggregation']:
		items = cursor.execute("SELECT * FROM Output_PlayerWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerWarAggregation(i) for i in items]

class DB_Output_PlayerHighestLevelAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.isHitter = values[2]
		self.year = values[3]
		self.month = values[4]
		self.DSL = values[5]
		self.CPX = values[6]
		self.A_LOW = values[7]
		self.A = values[8]
		self.A_HIGH = values[9]
		self.AA = values[10]
		self.AAA = values[11]
		self.MLB = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.isHitter,self.year,self.month,self.DSL,self.CPX,self.A_LOW,self.A,self.A_HIGH,self.AA,self.AAA,self.MLB)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PlayerHighestLevelAggregation']:
		items = cursor.execute("SELECT * FROM Output_PlayerHighestLevelAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerHighestLevelAggregation(i) for i in items]

class DB_Output_College_HitterAggregation:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.ModelId = values[1]
		self.year = values[2]
		self.draft0 = values[3]
		self.draft1 = values[4]
		self.draft2 = values[5]
		self.draft3 = values[6]
		self.draft4 = values[7]
		self.draft5 = values[8]
		self.draft6 = values[9]
		self.draft = values[10]
		self.war0 = values[11]
		self.war1 = values[12]
		self.war2 = values[13]
		self.war3 = values[14]
		self.war4 = values[15]
		self.war5 = values[16]
		self.war6 = values[17]
		self.war = values[18]
		self.off0 = values[19]
		self.off1 = values[20]
		self.off2 = values[21]
		self.off3 = values[22]
		self.off4 = values[23]
		self.off5 = values[24]
		self.off6 = values[25]
		self.offNone = values[26]
		self.def0 = values[27]
		self.def1 = values[28]
		self.def2 = values[29]
		self.def3 = values[30]
		self.def4 = values[31]
		self.def5 = values[32]
		self.def6 = values[33]
		self.defNone = values[34]
		self.pa0 = values[35]
		self.pa1 = values[36]
		self.pa2 = values[37]
		self.pa3 = values[38]
		self.pa4 = values[39]
		self.pa5 = values[40]
		self.pa6 = values[41]
		self.ProbC = values[42]
		self.Prob1B = values[43]
		self.Prob2B = values[44]
		self.Prob3B = values[45]
		self.ProbSS = values[46]
		self.ProbLF = values[47]
		self.ProbCF = values[48]
		self.ProbRF = values[49]
		self.ProbDH = values[50]

	NUM_ELEMENTS = 51

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.ModelId,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war,self.off0,self.off1,self.off2,self.off3,self.off4,self.off5,self.off6,self.offNone,self.def0,self.def1,self.def2,self.def3,self.def4,self.def5,self.def6,self.defNone,self.pa0,self.pa1,self.pa2,self.pa3,self.pa4,self.pa5,self.pa6,self.ProbC,self.Prob1B,self.Prob2B,self.Prob3B,self.ProbSS,self.ProbLF,self.ProbCF,self.ProbRF,self.ProbDH)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_College_HitterAggregation']:
		items = cursor.execute("SELECT * FROM Output_College_HitterAggregation " + conditional, values).fetchall()
		return [DB_Output_College_HitterAggregation(i) for i in items]

class DB_Output_College_PitcherAggregation:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.ModelId = values[1]
		self.year = values[2]
		self.draft0 = values[3]
		self.draft1 = values[4]
		self.draft2 = values[5]
		self.draft3 = values[6]
		self.draft4 = values[7]
		self.draft5 = values[8]
		self.draft6 = values[9]
		self.draft = values[10]
		self.war0 = values[11]
		self.war1 = values[12]
		self.war2 = values[13]
		self.war3 = values[14]
		self.war4 = values[15]
		self.war5 = values[16]
		self.war6 = values[17]
		self.war = values[18]
		self.ProbSP = values[19]
		self.ProbRP = values[20]

	NUM_ELEMENTS = 21

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.ModelId,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war,self.ProbSP,self.ProbRP)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_College_PitcherAggregation']:
		items = cursor.execute("SELECT * FROM Output_College_PitcherAggregation " + conditional, values).fetchall()
		return [DB_Output_College_PitcherAggregation(i) for i in items]

class DB_Output_HitterMlbWarAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.war_Yr0_Bkt0 = values[4]
		self.war_Yr0_Bkt1 = values[5]
		self.war_Yr0_Bkt2 = values[6]
		self.war_Yr0_Bkt3 = values[7]
		self.war_Yr0_Bkt4 = values[8]
		self.war_Yr0_Bkt5 = values[9]
		self.war_Yr0_Bkt6 = values[10]
		self.war_Yr0_Bkt7 = values[11]
		self.war0 = values[12]
		self.pa_Yr0_Bkt0 = values[13]
		self.pa_Yr0_Bkt1 = values[14]
		self.pa_Yr0_Bkt2 = values[15]
		self.pa_Yr0_Bkt3 = values[16]
		self.pa_Yr0_Bkt4 = values[17]
		self.pa_Yr0_Bkt5 = values[18]
		self.pa_Yr0_Bkt6 = values[19]
		self.pa0 = values[20]
		self.war_Yr1_Bkt0 = values[21]
		self.war_Yr1_Bkt1 = values[22]
		self.war_Yr1_Bkt2 = values[23]
		self.war_Yr1_Bkt3 = values[24]
		self.war_Yr1_Bkt4 = values[25]
		self.war_Yr1_Bkt5 = values[26]
		self.war_Yr1_Bkt6 = values[27]
		self.war_Yr1_Bkt7 = values[28]
		self.war1 = values[29]
		self.pa_Yr1_Bkt0 = values[30]
		self.pa_Yr1_Bkt1 = values[31]
		self.pa_Yr1_Bkt2 = values[32]
		self.pa_Yr1_Bkt3 = values[33]
		self.pa_Yr1_Bkt4 = values[34]
		self.pa_Yr1_Bkt5 = values[35]
		self.pa_Yr1_Bkt6 = values[36]
		self.pa1 = values[37]
		self.war_Yr2_Bkt0 = values[38]
		self.war_Yr2_Bkt1 = values[39]
		self.war_Yr2_Bkt2 = values[40]
		self.war_Yr2_Bkt3 = values[41]
		self.war_Yr2_Bkt4 = values[42]
		self.war_Yr2_Bkt5 = values[43]
		self.war_Yr2_Bkt6 = values[44]
		self.war_Yr2_Bkt7 = values[45]
		self.war2 = values[46]
		self.pa_Yr2_Bkt0 = values[47]
		self.pa_Yr2_Bkt1 = values[48]
		self.pa_Yr2_Bkt2 = values[49]
		self.pa_Yr2_Bkt3 = values[50]
		self.pa_Yr2_Bkt4 = values[51]
		self.pa_Yr2_Bkt5 = values[52]
		self.pa_Yr2_Bkt6 = values[53]
		self.pa2 = values[54]
		self.war_Yr3_Bkt0 = values[55]
		self.war_Yr3_Bkt1 = values[56]
		self.war_Yr3_Bkt2 = values[57]
		self.war_Yr3_Bkt3 = values[58]
		self.war_Yr3_Bkt4 = values[59]
		self.war_Yr3_Bkt5 = values[60]
		self.war_Yr3_Bkt6 = values[61]
		self.war_Yr3_Bkt7 = values[62]
		self.war3 = values[63]
		self.pa_Yr3_Bkt0 = values[64]
		self.pa_Yr3_Bkt1 = values[65]
		self.pa_Yr3_Bkt2 = values[66]
		self.pa_Yr3_Bkt3 = values[67]
		self.pa_Yr3_Bkt4 = values[68]
		self.pa_Yr3_Bkt5 = values[69]
		self.pa_Yr3_Bkt6 = values[70]
		self.pa3 = values[71]
		self.war_Yr4_Bkt0 = values[72]
		self.war_Yr4_Bkt1 = values[73]
		self.war_Yr4_Bkt2 = values[74]
		self.war_Yr4_Bkt3 = values[75]
		self.war_Yr4_Bkt4 = values[76]
		self.war_Yr4_Bkt5 = values[77]
		self.war_Yr4_Bkt6 = values[78]
		self.war_Yr4_Bkt7 = values[79]
		self.war4 = values[80]
		self.pa_Yr4_Bkt0 = values[81]
		self.pa_Yr4_Bkt1 = values[82]
		self.pa_Yr4_Bkt2 = values[83]
		self.pa_Yr4_Bkt3 = values[84]
		self.pa_Yr4_Bkt4 = values[85]
		self.pa_Yr4_Bkt5 = values[86]
		self.pa_Yr4_Bkt6 = values[87]
		self.pa4 = values[88]
		self.war_Yr5_Bkt0 = values[89]
		self.war_Yr5_Bkt1 = values[90]
		self.war_Yr5_Bkt2 = values[91]
		self.war_Yr5_Bkt3 = values[92]
		self.war_Yr5_Bkt4 = values[93]
		self.war_Yr5_Bkt5 = values[94]
		self.war_Yr5_Bkt6 = values[95]
		self.war_Yr5_Bkt7 = values[96]
		self.war5 = values[97]
		self.pa_Yr5_Bkt0 = values[98]
		self.pa_Yr5_Bkt1 = values[99]
		self.pa_Yr5_Bkt2 = values[100]
		self.pa_Yr5_Bkt3 = values[101]
		self.pa_Yr5_Bkt4 = values[102]
		self.pa_Yr5_Bkt5 = values[103]
		self.pa_Yr5_Bkt6 = values[104]
		self.pa5 = values[105]
		self.war_Yr6_Bkt0 = values[106]
		self.war_Yr6_Bkt1 = values[107]
		self.war_Yr6_Bkt2 = values[108]
		self.war_Yr6_Bkt3 = values[109]
		self.war_Yr6_Bkt4 = values[110]
		self.war_Yr6_Bkt5 = values[111]
		self.war_Yr6_Bkt6 = values[112]
		self.war_Yr6_Bkt7 = values[113]
		self.war6 = values[114]
		self.pa_Yr6_Bkt0 = values[115]
		self.pa_Yr6_Bkt1 = values[116]
		self.pa_Yr6_Bkt2 = values[117]
		self.pa_Yr6_Bkt3 = values[118]
		self.pa_Yr6_Bkt4 = values[119]
		self.pa_Yr6_Bkt5 = values[120]
		self.pa_Yr6_Bkt6 = values[121]
		self.pa6 = values[122]

	NUM_ELEMENTS = 123

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.year,self.month,self.war_Yr0_Bkt0,self.war_Yr0_Bkt1,self.war_Yr0_Bkt2,self.war_Yr0_Bkt3,self.war_Yr0_Bkt4,self.war_Yr0_Bkt5,self.war_Yr0_Bkt6,self.war_Yr0_Bkt7,self.war0,self.pa_Yr0_Bkt0,self.pa_Yr0_Bkt1,self.pa_Yr0_Bkt2,self.pa_Yr0_Bkt3,self.pa_Yr0_Bkt4,self.pa_Yr0_Bkt5,self.pa_Yr0_Bkt6,self.pa0,self.war_Yr1_Bkt0,self.war_Yr1_Bkt1,self.war_Yr1_Bkt2,self.war_Yr1_Bkt3,self.war_Yr1_Bkt4,self.war_Yr1_Bkt5,self.war_Yr1_Bkt6,self.war_Yr1_Bkt7,self.war1,self.pa_Yr1_Bkt0,self.pa_Yr1_Bkt1,self.pa_Yr1_Bkt2,self.pa_Yr1_Bkt3,self.pa_Yr1_Bkt4,self.pa_Yr1_Bkt5,self.pa_Yr1_Bkt6,self.pa1,self.war_Yr2_Bkt0,self.war_Yr2_Bkt1,self.war_Yr2_Bkt2,self.war_Yr2_Bkt3,self.war_Yr2_Bkt4,self.war_Yr2_Bkt5,self.war_Yr2_Bkt6,self.war_Yr2_Bkt7,self.war2,self.pa_Yr2_Bkt0,self.pa_Yr2_Bkt1,self.pa_Yr2_Bkt2,self.pa_Yr2_Bkt3,self.pa_Yr2_Bkt4,self.pa_Yr2_Bkt5,self.pa_Yr2_Bkt6,self.pa2,self.war_Yr3_Bkt0,self.war_Yr3_Bkt1,self.war_Yr3_Bkt2,self.war_Yr3_Bkt3,self.war_Yr3_Bkt4,self.war_Yr3_Bkt5,self.war_Yr3_Bkt6,self.war_Yr3_Bkt7,self.war3,self.pa_Yr3_Bkt0,self.pa_Yr3_Bkt1,self.pa_Yr3_Bkt2,self.pa_Yr3_Bkt3,self.pa_Yr3_Bkt4,self.pa_Yr3_Bkt5,self.pa_Yr3_Bkt6,self.pa3,self.war_Yr4_Bkt0,self.war_Yr4_Bkt1,self.war_Yr4_Bkt2,self.war_Yr4_Bkt3,self.war_Yr4_Bkt4,self.war_Yr4_Bkt5,self.war_Yr4_Bkt6,self.war_Yr4_Bkt7,self.war4,self.pa_Yr4_Bkt0,self.pa_Yr4_Bkt1,self.pa_Yr4_Bkt2,self.pa_Yr4_Bkt3,self.pa_Yr4_Bkt4,self.pa_Yr4_Bkt5,self.pa_Yr4_Bkt6,self.pa4,self.war_Yr5_Bkt0,self.war_Yr5_Bkt1,self.war_Yr5_Bkt2,self.war_Yr5_Bkt3,self.war_Yr5_Bkt4,self.war_Yr5_Bkt5,self.war_Yr5_Bkt6,self.war_Yr5_Bkt7,self.war5,self.pa_Yr5_Bkt0,self.pa_Yr5_Bkt1,self.pa_Yr5_Bkt2,self.pa_Yr5_Bkt3,self.pa_Yr5_Bkt4,self.pa_Yr5_Bkt5,self.pa_Yr5_Bkt6,self.pa5,self.war_Yr6_Bkt0,self.war_Yr6_Bkt1,self.war_Yr6_Bkt2,self.war_Yr6_Bkt3,self.war_Yr6_Bkt4,self.war_Yr6_Bkt5,self.war_Yr6_Bkt6,self.war_Yr6_Bkt7,self.war6,self.pa_Yr6_Bkt0,self.pa_Yr6_Bkt1,self.pa_Yr6_Bkt2,self.pa_Yr6_Bkt3,self.pa_Yr6_Bkt4,self.pa_Yr6_Bkt5,self.pa_Yr6_Bkt6,self.pa6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_HitterMlbWarAggregation']:
		items = cursor.execute("SELECT * FROM Output_HitterMlbWarAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterMlbWarAggregation(i) for i in items]

class DB_Output_PitcherMlbWarAggregation:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.ModelId = values[1]
		self.year = values[2]
		self.month = values[3]
		self.outsSP_Yr0_Bkt0 = values[4]
		self.outsSP_Yr0_Bkt1 = values[5]
		self.outsSP_Yr0_Bkt2 = values[6]
		self.outsSP_Yr0_Bkt3 = values[7]
		self.outsSP_Yr0_Bkt4 = values[8]
		self.outsSP_Yr0_Bkt5 = values[9]
		self.outsSP_Yr0_Bkt6 = values[10]
		self.outsSP_Yr0_Bkt7 = values[11]
		self.outsSP0 = values[12]
		self.warSP_Yr0_Bkt0 = values[13]
		self.warSP_Yr0_Bkt1 = values[14]
		self.warSP_Yr0_Bkt2 = values[15]
		self.warSP_Yr0_Bkt3 = values[16]
		self.warSP_Yr0_Bkt4 = values[17]
		self.warSP_Yr0_Bkt5 = values[18]
		self.warSP_Yr0_Bkt6 = values[19]
		self.warSP_Yr0_Bkt7 = values[20]
		self.warSP0 = values[21]
		self.outsRP_Yr0_Bkt0 = values[22]
		self.outsRP_Yr0_Bkt1 = values[23]
		self.outsRP_Yr0_Bkt2 = values[24]
		self.outsRP_Yr0_Bkt3 = values[25]
		self.outsRP_Yr0_Bkt4 = values[26]
		self.outsRP_Yr0_Bkt5 = values[27]
		self.outsRP_Yr0_Bkt6 = values[28]
		self.outsRP_Yr0_Bkt7 = values[29]
		self.outsRP0 = values[30]
		self.warRP_Yr0_Bkt0 = values[31]
		self.warRP_Yr0_Bkt1 = values[32]
		self.warRP_Yr0_Bkt2 = values[33]
		self.warRP_Yr0_Bkt3 = values[34]
		self.warRP_Yr0_Bkt4 = values[35]
		self.warRP_Yr0_Bkt5 = values[36]
		self.warRP_Yr0_Bkt6 = values[37]
		self.warRP_Yr0_Bkt7 = values[38]
		self.warRP0 = values[39]
		self.outsSP_Yr1_Bkt0 = values[40]
		self.outsSP_Yr1_Bkt1 = values[41]
		self.outsSP_Yr1_Bkt2 = values[42]
		self.outsSP_Yr1_Bkt3 = values[43]
		self.outsSP_Yr1_Bkt4 = values[44]
		self.outsSP_Yr1_Bkt5 = values[45]
		self.outsSP_Yr1_Bkt6 = values[46]
		self.outsSP_Yr1_Bkt7 = values[47]
		self.outsSP1 = values[48]
		self.warSP_Yr1_Bkt0 = values[49]
		self.warSP_Yr1_Bkt1 = values[50]
		self.warSP_Yr1_Bkt2 = values[51]
		self.warSP_Yr1_Bkt3 = values[52]
		self.warSP_Yr1_Bkt4 = values[53]
		self.warSP_Yr1_Bkt5 = values[54]
		self.warSP_Yr1_Bkt6 = values[55]
		self.warSP_Yr1_Bkt7 = values[56]
		self.warSP1 = values[57]
		self.outsRP_Yr1_Bkt0 = values[58]
		self.outsRP_Yr1_Bkt1 = values[59]
		self.outsRP_Yr1_Bkt2 = values[60]
		self.outsRP_Yr1_Bkt3 = values[61]
		self.outsRP_Yr1_Bkt4 = values[62]
		self.outsRP_Yr1_Bkt5 = values[63]
		self.outsRP_Yr1_Bkt6 = values[64]
		self.outsRP_Yr1_Bkt7 = values[65]
		self.outsRP1 = values[66]
		self.warRP_Yr1_Bkt0 = values[67]
		self.warRP_Yr1_Bkt1 = values[68]
		self.warRP_Yr1_Bkt2 = values[69]
		self.warRP_Yr1_Bkt3 = values[70]
		self.warRP_Yr1_Bkt4 = values[71]
		self.warRP_Yr1_Bkt5 = values[72]
		self.warRP_Yr1_Bkt6 = values[73]
		self.warRP_Yr1_Bkt7 = values[74]
		self.warRP1 = values[75]
		self.outsSP_Yr2_Bkt0 = values[76]
		self.outsSP_Yr2_Bkt1 = values[77]
		self.outsSP_Yr2_Bkt2 = values[78]
		self.outsSP_Yr2_Bkt3 = values[79]
		self.outsSP_Yr2_Bkt4 = values[80]
		self.outsSP_Yr2_Bkt5 = values[81]
		self.outsSP_Yr2_Bkt6 = values[82]
		self.outsSP_Yr2_Bkt7 = values[83]
		self.outsSP2 = values[84]
		self.warSP_Yr2_Bkt0 = values[85]
		self.warSP_Yr2_Bkt1 = values[86]
		self.warSP_Yr2_Bkt2 = values[87]
		self.warSP_Yr2_Bkt3 = values[88]
		self.warSP_Yr2_Bkt4 = values[89]
		self.warSP_Yr2_Bkt5 = values[90]
		self.warSP_Yr2_Bkt6 = values[91]
		self.warSP_Yr2_Bkt7 = values[92]
		self.warSP2 = values[93]
		self.outsRP_Yr2_Bkt0 = values[94]
		self.outsRP_Yr2_Bkt1 = values[95]
		self.outsRP_Yr2_Bkt2 = values[96]
		self.outsRP_Yr2_Bkt3 = values[97]
		self.outsRP_Yr2_Bkt4 = values[98]
		self.outsRP_Yr2_Bkt5 = values[99]
		self.outsRP_Yr2_Bkt6 = values[100]
		self.outsRP_Yr2_Bkt7 = values[101]
		self.outsRP2 = values[102]
		self.warRP_Yr2_Bkt0 = values[103]
		self.warRP_Yr2_Bkt1 = values[104]
		self.warRP_Yr2_Bkt2 = values[105]
		self.warRP_Yr2_Bkt3 = values[106]
		self.warRP_Yr2_Bkt4 = values[107]
		self.warRP_Yr2_Bkt5 = values[108]
		self.warRP_Yr2_Bkt6 = values[109]
		self.warRP_Yr2_Bkt7 = values[110]
		self.warRP2 = values[111]
		self.outsSP_Yr3_Bkt0 = values[112]
		self.outsSP_Yr3_Bkt1 = values[113]
		self.outsSP_Yr3_Bkt2 = values[114]
		self.outsSP_Yr3_Bkt3 = values[115]
		self.outsSP_Yr3_Bkt4 = values[116]
		self.outsSP_Yr3_Bkt5 = values[117]
		self.outsSP_Yr3_Bkt6 = values[118]
		self.outsSP_Yr3_Bkt7 = values[119]
		self.outsSP3 = values[120]
		self.warSP_Yr3_Bkt0 = values[121]
		self.warSP_Yr3_Bkt1 = values[122]
		self.warSP_Yr3_Bkt2 = values[123]
		self.warSP_Yr3_Bkt3 = values[124]
		self.warSP_Yr3_Bkt4 = values[125]
		self.warSP_Yr3_Bkt5 = values[126]
		self.warSP_Yr3_Bkt6 = values[127]
		self.warSP_Yr3_Bkt7 = values[128]
		self.warSP3 = values[129]
		self.outsRP_Yr3_Bkt0 = values[130]
		self.outsRP_Yr3_Bkt1 = values[131]
		self.outsRP_Yr3_Bkt2 = values[132]
		self.outsRP_Yr3_Bkt3 = values[133]
		self.outsRP_Yr3_Bkt4 = values[134]
		self.outsRP_Yr3_Bkt5 = values[135]
		self.outsRP_Yr3_Bkt6 = values[136]
		self.outsRP_Yr3_Bkt7 = values[137]
		self.outsRP3 = values[138]
		self.warRP_Yr3_Bkt0 = values[139]
		self.warRP_Yr3_Bkt1 = values[140]
		self.warRP_Yr3_Bkt2 = values[141]
		self.warRP_Yr3_Bkt3 = values[142]
		self.warRP_Yr3_Bkt4 = values[143]
		self.warRP_Yr3_Bkt5 = values[144]
		self.warRP_Yr3_Bkt6 = values[145]
		self.warRP_Yr3_Bkt7 = values[146]
		self.warRP3 = values[147]
		self.outsSP_Yr4_Bkt0 = values[148]
		self.outsSP_Yr4_Bkt1 = values[149]
		self.outsSP_Yr4_Bkt2 = values[150]
		self.outsSP_Yr4_Bkt3 = values[151]
		self.outsSP_Yr4_Bkt4 = values[152]
		self.outsSP_Yr4_Bkt5 = values[153]
		self.outsSP_Yr4_Bkt6 = values[154]
		self.outsSP_Yr4_Bkt7 = values[155]
		self.outsSP4 = values[156]
		self.warSP_Yr4_Bkt0 = values[157]
		self.warSP_Yr4_Bkt1 = values[158]
		self.warSP_Yr4_Bkt2 = values[159]
		self.warSP_Yr4_Bkt3 = values[160]
		self.warSP_Yr4_Bkt4 = values[161]
		self.warSP_Yr4_Bkt5 = values[162]
		self.warSP_Yr4_Bkt6 = values[163]
		self.warSP_Yr4_Bkt7 = values[164]
		self.warSP4 = values[165]
		self.outsRP_Yr4_Bkt0 = values[166]
		self.outsRP_Yr4_Bkt1 = values[167]
		self.outsRP_Yr4_Bkt2 = values[168]
		self.outsRP_Yr4_Bkt3 = values[169]
		self.outsRP_Yr4_Bkt4 = values[170]
		self.outsRP_Yr4_Bkt5 = values[171]
		self.outsRP_Yr4_Bkt6 = values[172]
		self.outsRP_Yr4_Bkt7 = values[173]
		self.outsRP4 = values[174]
		self.warRP_Yr4_Bkt0 = values[175]
		self.warRP_Yr4_Bkt1 = values[176]
		self.warRP_Yr4_Bkt2 = values[177]
		self.warRP_Yr4_Bkt3 = values[178]
		self.warRP_Yr4_Bkt4 = values[179]
		self.warRP_Yr4_Bkt5 = values[180]
		self.warRP_Yr4_Bkt6 = values[181]
		self.warRP_Yr4_Bkt7 = values[182]
		self.warRP4 = values[183]
		self.outsSP_Yr5_Bkt0 = values[184]
		self.outsSP_Yr5_Bkt1 = values[185]
		self.outsSP_Yr5_Bkt2 = values[186]
		self.outsSP_Yr5_Bkt3 = values[187]
		self.outsSP_Yr5_Bkt4 = values[188]
		self.outsSP_Yr5_Bkt5 = values[189]
		self.outsSP_Yr5_Bkt6 = values[190]
		self.outsSP_Yr5_Bkt7 = values[191]
		self.outsSP5 = values[192]
		self.warSP_Yr5_Bkt0 = values[193]
		self.warSP_Yr5_Bkt1 = values[194]
		self.warSP_Yr5_Bkt2 = values[195]
		self.warSP_Yr5_Bkt3 = values[196]
		self.warSP_Yr5_Bkt4 = values[197]
		self.warSP_Yr5_Bkt5 = values[198]
		self.warSP_Yr5_Bkt6 = values[199]
		self.warSP_Yr5_Bkt7 = values[200]
		self.warSP5 = values[201]
		self.outsRP_Yr5_Bkt0 = values[202]
		self.outsRP_Yr5_Bkt1 = values[203]
		self.outsRP_Yr5_Bkt2 = values[204]
		self.outsRP_Yr5_Bkt3 = values[205]
		self.outsRP_Yr5_Bkt4 = values[206]
		self.outsRP_Yr5_Bkt5 = values[207]
		self.outsRP_Yr5_Bkt6 = values[208]
		self.outsRP_Yr5_Bkt7 = values[209]
		self.outsRP5 = values[210]
		self.warRP_Yr5_Bkt0 = values[211]
		self.warRP_Yr5_Bkt1 = values[212]
		self.warRP_Yr5_Bkt2 = values[213]
		self.warRP_Yr5_Bkt3 = values[214]
		self.warRP_Yr5_Bkt4 = values[215]
		self.warRP_Yr5_Bkt5 = values[216]
		self.warRP_Yr5_Bkt6 = values[217]
		self.warRP_Yr5_Bkt7 = values[218]
		self.warRP5 = values[219]
		self.outsSP_Yr6_Bkt0 = values[220]
		self.outsSP_Yr6_Bkt1 = values[221]
		self.outsSP_Yr6_Bkt2 = values[222]
		self.outsSP_Yr6_Bkt3 = values[223]
		self.outsSP_Yr6_Bkt4 = values[224]
		self.outsSP_Yr6_Bkt5 = values[225]
		self.outsSP_Yr6_Bkt6 = values[226]
		self.outsSP_Yr6_Bkt7 = values[227]
		self.outsSP6 = values[228]
		self.warSP_Yr6_Bkt0 = values[229]
		self.warSP_Yr6_Bkt1 = values[230]
		self.warSP_Yr6_Bkt2 = values[231]
		self.warSP_Yr6_Bkt3 = values[232]
		self.warSP_Yr6_Bkt4 = values[233]
		self.warSP_Yr6_Bkt5 = values[234]
		self.warSP_Yr6_Bkt6 = values[235]
		self.warSP_Yr6_Bkt7 = values[236]
		self.warSP6 = values[237]
		self.outsRP_Yr6_Bkt0 = values[238]
		self.outsRP_Yr6_Bkt1 = values[239]
		self.outsRP_Yr6_Bkt2 = values[240]
		self.outsRP_Yr6_Bkt3 = values[241]
		self.outsRP_Yr6_Bkt4 = values[242]
		self.outsRP_Yr6_Bkt5 = values[243]
		self.outsRP_Yr6_Bkt6 = values[244]
		self.outsRP_Yr6_Bkt7 = values[245]
		self.outsRP6 = values[246]
		self.warRP_Yr6_Bkt0 = values[247]
		self.warRP_Yr6_Bkt1 = values[248]
		self.warRP_Yr6_Bkt2 = values[249]
		self.warRP_Yr6_Bkt3 = values[250]
		self.warRP_Yr6_Bkt4 = values[251]
		self.warRP_Yr6_Bkt5 = values[252]
		self.warRP_Yr6_Bkt6 = values[253]
		self.warRP_Yr6_Bkt7 = values[254]
		self.warRP6 = values[255]

	NUM_ELEMENTS = 256

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.year,self.month,self.outsSP_Yr0_Bkt0,self.outsSP_Yr0_Bkt1,self.outsSP_Yr0_Bkt2,self.outsSP_Yr0_Bkt3,self.outsSP_Yr0_Bkt4,self.outsSP_Yr0_Bkt5,self.outsSP_Yr0_Bkt6,self.outsSP_Yr0_Bkt7,self.outsSP0,self.warSP_Yr0_Bkt0,self.warSP_Yr0_Bkt1,self.warSP_Yr0_Bkt2,self.warSP_Yr0_Bkt3,self.warSP_Yr0_Bkt4,self.warSP_Yr0_Bkt5,self.warSP_Yr0_Bkt6,self.warSP_Yr0_Bkt7,self.warSP0,self.outsRP_Yr0_Bkt0,self.outsRP_Yr0_Bkt1,self.outsRP_Yr0_Bkt2,self.outsRP_Yr0_Bkt3,self.outsRP_Yr0_Bkt4,self.outsRP_Yr0_Bkt5,self.outsRP_Yr0_Bkt6,self.outsRP_Yr0_Bkt7,self.outsRP0,self.warRP_Yr0_Bkt0,self.warRP_Yr0_Bkt1,self.warRP_Yr0_Bkt2,self.warRP_Yr0_Bkt3,self.warRP_Yr0_Bkt4,self.warRP_Yr0_Bkt5,self.warRP_Yr0_Bkt6,self.warRP_Yr0_Bkt7,self.warRP0,self.outsSP_Yr1_Bkt0,self.outsSP_Yr1_Bkt1,self.outsSP_Yr1_Bkt2,self.outsSP_Yr1_Bkt3,self.outsSP_Yr1_Bkt4,self.outsSP_Yr1_Bkt5,self.outsSP_Yr1_Bkt6,self.outsSP_Yr1_Bkt7,self.outsSP1,self.warSP_Yr1_Bkt0,self.warSP_Yr1_Bkt1,self.warSP_Yr1_Bkt2,self.warSP_Yr1_Bkt3,self.warSP_Yr1_Bkt4,self.warSP_Yr1_Bkt5,self.warSP_Yr1_Bkt6,self.warSP_Yr1_Bkt7,self.warSP1,self.outsRP_Yr1_Bkt0,self.outsRP_Yr1_Bkt1,self.outsRP_Yr1_Bkt2,self.outsRP_Yr1_Bkt3,self.outsRP_Yr1_Bkt4,self.outsRP_Yr1_Bkt5,self.outsRP_Yr1_Bkt6,self.outsRP_Yr1_Bkt7,self.outsRP1,self.warRP_Yr1_Bkt0,self.warRP_Yr1_Bkt1,self.warRP_Yr1_Bkt2,self.warRP_Yr1_Bkt3,self.warRP_Yr1_Bkt4,self.warRP_Yr1_Bkt5,self.warRP_Yr1_Bkt6,self.warRP_Yr1_Bkt7,self.warRP1,self.outsSP_Yr2_Bkt0,self.outsSP_Yr2_Bkt1,self.outsSP_Yr2_Bkt2,self.outsSP_Yr2_Bkt3,self.outsSP_Yr2_Bkt4,self.outsSP_Yr2_Bkt5,self.outsSP_Yr2_Bkt6,self.outsSP_Yr2_Bkt7,self.outsSP2,self.warSP_Yr2_Bkt0,self.warSP_Yr2_Bkt1,self.warSP_Yr2_Bkt2,self.warSP_Yr2_Bkt3,self.warSP_Yr2_Bkt4,self.warSP_Yr2_Bkt5,self.warSP_Yr2_Bkt6,self.warSP_Yr2_Bkt7,self.warSP2,self.outsRP_Yr2_Bkt0,self.outsRP_Yr2_Bkt1,self.outsRP_Yr2_Bkt2,self.outsRP_Yr2_Bkt3,self.outsRP_Yr2_Bkt4,self.outsRP_Yr2_Bkt5,self.outsRP_Yr2_Bkt6,self.outsRP_Yr2_Bkt7,self.outsRP2,self.warRP_Yr2_Bkt0,self.warRP_Yr2_Bkt1,self.warRP_Yr2_Bkt2,self.warRP_Yr2_Bkt3,self.warRP_Yr2_Bkt4,self.warRP_Yr2_Bkt5,self.warRP_Yr2_Bkt6,self.warRP_Yr2_Bkt7,self.warRP2,self.outsSP_Yr3_Bkt0,self.outsSP_Yr3_Bkt1,self.outsSP_Yr3_Bkt2,self.outsSP_Yr3_Bkt3,self.outsSP_Yr3_Bkt4,self.outsSP_Yr3_Bkt5,self.outsSP_Yr3_Bkt6,self.outsSP_Yr3_Bkt7,self.outsSP3,self.warSP_Yr3_Bkt0,self.warSP_Yr3_Bkt1,self.warSP_Yr3_Bkt2,self.warSP_Yr3_Bkt3,self.warSP_Yr3_Bkt4,self.warSP_Yr3_Bkt5,self.warSP_Yr3_Bkt6,self.warSP_Yr3_Bkt7,self.warSP3,self.outsRP_Yr3_Bkt0,self.outsRP_Yr3_Bkt1,self.outsRP_Yr3_Bkt2,self.outsRP_Yr3_Bkt3,self.outsRP_Yr3_Bkt4,self.outsRP_Yr3_Bkt5,self.outsRP_Yr3_Bkt6,self.outsRP_Yr3_Bkt7,self.outsRP3,self.warRP_Yr3_Bkt0,self.warRP_Yr3_Bkt1,self.warRP_Yr3_Bkt2,self.warRP_Yr3_Bkt3,self.warRP_Yr3_Bkt4,self.warRP_Yr3_Bkt5,self.warRP_Yr3_Bkt6,self.warRP_Yr3_Bkt7,self.warRP3,self.outsSP_Yr4_Bkt0,self.outsSP_Yr4_Bkt1,self.outsSP_Yr4_Bkt2,self.outsSP_Yr4_Bkt3,self.outsSP_Yr4_Bkt4,self.outsSP_Yr4_Bkt5,self.outsSP_Yr4_Bkt6,self.outsSP_Yr4_Bkt7,self.outsSP4,self.warSP_Yr4_Bkt0,self.warSP_Yr4_Bkt1,self.warSP_Yr4_Bkt2,self.warSP_Yr4_Bkt3,self.warSP_Yr4_Bkt4,self.warSP_Yr4_Bkt5,self.warSP_Yr4_Bkt6,self.warSP_Yr4_Bkt7,self.warSP4,self.outsRP_Yr4_Bkt0,self.outsRP_Yr4_Bkt1,self.outsRP_Yr4_Bkt2,self.outsRP_Yr4_Bkt3,self.outsRP_Yr4_Bkt4,self.outsRP_Yr4_Bkt5,self.outsRP_Yr4_Bkt6,self.outsRP_Yr4_Bkt7,self.outsRP4,self.warRP_Yr4_Bkt0,self.warRP_Yr4_Bkt1,self.warRP_Yr4_Bkt2,self.warRP_Yr4_Bkt3,self.warRP_Yr4_Bkt4,self.warRP_Yr4_Bkt5,self.warRP_Yr4_Bkt6,self.warRP_Yr4_Bkt7,self.warRP4,self.outsSP_Yr5_Bkt0,self.outsSP_Yr5_Bkt1,self.outsSP_Yr5_Bkt2,self.outsSP_Yr5_Bkt3,self.outsSP_Yr5_Bkt4,self.outsSP_Yr5_Bkt5,self.outsSP_Yr5_Bkt6,self.outsSP_Yr5_Bkt7,self.outsSP5,self.warSP_Yr5_Bkt0,self.warSP_Yr5_Bkt1,self.warSP_Yr5_Bkt2,self.warSP_Yr5_Bkt3,self.warSP_Yr5_Bkt4,self.warSP_Yr5_Bkt5,self.warSP_Yr5_Bkt6,self.warSP_Yr5_Bkt7,self.warSP5,self.outsRP_Yr5_Bkt0,self.outsRP_Yr5_Bkt1,self.outsRP_Yr5_Bkt2,self.outsRP_Yr5_Bkt3,self.outsRP_Yr5_Bkt4,self.outsRP_Yr5_Bkt5,self.outsRP_Yr5_Bkt6,self.outsRP_Yr5_Bkt7,self.outsRP5,self.warRP_Yr5_Bkt0,self.warRP_Yr5_Bkt1,self.warRP_Yr5_Bkt2,self.warRP_Yr5_Bkt3,self.warRP_Yr5_Bkt4,self.warRP_Yr5_Bkt5,self.warRP_Yr5_Bkt6,self.warRP_Yr5_Bkt7,self.warRP5,self.outsSP_Yr6_Bkt0,self.outsSP_Yr6_Bkt1,self.outsSP_Yr6_Bkt2,self.outsSP_Yr6_Bkt3,self.outsSP_Yr6_Bkt4,self.outsSP_Yr6_Bkt5,self.outsSP_Yr6_Bkt6,self.outsSP_Yr6_Bkt7,self.outsSP6,self.warSP_Yr6_Bkt0,self.warSP_Yr6_Bkt1,self.warSP_Yr6_Bkt2,self.warSP_Yr6_Bkt3,self.warSP_Yr6_Bkt4,self.warSP_Yr6_Bkt5,self.warSP_Yr6_Bkt6,self.warSP_Yr6_Bkt7,self.warSP6,self.outsRP_Yr6_Bkt0,self.outsRP_Yr6_Bkt1,self.outsRP_Yr6_Bkt2,self.outsRP_Yr6_Bkt3,self.outsRP_Yr6_Bkt4,self.outsRP_Yr6_Bkt5,self.outsRP_Yr6_Bkt6,self.outsRP_Yr6_Bkt7,self.outsRP6,self.warRP_Yr6_Bkt0,self.warRP_Yr6_Bkt1,self.warRP_Yr6_Bkt2,self.warRP_Yr6_Bkt3,self.warRP_Yr6_Bkt4,self.warRP_Yr6_Bkt5,self.warRP_Yr6_Bkt6,self.warRP_Yr6_Bkt7,self.warRP6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitcherMlbWarAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitcherMlbWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherMlbWarAggregation(i) for i in items]

class DB_SingleYearHitterBucketAverages:
	def __init__(self, values : tuple[any]):
		self.Month = values[0]
		self.war0 = values[1]
		self.war1 = values[2]
		self.war2 = values[3]
		self.war3 = values[4]
		self.war4 = values[5]
		self.war5 = values[6]
		self.war6 = values[7]
		self.war7 = values[8]
		self.pa0 = values[9]
		self.pa1 = values[10]
		self.pa2 = values[11]
		self.pa3 = values[12]
		self.pa4 = values[13]
		self.pa5 = values[14]
		self.pa6 = values[15]

	NUM_ELEMENTS = 16

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Month,self.war0,self.war1,self.war2,self.war3,self.war4,self.war5,self.war6,self.war7,self.pa0,self.pa1,self.pa2,self.pa3,self.pa4,self.pa5,self.pa6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_SingleYearHitterBucketAverages']:
		items = cursor.execute("SELECT * FROM SingleYearHitterBucketAverages " + conditional, values).fetchall()
		return [DB_SingleYearHitterBucketAverages(i) for i in items]

class DB_SingleYearPitcherBucketAverages:
	def __init__(self, values : tuple[any]):
		self.Month = values[0]
		self.outsSP0 = values[1]
		self.outsSP1 = values[2]
		self.outsSP2 = values[3]
		self.outsSP3 = values[4]
		self.outsSP4 = values[5]
		self.outsSP5 = values[6]
		self.outsSP6 = values[7]
		self.outsSP7 = values[8]
		self.warSP0 = values[9]
		self.warSP1 = values[10]
		self.warSP2 = values[11]
		self.warSP3 = values[12]
		self.warSP4 = values[13]
		self.warSP5 = values[14]
		self.warSP6 = values[15]
		self.warSP7 = values[16]
		self.outsRP0 = values[17]
		self.outsRP1 = values[18]
		self.outsRP2 = values[19]
		self.outsRP3 = values[20]
		self.outsRP4 = values[21]
		self.outsRP5 = values[22]
		self.outsRP6 = values[23]
		self.outsRP7 = values[24]
		self.warRP0 = values[25]
		self.warRP1 = values[26]
		self.warRP2 = values[27]
		self.warRP3 = values[28]
		self.warRP4 = values[29]
		self.warRP5 = values[30]
		self.warRP6 = values[31]
		self.warRP7 = values[32]

	NUM_ELEMENTS = 33

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Month,self.outsSP0,self.outsSP1,self.outsSP2,self.outsSP3,self.outsSP4,self.outsSP5,self.outsSP6,self.outsSP7,self.warSP0,self.warSP1,self.warSP2,self.warSP3,self.warSP4,self.warSP5,self.warSP6,self.warSP7,self.outsRP0,self.outsRP1,self.outsRP2,self.outsRP3,self.outsRP4,self.outsRP5,self.outsRP6,self.outsRP7,self.warRP0,self.warRP1,self.warRP2,self.warRP3,self.warRP4,self.warRP5,self.warRP6,self.warRP7)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_SingleYearPitcherBucketAverages']:
		items = cursor.execute("SELECT * FROM SingleYearPitcherBucketAverages " + conditional, values).fetchall()
		return [DB_SingleYearPitcherBucketAverages(i) for i in items]


##############################################################################################
