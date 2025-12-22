namespace Db
{
	public class Output_PlayerWarAggregation
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float War0 {get; set;}
		public required float War1 {get; set;}
		public required float War2 {get; set;}
		public required float War3 {get; set;}
		public required float War4 {get; set;}
		public required float War5 {get; set;}
		public required float War6 {get; set;}
		public required float War {get; set;}

		public Output_PlayerWarAggregation Clone()
		{
			return new Output_PlayerWarAggregation
			{
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				Year = this.Year,
				Month = this.Month,
				War0 = this.War0,
				War1 = this.War1,
				War2 = this.War2,
				War3 = this.War3,
				War4 = this.War4,
				War5 = this.War5,
				War6 = this.War6,
				War = this.War,
				
			};
		}
	}
}