class DB_Player
{
	public MlbId : number
	public FirstName : string
	public LastName : string
	public BirthYear : number
	public BirthMonth : number
	public BirthDate : number
	public StartYear : number
	public Position : string
	public Status : string
	public OrgId : number
	public DraftPick : number
	public DraftRound : string
	public DraftBonus : number
	public IsHitter : number
	public IsPitcher : number
	public InTraining : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.FirstName = data['FirstName'] as string
		this.LastName = data['LastName'] as string
		this.BirthYear = data['BirthYear'] as number
		this.BirthMonth = data['BirthMonth'] as number
		this.BirthDate = data['BirthDate'] as number
		this.StartYear = data['StartYear'] as number
		this.Position = data['Position'] as string
		this.Status = data['Status'] as string
		this.OrgId = data['OrgId'] as number
		this.DraftPick = data['DraftPick'] as number
		this.DraftRound = data['DraftRound'] as string
		this.DraftBonus = data['DraftBonus'] as number
		this.IsHitter = data['IsHitter'] as number
		this.IsPitcher = data['IsPitcher'] as number
		this.InTraining = data['InTraining'] as number
	}
}

class DB_HitterYearStats
{
	public MlbId : number
	public LevelId : number
	public Year : number
	public TeamId : number
	public LeagueId : number
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
		this.MlbId = data['MlbId'] as number
		this.LevelId = data['LevelId'] as number
		this.Year = data['Year'] as number
		this.TeamId = data['TeamId'] as number
		this.LeagueId = data['LeagueId'] as number
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
	public MlbId : number
	public LevelId : number
	public Year : number
	public Month : number
	public TeamId : number
	public LeagueId : number
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
		this.MlbId = data['MlbId'] as number
		this.LevelId = data['LevelId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.TeamId = data['TeamId'] as number
		this.LeagueId = data['LeagueId'] as number
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
	public MlbId : number
	public LevelId : number
	public Year : number
	public TeamId : number
	public LeagueId : number
	public IP : string
	public ERA : number
	public FIP : number
	public HR9 : number
	public BBPerc : number
	public KPerc : number
	public GOPerc : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.LevelId = data['LevelId'] as number
		this.Year = data['Year'] as number
		this.TeamId = data['TeamId'] as number
		this.LeagueId = data['LeagueId'] as number
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
	public MlbId : number
	public LevelId : number
	public Year : number
	public Month : number
	public TeamId : number
	public LeagueId : number
	public IP : string
	public ERA : number
	public FIP : number
	public HR9 : number
	public BBPerc : number
	public KPerc : number
	public GOPerc : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.LevelId = data['LevelId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.TeamId = data['TeamId'] as number
		this.LeagueId = data['LeagueId'] as number
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
	public WRC : number
	public CrOFF : number
	public CrBSR : number
	public CrDEF : number
	public CrWAR : number

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
		this.WRC = data['WRC'] as number
		this.CrOFF = data['CrOFF'] as number
		this.CrBSR = data['CrBSR'] as number
		this.CrDEF = data['CrDEF'] as number
		this.CrWAR = data['CrWAR'] as number
	}
}

class DB_Prediction_PitcherStats
{
	public MlbId : number
	public Model : number
	public Year : number
	public Month : number
	public LevelId : number
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
	public CrRAA : number
	public CrWAR : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.Model = data['Model'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.LevelId = data['LevelId'] as number
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
		this.CrRAA = data['CrRAA'] as number
		this.CrWAR = data['CrWAR'] as number
	}
}

class DB_PlayerModel
{
	public MlbId : number
	public Year : number
	public Month : number
	public ModelId : number
	public IsHitter : number
	public ProbsWar : string
	public RankWar : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.ModelId = data['ModelId'] as number
		this.IsHitter = data['IsHitter'] as number
		this.ProbsWar = data['ProbsWar'] as string
		this.RankWar = data['RankWar'] as number
	}
}

