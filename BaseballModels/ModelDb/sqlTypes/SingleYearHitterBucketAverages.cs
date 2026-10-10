namespace ModelDb
{
	public partial class SingleYearHitterBucketAverages
	{
		public required int Month {get; set;}
		public required float War0 {get; set;}
		public required float War1 {get; set;}
		public required float War2 {get; set;}
		public required float War3 {get; set;}
		public required float War4 {get; set;}
		public required float War5 {get; set;}
		public required float War6 {get; set;}
		public required float War7 {get; set;}
		public required float Pa0 {get; set;}
		public required float Pa1 {get; set;}
		public required float Pa2 {get; set;}
		public required float Pa3 {get; set;}
		public required float Pa4 {get; set;}
		public required float Pa5 {get; set;}
		public required float Pa6 {get; set;}

		public SingleYearHitterBucketAverages Clone()
		{
			return new SingleYearHitterBucketAverages
			{
				Month = this.Month,
				War0 = this.War0,
				War1 = this.War1,
				War2 = this.War2,
				War3 = this.War3,
				War4 = this.War4,
				War5 = this.War5,
				War6 = this.War6,
				War7 = this.War7,
				Pa0 = this.Pa0,
				Pa1 = this.Pa1,
				Pa2 = this.Pa2,
				Pa3 = this.Pa3,
				Pa4 = this.Pa4,
				Pa5 = this.Pa5,
				Pa6 = this.Pa6,
			};
		}
	}
}