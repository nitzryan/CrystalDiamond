namespace Db
{
	public class GamePlayByPlay
	{
		public int EventId {get; set;}
		public required int GameId {get; set;}
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int HitterId {get; set;}
		public required int PitcherId {get; set;}
		public int? FielderId {get; set;}
		public int? Run1stId {get; set;}
		public int? Run2ndId {get; set;}
		public int? Run3rdId {get; set;}
		public required int StartOuts {get; set;}
		public required int Inning {get; set;}
		public required int IsTop {get; set;}
		public required DbEnums.BaseOccupancy StartBaseOccupancy {get; set;}
		public required int EndOuts {get; set;}
		public required DbEnums.BaseOccupancy EndBaseOccupancy {get; set;}
		public required int RunsScored {get; set;}
		public required int RunsScoredInningAfterEvent {get; set;}
		public required DbEnums.PBP_Events Result {get; set;}
		public int? HitZone {get; set;}
		public DbEnums.PBP_HitHardness? HitHardness {get; set;}
		public DbEnums.PBP_HitTrajectory? HitTrajectory {get; set;}
		public float? HitCoordX {get; set;}
		public float? HitCoordY {get; set;}
		public float? LaunchSpeed {get; set;}
		public float? LaunchAngle {get; set;}
		public float? LaunchDistance {get; set;}
		public int? Run1stOutcome {get; set;}
		public int? Run2ndOutcome {get; set;}
		public int? Run3rdOutcome {get; set;}
		public DbEnums.GameFlags? EventFlag {get; set;}

		public GamePlayByPlay Clone()
		{
			return new GamePlayByPlay
			{
				EventId = this.EventId,
				GameId = this.GameId,
				LeagueId = this.LeagueId,
				Year = this.Year,
				Month = this.Month,
				HitterId = this.HitterId,
				PitcherId = this.PitcherId,
				FielderId = this.FielderId,
				Run1stId = this.Run1stId,
				Run2ndId = this.Run2ndId,
				Run3rdId = this.Run3rdId,
				StartOuts = this.StartOuts,
				Inning = this.Inning,
				IsTop = this.IsTop,
				StartBaseOccupancy = this.StartBaseOccupancy,
				EndOuts = this.EndOuts,
				EndBaseOccupancy = this.EndBaseOccupancy,
				RunsScored = this.RunsScored,
				RunsScoredInningAfterEvent = this.RunsScoredInningAfterEvent,
				Result = this.Result,
				HitZone = this.HitZone,
				HitHardness = this.HitHardness,
				HitTrajectory = this.HitTrajectory,
				HitCoordX = this.HitCoordX,
				HitCoordY = this.HitCoordY,
				LaunchSpeed = this.LaunchSpeed,
				LaunchAngle = this.LaunchAngle,
				LaunchDistance = this.LaunchDistance,
				Run1stOutcome = this.Run1stOutcome,
				Run2ndOutcome = this.Run2ndOutcome,
				Run3rdOutcome = this.Run3rdOutcome,
				EventFlag = this.EventFlag,
			};
		}
	}
}