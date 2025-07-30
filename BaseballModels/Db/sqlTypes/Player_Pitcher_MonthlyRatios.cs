namespace Db
{
	public class Player_Pitcher_MonthlyRatios
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required float GBPercRatio {get; set;}
		public required float ERARatio {get; set;}
		public required float FIPRatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float HRPercRatio {get; set;}
		public required float BBPercRatio {get; set;}
		public required float KPercRatio {get; set;}
	}
}