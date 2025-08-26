namespace Db
{
	public class Player_Hitter_GameLog
	{
		public int GameLogId {get; set;}
		public required int GameId {get; set;}
		public required int MlbId {get; set;}
		public required int Day {get; set;}
		public required int Month {get; set;}
		public required int Year {get; set;}
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
		public required int Position {get; set;}
		public required int LevelId {get; set;}
		public required int HomeTeamId {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}

		public Player_Hitter_GameLog Clone()
		{
			return new Player_Hitter_GameLog
			{
				GameLogId = this.GameLogId,
				GameId = this.GameId,
				MlbId = this.MlbId,
				Day = this.Day,
				Month = this.Month,
				Year = this.Year,
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
				Position = this.Position,
				LevelId = this.LevelId,
				HomeTeamId = this.HomeTeamId,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				
			};
		}
	}
}