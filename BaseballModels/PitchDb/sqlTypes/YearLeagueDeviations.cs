namespace PitchDb
{
	public class YearLeagueDeviations
	{
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Balls {get; set;}
		public required int Strikes {get; set;}
		public required float StuffDev {get; set;}
		public required float PitchDev {get; set;}

		public YearLeagueDeviations Clone()
		{
			return new YearLeagueDeviations
			{
				ModelId = this.ModelId,
				Year = this.Year,
				Balls = this.Balls,
				Strikes = this.Strikes,
				StuffDev = this.StuffDev,
				PitchDev = this.PitchDev,
				
			};
		}
	}
}