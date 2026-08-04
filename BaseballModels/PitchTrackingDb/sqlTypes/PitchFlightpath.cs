namespace PitchTrackingDb
{
	public class PitchFlightpath
	{
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int Year {get; set;}
		public required int PitcherId {get; set;}
		public required Db.DbEnums.PitchClass PitchClass {get; set;}
		public required float BreakHoriz_05 {get; set;}
		public required float BreakVer_05 {get; set;}
		public required float BreakHoriz_10 {get; set;}
		public required float BreakVer_10 {get; set;}
		public required float BreakHoriz_15 {get; set;}
		public required float BreakVer_15 {get; set;}
		public required float BreakHoriz_20 {get; set;}
		public required float BreakVer_20 {get; set;}
		public required float BreakHoriz_25 {get; set;}
		public required float BreakVer_25 {get; set;}
		public required float HAA {get; set;}
		public required float VAA {get; set;}
		public required float TrackingError {get; set;}

		public PitchFlightpath Clone()
		{
			return new PitchFlightpath
			{
				GameId = this.GameId,
				PitchId = this.PitchId,
				Year = this.Year,
				PitcherId = this.PitcherId,
				PitchClass = this.PitchClass,
				BreakHoriz_05 = this.BreakHoriz_05,
				BreakVer_05 = this.BreakVer_05,
				BreakHoriz_10 = this.BreakHoriz_10,
				BreakVer_10 = this.BreakVer_10,
				BreakHoriz_15 = this.BreakHoriz_15,
				BreakVer_15 = this.BreakVer_15,
				BreakHoriz_20 = this.BreakHoriz_20,
				BreakVer_20 = this.BreakVer_20,
				BreakHoriz_25 = this.BreakHoriz_25,
				BreakVer_25 = this.BreakVer_25,
				HAA = this.HAA,
				VAA = this.VAA,
				TrackingError = this.TrackingError,
			};
		}
	}
}