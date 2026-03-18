namespace Db
{
	public class Output_WarQuantsAggregation
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Perc5 {get; set;}
		public required float Perc15 {get; set;}
		public required float Perc25 {get; set;}
		public required float Perc35 {get; set;}
		public required float Perc50 {get; set;}
		public required float Perc65 {get; set;}
		public required float Perc75 {get; set;}
		public required float Perc85 {get; set;}
		public required float Perc95 {get; set;}
		public required float War {get; set;}

		public Output_WarQuantsAggregation Clone()
		{
			return new Output_WarQuantsAggregation
			{
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				Year = this.Year,
				Month = this.Month,
				Perc5 = this.Perc5,
				Perc15 = this.Perc15,
				Perc25 = this.Perc25,
				Perc35 = this.Perc35,
				Perc50 = this.Perc50,
				Perc65 = this.Perc65,
				Perc75 = this.Perc75,
				Perc85 = this.Perc85,
				Perc95 = this.Perc95,
				War = this.War,
			};
		}
	}
}