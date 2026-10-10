namespace Db
{
	public partial class Model_HitterWarValues
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Offset {get; set;}
		public required int PA {get; set;}
		public required float OFF {get; set;}
		public required float DRAA {get; set;}
		public required float DEF {get; set;}
		public required float BSR {get; set;}
		public required float WAR {get; set;}

		public Model_HitterWarValues Clone()
		{
			return new Model_HitterWarValues
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Offset = this.Offset,
				PA = this.PA,
				OFF = this.OFF,
				DRAA = this.DRAA,
				DEF = this.DEF,
				BSR = this.BSR,
				WAR = this.WAR,
			};
		}
	}
}