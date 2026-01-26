namespace Db
{
	public class Output_WarQuants
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Perc10 {get; set;}
		public required float Perc20 {get; set;}
		public required float Perc30 {get; set;}
		public required float Perc40 {get; set;}
		public required float Perc50 {get; set;}
		public required float Perc60 {get; set;}
		public required float Perc70 {get; set;}
		public required float Perc80 {get; set;}
		public required float Perc90 {get; set;}
		public required float Perc95 {get; set;}
		public required float Perc99 {get; set;}

		public Output_WarQuants Clone()
		{
			return new Output_WarQuants
			{
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				Perc10 = this.Perc10,
				Perc20 = this.Perc20,
				Perc30 = this.Perc30,
				Perc40 = this.Perc40,
				Perc50 = this.Perc50,
				Perc60 = this.Perc60,
				Perc70 = this.Perc70,
				Perc80 = this.Perc80,
				Perc90 = this.Perc90,
				Perc95 = this.Perc95,
				Perc99 = this.Perc99,
			};
		}
	}
}