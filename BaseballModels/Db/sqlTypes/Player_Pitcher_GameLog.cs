namespace Db
{
	public class Player_Pitcher_GameLog
	{
		public int GameLogId {get; set;}
		public required int GameId {get; set;}
		public required int MlbId {get; set;}
		public required int Day {get; set;}
		public required int Month {get; set;}
		public required int Year {get; set;}
		public required int Started {get; set;}
		public required int BattersFaced {get; set;}
		public required int Outs {get; set;}
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
		public required int LevelId {get; set;}
		public required int StadiumId {get; set;}
		public required int IsHome {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}

		public Player_Pitcher_GameLog Clone()
		{
			return new Player_Pitcher_GameLog
			{
				GameLogId = this.GameLogId,
				GameId = this.GameId,
				MlbId = this.MlbId,
				Day = this.Day,
				Month = this.Month,
				Year = this.Year,
				Started = this.Started,
				BattersFaced = this.BattersFaced,
				Outs = this.Outs,
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
				LevelId = this.LevelId,
				StadiumId = this.StadiumId,
				IsHome = this.IsHome,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				
			};
		}
	}
}