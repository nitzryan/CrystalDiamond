import sqlite3

class DB_Output_PitchValue:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.ModelRun = values[3]
		self.absValue = values[4]
		self.stuffOnly = values[5]
		self.locationOnly = values[6]
		self.combined = values[7]

	NUM_ELEMENTS = 8

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.ModelRun,self.absValue,self.stuffOnly,self.locationOnly,self.combined)
                        
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
		self.LossLocation = values[2]
		self.LossStuff = values[3]
		self.LossCombined = values[4]
		self.Arch = values[5]

	NUM_ELEMENTS = 6

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.ModelRun,self.LossLocation,self.LossStuff,self.LossCombined,self.Arch)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelTrainingHistory_PitchValue']:
		items = cursor.execute("SELECT * FROM ModelTrainingHistory_PitchValue " + conditional, values).fetchall()
		return [DB_ModelTrainingHistory_PitchValue(i) for i in items]

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

class DB_Output_PitchValueAggregation:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.absValue = values[3]
		self.stuffOnly = values[4]
		self.locationOnly = values[5]
		self.combined = values[6]

	NUM_ELEMENTS = 7

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.absValue,self.stuffOnly,self.locationOnly,self.combined)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitchValueAggregation " + conditional, values).fetchall()
		return [DB_Output_PitchValueAggregation(i) for i in items]

