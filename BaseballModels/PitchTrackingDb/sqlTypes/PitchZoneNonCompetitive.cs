namespace PitchTrackingDb
{
	public partial class PitchZoneNonCompetitive
	{
		public required int Year {get; set;}
		public required Db.DbEnums.PitchClass PitchClass {get; set;}
		public required bool PitIsR {get; set;}
		public required bool HitIsR {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrikes {get; set;}
		public required int PitchCount {get; set;}
		public required float ExpectedValueHitter {get; set;}

		public PitchZoneNonCompetitive Clone()
		{
			return new PitchZoneNonCompetitive
			{
				Year = this.Year,
				PitchClass = this.PitchClass,
				PitIsR = this.PitIsR,
				HitIsR = this.HitIsR,
				CountBalls = this.CountBalls,
				CountStrikes = this.CountStrikes,
				PitchCount = this.PitchCount,
				ExpectedValueHitter = this.ExpectedValueHitter,
			};
		}
	}
}