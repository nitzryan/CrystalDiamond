namespace Db
{
	public class Player_Pitcher_MonthAdvanced
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int BF {get; set;}
		public required int Outs {get; set;}
		public required float GBRatio {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float KPerc {get; set;}
		public required float BBPerc {get; set;}
		public required float HRPerc {get; set;}
		public required float WOBA {get; set;}
	}
}