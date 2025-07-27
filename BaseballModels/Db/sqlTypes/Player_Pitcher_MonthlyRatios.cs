namespace Db
{
	public class Player_Pitcher_MonthlyRatios
	{
		public required int Mlbid {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Level {get; set;}
		public required float Gbpercratio {get; set;}
		public required float Eraratio {get; set;}
		public required float Fipratio {get; set;}
		public required float Wobaratio {get; set;}
		public required float Hrpercratio {get; set;}
		public required float Bbpercratio {get; set;}
		public required float Kpercratio {get; set;}
	}
}