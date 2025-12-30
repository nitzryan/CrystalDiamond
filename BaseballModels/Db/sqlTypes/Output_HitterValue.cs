namespace Db
{
	public class Output_HitterValue
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float WAR1Year {get; set;}
		public required float OFF1Year {get; set;}
		public required float BSR1Year {get; set;}
		public required float DEF1Year {get; set;}
		public required float WAR2Year {get; set;}
		public required float OFF2Year {get; set;}
		public required float BSR2Year {get; set;}
		public required float DEF2Year {get; set;}
		public required float WAR3Year {get; set;}
		public required float OFF3Year {get; set;}
		public required float BSR3Year {get; set;}
		public required float DEF3Year {get; set;}
		public required float PA1Year {get; set;}
		public required float PA2Year {get; set;}
		public required float PA3Year {get; set;}

		public Output_HitterValue Clone()
		{
			return new Output_HitterValue
			{
				MlbId = this.MlbId,
				Model = this.Model,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				WAR1Year = this.WAR1Year,
				OFF1Year = this.OFF1Year,
				BSR1Year = this.BSR1Year,
				DEF1Year = this.DEF1Year,
				WAR2Year = this.WAR2Year,
				OFF2Year = this.OFF2Year,
				BSR2Year = this.BSR2Year,
				DEF2Year = this.DEF2Year,
				WAR3Year = this.WAR3Year,
				OFF3Year = this.OFF3Year,
				BSR3Year = this.BSR3Year,
				DEF3Year = this.DEF3Year,
				PA1Year = this.PA1Year,
				PA2Year = this.PA2Year,
				PA3Year = this.PA3Year,
			};
		}
	}
}