namespace Db
{
	public class Model_HitterStats
	{
		public required int Mlbid {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Age {get; set;}
		public required int Pa {get; set;}
		public required int Level {get; set;}
		public required float Parkrunfactor {get; set;}
		public required float Parkhrfactor {get; set;}
		public required float Avgratio {get; set;}
		public required float Obpratio {get; set;}
		public required float Isoratio {get; set;}
		public required float Wobaratio {get; set;}
		public required float Sbrateratio {get; set;}
		public required float Sbpercratio {get; set;}
		public required float Hrpercratio {get; set;}
		public required float Bbpercratio {get; set;}
		public required float Kpercratio {get; set;}
		public required float Percc {get; set;}
		public required float Perc1b {get; set;}
		public required float Perc2b {get; set;}
		public required float Perc3b {get; set;}
		public required float Percss {get; set;}
		public required float Perclf {get; set;}
		public required float Perccf {get; set;}
		public required float Percrf {get; set;}
		public required float Percdh {get; set;}
	}
}