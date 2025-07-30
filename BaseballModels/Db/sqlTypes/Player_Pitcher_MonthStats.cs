namespace Db
{
	public class Player_Pitcher_MonthStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int BattersFaced {get; set;}
		public required int Outs {get; set;}
		public required int GO {get; set;}
		public required int AO {get; set;}
		public required int R {get; set;}
		public required int ER {get; set;}
		public required int H {get; set;}
		public required int K {get; set;}
		public required int BB {get; set;}
		public required int HBP {get; set;}
		public required int Hit2B {get; set;}
		public required int Hit3B {get; set;}
		public required int HR {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
	}
}