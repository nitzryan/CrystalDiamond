namespace Db
{
	public class Player_MonthlyWar
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int PA {get; set;}
		public required float IP_SP {get; set;}
		public required float IP_RP {get; set;}
		public required float WAR_h {get; set;}
		public required float WAR_s {get; set;}
		public required float WAR_r {get; set;}
		public required float OFF {get; set;}
		public required float DRAA {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}
		public required float REP {get; set;}

		public Player_MonthlyWar Clone()
		{
			return new Player_MonthlyWar
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				PA = this.PA,
				IP_SP = this.IP_SP,
				IP_RP = this.IP_RP,
				WAR_h = this.WAR_h,
				WAR_s = this.WAR_s,
				WAR_r = this.WAR_r,
				OFF = this.OFF,
				DRAA = this.DRAA,
				DEF = this.DEF,
				BSR = this.BSR,
				REP = this.REP,
			};
		}
	}
}