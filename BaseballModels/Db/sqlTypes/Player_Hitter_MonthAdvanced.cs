namespace Db
{
	public class Player_Hitter_MonthAdvanced
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int PA {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WOBA {get; set;}
		public required float WRC {get; set;}
		public required float HRPerc {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required float SBRate {get; set;}
		public required float SBPerc {get; set;}

		public Player_Hitter_MonthAdvanced Clone()
		{
			return new Player_Hitter_MonthAdvanced
			{
				MlbId = this.MlbId,
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				PA = this.PA,
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				ISO = this.ISO,
				WOBA = this.WOBA,
				WRC = this.WRC,
				HRPerc = this.HRPerc,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				SBRate = this.SBRate,
				SBPerc = this.SBPerc,
				
			};
		}
	}
}