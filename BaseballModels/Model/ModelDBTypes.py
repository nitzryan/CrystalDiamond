from __future__ import annotations
import sqlite3

class DB_Output_PlayerWarAggregation:
	__slots__ = ('mlbId','ModelId','isHitter','year','month','war0','war1','war2','war3','war4','war5','war6','war')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_PlayerWarAggregation]:
		items = cursor.execute("SELECT * FROM Output_PlayerWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerWarAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_PlayerWarAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_PlayerWarAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_PlayerWarAggregation) -> DB_Output_PlayerWarAggregation:
		if isinstance(other, DB_Output_PlayerWarAggregation):
			self.war0 += other.war0
			self.war1 += other.war1
			self.war2 += other.war2
			self.war3 += other.war3
			self.war4 += other.war4
			self.war5 += other.war5
			self.war6 += other.war6
			self.war += other.war

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_PlayerWarAggregation:
		self.war0 *= m
		self.war1 *= m
		self.war2 *= m
		self.war3 *= m
		self.war4 *= m
		self.war5 *= m
		self.war6 *= m
		self.war *= m

		return self

##############################################################################################
class DB_WarBucketAverages:
	__slots__ = ('isHitter','war1','war2','war3','war4','war5','war6')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_WarBucketAverages]:
		items = cursor.execute("SELECT * FROM WarBucketAverages " + conditional, values).fetchall()
		return [DB_WarBucketAverages(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_WarBucketAverages]) -> None:
		cursor.executemany("INSERT INTO WarBucketAverages VALUES(?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_WarBucketAverages) -> DB_WarBucketAverages:
		if isinstance(other, DB_WarBucketAverages):
			self.war1 += other.war1
			self.war2 += other.war2
			self.war3 += other.war3
			self.war4 += other.war4
			self.war5 += other.war5
			self.war6 += other.war6

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_WarBucketAverages:
		self.war1 *= m
		self.war2 *= m
		self.war3 *= m
		self.war4 *= m
		self.war5 *= m
		self.war6 *= m

		return self

##############################################################################################
class DB_Output_PlayerHighestLevelAggregation:
	__slots__ = ('mlbId','ModelId','isHitter','year','month','DSL','CPX','A_LOW','A','A_HIGH','AA','AAA','MLB')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_PlayerHighestLevelAggregation]:
		items = cursor.execute("SELECT * FROM Output_PlayerHighestLevelAggregation " + conditional, values).fetchall()
		return [DB_Output_PlayerHighestLevelAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_PlayerHighestLevelAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_PlayerHighestLevelAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_PlayerHighestLevelAggregation) -> DB_Output_PlayerHighestLevelAggregation:
		if isinstance(other, DB_Output_PlayerHighestLevelAggregation):
			self.DSL += other.DSL
			self.CPX += other.CPX
			self.A_LOW += other.A_LOW
			self.A += other.A
			self.A_HIGH += other.A_HIGH
			self.AA += other.AA
			self.AAA += other.AAA
			self.MLB += other.MLB

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_PlayerHighestLevelAggregation:
		self.DSL *= m
		self.CPX *= m
		self.A_LOW *= m
		self.A *= m
		self.A_HIGH *= m
		self.AA *= m
		self.AAA *= m
		self.MLB *= m

		return self

##############################################################################################
class DB_Output_HitterStatsAggregation:
	__slots__ = ('MlbId','ModelId','Year','Month','LevelId','Pa','Hit1B','Hit2B','Hit3B','HitHR','BB','HBP','K','SB','CS','BSR','DRAA','ParkRunFactor','PercC','Perc1B','Perc2B','Perc3B','PercSS','PercLF','PercCF','PercRF','PercDH')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_HitterStatsAggregation]:
		items = cursor.execute("SELECT * FROM Output_HitterStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterStatsAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_HitterStatsAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_HitterStatsAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_HitterStatsAggregation) -> DB_Output_HitterStatsAggregation:
		if isinstance(other, DB_Output_HitterStatsAggregation):
			self.Pa += other.Pa
			self.Hit1B += other.Hit1B
			self.Hit2B += other.Hit2B
			self.Hit3B += other.Hit3B
			self.HitHR += other.HitHR
			self.BB += other.BB
			self.HBP += other.HBP
			self.K += other.K
			self.SB += other.SB
			self.CS += other.CS
			self.BSR += other.BSR
			self.DRAA += other.DRAA
			self.ParkRunFactor += other.ParkRunFactor
			self.PercC += other.PercC
			self.Perc1B += other.Perc1B
			self.Perc2B += other.Perc2B
			self.Perc3B += other.Perc3B
			self.PercSS += other.PercSS
			self.PercLF += other.PercLF
			self.PercCF += other.PercCF
			self.PercRF += other.PercRF
			self.PercDH += other.PercDH

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_HitterStatsAggregation:
		self.Pa *= m
		self.Hit1B *= m
		self.Hit2B *= m
		self.Hit3B *= m
		self.HitHR *= m
		self.BB *= m
		self.HBP *= m
		self.K *= m
		self.SB *= m
		self.CS *= m
		self.BSR *= m
		self.DRAA *= m
		self.ParkRunFactor *= m
		self.PercC *= m
		self.Perc1B *= m
		self.Perc2B *= m
		self.Perc3B *= m
		self.PercSS *= m
		self.PercLF *= m
		self.PercCF *= m
		self.PercRF *= m
		self.PercDH *= m

		return self

##############################################################################################
class DB_Output_PitcherStatsAggregation:
	__slots__ = ('mlbId','ModelId','Year','Month','levelId','Outs_SP','Outs_RP','GS','GR','ERA','FIP','HR','BB','HBP','K','ParkRunFactor','SP_Perc','RP_Perc')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_PitcherStatsAggregation]:
		items = cursor.execute("SELECT * FROM Output_PitcherStatsAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherStatsAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_PitcherStatsAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_PitcherStatsAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_PitcherStatsAggregation) -> DB_Output_PitcherStatsAggregation:
		if isinstance(other, DB_Output_PitcherStatsAggregation):
			self.Outs_SP += other.Outs_SP
			self.Outs_RP += other.Outs_RP
			self.GS += other.GS
			self.GR += other.GR
			self.ERA += other.ERA
			self.FIP += other.FIP
			self.HR += other.HR
			self.BB += other.BB
			self.HBP += other.HBP
			self.K += other.K
			self.ParkRunFactor += other.ParkRunFactor
			self.SP_Perc += other.SP_Perc
			self.RP_Perc += other.RP_Perc

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_PitcherStatsAggregation:
		self.Outs_SP *= m
		self.Outs_RP *= m
		self.GS *= m
		self.GR *= m
		self.ERA *= m
		self.FIP *= m
		self.HR *= m
		self.BB *= m
		self.HBP *= m
		self.K *= m
		self.ParkRunFactor *= m
		self.SP_Perc *= m
		self.RP_Perc *= m

		return self

##############################################################################################
class DB_Output_College_HitterAggregation:
	__slots__ = ('tbcId','ModelId','year','draft0','draft1','draft2','draft3','draft4','draft5','draft6','draft','war0','war1','war2','war3','war4','war5','war6','war','off0','off1','off2','off3','off4','off5','off6','offNone','def0','def1','def2','def3','def4','def5','def6','defNone','pa0','pa1','pa2','pa3','pa4','pa5','pa6','ProbC','Prob1B','Prob2B','Prob3B','ProbSS','ProbLF','ProbCF','ProbRF','ProbDH')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_College_HitterAggregation]:
		items = cursor.execute("SELECT * FROM Output_College_HitterAggregation " + conditional, values).fetchall()
		return [DB_Output_College_HitterAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_College_HitterAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_College_HitterAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_College_HitterAggregation) -> DB_Output_College_HitterAggregation:
		if isinstance(other, DB_Output_College_HitterAggregation):
			self.draft0 += other.draft0
			self.draft1 += other.draft1
			self.draft2 += other.draft2
			self.draft3 += other.draft3
			self.draft4 += other.draft4
			self.draft5 += other.draft5
			self.draft6 += other.draft6
			self.draft += other.draft
			self.war0 += other.war0
			self.war1 += other.war1
			self.war2 += other.war2
			self.war3 += other.war3
			self.war4 += other.war4
			self.war5 += other.war5
			self.war6 += other.war6
			self.war += other.war
			self.off0 += other.off0
			self.off1 += other.off1
			self.off2 += other.off2
			self.off3 += other.off3
			self.off4 += other.off4
			self.off5 += other.off5
			self.off6 += other.off6
			self.offNone += other.offNone
			self.def0 += other.def0
			self.def1 += other.def1
			self.def2 += other.def2
			self.def3 += other.def3
			self.def4 += other.def4
			self.def5 += other.def5
			self.def6 += other.def6
			self.defNone += other.defNone
			self.pa0 += other.pa0
			self.pa1 += other.pa1
			self.pa2 += other.pa2
			self.pa3 += other.pa3
			self.pa4 += other.pa4
			self.pa5 += other.pa5
			self.pa6 += other.pa6
			self.ProbC += other.ProbC
			self.Prob1B += other.Prob1B
			self.Prob2B += other.Prob2B
			self.Prob3B += other.Prob3B
			self.ProbSS += other.ProbSS
			self.ProbLF += other.ProbLF
			self.ProbCF += other.ProbCF
			self.ProbRF += other.ProbRF
			self.ProbDH += other.ProbDH

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_College_HitterAggregation:
		self.draft0 *= m
		self.draft1 *= m
		self.draft2 *= m
		self.draft3 *= m
		self.draft4 *= m
		self.draft5 *= m
		self.draft6 *= m
		self.draft *= m
		self.war0 *= m
		self.war1 *= m
		self.war2 *= m
		self.war3 *= m
		self.war4 *= m
		self.war5 *= m
		self.war6 *= m
		self.war *= m
		self.off0 *= m
		self.off1 *= m
		self.off2 *= m
		self.off3 *= m
		self.off4 *= m
		self.off5 *= m
		self.off6 *= m
		self.offNone *= m
		self.def0 *= m
		self.def1 *= m
		self.def2 *= m
		self.def3 *= m
		self.def4 *= m
		self.def5 *= m
		self.def6 *= m
		self.defNone *= m
		self.pa0 *= m
		self.pa1 *= m
		self.pa2 *= m
		self.pa3 *= m
		self.pa4 *= m
		self.pa5 *= m
		self.pa6 *= m
		self.ProbC *= m
		self.Prob1B *= m
		self.Prob2B *= m
		self.Prob3B *= m
		self.ProbSS *= m
		self.ProbLF *= m
		self.ProbCF *= m
		self.ProbRF *= m
		self.ProbDH *= m

		return self

##############################################################################################
class DB_Output_College_PitcherAggregation:
	__slots__ = ('tbcId','ModelId','year','draft0','draft1','draft2','draft3','draft4','draft5','draft6','draft','war0','war1','war2','war3','war4','war5','war6','war','ProbSP','ProbRP')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_College_PitcherAggregation]:
		items = cursor.execute("SELECT * FROM Output_College_PitcherAggregation " + conditional, values).fetchall()
		return [DB_Output_College_PitcherAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_College_PitcherAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_College_PitcherAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_College_PitcherAggregation) -> DB_Output_College_PitcherAggregation:
		if isinstance(other, DB_Output_College_PitcherAggregation):
			self.draft0 += other.draft0
			self.draft1 += other.draft1
			self.draft2 += other.draft2
			self.draft3 += other.draft3
			self.draft4 += other.draft4
			self.draft5 += other.draft5
			self.draft6 += other.draft6
			self.draft += other.draft
			self.war0 += other.war0
			self.war1 += other.war1
			self.war2 += other.war2
			self.war3 += other.war3
			self.war4 += other.war4
			self.war5 += other.war5
			self.war6 += other.war6
			self.war += other.war
			self.ProbSP += other.ProbSP
			self.ProbRP += other.ProbRP

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_College_PitcherAggregation:
		self.draft0 *= m
		self.draft1 *= m
		self.draft2 *= m
		self.draft3 *= m
		self.draft4 *= m
		self.draft5 *= m
		self.draft6 *= m
		self.draft *= m
		self.war0 *= m
		self.war1 *= m
		self.war2 *= m
		self.war3 *= m
		self.war4 *= m
		self.war5 *= m
		self.war6 *= m
		self.war *= m
		self.ProbSP *= m
		self.ProbRP *= m

		return self

