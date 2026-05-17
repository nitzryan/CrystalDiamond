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

class DB_YearLeagueDeviations:
	def __init__(self, values : tuple[any]):
		self.ModelId = values[0]
		self.Year = values[1]
		self.ActDev = values[2]
		self.StuffDev = values[3]
		self.LocDev = values[4]
		self.PitchDev = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.Year,self.ActDev,self.StuffDev,self.LocDev,self.PitchDev)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_YearLeagueDeviations']:
		items = cursor.execute("SELECT * FROM YearLeagueDeviations " + conditional, values).fetchall()
		return [DB_YearLeagueDeviations(i) for i in items]

class DB_PitcherStuff:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.GameId = values[3]
		self.PitchType = values[4]
		self.Scenario = values[5]
		self.NumPitches = values[6]
		self.ValueStuff = values[7]
		self.ValueLoc = values[8]
		self.ValueCombined = values[9]
		self.Vel = values[10]
		self.BreakHoriz = values[11]
		self.BreakVert = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.GameId,self.PitchType,self.Scenario,self.NumPitches,self.ValueStuff,self.ValueLoc,self.ValueCombined,self.Vel,self.BreakHoriz,self.BreakVert)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitcherStuff']:
		items = cursor.execute("SELECT * FROM PitcherStuff " + conditional, values).fetchall()
		return [DB_PitcherStuff(i) for i in items]

class DB_ModelTrainingHistory_PitchValue:
	def __init__(self, values : tuple[any]):
		self.ModelId = values[0]
		self.ModelRun = values[1]
		self.PitchType = values[2]
		self.LossLocationResult = values[3]
		self.LossLocationSwing = values[4]
		self.LossLocationInplay = values[5]
		self.LossStuffResult = values[6]
		self.LossStuffSwing = values[7]
		self.LossStuffInplay = values[8]
		self.LossCombinedResult = values[9]
		self.LossCombinedSwing = values[10]
		self.LossCombinedInplay = values[11]
		self.Arch = values[12]

	NUM_ELEMENTS = 13

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.ModelRun,self.PitchType,self.LossLocationResult,self.LossLocationSwing,self.LossLocationInplay,self.LossStuffResult,self.LossStuffSwing,self.LossStuffInplay,self.LossCombinedResult,self.LossCombinedSwing,self.LossCombinedInplay,self.Arch)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelTrainingHistory_PitchValue']:
		items = cursor.execute("SELECT * FROM ModelTrainingHistory_PitchValue " + conditional, values).fetchall()
		return [DB_ModelTrainingHistory_PitchValue(i) for i in items]

