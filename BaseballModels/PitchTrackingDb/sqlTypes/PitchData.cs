namespace PitchTrackingDb
{
	public class PitchData
	{
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int Year {get; set;}
		public required int PitcherId {get; set;}
		public required Db.DbEnums.PitchClass PitchClass {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrike {get; set;}
		public required bool PitIsR {get; set;}
		public required bool HitIsR {get; set;}
		public required Db.DbEnums.PitchResult Result {get; set;}
		public required bool HadSwing {get; set;}
		public required bool HadContact {get; set;}
		public required bool IsInPlay {get; set;}
		public required float RunValueInPlay {get; set;}
		public required float Vel {get; set;}
		public required float Extension {get; set;}
		public required float BreakInduced {get; set;}
		public required float BreakHorizontal {get; set;}
		public required float SpinRate {get; set;}
		public required float SpinAxis {get; set;}
		public required float ActiveSpin {get; set;}
		public required float VaaAboveAverage {get; set;}
		public required float HaaAboveAverage {get; set;}
		public required float PlateX {get; set;}
		public required float PlateZ {get; set;}

		public PitchData Clone()
		{
			return new PitchData
			{
				GameId = this.GameId,
				PitchId = this.PitchId,
				Year = this.Year,
				PitcherId = this.PitcherId,
				PitchClass = this.PitchClass,
				CountBalls = this.CountBalls,
				CountStrike = this.CountStrike,
				PitIsR = this.PitIsR,
				HitIsR = this.HitIsR,
				Result = this.Result,
				HadSwing = this.HadSwing,
				HadContact = this.HadContact,
				IsInPlay = this.IsInPlay,
				RunValueInPlay = this.RunValueInPlay,
				Vel = this.Vel,
				Extension = this.Extension,
				BreakInduced = this.BreakInduced,
				BreakHorizontal = this.BreakHorizontal,
				SpinRate = this.SpinRate,
				SpinAxis = this.SpinAxis,
				ActiveSpin = this.ActiveSpin,
				VaaAboveAverage = this.VaaAboveAverage,
				HaaAboveAverage = this.HaaAboveAverage,
				PlateX = this.PlateX,
				PlateZ = this.PlateZ,
			};
		}
	}
}