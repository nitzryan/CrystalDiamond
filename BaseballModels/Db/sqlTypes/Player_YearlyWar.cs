namespace Db
{
	public class Player_YearlyWar
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int IsHitter {get; set;}
		public required int PA {get; set;}
		public required float WAR_h {get; set;}
		public required float WAR_s {get; set;}
		public required float WAR_r {get; set;}
		public required float OFF {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}
		public required float REP {get; set;}

		public Player_YearlyWar Clone()
		{
			return new Player_YearlyWar
			{
				MlbId = this.MlbId,
				Year = this.Year,
				IsHitter = this.IsHitter,
				PA = this.PA,
				WAR_h = this.WAR_h,
				WAR_s = this.WAR_s,
				WAR_r = this.WAR_r,
				OFF = this.OFF,
				DEF = this.DEF,
				BSR = this.BSR,
				REP = this.REP,
				
			};
		}
	}
}