##############################################################################################
class DB_Model_TrainingHistory:
	__slots__ = ('ModelName','IsHitter','TestLoss','TestLossCollege','ModelRun')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Model_TrainingHistory]:
		items = cursor.execute("SELECT * FROM Model_TrainingHistory " + conditional, values).fetchall()
		return [DB_Model_TrainingHistory(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Model_TrainingHistory]) -> None:
		cursor.executemany("INSERT INTO Model_TrainingHistory VALUES(?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Model_TrainingHistory) -> DB_Model_TrainingHistory:
		if isinstance(other, DB_Model_TrainingHistory):
			self.TestLoss += other.TestLoss
			self.TestLossCollege += other.TestLossCollege

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Model_TrainingHistory:
		self.TestLoss *= m
		self.TestLossCollege *= m

		return self

##############################################################################################
class DB_PlayersInTrainingData:
	__slots__ = ('mlbId','tbcId','modelId','modelRun','isHitter','isTrain')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_PlayersInTrainingData]:
		items = cursor.execute("SELECT * FROM PlayersInTrainingData " + conditional, values).fetchall()
		return [DB_PlayersInTrainingData(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_PlayersInTrainingData]) -> None:
		cursor.executemany("INSERT INTO PlayersInTrainingData VALUES(?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_PlayersInTrainingData) -> DB_PlayersInTrainingData:
		if isinstance(other, DB_PlayersInTrainingData):
			raise Exception('Type invalid for addition')
		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_PlayersInTrainingData:
		raise Exception('Type invalid for multiplication')
		return self

##############################################################################################
class DB_ModelId:
	__slots__ = ('id','modelName')

	def __init__(self, values : tuple[any]):
		self.id = values[0]
		self.modelName = values[1]

	NUM_ELEMENTS = 2

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.id,self.modelName)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_ModelId]:
		items = cursor.execute("SELECT * FROM ModelId " + conditional, values).fetchall()
		return [DB_ModelId(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_ModelId]) -> None:
		cursor.executemany("INSERT INTO ModelId VALUES(?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_ModelId) -> DB_ModelId:
		if isinstance(other, DB_ModelId):
			raise Exception('Type invalid for addition')
		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_ModelId:
		raise Exception('Type invalid for multiplication')
		return self

##############################################################################################
class DB_Output_HitterMlbWarAggregation:
	__slots__ = ('mlbId','ModelId','year','month','war_Yr0_Bkt0','war_Yr0_Bkt1','war_Yr0_Bkt2','war_Yr0_Bkt3','war_Yr0_Bkt4','war_Yr0_Bkt5','war_Yr0_Bkt6','war_Yr0_Bkt7','war0','pa_Yr0_Bkt0','pa_Yr0_Bkt1','pa_Yr0_Bkt2','pa_Yr0_Bkt3','pa_Yr0_Bkt4','pa_Yr0_Bkt5','pa_Yr0_Bkt6','pa0','war_Yr1_Bkt0','war_Yr1_Bkt1','war_Yr1_Bkt2','war_Yr1_Bkt3','war_Yr1_Bkt4','war_Yr1_Bkt5','war_Yr1_Bkt6','war_Yr1_Bkt7','war1','pa_Yr1_Bkt0','pa_Yr1_Bkt1','pa_Yr1_Bkt2','pa_Yr1_Bkt3','pa_Yr1_Bkt4','pa_Yr1_Bkt5','pa_Yr1_Bkt6','pa1','war_Yr2_Bkt0','war_Yr2_Bkt1','war_Yr2_Bkt2','war_Yr2_Bkt3','war_Yr2_Bkt4','war_Yr2_Bkt5','war_Yr2_Bkt6','war_Yr2_Bkt7','war2','pa_Yr2_Bkt0','pa_Yr2_Bkt1','pa_Yr2_Bkt2','pa_Yr2_Bkt3','pa_Yr2_Bkt4','pa_Yr2_Bkt5','pa_Yr2_Bkt6','pa2','war_Yr3_Bkt0','war_Yr3_Bkt1','war_Yr3_Bkt2','war_Yr3_Bkt3','war_Yr3_Bkt4','war_Yr3_Bkt5','war_Yr3_Bkt6','war_Yr3_Bkt7','war3','pa_Yr3_Bkt0','pa_Yr3_Bkt1','pa_Yr3_Bkt2','pa_Yr3_Bkt3','pa_Yr3_Bkt4','pa_Yr3_Bkt5','pa_Yr3_Bkt6','pa3','war_Yr4_Bkt0','war_Yr4_Bkt1','war_Yr4_Bkt2','war_Yr4_Bkt3','war_Yr4_Bkt4','war_Yr4_Bkt5','war_Yr4_Bkt6','war_Yr4_Bkt7','war4','pa_Yr4_Bkt0','pa_Yr4_Bkt1','pa_Yr4_Bkt2','pa_Yr4_Bkt3','pa_Yr4_Bkt4','pa_Yr4_Bkt5','pa_Yr4_Bkt6','pa4','war_Yr5_Bkt0','war_Yr5_Bkt1','war_Yr5_Bkt2','war_Yr5_Bkt3','war_Yr5_Bkt4','war_Yr5_Bkt5','war_Yr5_Bkt6','war_Yr5_Bkt7','war5','pa_Yr5_Bkt0','pa_Yr5_Bkt1','pa_Yr5_Bkt2','pa_Yr5_Bkt3','pa_Yr5_Bkt4','pa_Yr5_Bkt5','pa_Yr5_Bkt6','pa5','war_Yr6_Bkt0','war_Yr6_Bkt1','war_Yr6_Bkt2','war_Yr6_Bkt3','war_Yr6_Bkt4','war_Yr6_Bkt5','war_Yr6_Bkt6','war_Yr6_Bkt7','war6','pa_Yr6_Bkt0','pa_Yr6_Bkt1','pa_Yr6_Bkt2','pa_Yr6_Bkt3','pa_Yr6_Bkt4','pa_Yr6_Bkt5','pa_Yr6_Bkt6','pa6')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_HitterMlbWarAggregation]:
		items = cursor.execute("SELECT * FROM Output_HitterMlbWarAggregation " + conditional, values).fetchall()
		return [DB_Output_HitterMlbWarAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_HitterMlbWarAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_HitterMlbWarAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_HitterMlbWarAggregation) -> DB_Output_HitterMlbWarAggregation:
		if isinstance(other, DB_Output_HitterMlbWarAggregation):
			self.war_Yr0_Bkt0 += other.war_Yr0_Bkt0
			self.war_Yr0_Bkt1 += other.war_Yr0_Bkt1
			self.war_Yr0_Bkt2 += other.war_Yr0_Bkt2
			self.war_Yr0_Bkt3 += other.war_Yr0_Bkt3
			self.war_Yr0_Bkt4 += other.war_Yr0_Bkt4
			self.war_Yr0_Bkt5 += other.war_Yr0_Bkt5
			self.war_Yr0_Bkt6 += other.war_Yr0_Bkt6
			self.war_Yr0_Bkt7 += other.war_Yr0_Bkt7
			self.war0 += other.war0
			self.pa_Yr0_Bkt0 += other.pa_Yr0_Bkt0
			self.pa_Yr0_Bkt1 += other.pa_Yr0_Bkt1
			self.pa_Yr0_Bkt2 += other.pa_Yr0_Bkt2
			self.pa_Yr0_Bkt3 += other.pa_Yr0_Bkt3
			self.pa_Yr0_Bkt4 += other.pa_Yr0_Bkt4
			self.pa_Yr0_Bkt5 += other.pa_Yr0_Bkt5
			self.pa_Yr0_Bkt6 += other.pa_Yr0_Bkt6
			self.pa0 += other.pa0
			self.war_Yr1_Bkt0 += other.war_Yr1_Bkt0
			self.war_Yr1_Bkt1 += other.war_Yr1_Bkt1
			self.war_Yr1_Bkt2 += other.war_Yr1_Bkt2
			self.war_Yr1_Bkt3 += other.war_Yr1_Bkt3
			self.war_Yr1_Bkt4 += other.war_Yr1_Bkt4
			self.war_Yr1_Bkt5 += other.war_Yr1_Bkt5
			self.war_Yr1_Bkt6 += other.war_Yr1_Bkt6
			self.war_Yr1_Bkt7 += other.war_Yr1_Bkt7
			self.war1 += other.war1
			self.pa_Yr1_Bkt0 += other.pa_Yr1_Bkt0
			self.pa_Yr1_Bkt1 += other.pa_Yr1_Bkt1
			self.pa_Yr1_Bkt2 += other.pa_Yr1_Bkt2
			self.pa_Yr1_Bkt3 += other.pa_Yr1_Bkt3
			self.pa_Yr1_Bkt4 += other.pa_Yr1_Bkt4
			self.pa_Yr1_Bkt5 += other.pa_Yr1_Bkt5
			self.pa_Yr1_Bkt6 += other.pa_Yr1_Bkt6
			self.pa1 += other.pa1
			self.war_Yr2_Bkt0 += other.war_Yr2_Bkt0
			self.war_Yr2_Bkt1 += other.war_Yr2_Bkt1
			self.war_Yr2_Bkt2 += other.war_Yr2_Bkt2
			self.war_Yr2_Bkt3 += other.war_Yr2_Bkt3
			self.war_Yr2_Bkt4 += other.war_Yr2_Bkt4
			self.war_Yr2_Bkt5 += other.war_Yr2_Bkt5
			self.war_Yr2_Bkt6 += other.war_Yr2_Bkt6
			self.war_Yr2_Bkt7 += other.war_Yr2_Bkt7
			self.war2 += other.war2
			self.pa_Yr2_Bkt0 += other.pa_Yr2_Bkt0
			self.pa_Yr2_Bkt1 += other.pa_Yr2_Bkt1
			self.pa_Yr2_Bkt2 += other.pa_Yr2_Bkt2
			self.pa_Yr2_Bkt3 += other.pa_Yr2_Bkt3
			self.pa_Yr2_Bkt4 += other.pa_Yr2_Bkt4
			self.pa_Yr2_Bkt5 += other.pa_Yr2_Bkt5
			self.pa_Yr2_Bkt6 += other.pa_Yr2_Bkt6
			self.pa2 += other.pa2
			self.war_Yr3_Bkt0 += other.war_Yr3_Bkt0
			self.war_Yr3_Bkt1 += other.war_Yr3_Bkt1
			self.war_Yr3_Bkt2 += other.war_Yr3_Bkt2
			self.war_Yr3_Bkt3 += other.war_Yr3_Bkt3
			self.war_Yr3_Bkt4 += other.war_Yr3_Bkt4
			self.war_Yr3_Bkt5 += other.war_Yr3_Bkt5
			self.war_Yr3_Bkt6 += other.war_Yr3_Bkt6
			self.war_Yr3_Bkt7 += other.war_Yr3_Bkt7
			self.war3 += other.war3
			self.pa_Yr3_Bkt0 += other.pa_Yr3_Bkt0
			self.pa_Yr3_Bkt1 += other.pa_Yr3_Bkt1
			self.pa_Yr3_Bkt2 += other.pa_Yr3_Bkt2
			self.pa_Yr3_Bkt3 += other.pa_Yr3_Bkt3
			self.pa_Yr3_Bkt4 += other.pa_Yr3_Bkt4
			self.pa_Yr3_Bkt5 += other.pa_Yr3_Bkt5
			self.pa_Yr3_Bkt6 += other.pa_Yr3_Bkt6
			self.pa3 += other.pa3
			self.war_Yr4_Bkt0 += other.war_Yr4_Bkt0
			self.war_Yr4_Bkt1 += other.war_Yr4_Bkt1
			self.war_Yr4_Bkt2 += other.war_Yr4_Bkt2
			self.war_Yr4_Bkt3 += other.war_Yr4_Bkt3
			self.war_Yr4_Bkt4 += other.war_Yr4_Bkt4
			self.war_Yr4_Bkt5 += other.war_Yr4_Bkt5
			self.war_Yr4_Bkt6 += other.war_Yr4_Bkt6
			self.war_Yr4_Bkt7 += other.war_Yr4_Bkt7
			self.war4 += other.war4
			self.pa_Yr4_Bkt0 += other.pa_Yr4_Bkt0
			self.pa_Yr4_Bkt1 += other.pa_Yr4_Bkt1
			self.pa_Yr4_Bkt2 += other.pa_Yr4_Bkt2
			self.pa_Yr4_Bkt3 += other.pa_Yr4_Bkt3
			self.pa_Yr4_Bkt4 += other.pa_Yr4_Bkt4
			self.pa_Yr4_Bkt5 += other.pa_Yr4_Bkt5
			self.pa_Yr4_Bkt6 += other.pa_Yr4_Bkt6
			self.pa4 += other.pa4
			self.war_Yr5_Bkt0 += other.war_Yr5_Bkt0
			self.war_Yr5_Bkt1 += other.war_Yr5_Bkt1
			self.war_Yr5_Bkt2 += other.war_Yr5_Bkt2
			self.war_Yr5_Bkt3 += other.war_Yr5_Bkt3
			self.war_Yr5_Bkt4 += other.war_Yr5_Bkt4
			self.war_Yr5_Bkt5 += other.war_Yr5_Bkt5
			self.war_Yr5_Bkt6 += other.war_Yr5_Bkt6
			self.war_Yr5_Bkt7 += other.war_Yr5_Bkt7
			self.war5 += other.war5
			self.pa_Yr5_Bkt0 += other.pa_Yr5_Bkt0
			self.pa_Yr5_Bkt1 += other.pa_Yr5_Bkt1
			self.pa_Yr5_Bkt2 += other.pa_Yr5_Bkt2
			self.pa_Yr5_Bkt3 += other.pa_Yr5_Bkt3
			self.pa_Yr5_Bkt4 += other.pa_Yr5_Bkt4
			self.pa_Yr5_Bkt5 += other.pa_Yr5_Bkt5
			self.pa_Yr5_Bkt6 += other.pa_Yr5_Bkt6
			self.pa5 += other.pa5
			self.war_Yr6_Bkt0 += other.war_Yr6_Bkt0
			self.war_Yr6_Bkt1 += other.war_Yr6_Bkt1
			self.war_Yr6_Bkt2 += other.war_Yr6_Bkt2
			self.war_Yr6_Bkt3 += other.war_Yr6_Bkt3
			self.war_Yr6_Bkt4 += other.war_Yr6_Bkt4
			self.war_Yr6_Bkt5 += other.war_Yr6_Bkt5
			self.war_Yr6_Bkt6 += other.war_Yr6_Bkt6
			self.war_Yr6_Bkt7 += other.war_Yr6_Bkt7
			self.war6 += other.war6
			self.pa_Yr6_Bkt0 += other.pa_Yr6_Bkt0
			self.pa_Yr6_Bkt1 += other.pa_Yr6_Bkt1
			self.pa_Yr6_Bkt2 += other.pa_Yr6_Bkt2
			self.pa_Yr6_Bkt3 += other.pa_Yr6_Bkt3
			self.pa_Yr6_Bkt4 += other.pa_Yr6_Bkt4
			self.pa_Yr6_Bkt5 += other.pa_Yr6_Bkt5
			self.pa_Yr6_Bkt6 += other.pa_Yr6_Bkt6
			self.pa6 += other.pa6

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_HitterMlbWarAggregation:
		self.war_Yr0_Bkt0 *= m
		self.war_Yr0_Bkt1 *= m
		self.war_Yr0_Bkt2 *= m
		self.war_Yr0_Bkt3 *= m
		self.war_Yr0_Bkt4 *= m
		self.war_Yr0_Bkt5 *= m
		self.war_Yr0_Bkt6 *= m
		self.war_Yr0_Bkt7 *= m
		self.war0 *= m
		self.pa_Yr0_Bkt0 *= m
		self.pa_Yr0_Bkt1 *= m
		self.pa_Yr0_Bkt2 *= m
		self.pa_Yr0_Bkt3 *= m
		self.pa_Yr0_Bkt4 *= m
		self.pa_Yr0_Bkt5 *= m
		self.pa_Yr0_Bkt6 *= m
		self.pa0 *= m
		self.war_Yr1_Bkt0 *= m
		self.war_Yr1_Bkt1 *= m
		self.war_Yr1_Bkt2 *= m
		self.war_Yr1_Bkt3 *= m
		self.war_Yr1_Bkt4 *= m
		self.war_Yr1_Bkt5 *= m
		self.war_Yr1_Bkt6 *= m
		self.war_Yr1_Bkt7 *= m
		self.war1 *= m
		self.pa_Yr1_Bkt0 *= m
		self.pa_Yr1_Bkt1 *= m
		self.pa_Yr1_Bkt2 *= m
		self.pa_Yr1_Bkt3 *= m
		self.pa_Yr1_Bkt4 *= m
		self.pa_Yr1_Bkt5 *= m
		self.pa_Yr1_Bkt6 *= m
		self.pa1 *= m
		self.war_Yr2_Bkt0 *= m
		self.war_Yr2_Bkt1 *= m
		self.war_Yr2_Bkt2 *= m
		self.war_Yr2_Bkt3 *= m
		self.war_Yr2_Bkt4 *= m
		self.war_Yr2_Bkt5 *= m
		self.war_Yr2_Bkt6 *= m
		self.war_Yr2_Bkt7 *= m
		self.war2 *= m
		self.pa_Yr2_Bkt0 *= m
		self.pa_Yr2_Bkt1 *= m
		self.pa_Yr2_Bkt2 *= m
		self.pa_Yr2_Bkt3 *= m
		self.pa_Yr2_Bkt4 *= m
		self.pa_Yr2_Bkt5 *= m
		self.pa_Yr2_Bkt6 *= m
		self.pa2 *= m
		self.war_Yr3_Bkt0 *= m
		self.war_Yr3_Bkt1 *= m
		self.war_Yr3_Bkt2 *= m
		self.war_Yr3_Bkt3 *= m
		self.war_Yr3_Bkt4 *= m
		self.war_Yr3_Bkt5 *= m
		self.war_Yr3_Bkt6 *= m
		self.war_Yr3_Bkt7 *= m
		self.war3 *= m
		self.pa_Yr3_Bkt0 *= m
		self.pa_Yr3_Bkt1 *= m
		self.pa_Yr3_Bkt2 *= m
		self.pa_Yr3_Bkt3 *= m
		self.pa_Yr3_Bkt4 *= m
		self.pa_Yr3_Bkt5 *= m
		self.pa_Yr3_Bkt6 *= m
		self.pa3 *= m
		self.war_Yr4_Bkt0 *= m
		self.war_Yr4_Bkt1 *= m
		self.war_Yr4_Bkt2 *= m
		self.war_Yr4_Bkt3 *= m
		self.war_Yr4_Bkt4 *= m
		self.war_Yr4_Bkt5 *= m
		self.war_Yr4_Bkt6 *= m
		self.war_Yr4_Bkt7 *= m
		self.war4 *= m
		self.pa_Yr4_Bkt0 *= m
		self.pa_Yr4_Bkt1 *= m
		self.pa_Yr4_Bkt2 *= m
		self.pa_Yr4_Bkt3 *= m
		self.pa_Yr4_Bkt4 *= m
		self.pa_Yr4_Bkt5 *= m
		self.pa_Yr4_Bkt6 *= m
		self.pa4 *= m
		self.war_Yr5_Bkt0 *= m
		self.war_Yr5_Bkt1 *= m
		self.war_Yr5_Bkt2 *= m
		self.war_Yr5_Bkt3 *= m
		self.war_Yr5_Bkt4 *= m
		self.war_Yr5_Bkt5 *= m
		self.war_Yr5_Bkt6 *= m
		self.war_Yr5_Bkt7 *= m
		self.war5 *= m
		self.pa_Yr5_Bkt0 *= m
		self.pa_Yr5_Bkt1 *= m
		self.pa_Yr5_Bkt2 *= m
		self.pa_Yr5_Bkt3 *= m
		self.pa_Yr5_Bkt4 *= m
		self.pa_Yr5_Bkt5 *= m
		self.pa_Yr5_Bkt6 *= m
		self.pa5 *= m
		self.war_Yr6_Bkt0 *= m
		self.war_Yr6_Bkt1 *= m
		self.war_Yr6_Bkt2 *= m
		self.war_Yr6_Bkt3 *= m
		self.war_Yr6_Bkt4 *= m
		self.war_Yr6_Bkt5 *= m
		self.war_Yr6_Bkt6 *= m
		self.war_Yr6_Bkt7 *= m
		self.war6 *= m
		self.pa_Yr6_Bkt0 *= m
		self.pa_Yr6_Bkt1 *= m
		self.pa_Yr6_Bkt2 *= m
		self.pa_Yr6_Bkt3 *= m
		self.pa_Yr6_Bkt4 *= m
		self.pa_Yr6_Bkt5 *= m
		self.pa_Yr6_Bkt6 *= m
		self.pa6 *= m

		return self

