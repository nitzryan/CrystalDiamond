namespace Db
{
	public class Player_Pitcher_MonthStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int LeagueId {get; set;}
		public required int BattersFaced {get; set;}
		public required int Outs {get; set;}
		public required float SPPerc {get; set;}
		public required int GO {get; set;}
		public required int AO {get; set;}
		public required int R {get; set;}
		public required int ER {get; set;}
		public required int H {get; set;}
		public required int K {get; set;}
		public required int BB {get; set;}
		public required int HBP {get; set;}
		public required int Hit2B {get; set;}
		public required int Hit3B {get; set;}
		public required int HR {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}

		public Player_Pitcher_MonthStats Clone()
		{
			return new Player_Pitcher_MonthStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				LeagueId = this.LeagueId,
				BattersFaced = this.BattersFaced,
				Outs = this.Outs,
				SPPerc = this.SPPerc,
				GO = this.GO,
				AO = this.AO,
				R = this.R,
				ER = this.ER,
				H = this.H,
				K = this.K,
				BB = this.BB,
				HBP = this.HBP,
				Hit2B = this.Hit2B,
				Hit3B = this.Hit3B,
				HR = this.HR,
				ParkRunFactor = this.ParkRunFactor,
				ParkHRFactor = this.ParkHRFactor,
			};
		}
	}
}