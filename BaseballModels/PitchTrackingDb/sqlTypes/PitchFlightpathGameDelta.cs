namespace PitchTrackingDb
{
	public class PitchFlightpathGameDelta
	{
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int PitcherId {get; set;}
		public required Db.DbEnums.PitchType FastballPitchType {get; set;}
		public required float BreakHoriz_05Delta {get; set;}
		public required float BreakVer_05Delta {get; set;}
		public required float BreakHoriz_10Delta {get; set;}
		public required float BreakVer_10Delta {get; set;}
		public required float BreakHoriz_15Delta {get; set;}
		public required float BreakVer_15Delta {get; set;}
		public required float BreakHoriz_20Delta {get; set;}
		public required float BreakVer_20Delta {get; set;}
		public required float BreakHoriz_25Delta {get; set;}
		public required float BreakVer_25Delta {get; set;}
		public required float BreakHoriz_Delta {get; set;}
		public required float BreakVert_Delta {get; set;}
		public required float BreakIVB_Delta {get; set;}
		public required float Vel_Delta {get; set;}

		public PitchFlightpathGameDelta Clone()
		{
			return new PitchFlightpathGameDelta
			{
				GameId = this.GameId,
				PitchId = this.PitchId,
				PitcherId = this.PitcherId,
				FastballPitchType = this.FastballPitchType,
				BreakHoriz_05Delta = this.BreakHoriz_05Delta,
				BreakVer_05Delta = this.BreakVer_05Delta,
				BreakHoriz_10Delta = this.BreakHoriz_10Delta,
				BreakVer_10Delta = this.BreakVer_10Delta,
				BreakHoriz_15Delta = this.BreakHoriz_15Delta,
				BreakVer_15Delta = this.BreakVer_15Delta,
				BreakHoriz_20Delta = this.BreakHoriz_20Delta,
				BreakVer_20Delta = this.BreakVer_20Delta,
				BreakHoriz_25Delta = this.BreakHoriz_25Delta,
				BreakVer_25Delta = this.BreakVer_25Delta,
				BreakHoriz_Delta = this.BreakHoriz_Delta,
				BreakVert_Delta = this.BreakVert_Delta,
				BreakIVB_Delta = this.BreakIVB_Delta,
				Vel_Delta = this.Vel_Delta,
			};
		}
	}
}