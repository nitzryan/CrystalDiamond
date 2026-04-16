namespace Db
{
	public class PitchStatcast
	{
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int PitcherId {get; set;}
		public required int HitterId {get; set;}
		public required int LeagueId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrike {get; set;}
		public required int Outs {get; set;}
		public required DbEnums.BaseOccupancy BaseOccupancy {get; set;}
		public required DbEnums.PitchType PitchType {get; set;}
		public required DbEnums.PitchPaResult PaResult {get; set;}
		public required DbEnums.BaseOccupancy PaResultOccupancy {get; set;}
		public required int PaResultOuts {get; set;}
		public required int PaResultDirectRuns {get; set;}
		public required int RunsAfterPa {get; set;}
		public required DbEnums.PitchResult Result {get; set;}
		public required bool HadSwing {get; set;}
		public required bool HadContact {get; set;}
		public required bool IsInPlay {get; set;}
		public required bool HitIsR {get; set;}
		public required bool PitIsR {get; set;}
		public float? VX {get; set;}
		public float? VY {get; set;}
		public float? VZ {get; set;}
		public float? VStart {get; set;}
		public float? VEnd {get; set;}
		public float? AX {get; set;}
		public float? AY {get; set;}
		public float? AZ {get; set;}
		public float? PfxX {get; set;}
		public float? PfxZ {get; set;}
		public float? BreakAngle {get; set;}
		public float? BreakVertical {get; set;}
		public float? BreakInduced {get; set;}
		public float? BreakHorizontal {get; set;}
		public int? SpinRate {get; set;}
		public int? SpinDirection {get; set;}
		public float? PX {get; set;}
		public float? PZ {get; set;}
		public float? ZoneTop {get; set;}
		public float? ZoneBot {get; set;}
		public float? Extension {get; set;}
		public float? X0 {get; set;}
		public float? Y0 {get; set;}
		public float? Z0 {get; set;}
		public float? PlateTime {get; set;}
		public float? LaunchSpeed {get; set;}
		public float? LaunchAngle {get; set;}
		public float? TotalDist {get; set;}
		public float? HitCoordX {get; set;}
		public float? HitCoordY {get; set;}
		public required float RunValueHitter {get; set;}

		public PitchStatcast Clone()
		{
			return new PitchStatcast
			{
				GameId = this.GameId,
				PitchId = this.PitchId,
				PitcherId = this.PitcherId,
				HitterId = this.HitterId,
				LeagueId = this.LeagueId,
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				CountBalls = this.CountBalls,
				CountStrike = this.CountStrike,
				Outs = this.Outs,
				BaseOccupancy = this.BaseOccupancy,
				PitchType = this.PitchType,
				PaResult = this.PaResult,
				PaResultOccupancy = this.PaResultOccupancy,
				PaResultOuts = this.PaResultOuts,
				PaResultDirectRuns = this.PaResultDirectRuns,
				RunsAfterPa = this.RunsAfterPa,
				Result = this.Result,
				HadSwing = this.HadSwing,
				HadContact = this.HadContact,
				IsInPlay = this.IsInPlay,
				HitIsR = this.HitIsR,
				PitIsR = this.PitIsR,
				VX = this.VX,
				VY = this.VY,
				VZ = this.VZ,
				VStart = this.VStart,
				VEnd = this.VEnd,
				AX = this.AX,
				AY = this.AY,
				AZ = this.AZ,
				PfxX = this.PfxX,
				PfxZ = this.PfxZ,
				BreakAngle = this.BreakAngle,
				BreakVertical = this.BreakVertical,
				BreakInduced = this.BreakInduced,
				BreakHorizontal = this.BreakHorizontal,
				SpinRate = this.SpinRate,
				SpinDirection = this.SpinDirection,
				PX = this.PX,
				PZ = this.PZ,
				ZoneTop = this.ZoneTop,
				ZoneBot = this.ZoneBot,
				Extension = this.Extension,
				X0 = this.X0,
				Y0 = this.Y0,
				Z0 = this.Z0,
				PlateTime = this.PlateTime,
				LaunchSpeed = this.LaunchSpeed,
				LaunchAngle = this.LaunchAngle,
				TotalDist = this.TotalDist,
				HitCoordX = this.HitCoordX,
				HitCoordY = this.HitCoordY,
				RunValueHitter = this.RunValueHitter,
			};
		}
	}
}