class DB_PlayerRank
{
	public MlbId : number
	public ModelId : number
	public IsHitter : number
	public Year : number
	public Month : number
	public TeamId : number
	public Position : string
	public War : number
	public RankWar : number
	public TeamRankWar : number
	public HighestLevel : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.ModelId = data['ModelId'] as number
		this.IsHitter = data['IsHitter'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.TeamId = data['TeamId'] as number
		this.Position = data['Position'] as string
		this.War = data['War'] as number
		this.RankWar = data['RankWar'] as number
		this.TeamRankWar = data['TeamRankWar'] as number
		this.HighestLevel = data['HighestLevel'] as number
	}
}

class DB_HitterWarRank
{
	public MlbId : number
	public ModelId : number
	public Year : number
	public Month : number
	public TeamId : number
	public Position : string
	public War : number
	public RankWar : number
	public Pa : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.ModelId = data['ModelId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.TeamId = data['TeamId'] as number
		this.Position = data['Position'] as string
		this.War = data['War'] as number
		this.RankWar = data['RankWar'] as number
		this.Pa = data['Pa'] as number
	}
}

class DB_PitcherWarRank
{
	public MlbId : number
	public ModelId : number
	public Year : number
	public Month : number
	public TeamId : number
	public SpWar : number
	public SpIP : number
	public RpWar : number
	public RpIP : number
	public SpRank : number
	public RpRank : number

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.ModelId = data['ModelId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.TeamId = data['TeamId'] as number
		this.SpWar = data['SpWar'] as number
		this.SpIP = data['SpIP'] as number
		this.RpWar = data['RpWar'] as number
		this.RpIP = data['RpIP'] as number
		this.SpRank = data['SpRank'] as number
		this.RpRank = data['RpRank'] as number
	}
}

class DB_TeamRank
{
	public TeamId : number
	public ModelId : number
	public Year : number
	public Month : number
	public HighestRank : number
	public Top10 : number
	public Top50 : number
	public Top100 : number
	public Top200 : number
	public Top500 : number
	public Rank : number
	public War : number

	constructor(data : JsonObject)
	{
		this.TeamId = data['TeamId'] as number
		this.ModelId = data['ModelId'] as number
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.HighestRank = data['HighestRank'] as number
		this.Top10 = data['Top10'] as number
		this.Top50 = data['Top50'] as number
		this.Top100 = data['Top100'] as number
		this.Top200 = data['Top200'] as number
		this.Top500 = data['Top500'] as number
		this.Rank = data['Rank'] as number
		this.War = data['War'] as number
	}
}

class DB_Models
{
	public ModelId : number
	public Name : string

	constructor(data : JsonObject)
	{
		this.ModelId = data['ModelId'] as number
		this.Name = data['Name'] as string
	}
}

class DB_PlayerYearPositions
{
	public MlbId : number
	public Year : number
	public IsHitter : number
	public Position : string

	constructor(data : JsonObject)
	{
		this.MlbId = data['MlbId'] as number
		this.Year = data['Year'] as number
		this.IsHitter = data['IsHitter'] as number
		this.Position = data['Position'] as string
	}
}

class DB_HomeData
{
	public Year : number
	public Month : number
	public RankType : number
	public ModelId : number
	public IsWar : number
	public MlbId : number
	public Data : string
	public Rank : number

	constructor(data : JsonObject)
	{
		this.Year = data['Year'] as number
		this.Month = data['Month'] as number
		this.RankType = data['RankType'] as number
		this.ModelId = data['ModelId'] as number
		this.IsWar = data['IsWar'] as number
		this.MlbId = data['MlbId'] as number
		this.Data = data['Data'] as string
		this.Rank = data['Rank'] as number
	}
}

class DB_HomeDataType
{
	public Type : number
	public Name : string

	constructor(data : JsonObject)
	{
		this.Type = data['Type'] as number
		this.Name = data['Name'] as string
	}
}

