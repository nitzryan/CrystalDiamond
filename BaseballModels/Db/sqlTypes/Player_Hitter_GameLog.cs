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
		public required int Level {get; set;}
		public required int HomeTeamId {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
	}
}