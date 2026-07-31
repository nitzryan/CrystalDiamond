namespace ModelDb
{
	public class WarBucketAverages
	{
		public required bool IsHitter {get; set;}
		public required float War1 {get; set;}
		public required float War2 {get; set;}
		public required float War3 {get; set;}
		public required float War4 {get; set;}
		public required float War5 {get; set;}
		public required float War6 {get; set;}

		public WarBucketAverages Clone()
		{
			return new WarBucketAverages
			{
				IsHitter = this.IsHitter,
				War1 = this.War1,
				War2 = this.War2,
				War3 = this.War3,
				War4 = this.War4,
				War5 = this.War5,
				War6 = this.War6,
			};
		}
	}
}