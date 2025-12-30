namespace Db
{
	public class LeagueStats
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required float AvgWOBA {get; set;}
		public required float AvgHitterWOBA {get; set;}
		public required float WOBAScale {get; set;}
		public required float WBB {get; set;}
		public required float WHBP {get; set;}
		public required float W1B {get; set;}
		public required float W2B {get; set;}
		public required float W3B {get; set;}
		public required float WHR {get; set;}
		public required float RunSB {get; set;}
		public required float RunCS {get; set;}
		public required float RunErr {get; set;}
		public required float RunGIDP {get; set;}
		public required float ProbGIDP {get; set;}
		public required float RPerPA {get; set;}
		public required float RPerWin {get; set;}
		public required int LeaguePA {get; set;}
		public required int LeagueGames {get; set;}
		public required float CFIP {get; set;}
		public required float FIPR9Adjustment {get; set;}
		public required float LeagueERA {get; set;}

		public LeagueStats Clone()
		{
			return new LeagueStats
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				AvgWOBA = this.AvgWOBA,
				AvgHitterWOBA = this.AvgHitterWOBA,
				WOBAScale = this.WOBAScale,
				WBB = this.WBB,
				WHBP = this.WHBP,
				W1B = this.W1B,
				W2B = this.W2B,
				W3B = this.W3B,
				WHR = this.WHR,
				RunSB = this.RunSB,
				RunCS = this.RunCS,
				RunErr = this.RunErr,
				RunGIDP = this.RunGIDP,
				ProbGIDP = this.ProbGIDP,
				RPerPA = this.RPerPA,
				RPerWin = this.RPerWin,
				LeaguePA = this.LeaguePA,
				LeagueGames = this.LeagueGames,
				CFIP = this.CFIP,
				FIPR9Adjustment = this.FIPR9Adjustment,
				LeagueERA = this.LeagueERA,
			};
		}
	}
}