class DB_Player
{
	public mlbId : number
	public firstName : string
	public lastName : string
	public birthYear : number
	public birthMonth : number
	public birthDate : number
	public startYear : number
	public position : string
	public status : string
	public orgId : number
	public draftPick : number
	public draftRound : string
	public draftBonus : number
	public isHitter : number
	public isPitcher : number
	public inTraining : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.firstName = data['firstName'] as string
		this.lastName = data['lastName'] as string
		this.birthYear = data['birthYear'] as number
		this.birthMonth = data['birthMonth'] as number
		this.birthDate = data['birthDate'] as number
		this.startYear = data['startYear'] as number
		this.position = data['position'] as string
		this.status = data['status'] as string
		this.orgId = data['orgId'] as number
		this.draftPick = data['draftPick'] as number
		this.draftRound = data['draftRound'] as string
		this.draftBonus = data['draftBonus'] as number
		this.isHitter = data['isHitter'] as number
		this.isPitcher = data['isPitcher'] as number
		this.inTraining = data['inTraining'] as number
	}
}

class DB_HitterYearStats
{
	public mlbId : number
	public levelId : number
	public year : number
	public teamId : number
	public leagueId : number
	public PA : number
	public AVG : number
	public OBP : number
	public SLG : number
	public ISO : number
	public WRC : number
	public HR : number
	public BBPerc : number
	public KPerc : number
	public SB : number
	public CS : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.levelId = data['levelId'] as number
		this.year = data['year'] as number
		this.teamId = data['teamId'] as number
		this.leagueId = data['leagueId'] as number
		this.PA = data['PA'] as number
		this.AVG = data['AVG'] as number
		this.OBP = data['OBP'] as number
		this.SLG = data['SLG'] as number
		this.ISO = data['ISO'] as number
		this.WRC = data['WRC'] as number
		this.HR = data['HR'] as number
		this.BBPerc = data['BBPerc'] as number
		this.KPerc = data['KPerc'] as number
		this.SB = data['SB'] as number
		this.CS = data['CS'] as number
	}
}

class DB_HitterMonthStats
{
	public mlbId : number
	public levelId : number
	public year : number
	public month : number
	public teamId : number
	public leagueId : number
	public PA : number
	public AVG : number
	public OBP : number
	public SLG : number
	public ISO : number
	public WRC : number
	public HR : number
	public BBPerc : number
	public KPerc : number
	public SB : number
	public CS : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.levelId = data['levelId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.teamId = data['teamId'] as number
		this.leagueId = data['leagueId'] as number
		this.PA = data['PA'] as number
		this.AVG = data['AVG'] as number
		this.OBP = data['OBP'] as number
		this.SLG = data['SLG'] as number
		this.ISO = data['ISO'] as number
		this.WRC = data['WRC'] as number
		this.HR = data['HR'] as number
		this.BBPerc = data['BBPerc'] as number
		this.KPerc = data['KPerc'] as number
		this.SB = data['SB'] as number
		this.CS = data['CS'] as number
	}
}

class DB_PitcherYearStats
{
	public mlbId : number
	public levelId : number
	public year : number
	public teamId : number
	public leagueId : number
	public IP : string
	public ERA : number
	public FIP : number
	public HR9 : number
	public BBPerc : number
	public KPerc : number
	public GOPerc : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.levelId = data['levelId'] as number
		this.year = data['year'] as number
		this.teamId = data['teamId'] as number
		this.leagueId = data['leagueId'] as number
		this.IP = data['IP'] as string
		this.ERA = data['ERA'] as number
		this.FIP = data['FIP'] as number
		this.HR9 = data['HR9'] as number
		this.BBPerc = data['BBPerc'] as number
		this.KPerc = data['KPerc'] as number
		this.GOPerc = data['GOPerc'] as number
	}
}

