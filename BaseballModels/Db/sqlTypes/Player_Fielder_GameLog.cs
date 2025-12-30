namespace Db
{
	public class Player_Fielder_GameLog
	{
		public int GameLogId {get; set;}
		public required int GameId {get; set;}
		public required int MlbId {get; set;}
		public required int LeagueId {get; set;}
		public required int TeamId {get; set;}
		public required int Day {get; set;}
		public required int Month {get; set;}
		public required int Year {get; set;}
		public required DbEnums.Position Position {get; set;}
		public required int Outs {get; set;}
		public required int Chances {get; set;}
		public required int Errors {get; set;}
		public required int ThrowErrors {get; set;}
		public required bool Started {get; set;}
		public required bool IsHome {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}
		public required int PassedBall {get; set;}

		public Player_Fielder_GameLog Clone()
		{
			return new Player_Fielder_GameLog
			{
				GameLogId = this.GameLogId,
				GameId = this.GameId,
				MlbId = this.MlbId,
				LeagueId = this.LeagueId,
				TeamId = this.TeamId,
				Day = this.Day,
				Month = this.Month,
				Year = this.Year,
				Position = this.Position,
				Outs = this.Outs,
				Chances = this.Chances,
				Errors = this.Errors,
				ThrowErrors = this.ThrowErrors,
				Started = this.Started,
				IsHome = this.IsHome,
				SB = this.SB,
				CS = this.CS,
				PassedBall = this.PassedBall,
				
			};
		}
	}
}