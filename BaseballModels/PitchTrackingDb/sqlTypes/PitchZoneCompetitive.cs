namespace PitchTrackingDb
{
	public partial class PitchZoneCompetitive
	{
		public required int Year {get; set;}
		public required Db.DbEnums.PitchClass PitchClass {get; set;}
		public required bool PitIsR {get; set;}
		public required bool HitIsR {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrikes {get; set;}
		public required int ZoneRow {get; set;}
		public required int ZoneCol {get; set;}
		public required int PitchCount {get; set;}
		public required int SwingCount {get; set;}
		public required bool IsCompetitive {get; set;}
		public required bool HasEnoughSample {get; set;}
		public required float ExpectedValueHitter {get; set;}

		public PitchZoneCompetitive Clone()
		{
			return new PitchZoneCompetitive
			{
				Year = this.Year,
				PitchClass = this.PitchClass,
				PitIsR = this.PitIsR,
				HitIsR = this.HitIsR,
				CountBalls = this.CountBalls,
				CountStrikes = this.CountStrikes,
				ZoneRow = this.ZoneRow,
				ZoneCol = this.ZoneCol,
				PitchCount = this.PitchCount,
				SwingCount = this.SwingCount,
				IsCompetitive = this.IsCompetitive,
				HasEnoughSample = this.HasEnoughSample,
				ExpectedValueHitter = this.ExpectedValueHitter,
			};
		}
	}
}