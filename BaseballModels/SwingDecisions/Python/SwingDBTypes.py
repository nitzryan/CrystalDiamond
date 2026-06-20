import sqlite3

class DB_SwingDecision:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.Year = values[2]
		self.Month = values[3]
		self.HitterId = values[4]
		self.PitcherId = values[5]
		self.LevelId = values[6]
		self.PitchType = values[7]
		self.CountBalls = values[8]
		self.CountStrikes = values[9]
		self.Outs = values[10]
		self.BaseOccupancy = values[11]
		self.DidSwing = values[12]
		self.ProbSwing = values[13]
		self.ValueSwing = values[14]
		self.ValueNoSwing = values[15]
		self.Value = values[16]

	NUM_ELEMENTS = 17

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.Year,self.Month,self.HitterId,self.PitcherId,self.LevelId,self.PitchType,self.CountBalls,self.CountStrikes,self.Outs,self.BaseOccupancy,self.DidSwing,self.ProbSwing,self.ValueSwing,self.ValueNoSwing,self.Value)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_SwingDecision']:
		items = cursor.execute("SELECT * FROM SwingDecision " + conditional, values).fetchall()
		return [DB_SwingDecision(i) for i in items]


##############################################################################################
class DB_SwingResultAggregation:
	def __init__(self, values : tuple[any]):
		self.HitterId = values[0]
		self.PitcherId = values[1]
		self.LevelId = values[2]
		self.Year = values[3]
		self.Month = values[4]
		self.PitchGroup = values[5]
		self.NumSwings = values[6]
		self.ValueSwings = values[7]
		self.ValuePer100Swings = values[8]
		self.NumNonSwings = values[9]
		self.ValueNonSwings = values[10]
		self.ValuePer100NonSwings = values[11]

	NUM_ELEMENTS = 12

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.HitterId,self.PitcherId,self.LevelId,self.Year,self.Month,self.PitchGroup,self.NumSwings,self.ValueSwings,self.ValuePer100Swings,self.NumNonSwings,self.ValueNonSwings,self.ValuePer100NonSwings)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_SwingResultAggregation']:
		items = cursor.execute("SELECT * FROM SwingResultAggregation " + conditional, values).fetchall()
		return [DB_SwingResultAggregation(i) for i in items]


##############################################################################################
