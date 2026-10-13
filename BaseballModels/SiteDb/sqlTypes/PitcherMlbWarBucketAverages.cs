namespace SiteDb
{
	public partial class PitcherMlbWarBucketAverages
	{
		public required int Month {get; set;}
		public required float WarSP0 {get; set;}
		public required float WarSP1 {get; set;}
		public required float WarSP2 {get; set;}
		public required float WarSP3 {get; set;}
		public required float WarSP4 {get; set;}
		public required float WarSP5 {get; set;}
		public required float WarSP6 {get; set;}
		public required float WarSP7 {get; set;}
		public required float WarRP0 {get; set;}
		public required float WarRP1 {get; set;}
		public required float WarRP2 {get; set;}
		public required float WarRP3 {get; set;}
		public required float WarRP4 {get; set;}
		public required float WarRP5 {get; set;}
		public required float WarRP6 {get; set;}
		public required float WarRP7 {get; set;}

		public PitcherMlbWarBucketAverages Clone()
		{
			return new PitcherMlbWarBucketAverages
			{
				Month = this.Month,
				WarSP0 = this.WarSP0,
				WarSP1 = this.WarSP1,
				WarSP2 = this.WarSP2,
				WarSP3 = this.WarSP3,
				WarSP4 = this.WarSP4,
				WarSP5 = this.WarSP5,
				WarSP6 = this.WarSP6,
				WarSP7 = this.WarSP7,
				WarRP0 = this.WarRP0,
				WarRP1 = this.WarRP1,
				WarRP2 = this.WarRP2,
				WarRP3 = this.WarRP3,
				WarRP4 = this.WarRP4,
				WarRP5 = this.WarRP5,
				WarRP6 = this.WarRP6,
				WarRP7 = this.WarRP7,
			};
		}
	}
}