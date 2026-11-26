namespace PitchDb
{
	public partial class SituationValues
	{
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required Db.DbEnums.PitchClass PitchClass {get; set;}
		public required int Balls {get; set;}
		public required int Strikes {get; set;}
		public required int NumPitches {get; set;}
		public required int NumStuffPitches {get; set;}
		public required float LocMean {get; set;}
		public required float StuffMean {get; set;}
		public required float SitRuns {get; set;}
		public required float ModelRuns {get; set;}
		public required float ActualRuns {get; set;}

		public SituationValues Clone()
		{
			return new SituationValues
			{
				ModelId = this.ModelId,
				Year = this.Year,
				PitchClass = this.PitchClass,
				Balls = this.Balls,
				Strikes = this.Strikes,
				NumPitches = this.NumPitches,
				NumStuffPitches = this.NumStuffPitches,
				LocMean = this.LocMean,
				StuffMean = this.StuffMean,
				SitRuns = this.SitRuns,
				ModelRuns = this.ModelRuns,
				ActualRuns = this.ActualRuns,
			};
		}
	}
}