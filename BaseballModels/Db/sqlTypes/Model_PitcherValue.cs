namespace Db
{
	public class Model_PitcherValue
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float WarSP1Year {get; set;}
		public required float WarSP2Year {get; set;}
		public required float WarSP3Year {get; set;}
		public required float WarRP1Year {get; set;}
		public required float WarRP2Year {get; set;}
		public required float WarRP3Year {get; set;}
		public required float IPSP1Year {get; set;}
		public required float IPSP2Year {get; set;}
		public required float IPSP3Year {get; set;}
		public required float IPRP1Year {get; set;}
		public required float IPRP2Year {get; set;}
		public required float IPRP3Year {get; set;}

		public Model_PitcherValue Clone()
		{
			return new Model_PitcherValue
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				WarSP1Year = this.WarSP1Year,
				WarSP2Year = this.WarSP2Year,
				WarSP3Year = this.WarSP3Year,
				WarRP1Year = this.WarRP1Year,
				WarRP2Year = this.WarRP2Year,
				WarRP3Year = this.WarRP3Year,
				IPSP1Year = this.IPSP1Year,
				IPSP2Year = this.IPSP2Year,
				IPSP3Year = this.IPSP3Year,
				IPRP1Year = this.IPRP1Year,
				IPRP2Year = this.IPRP2Year,
				IPRP3Year = this.IPRP3Year,
				
			};
		}
	}
}