class DB_PitcherMonthStats
{
	public mlbId : number
	public levelId : number
	public year : number
	public month : number
	public teamId : number
	public leagueId : number
	public IP : string
	public ERA : number
	public FIP : number
	public HR9 : number
	public BBPerc : number
	public KPerc : number
	public GOPerc : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.levelId = data['levelId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.teamId = data['teamId'] as number
		this.leagueId = data['leagueId'] as number
		this.IP = data['IP'] as string
		this.ERA = data['ERA'] as number
		this.FIP = data['FIP'] as number
		this.HR9 = data['HR9'] as number
		this.BBPerc = data['BBPerc'] as number
		this.KPerc = data['KPerc'] as number
		this.GOPerc = data['GOPerc'] as number
	}
}

class DB_Prediction_HitterStats
{
	public MlbId : number
	public Model : number
	public Year : number
	public Month : number
	public LevelId : number
	public Pa : number
	public Hit1B : number
	public Hit2B : number
	public Hit3B : number
	public HitHR : number
	public BB : number
	public HBP : number
	public K : number
	public SB : number
	public CS : number
	public ParkRunFactor : number
	public PercC : number
	public Perc1B : number
	public Perc2B : number
	public Perc3B : number
	public PercSS : number
	public PercLF : number
	public PercCF : number
	public PercRF : number
	public PercDH : number
	public AVG : number
	public OBP : number
	public SLG : number
	public ISO : number
	public wRC : number
	public crOFF : number
	public crBSR : number
	public crDEF : number
	public crWAR : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.Model = data['Model'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.LevelId = data['LevelId'] as number
		this.Pa = data['Pa'] as number
		this.Hit1B = data['Hit1B'] as number
		this.Hit2B = data['Hit2B'] as number
		this.Hit3B = data['Hit3B'] as number
		this.HitHR = data['HitHR'] as number
		this.BB = data['BB'] as number
		this.HBP = data['HBP'] as number
		this.K = data['K'] as number
		this.SB = data['SB'] as number
		this.CS = data['CS'] as number
		this.ParkRunFactor = data['ParkRunFactor'] as number
		this.PercC = data['PercC'] as number
		this.Perc1B = data['Perc1B'] as number
		this.Perc2B = data['Perc2B'] as number
		this.Perc3B = data['Perc3B'] as number
		this.PercSS = data['PercSS'] as number
		this.PercLF = data['PercLF'] as number
		this.PercCF = data['PercCF'] as number
		this.PercRF = data['PercRF'] as number
		this.PercDH = data['PercDH'] as number
		this.AVG = data['AVG'] as number
		this.OBP = data['OBP'] as number
		this.SLG = data['SLG'] as number
		this.ISO = data['ISO'] as number
		this.wRC = data['wRC'] as number
		this.crOFF = data['crOFF'] as number
		this.crBSR = data['crBSR'] as number
		this.crDEF = data['crDEF'] as number
		this.crWAR = data['crWAR'] as number
	}
}

class DB_Prediction_PitcherStats
{
	public mlbId : number
	public Model : number
	public Year : number
	public Month : number
	public levelId : number
	public Outs_SP : number
	public Outs_RP : number
	public GS : number
	public GR : number
	public ERA : number
	public FIP : number
	public HR : number
	public BB : number
	public HBP : number
	public K : number
	public ParkRunFactor : number
	public SP_Perc : number
	public RP_Perc : number
	public crRAA : number
	public crWAR : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.Model = data['Model'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.levelId = data['levelId'] as number
		this.Outs_SP = data['Outs_SP'] as number
		this.Outs_RP = data['Outs_RP'] as number
		this.GS = data['GS'] as number
		this.GR = data['GR'] as number
		this.ERA = data['ERA'] as number
		this.FIP = data['FIP'] as number
		this.HR = data['HR'] as number
		this.BB = data['BB'] as number
		this.HBP = data['HBP'] as number
		this.K = data['K'] as number
		this.ParkRunFactor = data['ParkRunFactor'] as number
		this.SP_Perc = data['SP_Perc'] as number
		this.RP_Perc = data['RP_Perc'] as number
		this.crRAA = data['crRAA'] as number
		this.crWAR = data['crWAR'] as number
	}
}

