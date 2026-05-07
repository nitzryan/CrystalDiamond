namespace PitchDb
{
	public class YearLeagueDeviations
	{
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required float ActDev {get; set;}
		public required float StuffDev {get; set;}
		public required float LocDev {get; set;}
		public required float PitchDev {get; set;}

		public YearLeagueDeviations Clone()
		{
			return new YearLeagueDeviations
			{
				ModelId = this.ModelId,
				Year = this.Year,
				ActDev = this.ActDev,
				StuffDev = this.StuffDev,
				LocDev = this.LocDev,
				PitchDev = this.PitchDev,
				
			};
		}
	}
}