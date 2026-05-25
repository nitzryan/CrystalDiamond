import sqlite3

class DB_Output_PitchValue:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.ModelRun = values[3]
		self.Year = values[4]
		self.locationCalledStrike = values[5]
		self.locationBall = values[6]
		self.locationHBP = values[7]
		self.locationSwing = values[8]
		self.locationWhiff = values[9]
		self.locationFoul = values[10]
		self.locationInPlay = values[11]
		self.locationInPlayExpected = values[12]
		self.stuffCalledStrike = values[13]
		self.stuffBall = values[14]
		self.stuffHBP = values[15]
		self.stuffSwing = values[16]
		self.stuffWhiff = values[17]
		self.stuffFoul = values[18]
		self.stuffInPlay = values[19]
		self.stuffInPlayExpected = values[20]
		self.combinedCalledStrike = values[21]
		self.combinedBall = values[22]
		self.combinedHBP = values[23]
		self.combinedSwing = values[24]
		self.combinedWhiff = values[25]
		self.combinedFoul = values[26]
		self.combinedInPlay = values[27]
		self.combinedInPlayExpected = values[28]

	NUM_ELEMENTS = 29

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.ModelRun,self.Year,self.locationCalledStrike,self.locationBall,self.locationHBP,self.locationSwing,self.locationWhiff,self.locationFoul,self.locationInPlay,self.locationInPlayExpected,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValue']:
		items = cursor.execute("SELECT * FROM Output_PitchValue " + conditional, values).fetchall()
		return [DB_Output_PitchValue(i) for i in items]

class DB_Output_PitchValueAggregation:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.Year = values[3]
		self.CountBalls = values[4]
		self.CountStrikes = values[5]
		self.locationCalledStrike = values[6]
		self.locationBall = values[7]
		self.locationHBP = values[8]
		self.locationSwing = values[9]
		self.locationWhiff = values[10]
		self.locationFoul = values[11]
		self.locationInPlay = values[12]
		self.locationInPlayExpected = values[13]
		self.stuffCalledStrike = values[14]
		self.stuffBall = values[15]
		self.stuffHBP = values[16]
		self.stuffSwing = values[17]
		self.stuffWhiff = values[18]
		self.stuffFoul = values[19]
		self.stuffInPlay = values[20]
		self.stuffInPlayExpected = values[21]
		self.combinedCalledStrike = values[22]
		self.combinedBall = values[23]
		self.combinedHBP = values[24]
		self.combinedSwing = values[25]
		self.combinedWhiff = values[26]
		self.combinedFoul = values[27]
		self.combinedInPlay = values[28]
		self.combinedInPlayExpected = values[29]
		self.locationRuns = values[30]
		self.stuffRuns = values[31]
		self.combinedRuns = values[32]

	NUM_ELEMENTS = 33

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.Year,self.CountBalls,self.CountStrikes,self.locationCalledStrike,self.locationBall,self.locationHBP,self.locationSwing,self.locationWhiff,self.locationFoul,self.locationInPlay,self.locationInPlayExpected,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected,self.locationRuns,self.stuffRuns,self.combinedRuns)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitchValueAggregation " + conditional, values).fetchall()
		return [DB_Output_PitchValueAggregation(i) for i in items]

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
		self.LossLocationResult = values[2]
		self.LossLocationSwing = values[3]
		self.LossLocationInplay = values[4]
		self.LossStuffResult = values[5]
		self.LossStuffSwing = values[6]
		self.LossStuffInplay = values[7]
		self.LossCombinedResult = values[8]
		self.LossCombinedSwing = values[9]
		self.LossCombinedInplay = values[10]
		self.Arch = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.ModelRun,self.LossLocationResult,self.LossLocationSwing,self.LossLocationInplay,self.LossStuffResult,self.LossStuffSwing,self.LossStuffInplay,self.LossCombinedResult,self.LossCombinedSwing,self.LossCombinedInplay,self.Arch)
                        
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
		self.LocDev = values[5]
		self.PitchDev = values[6]

	NUM_ELEMENTS = 7

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.Year,self.Balls,self.Strikes,self.StuffDev,self.LocDev,self.PitchDev)
                        
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
		self.ValueLoc = values[10]
		self.ValueCombined = values[11]
		self.ActualPlus = values[12]
		self.StuffPlus = values[13]
		self.LocationPlus = values[14]
		self.PitchPlus = values[15]
		self.Vel = values[16]
		self.BreakHoriz = values[17]
		self.BreakVert = values[18]

	NUM_ELEMENTS = 19

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.Model,self.GameId,self.PitchType,self.Scenario,self.NumPitches,self.ValueActual,self.ValueStuff,self.ValueLoc,self.ValueCombined,self.ActualPlus,self.StuffPlus,self.LocationPlus,self.PitchPlus,self.Vel,self.BreakHoriz,self.BreakVert)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitcherStuff']:
		items = cursor.execute("SELECT * FROM PitcherStuff " + conditional, values).fetchall()
		return [DB_PitcherStuff(i) for i in items]

