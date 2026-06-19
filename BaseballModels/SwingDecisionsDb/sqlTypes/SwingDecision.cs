namespace SwingDecisionsDb
{
	public class SwingDecision
	{
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int HitterId {get; set;}
		public required int PitcherId {get; set;}
		public required int LevelId {get; set;}
		public required Db.DbEnums.PitchType PitchType {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrikes {get; set;}
		public required int Outs {get; set;}
		public required Db.DbEnums.BaseOccupancy BaseOccupancy {get; set;}
		public required bool DidSwing {get; set;}
		public required float ProbSwing {get; set;}
		public required float ValueSwing {get; set;}
		public required float ValueNoSwing {get; set;}
		public required float Value {get; set;}

		public SwingDecision Clone()
		{
			return new SwingDecision
			{
				GameId = this.GameId,
				PitchId = this.PitchId,
				Year = this.Year,
				Month = this.Month,
				HitterId = this.HitterId,
				PitcherId = this.PitcherId,
				LevelId = this.LevelId,
				PitchType = this.PitchType,
				CountBalls = this.CountBalls,
				CountStrikes = this.CountStrikes,
				Outs = this.Outs,
				BaseOccupancy = this.BaseOccupancy,
				DidSwing = this.DidSwing,
				ProbSwing = this.ProbSwing,
				ValueSwing = this.ValueSwing,
				ValueNoSwing = this.ValueNoSwing,
				Value = this.Value,
			};
		}
	}
}