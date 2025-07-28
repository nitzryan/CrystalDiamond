namespace Db
{
	public class Player_Hitter_YearAdvanced
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int PA {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WOBA {get; set;}
		public required float WRC {get; set;}
		public required int HR {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}
	}
}