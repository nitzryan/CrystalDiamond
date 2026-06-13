import sqlite3

class DB_Output_PitchValue:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.ModelRun = values[3]
		self.Year = values[4]
		self.LevelId = values[5]
		self.mlbId = values[6]
		self.stuffCalledStrike = values[7]
		self.stuffBall = values[8]
		self.stuffHBP = values[9]
		self.stuffSwing = values[10]
		self.stuffWhiff = values[11]
		self.stuffFoul = values[12]
		self.stuffInPlay = values[13]
		self.stuffInPlayExpected = values[14]
		self.combinedCalledStrike = values[15]
		self.combinedBall = values[16]
		self.combinedHBP = values[17]
		self.combinedSwing = values[18]
		self.combinedWhiff = values[19]
		self.combinedFoul = values[20]
		self.combinedInPlay = values[21]
		self.combinedInPlayExpected = values[22]

	NUM_ELEMENTS = 23

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.ModelRun,self.Year,self.LevelId,self.mlbId,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValue']:
		items = cursor.execute("SELECT * FROM Output_PitchValue " + conditional, values).fetchall()
		return [DB_Output_PitchValue(i) for i in items]

class DB_Models_PitchValue:
	def __init__(self, values : tuple[any]):
		self.Id = values[0]
		self.Name = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Id,self.Name)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Models_PitchValue']:
		items = cursor.execute("SELECT * FROM Models_PitchValue " + conditional, values).fetchall()
		return [DB_Models_PitchValue(i) for i in items]

class DB_ModelTrainingHistory_PitchValue:
	def __init__(self, values : tuple[any]):
		self.ModelId = values[0]
		self.ModelRun = values[1]
		self.LossStuffResult = values[2]
		self.LossStuffSwing = values[3]
		self.LossStuffInplay = values[4]
		self.LossCombinedResult = values[5]
		self.LossCombinedSwing = values[6]
		self.LossCombinedInplay = values[7]
		self.Arch = values[8]

	NUM_ELEMENTS = 9

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.ModelRun,self.LossStuffResult,self.LossStuffSwing,self.LossStuffInplay,self.LossCombinedResult,self.LossCombinedSwing,self.LossCombinedInplay,self.Arch)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelTrainingHistory_PitchValue']:
		items = cursor.execute("SELECT * FROM ModelTrainingHistory_PitchValue " + conditional, values).fetchall()
		return [DB_ModelTrainingHistory_PitchValue(i) for i in items]

class DB_PlayersInTrainingData:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.modelId = values[1]
		self.modelRun = values[2]
		self.isTrain = values[3]

	NUM_ELEMENTS = 4

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.modelId,self.modelRun,self.isTrain)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PlayersInTrainingData']:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData(i) for i in items]

class DB_YearLeagueDeviations:
	def __init__(self, values : tuple[any]):
		self.ModelId = values[0]
		self.Year = values[1]
		self.Balls = values[2]
		self.Strikes = values[3]
		self.StuffDev = values[4]
		self.PitchDev = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.Year,self.Balls,self.Strikes,self.StuffDev,self.PitchDev)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_YearLeagueDeviations']:
		items = cursor.execute("SELECT * FROM YearLeagueDeviations " + conditional, values).fetchall()
		return [DB_YearLeagueDeviations(i) for i in items]

class DB_PitcherStuff:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.Model = values[3]
		self.GameId = values[4]
		self.PitchType = values[5]
		self.Scenario = values[6]
		self.NumPitches = values[7]
		self.ValueActual = values[8]
		self.ValueStuff = values[9]
		self.ValueCombined = values[10]
		self.ActualPlus = values[11]
		self.StuffPlus = values[12]
		self.PitchPlus = values[13]
		self.Vel = values[14]
		self.BreakHoriz = values[15]
		self.BreakVert = values[16]

	NUM_ELEMENTS = 17

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.Model,self.GameId,self.PitchType,self.Scenario,self.NumPitches,self.ValueActual,self.ValueStuff,self.ValueCombined,self.ActualPlus,self.StuffPlus,self.PitchPlus,self.Vel,self.BreakHoriz,self.BreakVert)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitcherStuff']:
		items = cursor.execute("SELECT * FROM PitcherStuff " + conditional, values).fetchall()
		return [DB_PitcherStuff(i) for i in items]

class DB_Output_PitchValueAggregation:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.Year = values[3]
		self.LevelId = values[4]
		self.mlbId = values[5]
		self.CountBalls = values[6]
		self.CountStrikes = values[7]
		self.stuffCalledStrike = values[8]
		self.stuffBall = values[9]
		self.stuffHBP = values[10]
		self.stuffSwing = values[11]
		self.stuffWhiff = values[12]
		self.stuffFoul = values[13]
		self.stuffInPlay = values[14]
		self.stuffInPlayExpected = values[15]
		self.combinedCalledStrike = values[16]
		self.combinedBall = values[17]
		self.combinedHBP = values[18]
		self.combinedSwing = values[19]
		self.combinedWhiff = values[20]
		self.combinedFoul = values[21]
		self.combinedInPlay = values[22]
		self.combinedInPlayExpected = values[23]
		self.stuffRuns = values[24]
		self.combinedRuns = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.Year,self.LevelId,self.mlbId,self.CountBalls,self.CountStrikes,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected,self.stuffRuns,self.combinedRuns)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitchValueAggregation " + conditional, values).fetchall()
		return [DB_Output_PitchValueAggregation(i) for i in items]

