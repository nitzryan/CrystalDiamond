import sqlite3

class DB_PitchData:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.Year = values[2]
		self.LevelId = values[3]
		self.PitcherId = values[4]
		self.PitchClass = values[5]
		self.CountBalls = values[6]
		self.CountStrike = values[7]
		self.PitIsR = values[8]
		self.HitIsR = values[9]
		self.Result = values[10]
		self.HadSwing = values[11]
		self.HadContact = values[12]
		self.IsInPlay = values[13]
		self.RunValueInPlay = values[14]
		self.Vel = values[15]
		self.Extension = values[16]
		self.BreakInduced = values[17]
		self.BreakHorizontal = values[18]
		self.SpinRate = values[19]
		self.SpinAxis = values[20]
		self.ActiveSpin = values[21]
		self.VaaAboveAverage = values[22]
		self.HaaAboveAverage = values[23]
		self.PlateX = values[24]
		self.PlateZ = values[25]
		self.ZoneTop = values[26]
		self.ZoneBot = values[27]

	NUM_ELEMENTS = 28

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.Year,self.LevelId,self.PitcherId,self.PitchClass,self.CountBalls,self.CountStrike,self.PitIsR,self.HitIsR,self.Result,self.HadSwing,self.HadContact,self.IsInPlay,self.RunValueInPlay,self.Vel,self.Extension,self.BreakInduced,self.BreakHorizontal,self.SpinRate,self.SpinAxis,self.ActiveSpin,self.VaaAboveAverage,self.HaaAboveAverage,self.PlateX,self.PlateZ,self.ZoneTop,self.ZoneBot)
                        
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
		self.PitchType = values[4]
		self.PitchClass = values[5]
		self.PitIsR = values[6]
		self.BreakHoriz_05 = values[7]
		self.BreakVer_05 = values[8]
		self.BreakHoriz_10 = values[9]
		self.BreakVer_10 = values[10]
		self.BreakHoriz_15 = values[11]
		self.BreakVer_15 = values[12]
		self.BreakHoriz_20 = values[13]
		self.BreakVer_20 = values[14]
		self.BreakHoriz_25 = values[15]
		self.BreakVer_25 = values[16]
		self.HAA = values[17]
		self.VAA = values[18]
		self.HB = values[19]
		self.IVB = values[20]
		self.VB = values[21]
		self.Vel = values[22]
		self.TrackingError = values[23]
		self.PlateX = values[24]
		self.PlateZ = values[25]

	NUM_ELEMENTS = 26

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.Year,self.PitcherId,self.PitchType,self.PitchClass,self.PitIsR,self.BreakHoriz_05,self.BreakVer_05,self.BreakHoriz_10,self.BreakVer_10,self.BreakHoriz_15,self.BreakVer_15,self.BreakHoriz_20,self.BreakVer_20,self.BreakHoriz_25,self.BreakVer_25,self.HAA,self.VAA,self.HB,self.IVB,self.VB,self.Vel,self.TrackingError,self.PlateX,self.PlateZ)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchFlightpath']:
		items = cursor.execute("SELECT * FROM PitchFlightpath " + conditional, values).fetchall()
		return [DB_PitchFlightpath(i) for i in items]

class DB_PitchFlightpathGameDelta:
	def __init__(self, values : tuple[any]):
		self.GameId = values[0]
		self.PitchId = values[1]
		self.PitcherId = values[2]
		self.FastballPitchType = values[3]
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
		self.BreakHoriz_Delta = values[14]
		self.BreakVert_Delta = values[15]
		self.BreakIVB_Delta = values[16]
		self.Vel_Delta = values[17]

	NUM_ELEMENTS = 18

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.GameId,self.PitchId,self.PitcherId,self.FastballPitchType,self.BreakHoriz_05Delta,self.BreakVer_05Delta,self.BreakHoriz_10Delta,self.BreakVer_10Delta,self.BreakHoriz_15Delta,self.BreakVer_15Delta,self.BreakHoriz_20Delta,self.BreakVer_20Delta,self.BreakHoriz_25Delta,self.BreakVer_25Delta,self.BreakHoriz_Delta,self.BreakVert_Delta,self.BreakIVB_Delta,self.Vel_Delta)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list['DB_PitchFlightpathGameDelta']:
		items = cursor.execute("SELECT * FROM PitchFlightpathGameDelta " + conditional, values).fetchall()
		return [DB_PitchFlightpathGameDelta(i) for i in items]


##############################################################################################
