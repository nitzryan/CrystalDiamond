namespace Db
{
	public class Player_Hitter_MonthStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
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
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
		public required int GamesC {get; set;}
		public required int Games1B {get; set;}
		public required int Games2B {get; set;}
		public required int Games3B {get; set;}
		public required int GamesSS {get; set;}
		public required int GamesLF {get; set;}
		public required int GamesCF {get; set;}
		public required int GamesRF {get; set;}
		public required int GamesDH {get; set;}
	}
}