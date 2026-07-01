namespace Db
{
	public class Player_YearlyWPA
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required bool IsHitter {get; set;}
		public required bool IsStarter {get; set;}
		public required int PA {get; set;}
		public required float IP {get; set;}
		public required float LI {get; set;}
		public required float FIP {get; set;}
		public required float ERA {get; set;}
		public required float WPA {get; set;}
		public required float WAR {get; set;}
		public required float OFF {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}
		public required float REP {get; set;}

		public Player_YearlyWPA Clone()
		{
			return new Player_YearlyWPA
			{
				MlbId = this.MlbId,
				Year = this.Year,
				IsHitter = this.IsHitter,
				IsStarter = this.IsStarter,
				PA = this.PA,
				IP = this.IP,
				LI = this.LI,
				FIP = this.FIP,
				ERA = this.ERA,
				WPA = this.WPA,
				WAR = this.WAR,
				OFF = this.OFF,
				DEF = this.DEF,
				BSR = this.BSR,
				REP = this.REP,
			};
		}
	}
}