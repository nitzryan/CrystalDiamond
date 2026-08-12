import sqlite3

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
		self.Year = values[1]
		self.ModelRun = values[2]
		self.TestStuffResult = values[3]
		self.TestStuffSwing = values[4]
		self.TestStuffInplay = values[5]
		self.TestCombinedResult = values[6]
		self.TestCombinedSwing = values[7]
		self.TestCombinedInplay = values[8]
		self.ValSeenStuffResult = values[9]
		self.ValSeenStuffSwing = values[10]
		self.ValSeenStuffInplay = values[11]
		self.ValSeenCombinedResult = values[12]
		self.ValSeenCombinedSwing = values[13]
		self.ValSeenCombinedInplay = values[14]
		self.ValUnseenStuffResult = values[15]
		self.ValUnseenStuffSwing = values[16]
		self.ValUnseenStuffInplay = values[17]
		self.ValUnseenCombinedResult = values[18]
		self.ValUnseenCombinedSwing = values[19]
		self.ValUnseenCombinedInplay = values[20]

	NUM_ELEMENTS = 21

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.Year,self.ModelRun,self.TestStuffResult,self.TestStuffSwing,self.TestStuffInplay,self.TestCombinedResult,self.TestCombinedSwing,self.TestCombinedInplay,self.ValSeenStuffResult,self.ValSeenStuffSwing,self.ValSeenStuffInplay,self.ValSeenCombinedResult,self.ValSeenCombinedSwing,self.ValSeenCombinedInplay,self.ValUnseenStuffResult,self.ValUnseenStuffSwing,self.ValUnseenStuffInplay,self.ValUnseenCombinedResult,self.ValUnseenCombinedSwing,self.ValUnseenCombinedInplay)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_ModelTrainingHistory_PitchValue']:
		items = cursor.execute("SELECT * FROM ModelTrainingHistory_PitchValue " + conditional, values).fetchall()
		return [DB_ModelTrainingHistory_PitchValue(i) for i in items]

class DB_PlayersInTrainingData:
	def __init__(self, values : tuple[any]):
		self.mlbId = values[0]
		self.modelId = values[1]
		self.Year = values[2]
		self.modelRun = values[3]
		self.isTrain = values[4]

	NUM_ELEMENTS = 5

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.modelId,self.Year,self.modelRun,self.isTrain)
                        
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

class DB_Output_PitchValue:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.ModelYear = values[3]
		self.ModelRun = values[4]
		self.Year = values[5]
		self.LevelId = values[6]
		self.mlbId = values[7]
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

	NUM_ELEMENTS = 24

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.ModelYear,self.ModelRun,self.Year,self.LevelId,self.mlbId,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValue']:
		items = cursor.execute("SELECT * FROM Output_PitchValue " + conditional, values).fetchall()
		return [DB_Output_PitchValue(i) for i in items]

class DB_Output_PitchValueAggregation:
	def __init__(self, values : tuple[any]):
		self.model = values[0]
		self.gameId = values[1]
		self.pitchId = values[2]
		self.ModelYear = values[3]
		self.Year = values[4]
		self.LevelId = values[5]
		self.mlbId = values[6]
		self.CountBalls = values[7]
		self.CountStrikes = values[8]
		self.stuffCalledStrike = values[9]
		self.stuffBall = values[10]
		self.stuffHBP = values[11]
		self.stuffSwing = values[12]
		self.stuffWhiff = values[13]
		self.stuffFoul = values[14]
		self.stuffInPlay = values[15]
		self.stuffInPlayExpected = values[16]
		self.combinedCalledStrike = values[17]
		self.combinedBall = values[18]
		self.combinedHBP = values[19]
		self.combinedSwing = values[20]
		self.combinedWhiff = values[21]
		self.combinedFoul = values[22]
		self.combinedInPlay = values[23]
		self.combinedInPlayExpected = values[24]
		self.stuffRuns = values[25]
		self.combinedRuns = values[26]

	NUM_ELEMENTS = 27

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.model,self.gameId,self.pitchId,self.ModelYear,self.Year,self.LevelId,self.mlbId,self.CountBalls,self.CountStrikes,self.stuffCalledStrike,self.stuffBall,self.stuffHBP,self.stuffSwing,self.stuffWhiff,self.stuffFoul,self.stuffInPlay,self.stuffInPlayExpected,self.combinedCalledStrike,self.combinedBall,self.combinedHBP,self.combinedSwing,self.combinedWhiff,self.combinedFoul,self.combinedInPlay,self.combinedInPlayExpected,self.stuffRuns,self.combinedRuns)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_Output_PitchValueAggregation']:
		items = cursor.execute("SELECT * FROM Output_PitchValueAggregation " + conditional, values).fetchall()
		return [DB_Output_PitchValueAggregation(i) for i in items]

