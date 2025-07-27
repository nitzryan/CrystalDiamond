namespace Db
{
	public class Model_PitcherStats
	{
		public required int Mlbid {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Age {get; set;}
		public required int Bf {get; set;}
		public required float Level {get; set;}
		public required float Parkrunfactor {get; set;}
		public required float Parkhrfactor {get; set;}
		public required float Gbpercratio {get; set;}
		public required float Eraratio {get; set;}
		public required float Fipratio {get; set;}
		public required float Wobaratio {get; set;}
		public required float Hrpercratio {get; set;}
		public required float Bbpercratio {get; set;}
		public required float Kpercratio {get; set;}
	}
}