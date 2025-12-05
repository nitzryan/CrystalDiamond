namespace Db
{
	public class Player_Hitter_MonthStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int LeagueId {get; set;}
		public required int AB {get; set;}
		public required int PA {get; set;}
		public required int H {get; set;}
		public required int Hit2B {get; set;}
		public required int Hit3B {get; set;}
		public required int HR {get; set;}
		public required int K {get; set;}
		public required int BB {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}
		public required int HBP {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
		public required int GamesC {get; set;}
		public required int Games1B {get; set;}
		public required int Games2B {get; set;}
		public required int Games3B {get; set;}
		public required int GamesSS {get; set;}
		public required int GamesLF {get; set;}
		public required int GamesCF {get; set;}
		public required int GamesRF {get; set;}
		public required int GamesDH {get; set;}

		public Player_Hitter_MonthStats Clone()
		{
			return new Player_Hitter_MonthStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				LeagueId = this.LeagueId,
				AB = this.AB,
				PA = this.PA,
				H = this.H,
				Hit2B = this.Hit2B,
				Hit3B = this.Hit3B,
				HR = this.HR,
				K = this.K,
				BB = this.BB,
				SB = this.SB,
				CS = this.CS,
				HBP = this.HBP,
				ParkRunFactor = this.ParkRunFactor,
				ParkHRFactor = this.ParkHRFactor,
				GamesC = this.GamesC,
				Games1B = this.Games1B,
				Games2B = this.Games2B,
				Games3B = this.Games3B,
				GamesSS = this.GamesSS,
				GamesLF = this.GamesLF,
				GamesCF = this.GamesCF,
				GamesRF = this.GamesRF,
				GamesDH = this.GamesDH,
				
			};
		}
	}
}