class DB_PitchValue:
	def __init__(self, values : tuple[any]):
		self.ModelId = values[0]
		self.GameId = values[1]
		self.PitchId = values[2]
		self.PitcherId = values[3]
		self.StuffPlus = values[4]
		self.StuffRuns = values[5]
		self.PitchPlus = values[6]
		self.PitchRuns = values[7]

	NUM_ELEMENTS = 8

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.ModelId,self.GameId,self.PitchId,self.PitcherId,self.StuffPlus,self.StuffRuns,self.PitchPlus,self.PitchRuns)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchValue']:
		items = cursor.execute("SELECT * FROM PitchValue " + conditional, values).fetchall()
		return [DB_PitchValue(i) for i in items]

class DB_PitchModelResultBasis:
	def __init__(self, values : tuple[any]):
		self.Year = values[0]
		self.ModelId = values[1]
		self.CountBalls = values[2]
		self.CountStrikes = values[3]
		self.OutputType = values[4]
		self.Min = values[5]
		self.Perc5 = values[6]
		self.Avg = values[7]
		self.Median = values[8]
		self.Perc95 = values[9]
		self.Max = values[10]

	NUM_ELEMENTS = 11

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Year,self.ModelId,self.CountBalls,self.CountStrikes,self.OutputType,self.Min,self.Perc5,self.Avg,self.Median,self.Perc95,self.Max)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchModelResultBasis']:
		items = cursor.execute("SELECT * FROM PitchModelResultBasis " + conditional, values).fetchall()
		return [DB_PitchModelResultBasis(i) for i in items]

class DB_PitcherStatcastMonth:
	def __init__(self, values : tuple[any]):
		self.MlbId = values[0]
		self.Year = values[1]
		self.Month = values[2]
		self.ModelId = values[3]
		self.IsValid = values[4]
		self.Stuff = values[5]
		self.Pitch = values[6]
		self.Actual = values[7]
		self.Smoothed = values[8]
		self.NumPitches = values[9]
		self.StuffFastball = values[10]
		self.PitchFastball = values[11]
		self.ActFastball = values[12]
		self.SmoothedFastball = values[13]
		self.NumFastballs = values[14]
		self.StuffBreaking = values[15]
		self.PitchBreaking = values[16]
		self.ActBreaking = values[17]
		self.SmoothedBreaking = values[18]
		self.NumBreaking = values[19]
		self.StuffChangeup = values[20]
		self.PitchChangeup = values[21]
		self.ActChangeup = values[22]
		self.SmoothedChangeup = values[23]
		self.NumChangeup = values[24]

	NUM_ELEMENTS = 25

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.MlbId,self.Year,self.Month,self.ModelId,self.IsValid,self.Stuff,self.Pitch,self.Actual,self.Smoothed,self.NumPitches,self.StuffFastball,self.PitchFastball,self.ActFastball,self.SmoothedFastball,self.NumFastballs,self.StuffBreaking,self.PitchBreaking,self.ActBreaking,self.SmoothedBreaking,self.NumBreaking,self.StuffChangeup,self.PitchChangeup,self.ActChangeup,self.SmoothedChangeup,self.NumChangeup)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitcherStatcastMonth']:
		items = cursor.execute("SELECT * FROM PitcherStatcastMonth " + conditional, values).fetchall()
		return [DB_PitcherStatcastMonth(i) for i in items]


##############################################################################################
