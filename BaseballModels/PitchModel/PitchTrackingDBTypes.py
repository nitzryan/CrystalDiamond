import sqlite3

class DB_PitchData:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.Year = values[2]
		self.PitcherId = values[3]
		self.PitchClass = values[4]
		self.CountBalls = values[5]
		self.CountStrike = values[6]
		self.PitIsR = values[7]
		self.HitIsR = values[8]
		self.Result = values[9]
		self.HadSwing = values[10]
		self.HadContact = values[11]
		self.IsInPlay = values[12]
		self.RunValueInPlay = values[13]
		self.Vel = values[14]
		self.Extension = values[15]
		self.BreakInduced = values[16]
		self.BreakHorizontal = values[17]
		self.SpinRate = values[18]
		self.SpinAxis = values[19]
		self.ActiveSpin = values[20]
		self.ReleaseHeight = values[21]
		self.ReleaseHorizontal = values[22]
		self.VaaAboveAverage = values[23]
		self.HaaAboveAverage = values[24]
		self.PlateX = values[25]
		self.PlateY = values[26]

	NUM_ELEMENTS = 27

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.Year,self.PitcherId,self.PitchClass,self.CountBalls,self.CountStrike,self.PitIsR,self.HitIsR,self.Result,self.HadSwing,self.HadContact,self.IsInPlay,self.RunValueInPlay,self.Vel,self.Extension,self.BreakInduced,self.BreakHorizontal,self.SpinRate,self.SpinAxis,self.ActiveSpin,self.ReleaseHeight,self.ReleaseHorizontal,self.VaaAboveAverage,self.HaaAboveAverage,self.PlateX,self.PlateY)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchData']:
		items = cursor.execute("SELECT * FROM PitchData " + conditional, values).fetchall()
		return [DB_PitchData(i) for i in items]

class DB_PitchFlightpath:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.Year = values[2]
		self.PitcherId = values[3]
		self.PitchClass = values[4]
		self.BreakHoriz_05 = values[5]
		self.BreakVer_05 = values[6]
		self.BreakHoriz_10 = values[7]
		self.BreakVer_10 = values[8]
		self.BreakHoriz_15 = values[9]
		self.BreakVer_15 = values[10]
		self.BreakHoriz_20 = values[11]
		self.BreakVer_20 = values[12]
		self.BreakHoriz_25 = values[13]
		self.BreakVer_25 = values[14]
		self.HAA = values[15]
		self.VAA = values[16]
		self.TrackingError = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.Year,self.PitcherId,self.PitchClass,self.BreakHoriz_05,self.BreakVer_05,self.BreakHoriz_10,self.BreakVer_10,self.BreakHoriz_15,self.BreakVer_15,self.BreakHoriz_20,self.BreakVer_20,self.BreakHoriz_25,self.BreakVer_25,self.HAA,self.VAA,self.TrackingError)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchFlightpath']:
		items = cursor.execute("SELECT * FROM PitchFlightpath " + conditional, values).fetchall()
		return [DB_PitchFlightpath(i) for i in items]

class DB_PitchFlightpathGameDelta:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.PitcherId = values[2]
		self.PitchType = values[3]
		self.BreakHoriz_05Delta = values[4]
		self.BreakVer_05Delta = values[5]
		self.BreakHoriz_10Delta = values[6]
		self.BreakVer_10Delta = values[7]
		self.BreakHoriz_15Delta = values[8]
		self.BreakVer_15Delta = values[9]
		self.BreakHoriz_20Delta = values[10]
		self.BreakVer_20Delta = values[11]
		self.BreakHoriz_25Delta = values[12]
		self.BreakVer_25Delta = values[13]

	NUM_ELEMENTS = 14

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.PitcherId,self.PitchType,self.BreakHoriz_05Delta,self.BreakVer_05Delta,self.BreakHoriz_10Delta,self.BreakVer_10Delta,self.BreakHoriz_15Delta,self.BreakVer_15Delta,self.BreakHoriz_20Delta,self.BreakVer_20Delta,self.BreakHoriz_25Delta,self.BreakVer_25Delta)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchFlightpathGameDelta']:
		items = cursor.execute("SELECT * FROM PitchFlightpathGameDelta " + conditional, values).fetchall()
		return [DB_PitchFlightpathGameDelta(i) for i in items]


##############################################################################################
