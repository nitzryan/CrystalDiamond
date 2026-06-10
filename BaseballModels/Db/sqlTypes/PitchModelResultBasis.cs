namespace Db
{
	public class PitchModelResultBasis
	{
		public required int Year {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrikes {get; set;}
		public required DbEnums.PitchModelOutputType OutputType {get; set;}
		public required float Min {get; set;}
		public required float Perc5 {get; set;}
		public required float Avg {get; set;}
		public required float Median {get; set;}
		public required float Perc95 {get; set;}
		public required float Max {get; set;}

		public PitchModelResultBasis Clone()
		{
			return new PitchModelResultBasis
			{
				Year = this.Year,
				CountBalls = this.CountBalls,
				CountStrikes = this.CountStrikes,
				OutputType = this.OutputType,
				Min = this.Min,
				Perc5 = this.Perc5,
				Avg = this.Avg,
				Median = this.Median,
				Perc95 = this.Perc95,
				Max = this.Max,
			};
		}
	}
}