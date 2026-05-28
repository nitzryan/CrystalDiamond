namespace Db
{
	public class HitterStatcastMonth
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required bool IsValid {get; set;}
		public required int BattedBallEvents {get; set;}
		public required float AvgExitVelo {get; set;}
		public required float PeakExitVelo {get; set;}
		public required int NumPitches {get; set;}
		public required int NumSwings {get; set;}
		public required float ChasePerc {get; set;}
		public required float WhiffPerc {get; set;}
		public required float ZoneSwingPerc {get; set;}
		public required float ZoneContactPerc {get; set;}
		public required int NumFastballs {get; set;}
		public required float FastballContactPerc {get; set;}
		public required int NumBreaking {get; set;}
		public required float BreakingContactPerc {get; set;}
		public required int NumChangeup {get; set;}
		public required float ChangeupContactPerc {get; set;}

		public HitterStatcastMonth Clone()
		{
			return new HitterStatcastMonth
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				IsValid = this.IsValid,
				BattedBallEvents = this.BattedBallEvents,
				AvgExitVelo = this.AvgExitVelo,
				PeakExitVelo = this.PeakExitVelo,
				NumPitches = this.NumPitches,
				NumSwings = this.NumSwings,
				ChasePerc = this.ChasePerc,
				WhiffPerc = this.WhiffPerc,
				ZoneSwingPerc = this.ZoneSwingPerc,
				ZoneContactPerc = this.ZoneContactPerc,
				NumFastballs = this.NumFastballs,
				FastballContactPerc = this.FastballContactPerc,
				NumBreaking = this.NumBreaking,
				BreakingContactPerc = this.BreakingContactPerc,
				NumChangeup = this.NumChangeup,
				ChangeupContactPerc = this.ChangeupContactPerc,
			};
		}
	}
}