##############################################################################################
class DB_Output_PitcherMlbWarAggregation:
	__slots__ = ('mlbId','ModelId','year','month','outsSP_Yr0_Bkt0','outsSP_Yr0_Bkt1','outsSP_Yr0_Bkt2','outsSP_Yr0_Bkt3','outsSP_Yr0_Bkt4','outsSP_Yr0_Bkt5','outsSP_Yr0_Bkt6','outsSP0','warSP_Yr0_Bkt0','warSP_Yr0_Bkt1','warSP_Yr0_Bkt2','warSP_Yr0_Bkt3','warSP_Yr0_Bkt4','warSP_Yr0_Bkt5','warSP_Yr0_Bkt6','warSP_Yr0_Bkt7','warSP0','outsRP_Yr0_Bkt0','outsRP_Yr0_Bkt1','outsRP_Yr0_Bkt2','outsRP_Yr0_Bkt3','outsRP_Yr0_Bkt4','outsRP_Yr0_Bkt5','outsRP_Yr0_Bkt6','outsRP0','warRP_Yr0_Bkt0','warRP_Yr0_Bkt1','warRP_Yr0_Bkt2','warRP_Yr0_Bkt3','warRP_Yr0_Bkt4','warRP_Yr0_Bkt5','warRP_Yr0_Bkt6','warRP_Yr0_Bkt7','warRP0','outsSP_Yr1_Bkt0','outsSP_Yr1_Bkt1','outsSP_Yr1_Bkt2','outsSP_Yr1_Bkt3','outsSP_Yr1_Bkt4','outsSP_Yr1_Bkt5','outsSP_Yr1_Bkt6','outsSP1','warSP_Yr1_Bkt0','warSP_Yr1_Bkt1','warSP_Yr1_Bkt2','warSP_Yr1_Bkt3','warSP_Yr1_Bkt4','warSP_Yr1_Bkt5','warSP_Yr1_Bkt6','warSP_Yr1_Bkt7','warSP1','outsRP_Yr1_Bkt0','outsRP_Yr1_Bkt1','outsRP_Yr1_Bkt2','outsRP_Yr1_Bkt3','outsRP_Yr1_Bkt4','outsRP_Yr1_Bkt5','outsRP_Yr1_Bkt6','outsRP1','warRP_Yr1_Bkt0','warRP_Yr1_Bkt1','warRP_Yr1_Bkt2','warRP_Yr1_Bkt3','warRP_Yr1_Bkt4','warRP_Yr1_Bkt5','warRP_Yr1_Bkt6','warRP_Yr1_Bkt7','warRP1','outsSP_Yr2_Bkt0','outsSP_Yr2_Bkt1','outsSP_Yr2_Bkt2','outsSP_Yr2_Bkt3','outsSP_Yr2_Bkt4','outsSP_Yr2_Bkt5','outsSP_Yr2_Bkt6','outsSP2','warSP_Yr2_Bkt0','warSP_Yr2_Bkt1','warSP_Yr2_Bkt2','warSP_Yr2_Bkt3','warSP_Yr2_Bkt4','warSP_Yr2_Bkt5','warSP_Yr2_Bkt6','warSP_Yr2_Bkt7','warSP2','outsRP_Yr2_Bkt0','outsRP_Yr2_Bkt1','outsRP_Yr2_Bkt2','outsRP_Yr2_Bkt3','outsRP_Yr2_Bkt4','outsRP_Yr2_Bkt5','outsRP_Yr2_Bkt6','outsRP2','warRP_Yr2_Bkt0','warRP_Yr2_Bkt1','warRP_Yr2_Bkt2','warRP_Yr2_Bkt3','warRP_Yr2_Bkt4','warRP_Yr2_Bkt5','warRP_Yr2_Bkt6','warRP_Yr2_Bkt7','warRP2','outsSP_Yr3_Bkt0','outsSP_Yr3_Bkt1','outsSP_Yr3_Bkt2','outsSP_Yr3_Bkt3','outsSP_Yr3_Bkt4','outsSP_Yr3_Bkt5','outsSP_Yr3_Bkt6','outsSP3','warSP_Yr3_Bkt0','warSP_Yr3_Bkt1','warSP_Yr3_Bkt2','warSP_Yr3_Bkt3','warSP_Yr3_Bkt4','warSP_Yr3_Bkt5','warSP_Yr3_Bkt6','warSP_Yr3_Bkt7','warSP3','outsRP_Yr3_Bkt0','outsRP_Yr3_Bkt1','outsRP_Yr3_Bkt2','outsRP_Yr3_Bkt3','outsRP_Yr3_Bkt4','outsRP_Yr3_Bkt5','outsRP_Yr3_Bkt6','outsRP3','warRP_Yr3_Bkt0','warRP_Yr3_Bkt1','warRP_Yr3_Bkt2','warRP_Yr3_Bkt3','warRP_Yr3_Bkt4','warRP_Yr3_Bkt5','warRP_Yr3_Bkt6','warRP_Yr3_Bkt7','warRP3','outsSP_Yr4_Bkt0','outsSP_Yr4_Bkt1','outsSP_Yr4_Bkt2','outsSP_Yr4_Bkt3','outsSP_Yr4_Bkt4','outsSP_Yr4_Bkt5','outsSP_Yr4_Bkt6','outsSP4','warSP_Yr4_Bkt0','warSP_Yr4_Bkt1','warSP_Yr4_Bkt2','warSP_Yr4_Bkt3','warSP_Yr4_Bkt4','warSP_Yr4_Bkt5','warSP_Yr4_Bkt6','warSP_Yr4_Bkt7','warSP4','outsRP_Yr4_Bkt0','outsRP_Yr4_Bkt1','outsRP_Yr4_Bkt2','outsRP_Yr4_Bkt3','outsRP_Yr4_Bkt4','outsRP_Yr4_Bkt5','outsRP_Yr4_Bkt6','outsRP4','warRP_Yr4_Bkt0','warRP_Yr4_Bkt1','warRP_Yr4_Bkt2','warRP_Yr4_Bkt3','warRP_Yr4_Bkt4','warRP_Yr4_Bkt5','warRP_Yr4_Bkt6','warRP_Yr4_Bkt7','warRP4','outsSP_Yr5_Bkt0','outsSP_Yr5_Bkt1','outsSP_Yr5_Bkt2','outsSP_Yr5_Bkt3','outsSP_Yr5_Bkt4','outsSP_Yr5_Bkt5','outsSP_Yr5_Bkt6','outsSP5','warSP_Yr5_Bkt0','warSP_Yr5_Bkt1','warSP_Yr5_Bkt2','warSP_Yr5_Bkt3','warSP_Yr5_Bkt4','warSP_Yr5_Bkt5','warSP_Yr5_Bkt6','warSP_Yr5_Bkt7','warSP5','outsRP_Yr5_Bkt0','outsRP_Yr5_Bkt1','outsRP_Yr5_Bkt2','outsRP_Yr5_Bkt3','outsRP_Yr5_Bkt4','outsRP_Yr5_Bkt5','outsRP_Yr5_Bkt6','outsRP5','warRP_Yr5_Bkt0','warRP_Yr5_Bkt1','warRP_Yr5_Bkt2','warRP_Yr5_Bkt3','warRP_Yr5_Bkt4','warRP_Yr5_Bkt5','warRP_Yr5_Bkt6','warRP_Yr5_Bkt7','warRP5','outsSP_Yr6_Bkt0','outsSP_Yr6_Bkt1','outsSP_Yr6_Bkt2','outsSP_Yr6_Bkt3','outsSP_Yr6_Bkt4','outsSP_Yr6_Bkt5','outsSP_Yr6_Bkt6','outsSP6','warSP_Yr6_Bkt0','warSP_Yr6_Bkt1','warSP_Yr6_Bkt2','warSP_Yr6_Bkt3','warSP_Yr6_Bkt4','warSP_Yr6_Bkt5','warSP_Yr6_Bkt6','warSP_Yr6_Bkt7','warSP6','outsRP_Yr6_Bkt0','outsRP_Yr6_Bkt1','outsRP_Yr6_Bkt2','outsRP_Yr6_Bkt3','outsRP_Yr6_Bkt4','outsRP_Yr6_Bkt5','outsRP_Yr6_Bkt6','outsRP6','warRP_Yr6_Bkt0','warRP_Yr6_Bkt1','warRP_Yr6_Bkt2','warRP_Yr6_Bkt3','warRP_Yr6_Bkt4','warRP_Yr6_Bkt5','warRP_Yr6_Bkt6','warRP_Yr6_Bkt7','warRP6')

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
		self.outsSP0 = values[11]
		self.warSP_Yr0_Bkt0 = values[12]
		self.warSP_Yr0_Bkt1 = values[13]
		self.warSP_Yr0_Bkt2 = values[14]
		self.warSP_Yr0_Bkt3 = values[15]
		self.warSP_Yr0_Bkt4 = values[16]
		self.warSP_Yr0_Bkt5 = values[17]
		self.warSP_Yr0_Bkt6 = values[18]
		self.warSP_Yr0_Bkt7 = values[19]
		self.warSP0 = values[20]
		self.outsRP_Yr0_Bkt0 = values[21]
		self.outsRP_Yr0_Bkt1 = values[22]
		self.outsRP_Yr0_Bkt2 = values[23]
		self.outsRP_Yr0_Bkt3 = values[24]
		self.outsRP_Yr0_Bkt4 = values[25]
		self.outsRP_Yr0_Bkt5 = values[26]
		self.outsRP_Yr0_Bkt6 = values[27]
		self.outsRP0 = values[28]
		self.warRP_Yr0_Bkt0 = values[29]
		self.warRP_Yr0_Bkt1 = values[30]
		self.warRP_Yr0_Bkt2 = values[31]
		self.warRP_Yr0_Bkt3 = values[32]
		self.warRP_Yr0_Bkt4 = values[33]
		self.warRP_Yr0_Bkt5 = values[34]
		self.warRP_Yr0_Bkt6 = values[35]
		self.warRP_Yr0_Bkt7 = values[36]
		self.warRP0 = values[37]
		self.outsSP_Yr1_Bkt0 = values[38]
		self.outsSP_Yr1_Bkt1 = values[39]
		self.outsSP_Yr1_Bkt2 = values[40]
		self.outsSP_Yr1_Bkt3 = values[41]
		self.outsSP_Yr1_Bkt4 = values[42]
		self.outsSP_Yr1_Bkt5 = values[43]
		self.outsSP_Yr1_Bkt6 = values[44]
		self.outsSP1 = values[45]
		self.warSP_Yr1_Bkt0 = values[46]
		self.warSP_Yr1_Bkt1 = values[47]
		self.warSP_Yr1_Bkt2 = values[48]
		self.warSP_Yr1_Bkt3 = values[49]
		self.warSP_Yr1_Bkt4 = values[50]
		self.warSP_Yr1_Bkt5 = values[51]
		self.warSP_Yr1_Bkt6 = values[52]
		self.warSP_Yr1_Bkt7 = values[53]
		self.warSP1 = values[54]
		self.outsRP_Yr1_Bkt0 = values[55]
		self.outsRP_Yr1_Bkt1 = values[56]
		self.outsRP_Yr1_Bkt2 = values[57]
		self.outsRP_Yr1_Bkt3 = values[58]
		self.outsRP_Yr1_Bkt4 = values[59]
		self.outsRP_Yr1_Bkt5 = values[60]
		self.outsRP_Yr1_Bkt6 = values[61]
		self.outsRP1 = values[62]
		self.warRP_Yr1_Bkt0 = values[63]
		self.warRP_Yr1_Bkt1 = values[64]
		self.warRP_Yr1_Bkt2 = values[65]
		self.warRP_Yr1_Bkt3 = values[66]
		self.warRP_Yr1_Bkt4 = values[67]
		self.warRP_Yr1_Bkt5 = values[68]
		self.warRP_Yr1_Bkt6 = values[69]
		self.warRP_Yr1_Bkt7 = values[70]
		self.warRP1 = values[71]
		self.outsSP_Yr2_Bkt0 = values[72]
		self.outsSP_Yr2_Bkt1 = values[73]
		self.outsSP_Yr2_Bkt2 = values[74]
		self.outsSP_Yr2_Bkt3 = values[75]
		self.outsSP_Yr2_Bkt4 = values[76]
		self.outsSP_Yr2_Bkt5 = values[77]
		self.outsSP_Yr2_Bkt6 = values[78]
		self.outsSP2 = values[79]
		self.warSP_Yr2_Bkt0 = values[80]
		self.warSP_Yr2_Bkt1 = values[81]
		self.warSP_Yr2_Bkt2 = values[82]
		self.warSP_Yr2_Bkt3 = values[83]
		self.warSP_Yr2_Bkt4 = values[84]
		self.warSP_Yr2_Bkt5 = values[85]
		self.warSP_Yr2_Bkt6 = values[86]
		self.warSP_Yr2_Bkt7 = values[87]
		self.warSP2 = values[88]
		self.outsRP_Yr2_Bkt0 = values[89]
		self.outsRP_Yr2_Bkt1 = values[90]
		self.outsRP_Yr2_Bkt2 = values[91]
		self.outsRP_Yr2_Bkt3 = values[92]
		self.outsRP_Yr2_Bkt4 = values[93]
		self.outsRP_Yr2_Bkt5 = values[94]
		self.outsRP_Yr2_Bkt6 = values[95]
		self.outsRP2 = values[96]
		self.warRP_Yr2_Bkt0 = values[97]
		self.warRP_Yr2_Bkt1 = values[98]
		self.warRP_Yr2_Bkt2 = values[99]
		self.warRP_Yr2_Bkt3 = values[100]
		self.warRP_Yr2_Bkt4 = values[101]
		self.warRP_Yr2_Bkt5 = values[102]
		self.warRP_Yr2_Bkt6 = values[103]
		self.warRP_Yr2_Bkt7 = values[104]
		self.warRP2 = values[105]
		self.outsSP_Yr3_Bkt0 = values[106]
		self.outsSP_Yr3_Bkt1 = values[107]
		self.outsSP_Yr3_Bkt2 = values[108]
		self.outsSP_Yr3_Bkt3 = values[109]
		self.outsSP_Yr3_Bkt4 = values[110]
		self.outsSP_Yr3_Bkt5 = values[111]
		self.outsSP_Yr3_Bkt6 = values[112]
		self.outsSP3 = values[113]
		self.warSP_Yr3_Bkt0 = values[114]
		self.warSP_Yr3_Bkt1 = values[115]
		self.warSP_Yr3_Bkt2 = values[116]
		self.warSP_Yr3_Bkt3 = values[117]
		self.warSP_Yr3_Bkt4 = values[118]
		self.warSP_Yr3_Bkt5 = values[119]
		self.warSP_Yr3_Bkt6 = values[120]
		self.warSP_Yr3_Bkt7 = values[121]
		self.warSP3 = values[122]
		self.outsRP_Yr3_Bkt0 = values[123]
		self.outsRP_Yr3_Bkt1 = values[124]
		self.outsRP_Yr3_Bkt2 = values[125]
		self.outsRP_Yr3_Bkt3 = values[126]
		self.outsRP_Yr3_Bkt4 = values[127]
		self.outsRP_Yr3_Bkt5 = values[128]
		self.outsRP_Yr3_Bkt6 = values[129]
		self.outsRP3 = values[130]
		self.warRP_Yr3_Bkt0 = values[131]
		self.warRP_Yr3_Bkt1 = values[132]
		self.warRP_Yr3_Bkt2 = values[133]
		self.warRP_Yr3_Bkt3 = values[134]
		self.warRP_Yr3_Bkt4 = values[135]
		self.warRP_Yr3_Bkt5 = values[136]
		self.warRP_Yr3_Bkt6 = values[137]
		self.warRP_Yr3_Bkt7 = values[138]
		self.warRP3 = values[139]
		self.outsSP_Yr4_Bkt0 = values[140]
		self.outsSP_Yr4_Bkt1 = values[141]
		self.outsSP_Yr4_Bkt2 = values[142]
		self.outsSP_Yr4_Bkt3 = values[143]
		self.outsSP_Yr4_Bkt4 = values[144]
		self.outsSP_Yr4_Bkt5 = values[145]
		self.outsSP_Yr4_Bkt6 = values[146]
		self.outsSP4 = values[147]
		self.warSP_Yr4_Bkt0 = values[148]
		self.warSP_Yr4_Bkt1 = values[149]
		self.warSP_Yr4_Bkt2 = values[150]
		self.warSP_Yr4_Bkt3 = values[151]
		self.warSP_Yr4_Bkt4 = values[152]
		self.warSP_Yr4_Bkt5 = values[153]
		self.warSP_Yr4_Bkt6 = values[154]
		self.warSP_Yr4_Bkt7 = values[155]
		self.warSP4 = values[156]
		self.outsRP_Yr4_Bkt0 = values[157]
		self.outsRP_Yr4_Bkt1 = values[158]
		self.outsRP_Yr4_Bkt2 = values[159]
		self.outsRP_Yr4_Bkt3 = values[160]
		self.outsRP_Yr4_Bkt4 = values[161]
		self.outsRP_Yr4_Bkt5 = values[162]
		self.outsRP_Yr4_Bkt6 = values[163]
		self.outsRP4 = values[164]
		self.warRP_Yr4_Bkt0 = values[165]
		self.warRP_Yr4_Bkt1 = values[166]
		self.warRP_Yr4_Bkt2 = values[167]
		self.warRP_Yr4_Bkt3 = values[168]
		self.warRP_Yr4_Bkt4 = values[169]
		self.warRP_Yr4_Bkt5 = values[170]
		self.warRP_Yr4_Bkt6 = values[171]
		self.warRP_Yr4_Bkt7 = values[172]
		self.warRP4 = values[173]
		self.outsSP_Yr5_Bkt0 = values[174]
		self.outsSP_Yr5_Bkt1 = values[175]
		self.outsSP_Yr5_Bkt2 = values[176]
		self.outsSP_Yr5_Bkt3 = values[177]
		self.outsSP_Yr5_Bkt4 = values[178]
		self.outsSP_Yr5_Bkt5 = values[179]
		self.outsSP_Yr5_Bkt6 = values[180]
		self.outsSP5 = values[181]
		self.warSP_Yr5_Bkt0 = values[182]
		self.warSP_Yr5_Bkt1 = values[183]
		self.warSP_Yr5_Bkt2 = values[184]
		self.warSP_Yr5_Bkt3 = values[185]
		self.warSP_Yr5_Bkt4 = values[186]
		self.warSP_Yr5_Bkt5 = values[187]
		self.warSP_Yr5_Bkt6 = values[188]
		self.warSP_Yr5_Bkt7 = values[189]
		self.warSP5 = values[190]
		self.outsRP_Yr5_Bkt0 = values[191]
		self.outsRP_Yr5_Bkt1 = values[192]
		self.outsRP_Yr5_Bkt2 = values[193]
		self.outsRP_Yr5_Bkt3 = values[194]
		self.outsRP_Yr5_Bkt4 = values[195]
		self.outsRP_Yr5_Bkt5 = values[196]
		self.outsRP_Yr5_Bkt6 = values[197]
		self.outsRP5 = values[198]
		self.warRP_Yr5_Bkt0 = values[199]
		self.warRP_Yr5_Bkt1 = values[200]
		self.warRP_Yr5_Bkt2 = values[201]
		self.warRP_Yr5_Bkt3 = values[202]
		self.warRP_Yr5_Bkt4 = values[203]
		self.warRP_Yr5_Bkt5 = values[204]
		self.warRP_Yr5_Bkt6 = values[205]
		self.warRP_Yr5_Bkt7 = values[206]
		self.warRP5 = values[207]
		self.outsSP_Yr6_Bkt0 = values[208]
		self.outsSP_Yr6_Bkt1 = values[209]
		self.outsSP_Yr6_Bkt2 = values[210]
		self.outsSP_Yr6_Bkt3 = values[211]
		self.outsSP_Yr6_Bkt4 = values[212]
		self.outsSP_Yr6_Bkt5 = values[213]
		self.outsSP_Yr6_Bkt6 = values[214]
		self.outsSP6 = values[215]
		self.warSP_Yr6_Bkt0 = values[216]
		self.warSP_Yr6_Bkt1 = values[217]
		self.warSP_Yr6_Bkt2 = values[218]
		self.warSP_Yr6_Bkt3 = values[219]
		self.warSP_Yr6_Bkt4 = values[220]
		self.warSP_Yr6_Bkt5 = values[221]
		self.warSP_Yr6_Bkt6 = values[222]
		self.warSP_Yr6_Bkt7 = values[223]
		self.warSP6 = values[224]
		self.outsRP_Yr6_Bkt0 = values[225]
		self.outsRP_Yr6_Bkt1 = values[226]
		self.outsRP_Yr6_Bkt2 = values[227]
		self.outsRP_Yr6_Bkt3 = values[228]
		self.outsRP_Yr6_Bkt4 = values[229]
		self.outsRP_Yr6_Bkt5 = values[230]
		self.outsRP_Yr6_Bkt6 = values[231]
		self.outsRP6 = values[232]
		self.warRP_Yr6_Bkt0 = values[233]
		self.warRP_Yr6_Bkt1 = values[234]
		self.warRP_Yr6_Bkt2 = values[235]
		self.warRP_Yr6_Bkt3 = values[236]
		self.warRP_Yr6_Bkt4 = values[237]
		self.warRP_Yr6_Bkt5 = values[238]
		self.warRP_Yr6_Bkt6 = values[239]
		self.warRP_Yr6_Bkt7 = values[240]
		self.warRP6 = values[241]

	NUM_ELEMENTS = 242

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.mlbId,self.ModelId,self.year,self.month,self.outsSP_Yr0_Bkt0,self.outsSP_Yr0_Bkt1,self.outsSP_Yr0_Bkt2,self.outsSP_Yr0_Bkt3,self.outsSP_Yr0_Bkt4,self.outsSP_Yr0_Bkt5,self.outsSP_Yr0_Bkt6,self.outsSP0,self.warSP_Yr0_Bkt0,self.warSP_Yr0_Bkt1,self.warSP_Yr0_Bkt2,self.warSP_Yr0_Bkt3,self.warSP_Yr0_Bkt4,self.warSP_Yr0_Bkt5,self.warSP_Yr0_Bkt6,self.warSP_Yr0_Bkt7,self.warSP0,self.outsRP_Yr0_Bkt0,self.outsRP_Yr0_Bkt1,self.outsRP_Yr0_Bkt2,self.outsRP_Yr0_Bkt3,self.outsRP_Yr0_Bkt4,self.outsRP_Yr0_Bkt5,self.outsRP_Yr0_Bkt6,self.outsRP0,self.warRP_Yr0_Bkt0,self.warRP_Yr0_Bkt1,self.warRP_Yr0_Bkt2,self.warRP_Yr0_Bkt3,self.warRP_Yr0_Bkt4,self.warRP_Yr0_Bkt5,self.warRP_Yr0_Bkt6,self.warRP_Yr0_Bkt7,self.warRP0,self.outsSP_Yr1_Bkt0,self.outsSP_Yr1_Bkt1,self.outsSP_Yr1_Bkt2,self.outsSP_Yr1_Bkt3,self.outsSP_Yr1_Bkt4,self.outsSP_Yr1_Bkt5,self.outsSP_Yr1_Bkt6,self.outsSP1,self.warSP_Yr1_Bkt0,self.warSP_Yr1_Bkt1,self.warSP_Yr1_Bkt2,self.warSP_Yr1_Bkt3,self.warSP_Yr1_Bkt4,self.warSP_Yr1_Bkt5,self.warSP_Yr1_Bkt6,self.warSP_Yr1_Bkt7,self.warSP1,self.outsRP_Yr1_Bkt0,self.outsRP_Yr1_Bkt1,self.outsRP_Yr1_Bkt2,self.outsRP_Yr1_Bkt3,self.outsRP_Yr1_Bkt4,self.outsRP_Yr1_Bkt5,self.outsRP_Yr1_Bkt6,self.outsRP1,self.warRP_Yr1_Bkt0,self.warRP_Yr1_Bkt1,self.warRP_Yr1_Bkt2,self.warRP_Yr1_Bkt3,self.warRP_Yr1_Bkt4,self.warRP_Yr1_Bkt5,self.warRP_Yr1_Bkt6,self.warRP_Yr1_Bkt7,self.warRP1,self.outsSP_Yr2_Bkt0,self.outsSP_Yr2_Bkt1,self.outsSP_Yr2_Bkt2,self.outsSP_Yr2_Bkt3,self.outsSP_Yr2_Bkt4,self.outsSP_Yr2_Bkt5,self.outsSP_Yr2_Bkt6,self.outsSP2,self.warSP_Yr2_Bkt0,self.warSP_Yr2_Bkt1,self.warSP_Yr2_Bkt2,self.warSP_Yr2_Bkt3,self.warSP_Yr2_Bkt4,self.warSP_Yr2_Bkt5,self.warSP_Yr2_Bkt6,self.warSP_Yr2_Bkt7,self.warSP2,self.outsRP_Yr2_Bkt0,self.outsRP_Yr2_Bkt1,self.outsRP_Yr2_Bkt2,self.outsRP_Yr2_Bkt3,self.outsRP_Yr2_Bkt4,self.outsRP_Yr2_Bkt5,self.outsRP_Yr2_Bkt6,self.outsRP2,self.warRP_Yr2_Bkt0,self.warRP_Yr2_Bkt1,self.warRP_Yr2_Bkt2,self.warRP_Yr2_Bkt3,self.warRP_Yr2_Bkt4,self.warRP_Yr2_Bkt5,self.warRP_Yr2_Bkt6,self.warRP_Yr2_Bkt7,self.warRP2,self.outsSP_Yr3_Bkt0,self.outsSP_Yr3_Bkt1,self.outsSP_Yr3_Bkt2,self.outsSP_Yr3_Bkt3,self.outsSP_Yr3_Bkt4,self.outsSP_Yr3_Bkt5,self.outsSP_Yr3_Bkt6,self.outsSP3,self.warSP_Yr3_Bkt0,self.warSP_Yr3_Bkt1,self.warSP_Yr3_Bkt2,self.warSP_Yr3_Bkt3,self.warSP_Yr3_Bkt4,self.warSP_Yr3_Bkt5,self.warSP_Yr3_Bkt6,self.warSP_Yr3_Bkt7,self.warSP3,self.outsRP_Yr3_Bkt0,self.outsRP_Yr3_Bkt1,self.outsRP_Yr3_Bkt2,self.outsRP_Yr3_Bkt3,self.outsRP_Yr3_Bkt4,self.outsRP_Yr3_Bkt5,self.outsRP_Yr3_Bkt6,self.outsRP3,self.warRP_Yr3_Bkt0,self.warRP_Yr3_Bkt1,self.warRP_Yr3_Bkt2,self.warRP_Yr3_Bkt3,self.warRP_Yr3_Bkt4,self.warRP_Yr3_Bkt5,self.warRP_Yr3_Bkt6,self.warRP_Yr3_Bkt7,self.warRP3,self.outsSP_Yr4_Bkt0,self.outsSP_Yr4_Bkt1,self.outsSP_Yr4_Bkt2,self.outsSP_Yr4_Bkt3,self.outsSP_Yr4_Bkt4,self.outsSP_Yr4_Bkt5,self.outsSP_Yr4_Bkt6,self.outsSP4,self.warSP_Yr4_Bkt0,self.warSP_Yr4_Bkt1,self.warSP_Yr4_Bkt2,self.warSP_Yr4_Bkt3,self.warSP_Yr4_Bkt4,self.warSP_Yr4_Bkt5,self.warSP_Yr4_Bkt6,self.warSP_Yr4_Bkt7,self.warSP4,self.outsRP_Yr4_Bkt0,self.outsRP_Yr4_Bkt1,self.outsRP_Yr4_Bkt2,self.outsRP_Yr4_Bkt3,self.outsRP_Yr4_Bkt4,self.outsRP_Yr4_Bkt5,self.outsRP_Yr4_Bkt6,self.outsRP4,self.warRP_Yr4_Bkt0,self.warRP_Yr4_Bkt1,self.warRP_Yr4_Bkt2,self.warRP_Yr4_Bkt3,self.warRP_Yr4_Bkt4,self.warRP_Yr4_Bkt5,self.warRP_Yr4_Bkt6,self.warRP_Yr4_Bkt7,self.warRP4,self.outsSP_Yr5_Bkt0,self.outsSP_Yr5_Bkt1,self.outsSP_Yr5_Bkt2,self.outsSP_Yr5_Bkt3,self.outsSP_Yr5_Bkt4,self.outsSP_Yr5_Bkt5,self.outsSP_Yr5_Bkt6,self.outsSP5,self.warSP_Yr5_Bkt0,self.warSP_Yr5_Bkt1,self.warSP_Yr5_Bkt2,self.warSP_Yr5_Bkt3,self.warSP_Yr5_Bkt4,self.warSP_Yr5_Bkt5,self.warSP_Yr5_Bkt6,self.warSP_Yr5_Bkt7,self.warSP5,self.outsRP_Yr5_Bkt0,self.outsRP_Yr5_Bkt1,self.outsRP_Yr5_Bkt2,self.outsRP_Yr5_Bkt3,self.outsRP_Yr5_Bkt4,self.outsRP_Yr5_Bkt5,self.outsRP_Yr5_Bkt6,self.outsRP5,self.warRP_Yr5_Bkt0,self.warRP_Yr5_Bkt1,self.warRP_Yr5_Bkt2,self.warRP_Yr5_Bkt3,self.warRP_Yr5_Bkt4,self.warRP_Yr5_Bkt5,self.warRP_Yr5_Bkt6,self.warRP_Yr5_Bkt7,self.warRP5,self.outsSP_Yr6_Bkt0,self.outsSP_Yr6_Bkt1,self.outsSP_Yr6_Bkt2,self.outsSP_Yr6_Bkt3,self.outsSP_Yr6_Bkt4,self.outsSP_Yr6_Bkt5,self.outsSP_Yr6_Bkt6,self.outsSP6,self.warSP_Yr6_Bkt0,self.warSP_Yr6_Bkt1,self.warSP_Yr6_Bkt2,self.warSP_Yr6_Bkt3,self.warSP_Yr6_Bkt4,self.warSP_Yr6_Bkt5,self.warSP_Yr6_Bkt6,self.warSP_Yr6_Bkt7,self.warSP6,self.outsRP_Yr6_Bkt0,self.outsRP_Yr6_Bkt1,self.outsRP_Yr6_Bkt2,self.outsRP_Yr6_Bkt3,self.outsRP_Yr6_Bkt4,self.outsRP_Yr6_Bkt5,self.outsRP_Yr6_Bkt6,self.outsRP6,self.warRP_Yr6_Bkt0,self.warRP_Yr6_Bkt1,self.warRP_Yr6_Bkt2,self.warRP_Yr6_Bkt3,self.warRP_Yr6_Bkt4,self.warRP_Yr6_Bkt5,self.warRP_Yr6_Bkt6,self.warRP_Yr6_Bkt7,self.warRP6)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_Output_PitcherMlbWarAggregation]:
		items = cursor.execute("SELECT * FROM Output_PitcherMlbWarAggregation " + conditional, values).fetchall()
		return [DB_Output_PitcherMlbWarAggregation(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_Output_PitcherMlbWarAggregation]) -> None:
		cursor.executemany("INSERT INTO Output_PitcherMlbWarAggregation VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_Output_PitcherMlbWarAggregation) -> DB_Output_PitcherMlbWarAggregation:
		if isinstance(other, DB_Output_PitcherMlbWarAggregation):
			self.outsSP_Yr0_Bkt0 += other.outsSP_Yr0_Bkt0
			self.outsSP_Yr0_Bkt1 += other.outsSP_Yr0_Bkt1
			self.outsSP_Yr0_Bkt2 += other.outsSP_Yr0_Bkt2
			self.outsSP_Yr0_Bkt3 += other.outsSP_Yr0_Bkt3
			self.outsSP_Yr0_Bkt4 += other.outsSP_Yr0_Bkt4
			self.outsSP_Yr0_Bkt5 += other.outsSP_Yr0_Bkt5
			self.outsSP_Yr0_Bkt6 += other.outsSP_Yr0_Bkt6
			self.outsSP0 += other.outsSP0
			self.warSP_Yr0_Bkt0 += other.warSP_Yr0_Bkt0
			self.warSP_Yr0_Bkt1 += other.warSP_Yr0_Bkt1
			self.warSP_Yr0_Bkt2 += other.warSP_Yr0_Bkt2
			self.warSP_Yr0_Bkt3 += other.warSP_Yr0_Bkt3
			self.warSP_Yr0_Bkt4 += other.warSP_Yr0_Bkt4
			self.warSP_Yr0_Bkt5 += other.warSP_Yr0_Bkt5
			self.warSP_Yr0_Bkt6 += other.warSP_Yr0_Bkt6
			self.warSP_Yr0_Bkt7 += other.warSP_Yr0_Bkt7
			self.warSP0 += other.warSP0
			self.outsRP_Yr0_Bkt0 += other.outsRP_Yr0_Bkt0
			self.outsRP_Yr0_Bkt1 += other.outsRP_Yr0_Bkt1
			self.outsRP_Yr0_Bkt2 += other.outsRP_Yr0_Bkt2
			self.outsRP_Yr0_Bkt3 += other.outsRP_Yr0_Bkt3
			self.outsRP_Yr0_Bkt4 += other.outsRP_Yr0_Bkt4
			self.outsRP_Yr0_Bkt5 += other.outsRP_Yr0_Bkt5
			self.outsRP_Yr0_Bkt6 += other.outsRP_Yr0_Bkt6
			self.outsRP0 += other.outsRP0
			self.warRP_Yr0_Bkt0 += other.warRP_Yr0_Bkt0
			self.warRP_Yr0_Bkt1 += other.warRP_Yr0_Bkt1
			self.warRP_Yr0_Bkt2 += other.warRP_Yr0_Bkt2
			self.warRP_Yr0_Bkt3 += other.warRP_Yr0_Bkt3
			self.warRP_Yr0_Bkt4 += other.warRP_Yr0_Bkt4
			self.warRP_Yr0_Bkt5 += other.warRP_Yr0_Bkt5
			self.warRP_Yr0_Bkt6 += other.warRP_Yr0_Bkt6
			self.warRP_Yr0_Bkt7 += other.warRP_Yr0_Bkt7
			self.warRP0 += other.warRP0
			self.outsSP_Yr1_Bkt0 += other.outsSP_Yr1_Bkt0
			self.outsSP_Yr1_Bkt1 += other.outsSP_Yr1_Bkt1
			self.outsSP_Yr1_Bkt2 += other.outsSP_Yr1_Bkt2
			self.outsSP_Yr1_Bkt3 += other.outsSP_Yr1_Bkt3
			self.outsSP_Yr1_Bkt4 += other.outsSP_Yr1_Bkt4
			self.outsSP_Yr1_Bkt5 += other.outsSP_Yr1_Bkt5
			self.outsSP_Yr1_Bkt6 += other.outsSP_Yr1_Bkt6
			self.outsSP1 += other.outsSP1
			self.warSP_Yr1_Bkt0 += other.warSP_Yr1_Bkt0
			self.warSP_Yr1_Bkt1 += other.warSP_Yr1_Bkt1
			self.warSP_Yr1_Bkt2 += other.warSP_Yr1_Bkt2
			self.warSP_Yr1_Bkt3 += other.warSP_Yr1_Bkt3
			self.warSP_Yr1_Bkt4 += other.warSP_Yr1_Bkt4
			self.warSP_Yr1_Bkt5 += other.warSP_Yr1_Bkt5
			self.warSP_Yr1_Bkt6 += other.warSP_Yr1_Bkt6
			self.warSP_Yr1_Bkt7 += other.warSP_Yr1_Bkt7
			self.warSP1 += other.warSP1
			self.outsRP_Yr1_Bkt0 += other.outsRP_Yr1_Bkt0
			self.outsRP_Yr1_Bkt1 += other.outsRP_Yr1_Bkt1
			self.outsRP_Yr1_Bkt2 += other.outsRP_Yr1_Bkt2
			self.outsRP_Yr1_Bkt3 += other.outsRP_Yr1_Bkt3
			self.outsRP_Yr1_Bkt4 += other.outsRP_Yr1_Bkt4
			self.outsRP_Yr1_Bkt5 += other.outsRP_Yr1_Bkt5
			self.outsRP_Yr1_Bkt6 += other.outsRP_Yr1_Bkt6
			self.outsRP1 += other.outsRP1
			self.warRP_Yr1_Bkt0 += other.warRP_Yr1_Bkt0
			self.warRP_Yr1_Bkt1 += other.warRP_Yr1_Bkt1
			self.warRP_Yr1_Bkt2 += other.warRP_Yr1_Bkt2
			self.warRP_Yr1_Bkt3 += other.warRP_Yr1_Bkt3
			self.warRP_Yr1_Bkt4 += other.warRP_Yr1_Bkt4
			self.warRP_Yr1_Bkt5 += other.warRP_Yr1_Bkt5
			self.warRP_Yr1_Bkt6 += other.warRP_Yr1_Bkt6
			self.warRP_Yr1_Bkt7 += other.warRP_Yr1_Bkt7
			self.warRP1 += other.warRP1
			self.outsSP_Yr2_Bkt0 += other.outsSP_Yr2_Bkt0
			self.outsSP_Yr2_Bkt1 += other.outsSP_Yr2_Bkt1
			self.outsSP_Yr2_Bkt2 += other.outsSP_Yr2_Bkt2
			self.outsSP_Yr2_Bkt3 += other.outsSP_Yr2_Bkt3
			self.outsSP_Yr2_Bkt4 += other.outsSP_Yr2_Bkt4
			self.outsSP_Yr2_Bkt5 += other.outsSP_Yr2_Bkt5
			self.outsSP_Yr2_Bkt6 += other.outsSP_Yr2_Bkt6
			self.outsSP2 += other.outsSP2
			self.warSP_Yr2_Bkt0 += other.warSP_Yr2_Bkt0
			self.warSP_Yr2_Bkt1 += other.warSP_Yr2_Bkt1
			self.warSP_Yr2_Bkt2 += other.warSP_Yr2_Bkt2
			self.warSP_Yr2_Bkt3 += other.warSP_Yr2_Bkt3
			self.warSP_Yr2_Bkt4 += other.warSP_Yr2_Bkt4
			self.warSP_Yr2_Bkt5 += other.warSP_Yr2_Bkt5
			self.warSP_Yr2_Bkt6 += other.warSP_Yr2_Bkt6
			self.warSP_Yr2_Bkt7 += other.warSP_Yr2_Bkt7
			self.warSP2 += other.warSP2
			self.outsRP_Yr2_Bkt0 += other.outsRP_Yr2_Bkt0
			self.outsRP_Yr2_Bkt1 += other.outsRP_Yr2_Bkt1
			self.outsRP_Yr2_Bkt2 += other.outsRP_Yr2_Bkt2
			self.outsRP_Yr2_Bkt3 += other.outsRP_Yr2_Bkt3
			self.outsRP_Yr2_Bkt4 += other.outsRP_Yr2_Bkt4
			self.outsRP_Yr2_Bkt5 += other.outsRP_Yr2_Bkt5
			self.outsRP_Yr2_Bkt6 += other.outsRP_Yr2_Bkt6
			self.outsRP2 += other.outsRP2
			self.warRP_Yr2_Bkt0 += other.warRP_Yr2_Bkt0
			self.warRP_Yr2_Bkt1 += other.warRP_Yr2_Bkt1
			self.warRP_Yr2_Bkt2 += other.warRP_Yr2_Bkt2
			self.warRP_Yr2_Bkt3 += other.warRP_Yr2_Bkt3
			self.warRP_Yr2_Bkt4 += other.warRP_Yr2_Bkt4
			self.warRP_Yr2_Bkt5 += other.warRP_Yr2_Bkt5
			self.warRP_Yr2_Bkt6 += other.warRP_Yr2_Bkt6
			self.warRP_Yr2_Bkt7 += other.warRP_Yr2_Bkt7
			self.warRP2 += other.warRP2
			self.outsSP_Yr3_Bkt0 += other.outsSP_Yr3_Bkt0
			self.outsSP_Yr3_Bkt1 += other.outsSP_Yr3_Bkt1
			self.outsSP_Yr3_Bkt2 += other.outsSP_Yr3_Bkt2
			self.outsSP_Yr3_Bkt3 += other.outsSP_Yr3_Bkt3
			self.outsSP_Yr3_Bkt4 += other.outsSP_Yr3_Bkt4
			self.outsSP_Yr3_Bkt5 += other.outsSP_Yr3_Bkt5
			self.outsSP_Yr3_Bkt6 += other.outsSP_Yr3_Bkt6
			self.outsSP3 += other.outsSP3
			self.warSP_Yr3_Bkt0 += other.warSP_Yr3_Bkt0
			self.warSP_Yr3_Bkt1 += other.warSP_Yr3_Bkt1
			self.warSP_Yr3_Bkt2 += other.warSP_Yr3_Bkt2
			self.warSP_Yr3_Bkt3 += other.warSP_Yr3_Bkt3
			self.warSP_Yr3_Bkt4 += other.warSP_Yr3_Bkt4
			self.warSP_Yr3_Bkt5 += other.warSP_Yr3_Bkt5
			self.warSP_Yr3_Bkt6 += other.warSP_Yr3_Bkt6
			self.warSP_Yr3_Bkt7 += other.warSP_Yr3_Bkt7
			self.warSP3 += other.warSP3
			self.outsRP_Yr3_Bkt0 += other.outsRP_Yr3_Bkt0
			self.outsRP_Yr3_Bkt1 += other.outsRP_Yr3_Bkt1
			self.outsRP_Yr3_Bkt2 += other.outsRP_Yr3_Bkt2
			self.outsRP_Yr3_Bkt3 += other.outsRP_Yr3_Bkt3
			self.outsRP_Yr3_Bkt4 += other.outsRP_Yr3_Bkt4
			self.outsRP_Yr3_Bkt5 += other.outsRP_Yr3_Bkt5
			self.outsRP_Yr3_Bkt6 += other.outsRP_Yr3_Bkt6
			self.outsRP3 += other.outsRP3
			self.warRP_Yr3_Bkt0 += other.warRP_Yr3_Bkt0
			self.warRP_Yr3_Bkt1 += other.warRP_Yr3_Bkt1
			self.warRP_Yr3_Bkt2 += other.warRP_Yr3_Bkt2
			self.warRP_Yr3_Bkt3 += other.warRP_Yr3_Bkt3
			self.warRP_Yr3_Bkt4 += other.warRP_Yr3_Bkt4
			self.warRP_Yr3_Bkt5 += other.warRP_Yr3_Bkt5
			self.warRP_Yr3_Bkt6 += other.warRP_Yr3_Bkt6
			self.warRP_Yr3_Bkt7 += other.warRP_Yr3_Bkt7
			self.warRP3 += other.warRP3
			self.outsSP_Yr4_Bkt0 += other.outsSP_Yr4_Bkt0
			self.outsSP_Yr4_Bkt1 += other.outsSP_Yr4_Bkt1
			self.outsSP_Yr4_Bkt2 += other.outsSP_Yr4_Bkt2
			self.outsSP_Yr4_Bkt3 += other.outsSP_Yr4_Bkt3
			self.outsSP_Yr4_Bkt4 += other.outsSP_Yr4_Bkt4
			self.outsSP_Yr4_Bkt5 += other.outsSP_Yr4_Bkt5
			self.outsSP_Yr4_Bkt6 += other.outsSP_Yr4_Bkt6
			self.outsSP4 += other.outsSP4
			self.warSP_Yr4_Bkt0 += other.warSP_Yr4_Bkt0
			self.warSP_Yr4_Bkt1 += other.warSP_Yr4_Bkt1
			self.warSP_Yr4_Bkt2 += other.warSP_Yr4_Bkt2
			self.warSP_Yr4_Bkt3 += other.warSP_Yr4_Bkt3
			self.warSP_Yr4_Bkt4 += other.warSP_Yr4_Bkt4
			self.warSP_Yr4_Bkt5 += other.warSP_Yr4_Bkt5
			self.warSP_Yr4_Bkt6 += other.warSP_Yr4_Bkt6
			self.warSP_Yr4_Bkt7 += other.warSP_Yr4_Bkt7
			self.warSP4 += other.warSP4
			self.outsRP_Yr4_Bkt0 += other.outsRP_Yr4_Bkt0
			self.outsRP_Yr4_Bkt1 += other.outsRP_Yr4_Bkt1
			self.outsRP_Yr4_Bkt2 += other.outsRP_Yr4_Bkt2
			self.outsRP_Yr4_Bkt3 += other.outsRP_Yr4_Bkt3
			self.outsRP_Yr4_Bkt4 += other.outsRP_Yr4_Bkt4
			self.outsRP_Yr4_Bkt5 += other.outsRP_Yr4_Bkt5
			self.outsRP_Yr4_Bkt6 += other.outsRP_Yr4_Bkt6
			self.outsRP4 += other.outsRP4
			self.warRP_Yr4_Bkt0 += other.warRP_Yr4_Bkt0
			self.warRP_Yr4_Bkt1 += other.warRP_Yr4_Bkt1
			self.warRP_Yr4_Bkt2 += other.warRP_Yr4_Bkt2
			self.warRP_Yr4_Bkt3 += other.warRP_Yr4_Bkt3
			self.warRP_Yr4_Bkt4 += other.warRP_Yr4_Bkt4
			self.warRP_Yr4_Bkt5 += other.warRP_Yr4_Bkt5
			self.warRP_Yr4_Bkt6 += other.warRP_Yr4_Bkt6
			self.warRP_Yr4_Bkt7 += other.warRP_Yr4_Bkt7
			self.warRP4 += other.warRP4
			self.outsSP_Yr5_Bkt0 += other.outsSP_Yr5_Bkt0
			self.outsSP_Yr5_Bkt1 += other.outsSP_Yr5_Bkt1
			self.outsSP_Yr5_Bkt2 += other.outsSP_Yr5_Bkt2
			self.outsSP_Yr5_Bkt3 += other.outsSP_Yr5_Bkt3
			self.outsSP_Yr5_Bkt4 += other.outsSP_Yr5_Bkt4
			self.outsSP_Yr5_Bkt5 += other.outsSP_Yr5_Bkt5
			self.outsSP_Yr5_Bkt6 += other.outsSP_Yr5_Bkt6
			self.outsSP5 += other.outsSP5
			self.warSP_Yr5_Bkt0 += other.warSP_Yr5_Bkt0
			self.warSP_Yr5_Bkt1 += other.warSP_Yr5_Bkt1
			self.warSP_Yr5_Bkt2 += other.warSP_Yr5_Bkt2
			self.warSP_Yr5_Bkt3 += other.warSP_Yr5_Bkt3
			self.warSP_Yr5_Bkt4 += other.warSP_Yr5_Bkt4
			self.warSP_Yr5_Bkt5 += other.warSP_Yr5_Bkt5
			self.warSP_Yr5_Bkt6 += other.warSP_Yr5_Bkt6
			self.warSP_Yr5_Bkt7 += other.warSP_Yr5_Bkt7
			self.warSP5 += other.warSP5
			self.outsRP_Yr5_Bkt0 += other.outsRP_Yr5_Bkt0
			self.outsRP_Yr5_Bkt1 += other.outsRP_Yr5_Bkt1
			self.outsRP_Yr5_Bkt2 += other.outsRP_Yr5_Bkt2
			self.outsRP_Yr5_Bkt3 += other.outsRP_Yr5_Bkt3
			self.outsRP_Yr5_Bkt4 += other.outsRP_Yr5_Bkt4
			self.outsRP_Yr5_Bkt5 += other.outsRP_Yr5_Bkt5
			self.outsRP_Yr5_Bkt6 += other.outsRP_Yr5_Bkt6
			self.outsRP5 += other.outsRP5
			self.warRP_Yr5_Bkt0 += other.warRP_Yr5_Bkt0
			self.warRP_Yr5_Bkt1 += other.warRP_Yr5_Bkt1
			self.warRP_Yr5_Bkt2 += other.warRP_Yr5_Bkt2
			self.warRP_Yr5_Bkt3 += other.warRP_Yr5_Bkt3
			self.warRP_Yr5_Bkt4 += other.warRP_Yr5_Bkt4
			self.warRP_Yr5_Bkt5 += other.warRP_Yr5_Bkt5
			self.warRP_Yr5_Bkt6 += other.warRP_Yr5_Bkt6
			self.warRP_Yr5_Bkt7 += other.warRP_Yr5_Bkt7
			self.warRP5 += other.warRP5
			self.outsSP_Yr6_Bkt0 += other.outsSP_Yr6_Bkt0
			self.outsSP_Yr6_Bkt1 += other.outsSP_Yr6_Bkt1
			self.outsSP_Yr6_Bkt2 += other.outsSP_Yr6_Bkt2
			self.outsSP_Yr6_Bkt3 += other.outsSP_Yr6_Bkt3
			self.outsSP_Yr6_Bkt4 += other.outsSP_Yr6_Bkt4
			self.outsSP_Yr6_Bkt5 += other.outsSP_Yr6_Bkt5
			self.outsSP_Yr6_Bkt6 += other.outsSP_Yr6_Bkt6
			self.outsSP6 += other.outsSP6
			self.warSP_Yr6_Bkt0 += other.warSP_Yr6_Bkt0
			self.warSP_Yr6_Bkt1 += other.warSP_Yr6_Bkt1
			self.warSP_Yr6_Bkt2 += other.warSP_Yr6_Bkt2
			self.warSP_Yr6_Bkt3 += other.warSP_Yr6_Bkt3
			self.warSP_Yr6_Bkt4 += other.warSP_Yr6_Bkt4
			self.warSP_Yr6_Bkt5 += other.warSP_Yr6_Bkt5
			self.warSP_Yr6_Bkt6 += other.warSP_Yr6_Bkt6
			self.warSP_Yr6_Bkt7 += other.warSP_Yr6_Bkt7
			self.warSP6 += other.warSP6
			self.outsRP_Yr6_Bkt0 += other.outsRP_Yr6_Bkt0
			self.outsRP_Yr6_Bkt1 += other.outsRP_Yr6_Bkt1
			self.outsRP_Yr6_Bkt2 += other.outsRP_Yr6_Bkt2
			self.outsRP_Yr6_Bkt3 += other.outsRP_Yr6_Bkt3
			self.outsRP_Yr6_Bkt4 += other.outsRP_Yr6_Bkt4
			self.outsRP_Yr6_Bkt5 += other.outsRP_Yr6_Bkt5
			self.outsRP_Yr6_Bkt6 += other.outsRP_Yr6_Bkt6
			self.outsRP6 += other.outsRP6
			self.warRP_Yr6_Bkt0 += other.warRP_Yr6_Bkt0
			self.warRP_Yr6_Bkt1 += other.warRP_Yr6_Bkt1
			self.warRP_Yr6_Bkt2 += other.warRP_Yr6_Bkt2
			self.warRP_Yr6_Bkt3 += other.warRP_Yr6_Bkt3
			self.warRP_Yr6_Bkt4 += other.warRP_Yr6_Bkt4
			self.warRP_Yr6_Bkt5 += other.warRP_Yr6_Bkt5
			self.warRP_Yr6_Bkt6 += other.warRP_Yr6_Bkt6
			self.warRP_Yr6_Bkt7 += other.warRP_Yr6_Bkt7
			self.warRP6 += other.warRP6

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_Output_PitcherMlbWarAggregation:
		self.outsSP_Yr0_Bkt0 *= m
		self.outsSP_Yr0_Bkt1 *= m
		self.outsSP_Yr0_Bkt2 *= m
		self.outsSP_Yr0_Bkt3 *= m
		self.outsSP_Yr0_Bkt4 *= m
		self.outsSP_Yr0_Bkt5 *= m
		self.outsSP_Yr0_Bkt6 *= m
		self.outsSP0 *= m
		self.warSP_Yr0_Bkt0 *= m
		self.warSP_Yr0_Bkt1 *= m
		self.warSP_Yr0_Bkt2 *= m
		self.warSP_Yr0_Bkt3 *= m
		self.warSP_Yr0_Bkt4 *= m
		self.warSP_Yr0_Bkt5 *= m
		self.warSP_Yr0_Bkt6 *= m
		self.warSP_Yr0_Bkt7 *= m
		self.warSP0 *= m
		self.outsRP_Yr0_Bkt0 *= m
		self.outsRP_Yr0_Bkt1 *= m
		self.outsRP_Yr0_Bkt2 *= m
		self.outsRP_Yr0_Bkt3 *= m
		self.outsRP_Yr0_Bkt4 *= m
		self.outsRP_Yr0_Bkt5 *= m
		self.outsRP_Yr0_Bkt6 *= m
		self.outsRP0 *= m
		self.warRP_Yr0_Bkt0 *= m
		self.warRP_Yr0_Bkt1 *= m
		self.warRP_Yr0_Bkt2 *= m
		self.warRP_Yr0_Bkt3 *= m
		self.warRP_Yr0_Bkt4 *= m
		self.warRP_Yr0_Bkt5 *= m
		self.warRP_Yr0_Bkt6 *= m
		self.warRP_Yr0_Bkt7 *= m
		self.warRP0 *= m
		self.outsSP_Yr1_Bkt0 *= m
		self.outsSP_Yr1_Bkt1 *= m
		self.outsSP_Yr1_Bkt2 *= m
		self.outsSP_Yr1_Bkt3 *= m
		self.outsSP_Yr1_Bkt4 *= m
		self.outsSP_Yr1_Bkt5 *= m
		self.outsSP_Yr1_Bkt6 *= m
		self.outsSP1 *= m
		self.warSP_Yr1_Bkt0 *= m
		self.warSP_Yr1_Bkt1 *= m
		self.warSP_Yr1_Bkt2 *= m
		self.warSP_Yr1_Bkt3 *= m
		self.warSP_Yr1_Bkt4 *= m
		self.warSP_Yr1_Bkt5 *= m
		self.warSP_Yr1_Bkt6 *= m
		self.warSP_Yr1_Bkt7 *= m
		self.warSP1 *= m
		self.outsRP_Yr1_Bkt0 *= m
		self.outsRP_Yr1_Bkt1 *= m
		self.outsRP_Yr1_Bkt2 *= m
		self.outsRP_Yr1_Bkt3 *= m
		self.outsRP_Yr1_Bkt4 *= m
		self.outsRP_Yr1_Bkt5 *= m
		self.outsRP_Yr1_Bkt6 *= m
		self.outsRP1 *= m
		self.warRP_Yr1_Bkt0 *= m
		self.warRP_Yr1_Bkt1 *= m
		self.warRP_Yr1_Bkt2 *= m
		self.warRP_Yr1_Bkt3 *= m
		self.warRP_Yr1_Bkt4 *= m
		self.warRP_Yr1_Bkt5 *= m
		self.warRP_Yr1_Bkt6 *= m
		self.warRP_Yr1_Bkt7 *= m
		self.warRP1 *= m
		self.outsSP_Yr2_Bkt0 *= m
		self.outsSP_Yr2_Bkt1 *= m
		self.outsSP_Yr2_Bkt2 *= m
		self.outsSP_Yr2_Bkt3 *= m
		self.outsSP_Yr2_Bkt4 *= m
		self.outsSP_Yr2_Bkt5 *= m
		self.outsSP_Yr2_Bkt6 *= m
		self.outsSP2 *= m
		self.warSP_Yr2_Bkt0 *= m
		self.warSP_Yr2_Bkt1 *= m
		self.warSP_Yr2_Bkt2 *= m
		self.warSP_Yr2_Bkt3 *= m
		self.warSP_Yr2_Bkt4 *= m
		self.warSP_Yr2_Bkt5 *= m
		self.warSP_Yr2_Bkt6 *= m
		self.warSP_Yr2_Bkt7 *= m
		self.warSP2 *= m
		self.outsRP_Yr2_Bkt0 *= m
		self.outsRP_Yr2_Bkt1 *= m
		self.outsRP_Yr2_Bkt2 *= m
		self.outsRP_Yr2_Bkt3 *= m
		self.outsRP_Yr2_Bkt4 *= m
		self.outsRP_Yr2_Bkt5 *= m
		self.outsRP_Yr2_Bkt6 *= m
		self.outsRP2 *= m
		self.warRP_Yr2_Bkt0 *= m
		self.warRP_Yr2_Bkt1 *= m
		self.warRP_Yr2_Bkt2 *= m
		self.warRP_Yr2_Bkt3 *= m
		self.warRP_Yr2_Bkt4 *= m
		self.warRP_Yr2_Bkt5 *= m
		self.warRP_Yr2_Bkt6 *= m
		self.warRP_Yr2_Bkt7 *= m
		self.warRP2 *= m
		self.outsSP_Yr3_Bkt0 *= m
		self.outsSP_Yr3_Bkt1 *= m
		self.outsSP_Yr3_Bkt2 *= m
		self.outsSP_Yr3_Bkt3 *= m
		self.outsSP_Yr3_Bkt4 *= m
		self.outsSP_Yr3_Bkt5 *= m
		self.outsSP_Yr3_Bkt6 *= m
		self.outsSP3 *= m
		self.warSP_Yr3_Bkt0 *= m
		self.warSP_Yr3_Bkt1 *= m
		self.warSP_Yr3_Bkt2 *= m
		self.warSP_Yr3_Bkt3 *= m
		self.warSP_Yr3_Bkt4 *= m
		self.warSP_Yr3_Bkt5 *= m
		self.warSP_Yr3_Bkt6 *= m
		self.warSP_Yr3_Bkt7 *= m
		self.warSP3 *= m
		self.outsRP_Yr3_Bkt0 *= m
		self.outsRP_Yr3_Bkt1 *= m
		self.outsRP_Yr3_Bkt2 *= m
		self.outsRP_Yr3_Bkt3 *= m
		self.outsRP_Yr3_Bkt4 *= m
		self.outsRP_Yr3_Bkt5 *= m
		self.outsRP_Yr3_Bkt6 *= m
		self.outsRP3 *= m
		self.warRP_Yr3_Bkt0 *= m
		self.warRP_Yr3_Bkt1 *= m
		self.warRP_Yr3_Bkt2 *= m
		self.warRP_Yr3_Bkt3 *= m
		self.warRP_Yr3_Bkt4 *= m
		self.warRP_Yr3_Bkt5 *= m
		self.warRP_Yr3_Bkt6 *= m
		self.warRP_Yr3_Bkt7 *= m
		self.warRP3 *= m
		self.outsSP_Yr4_Bkt0 *= m
		self.outsSP_Yr4_Bkt1 *= m
		self.outsSP_Yr4_Bkt2 *= m
		self.outsSP_Yr4_Bkt3 *= m
		self.outsSP_Yr4_Bkt4 *= m
		self.outsSP_Yr4_Bkt5 *= m
		self.outsSP_Yr4_Bkt6 *= m
		self.outsSP4 *= m
		self.warSP_Yr4_Bkt0 *= m
		self.warSP_Yr4_Bkt1 *= m
		self.warSP_Yr4_Bkt2 *= m
		self.warSP_Yr4_Bkt3 *= m
		self.warSP_Yr4_Bkt4 *= m
		self.warSP_Yr4_Bkt5 *= m
		self.warSP_Yr4_Bkt6 *= m
		self.warSP_Yr4_Bkt7 *= m
		self.warSP4 *= m
		self.outsRP_Yr4_Bkt0 *= m
		self.outsRP_Yr4_Bkt1 *= m
		self.outsRP_Yr4_Bkt2 *= m
		self.outsRP_Yr4_Bkt3 *= m
		self.outsRP_Yr4_Bkt4 *= m
		self.outsRP_Yr4_Bkt5 *= m
		self.outsRP_Yr4_Bkt6 *= m
		self.outsRP4 *= m
		self.warRP_Yr4_Bkt0 *= m
		self.warRP_Yr4_Bkt1 *= m
		self.warRP_Yr4_Bkt2 *= m
		self.warRP_Yr4_Bkt3 *= m
		self.warRP_Yr4_Bkt4 *= m
		self.warRP_Yr4_Bkt5 *= m
		self.warRP_Yr4_Bkt6 *= m
		self.warRP_Yr4_Bkt7 *= m
		self.warRP4 *= m
		self.outsSP_Yr5_Bkt0 *= m
		self.outsSP_Yr5_Bkt1 *= m
		self.outsSP_Yr5_Bkt2 *= m
		self.outsSP_Yr5_Bkt3 *= m
		self.outsSP_Yr5_Bkt4 *= m
		self.outsSP_Yr5_Bkt5 *= m
		self.outsSP_Yr5_Bkt6 *= m
		self.outsSP5 *= m
		self.warSP_Yr5_Bkt0 *= m
		self.warSP_Yr5_Bkt1 *= m
		self.warSP_Yr5_Bkt2 *= m
		self.warSP_Yr5_Bkt3 *= m
		self.warSP_Yr5_Bkt4 *= m
		self.warSP_Yr5_Bkt5 *= m
		self.warSP_Yr5_Bkt6 *= m
		self.warSP_Yr5_Bkt7 *= m
		self.warSP5 *= m
		self.outsRP_Yr5_Bkt0 *= m
		self.outsRP_Yr5_Bkt1 *= m
		self.outsRP_Yr5_Bkt2 *= m
		self.outsRP_Yr5_Bkt3 *= m
		self.outsRP_Yr5_Bkt4 *= m
		self.outsRP_Yr5_Bkt5 *= m
		self.outsRP_Yr5_Bkt6 *= m
		self.outsRP5 *= m
		self.warRP_Yr5_Bkt0 *= m
		self.warRP_Yr5_Bkt1 *= m
		self.warRP_Yr5_Bkt2 *= m
		self.warRP_Yr5_Bkt3 *= m
		self.warRP_Yr5_Bkt4 *= m
		self.warRP_Yr5_Bkt5 *= m
		self.warRP_Yr5_Bkt6 *= m
		self.warRP_Yr5_Bkt7 *= m
		self.warRP5 *= m
		self.outsSP_Yr6_Bkt0 *= m
		self.outsSP_Yr6_Bkt1 *= m
		self.outsSP_Yr6_Bkt2 *= m
		self.outsSP_Yr6_Bkt3 *= m
		self.outsSP_Yr6_Bkt4 *= m
		self.outsSP_Yr6_Bkt5 *= m
		self.outsSP_Yr6_Bkt6 *= m
		self.outsSP6 *= m
		self.warSP_Yr6_Bkt0 *= m
		self.warSP_Yr6_Bkt1 *= m
		self.warSP_Yr6_Bkt2 *= m
		self.warSP_Yr6_Bkt3 *= m
		self.warSP_Yr6_Bkt4 *= m
		self.warSP_Yr6_Bkt5 *= m
		self.warSP_Yr6_Bkt6 *= m
		self.warSP_Yr6_Bkt7 *= m
		self.warSP6 *= m
		self.outsRP_Yr6_Bkt0 *= m
		self.outsRP_Yr6_Bkt1 *= m
		self.outsRP_Yr6_Bkt2 *= m
		self.outsRP_Yr6_Bkt3 *= m
		self.outsRP_Yr6_Bkt4 *= m
		self.outsRP_Yr6_Bkt5 *= m
		self.outsRP_Yr6_Bkt6 *= m
		self.outsRP6 *= m
		self.warRP_Yr6_Bkt0 *= m
		self.warRP_Yr6_Bkt1 *= m
		self.warRP_Yr6_Bkt2 *= m
		self.warRP_Yr6_Bkt3 *= m
		self.warRP_Yr6_Bkt4 *= m
		self.warRP_Yr6_Bkt5 *= m
		self.warRP_Yr6_Bkt6 *= m
		self.warRP_Yr6_Bkt7 *= m
		self.warRP6 *= m

		return self

##############################################################################################
class DB_SingleYearHitterBucketAverages:
	__slots__ = ('Month','war0','war1','war2','war3','war4','war5','war6','war7','pa0','pa1','pa2','pa3','pa4','pa5','pa6')

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
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_SingleYearHitterBucketAverages]:
		items = cursor.execute("SELECT * FROM SingleYearHitterBucketAverages " + conditional, values).fetchall()
		return [DB_SingleYearHitterBucketAverages(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_SingleYearHitterBucketAverages]) -> None:
		cursor.executemany("INSERT INTO SingleYearHitterBucketAverages VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_SingleYearHitterBucketAverages) -> DB_SingleYearHitterBucketAverages:
		if isinstance(other, DB_SingleYearHitterBucketAverages):
			self.war0 += other.war0
			self.war1 += other.war1
			self.war2 += other.war2
			self.war3 += other.war3
			self.war4 += other.war4
			self.war5 += other.war5
			self.war6 += other.war6
			self.war7 += other.war7
			self.pa0 += other.pa0
			self.pa1 += other.pa1
			self.pa2 += other.pa2
			self.pa3 += other.pa3
			self.pa4 += other.pa4
			self.pa5 += other.pa5
			self.pa6 += other.pa6

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_SingleYearHitterBucketAverages:
		self.war0 *= m
		self.war1 *= m
		self.war2 *= m
		self.war3 *= m
		self.war4 *= m
		self.war5 *= m
		self.war6 *= m
		self.war7 *= m
		self.pa0 *= m
		self.pa1 *= m
		self.pa2 *= m
		self.pa3 *= m
		self.pa4 *= m
		self.pa5 *= m
		self.pa6 *= m

		return self

##############################################################################################
class DB_SingleYearPitcherBucketAverages:
	__slots__ = ('Month','outsSP0','outsSP1','outsSP2','outsSP3','outsSP4','outsSP5','outsSP6','warSP0','warSP1','warSP2','warSP3','warSP4','warSP5','warSP6','warSP7','outsRP0','outsRP1','outsRP2','outsRP3','outsRP4','outsRP5','outsRP6','warRP0','warRP1','warRP2','warRP3','warRP4','warRP5','warRP6','warRP7')

	def __init__(self, values : tuple[any]):
		self.Month = values[0]
		self.outsSP0 = values[1]
		self.outsSP1 = values[2]
		self.outsSP2 = values[3]
		self.outsSP3 = values[4]
		self.outsSP4 = values[5]
		self.outsSP5 = values[6]
		self.outsSP6 = values[7]
		self.warSP0 = values[8]
		self.warSP1 = values[9]
		self.warSP2 = values[10]
		self.warSP3 = values[11]
		self.warSP4 = values[12]
		self.warSP5 = values[13]
		self.warSP6 = values[14]
		self.warSP7 = values[15]
		self.outsRP0 = values[16]
		self.outsRP1 = values[17]
		self.outsRP2 = values[18]
		self.outsRP3 = values[19]
		self.outsRP4 = values[20]
		self.outsRP5 = values[21]
		self.outsRP6 = values[22]
		self.warRP0 = values[23]
		self.warRP1 = values[24]
		self.warRP2 = values[25]
		self.warRP3 = values[26]
		self.warRP4 = values[27]
		self.warRP5 = values[28]
		self.warRP6 = values[29]
		self.warRP7 = values[30]

	NUM_ELEMENTS = 31

                            
	def To_Tuple(self) -> tuple[any]:
		return (self.Month,self.outsSP0,self.outsSP1,self.outsSP2,self.outsSP3,self.outsSP4,self.outsSP5,self.outsSP6,self.warSP0,self.warSP1,self.warSP2,self.warSP3,self.warSP4,self.warSP5,self.warSP6,self.warSP7,self.outsRP0,self.outsRP1,self.outsRP2,self.outsRP3,self.outsRP4,self.outsRP5,self.outsRP6,self.warRP0,self.warRP1,self.warRP2,self.warRP3,self.warRP4,self.warRP5,self.warRP6,self.warRP7)
                        
	@staticmethod
	def Select_From_DB(cursor : 'sqlite3.Cursor', conditional: str, values: tuple) -> list[DB_SingleYearPitcherBucketAverages]:
		items = cursor.execute("SELECT * FROM SingleYearPitcherBucketAverages " + conditional, values).fetchall()
		return [DB_SingleYearPitcherBucketAverages(i) for i in items]

	@staticmethod 
	def Insert_Into_DB(cursor : 'sqlite3.Cursor', items : list[DB_SingleYearPitcherBucketAverages]) -> None:
		cursor.executemany("INSERT INTO SingleYearPitcherBucketAverages VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [i.To_Tuple() for i in items])

	def __add__(self, other : DB_SingleYearPitcherBucketAverages) -> DB_SingleYearPitcherBucketAverages:
		if isinstance(other, DB_SingleYearPitcherBucketAverages):
			self.outsSP0 += other.outsSP0
			self.outsSP1 += other.outsSP1
			self.outsSP2 += other.outsSP2
			self.outsSP3 += other.outsSP3
			self.outsSP4 += other.outsSP4
			self.outsSP5 += other.outsSP5
			self.outsSP6 += other.outsSP6
			self.warSP0 += other.warSP0
			self.warSP1 += other.warSP1
			self.warSP2 += other.warSP2
			self.warSP3 += other.warSP3
			self.warSP4 += other.warSP4
			self.warSP5 += other.warSP5
			self.warSP6 += other.warSP6
			self.warSP7 += other.warSP7
			self.outsRP0 += other.outsRP0
			self.outsRP1 += other.outsRP1
			self.outsRP2 += other.outsRP2
			self.outsRP3 += other.outsRP3
			self.outsRP4 += other.outsRP4
			self.outsRP5 += other.outsRP5
			self.outsRP6 += other.outsRP6
			self.warRP0 += other.warRP0
			self.warRP1 += other.warRP1
			self.warRP2 += other.warRP2
			self.warRP3 += other.warRP3
			self.warRP4 += other.warRP4
			self.warRP5 += other.warRP5
			self.warRP6 += other.warRP6
			self.warRP7 += other.warRP7

		return self
		return NotImplemented
                    
	def __mul__(self, m : float) -> DB_SingleYearPitcherBucketAverages:
		self.outsSP0 *= m
		self.outsSP1 *= m
		self.outsSP2 *= m
		self.outsSP3 *= m
		self.outsSP4 *= m
		self.outsSP5 *= m
		self.outsSP6 *= m
		self.warSP0 *= m
		self.warSP1 *= m
		self.warSP2 *= m
		self.warSP3 *= m
		self.warSP4 *= m
		self.warSP5 *= m
		self.warSP6 *= m
		self.warSP7 *= m
		self.outsRP0 *= m
		self.outsRP1 *= m
		self.outsRP2 *= m
		self.outsRP3 *= m
		self.outsRP4 *= m
		self.outsRP5 *= m
		self.outsRP6 *= m
		self.warRP0 *= m
		self.warRP1 *= m
		self.warRP2 *= m
		self.warRP3 *= m
		self.warRP4 *= m
		self.warRP5 *= m
		self.warRP6 *= m
		self.warRP7 *= m

		return self

##############################################################################################
