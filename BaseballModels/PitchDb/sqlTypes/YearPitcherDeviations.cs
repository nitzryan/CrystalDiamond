namespace PitchDb
{
	public partial class YearPitcherDeviations
	{
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required float ScoreDev {get; set;}
		public required float MeanCountDev {get; set;}
		public required int NumQualified {get; set;}

		public YearPitcherDeviations Clone()
		{
			return new YearPitcherDeviations
			{
				ModelId = this.ModelId,
				Year = this.Year,
				ScoreDev = this.ScoreDev,
				MeanCountDev = this.MeanCountDev,
				NumQualified = this.NumQualified,
			};
		}
	}
}