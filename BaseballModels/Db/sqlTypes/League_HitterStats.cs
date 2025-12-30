namespace Db
{
	public class League_HitterStats
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int AB {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WOBA {get; set;}
		public required float HRPerc {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required float SBRate {get; set;}
		public required float SBPerc {get; set;}
		public required float Hit1B {get; set;}
		public required float Hit2B {get; set;}
		public required float Hit3B {get; set;}
		public required float HitHR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}
		public required float SB {get; set;}
		public required float CS {get; set;}

		public League_HitterStats Clone()
		{
			return new League_HitterStats
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				Month = this.Month,
				AB = this.AB,
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				ISO = this.ISO,
				WOBA = this.WOBA,
				HRPerc = this.HRPerc,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				SBRate = this.SBRate,
				SBPerc = this.SBPerc,
				Hit1B = this.Hit1B,
				Hit2B = this.Hit2B,
				Hit3B = this.Hit3B,
				HitHR = this.HitHR,
				BB = this.BB,
				HBP = this.HBP,
				K = this.K,
				SB = this.SB,
				CS = this.CS,
			};
		}
	}
}