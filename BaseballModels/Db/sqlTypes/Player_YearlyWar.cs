namespace Db
{
	public class Player_YearlyWar
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int IsHitter {get; set;}
		public required int PA {get; set;}
		public required float WAR {get; set;}
		public required float OFF {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}
	}
}