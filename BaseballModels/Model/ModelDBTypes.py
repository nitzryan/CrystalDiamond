import sqlite3

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
		return (self.MlbId,self.Model,self.ModelIdx,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.BSR,self.DRAA,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
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

class DB_Output_College:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.ModelIdx = values[3]
		self.year = values[4]
		self.draft0 = values[5]
		self.draft1 = values[6]
		self.draft2 = values[7]
		self.draft3 = values[8]
		self.draft4 = values[9]
		self.draft5 = values[10]
		self.draft6 = values[11]
		self.draft = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.model,self.isHitter,self.ModelIdx,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_College']:
		items = cursor.execute("SELECT * FROM Output_College " + conditional, values).fetchall()
		return [DB_Output_College(i) for i in items]

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

class DB_Model_TrainingHistory_College:
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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Model_TrainingHistory_College']:
		items = cursor.execute("SELECT * FROM Model_TrainingHistory_College " + conditional, values).fetchall()
		return [DB_Model_TrainingHistory_College(i) for i in items]

class DB_PlayersInTrainingData_College:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.modelIdx = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.modelIdx)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PlayersInTrainingData_College']:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData_College " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData_College(i) for i in items]

class DB_ModelIdx_College:
	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.pitcherModelName = values[1]
		self.hitterModelName = values[2]
		self.modelName = values[3]

	NUM_ELEMENTS = 4

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.pitcherModelName,self.hitterModelName,self.modelName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelIdx_College']:
		items = cursor.execute("SELECT * FROM ModelIdx_College " + conditional, values).fetchall()
		return [DB_ModelIdx_College(i) for i in items]

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
		return (self.MlbId,self.Model,self.Year,self.Month,self.LevelId,self.Pa,self.Hit1B,self.Hit2B,self.Hit3B,self.HitHR,self.BB,self.HBP,self.K,self.SB,self.CS,self.BSR,self.DRAA,self.ParkRunFactor,self.PercC,self.Perc1B,self.Perc2B,self.Perc3B,self.PercSS,self.PercLF,self.PercCF,self.PercRF,self.PercDH)
                        
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

class DB_Output_CollegeAggregation:
	def __init__(self, values : tuple[any]):
		self.tbcId = values[0]
		self.model = values[1]
		self.isHitter = values[2]
		self.year = values[3]
		self.draft0 = values[4]
		self.draft1 = values[5]
		self.draft2 = values[6]
		self.draft3 = values[7]
		self.draft4 = values[8]
		self.draft5 = values[9]
		self.draft6 = values[10]
		self.draft = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.tbcId,self.model,self.isHitter,self.year,self.draft0,self.draft1,self.draft2,self.draft3,self.draft4,self.draft5,self.draft6,self.draft)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_CollegeAggregation']:
		items = cursor.execute("SELECT * FROM Output_CollegeAggregation " + conditional, values).fetchall()
		return [DB_Output_CollegeAggregation(i) for i in items]


##############################################################################################
