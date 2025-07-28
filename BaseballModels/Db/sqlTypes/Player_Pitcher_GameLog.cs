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
		public required int BattersFaced {get; set;}
		public required int Outs {get; set;}
		public required int Go {get; set;}
		public required int Ao {get; set;}
		public required int R {get; set;}
		public required int Er {get; set;}
		public required int H {get; set;}
		public required int K {get; set;}
		public required int Bb {get; set;}
		public required int Hbp {get; set;}
		public required int Hit2B {get; set;}
		public required int Hit3B {get; set;}
		public required int HR {get; set;}
		public required int Level {get; set;}
		public required int HomeTeamId {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
	}
}