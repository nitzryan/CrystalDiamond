namespace Db
{
	public class PitchNonStatcast
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
		public required DbEnums.PitchPaResult PaResult {get; set;}
		public required DbEnums.PitchResult Result {get; set;}
		public required bool HadSwing {get; set;}
		public required bool HadContact {get; set;}
		public required bool IsInPlay {get; set;}
		public required bool HitIsR {get; set;}
		public required bool PitIsR {get; set;}
		public required float RunValueHitter {get; set;}

		public PitchNonStatcast Clone()
		{
			return new PitchNonStatcast
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
				PaResult = this.PaResult,
				Result = this.Result,
				HadSwing = this.HadSwing,
				HadContact = this.HadContact,
				IsInPlay = this.IsInPlay,
				HitIsR = this.HitIsR,
				PitIsR = this.PitIsR,
				RunValueHitter = this.RunValueHitter,
			};
		}
	}
}