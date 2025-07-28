namespace Db
{
	public class Player_Hitter_MonthlyRatios
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Level {get; set;}
		public required float AvgRatio {get; set;}
		public required float ObpRatio {get; set;}
		public required float IsoRatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float SbRateRatio {get; set;}
		public required float SbPercRatio {get; set;}
		public required float HrPercRatio {get; set;}
		public required float BbPercRatio {get; set;}
		public required float KPercRatio {get; set;}
		public required float PercC {get; set;}
		public required float Perc1B {get; set;}
		public required float Perc2B {get; set;}
		public required float Perc3B {get; set;}
		public required float PercSS {get; set;}
		public required float PercLF {get; set;}
		public required float PercCF {get; set;}
		public required float PercRF {get; set;}
		public required float PercDH {get; set;}
	}
}