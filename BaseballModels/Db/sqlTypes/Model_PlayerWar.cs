namespace Db
{
	public class Model_PlayerWar
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int IsHitter {get; set;}
		public required int PA {get; set;}
		public required float WAR {get; set;}
		public required float OFF {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}

		public Model_PlayerWar Clone()
		{
			return new Model_PlayerWar
			{
				MlbId = this.MlbId,
				Year = this.Year,
				IsHitter = this.IsHitter,
				PA = this.PA,
				WAR = this.WAR,
				OFF = this.OFF,
				DEF = this.DEF,
				BSR = this.BSR,
				
			};
		}
	}
}