class DB_PlayerModel
{
	public mlbId : number
	public year : number
	public month : number
	public modelId : number
	public isHitter : number
	public probsWar : string
	public rankWar : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.modelId = data['modelId'] as number
		this.isHitter = data['isHitter'] as number
		this.probsWar = data['probsWar'] as string
		this.rankWar = data['rankWar'] as number
	}
}

class DB_PlayerRank
{
	public mlbId : number
	public modelId : number
	public isHitter : number
	public year : number
	public month : number
	public teamId : number
	public position : string
	public war : number
	public rankWar : number
	public teamRankWar : number
	public highestLevel : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.modelId = data['modelId'] as number
		this.isHitter = data['isHitter'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.teamId = data['teamId'] as number
		this.position = data['position'] as string
		this.war = data['war'] as number
		this.rankWar = data['rankWar'] as number
		this.teamRankWar = data['teamRankWar'] as number
		this.highestLevel = data['highestLevel'] as number
	}
}

class DB_HitterWarRank
{
	public mlbId : number
	public modelId : number
	public year : number
	public month : number
	public teamId : number
	public position : string
	public war : number
	public rankWar : number
	public pa : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.modelId = data['modelId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.teamId = data['teamId'] as number
		this.position = data['position'] as string
		this.war = data['war'] as number
		this.rankWar = data['rankWar'] as number
		this.pa = data['pa'] as number
	}
}

class DB_PitcherWarRank
{
	public mlbId : number
	public modelId : number
	public year : number
	public month : number
	public teamId : number
	public spWar : number
	public spIP : number
	public rpWar : number
	public rpIP : number
	public spRank : number
	public rpRank : number

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.modelId = data['modelId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.teamId = data['teamId'] as number
		this.spWar = data['spWar'] as number
		this.spIP = data['spIP'] as number
		this.rpWar = data['rpWar'] as number
		this.rpIP = data['rpIP'] as number
		this.spRank = data['spRank'] as number
		this.rpRank = data['rpRank'] as number
	}
}

class DB_TeamRank
{
	public teamId : number
	public modelId : number
	public year : number
	public month : number
	public highestRank : number
	public top10 : number
	public top50 : number
	public top100 : number
	public top200 : number
	public top500 : number
	public rank : number
	public war : number

	constructor(data : JsonObject)
	{
		this.teamId = data['teamId'] as number
		this.modelId = data['modelId'] as number
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.highestRank = data['highestRank'] as number
		this.top10 = data['top10'] as number
		this.top50 = data['top50'] as number
		this.top100 = data['top100'] as number
		this.top200 = data['top200'] as number
		this.top500 = data['top500'] as number
		this.rank = data['rank'] as number
		this.war = data['war'] as number
	}
}

class DB_Models
{
	public modelId : number
	public name : string

	constructor(data : JsonObject)
	{
		this.modelId = data['modelId'] as number
		this.name = data['name'] as string
	}
}

class DB_PlayerYearPositions
{
	public mlbId : number
	public year : number
	public isHitter : number
	public position : string

	constructor(data : JsonObject)
	{
		this.mlbId = data['mlbId'] as number
		this.year = data['year'] as number
		this.isHitter = data['isHitter'] as number
		this.position = data['position'] as string
	}
}

class DB_HomeData
{
	public year : number
	public month : number
	public rankType : number
	public modelId : number
	public isWar : number
	public mlbId : number
	public data : string
	public rank : number

	constructor(data : JsonObject)
	{
		this.year = data['year'] as number
		this.month = data['month'] as number
		this.rankType = data['rankType'] as number
		this.modelId = data['modelId'] as number
		this.isWar = data['isWar'] as number
		this.mlbId = data['mlbId'] as number
		this.data = data['data'] as string
		this.rank = data['rank'] as number
	}
}

class DB_HomeDataType
{
	public type : number
	public name : string

	constructor(data : JsonObject)
	{
		this.type = data['type'] as number
		this.name = data['name'] as string
	}
}

