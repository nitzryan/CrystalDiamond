namespace ModelDb
{
	public partial class SingleYearPitcherBucketAverages
	{
		public required int Month {get; set;}
		public required float OutsSP0 {get; set;}
		public required float OutsSP1 {get; set;}
		public required float OutsSP2 {get; set;}
		public required float OutsSP3 {get; set;}
		public required float OutsSP4 {get; set;}
		public required float OutsSP5 {get; set;}
		public required float OutsSP6 {get; set;}
		public required float OutsSP7 {get; set;}
		public required float WarSP0 {get; set;}
		public required float WarSP1 {get; set;}
		public required float WarSP2 {get; set;}
		public required float WarSP3 {get; set;}
		public required float WarSP4 {get; set;}
		public required float WarSP5 {get; set;}
		public required float WarSP6 {get; set;}
		public required float WarSP7 {get; set;}
		public required float OutsRP0 {get; set;}
		public required float OutsRP1 {get; set;}
		public required float OutsRP2 {get; set;}
		public required float OutsRP3 {get; set;}
		public required float OutsRP4 {get; set;}
		public required float OutsRP5 {get; set;}
		public required float OutsRP6 {get; set;}
		public required float OutsRP7 {get; set;}
		public required float WarRP0 {get; set;}
		public required float WarRP1 {get; set;}
		public required float WarRP2 {get; set;}
		public required float WarRP3 {get; set;}
		public required float WarRP4 {get; set;}
		public required float WarRP5 {get; set;}
		public required float WarRP6 {get; set;}
		public required float WarRP7 {get; set;}

		public SingleYearPitcherBucketAverages Clone()
		{
			return new SingleYearPitcherBucketAverages
			{
				Month = this.Month,
				OutsSP0 = this.OutsSP0,
				OutsSP1 = this.OutsSP1,
				OutsSP2 = this.OutsSP2,
				OutsSP3 = this.OutsSP3,
				OutsSP4 = this.OutsSP4,
				OutsSP5 = this.OutsSP5,
				OutsSP6 = this.OutsSP6,
				OutsSP7 = this.OutsSP7,
				WarSP0 = this.WarSP0,
				WarSP1 = this.WarSP1,
				WarSP2 = this.WarSP2,
				WarSP3 = this.WarSP3,
				WarSP4 = this.WarSP4,
				WarSP5 = this.WarSP5,
				WarSP6 = this.WarSP6,
				WarSP7 = this.WarSP7,
				OutsRP0 = this.OutsRP0,
				OutsRP1 = this.OutsRP1,
				OutsRP2 = this.OutsRP2,
				OutsRP3 = this.OutsRP3,
				OutsRP4 = this.OutsRP4,
				OutsRP5 = this.OutsRP5,
				OutsRP6 = this.OutsRP6,
				OutsRP7 = this.OutsRP7,
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