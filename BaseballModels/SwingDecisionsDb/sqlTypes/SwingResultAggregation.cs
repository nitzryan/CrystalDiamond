namespace SwingDecisionsDb
{
	public class SwingResultAggregation
	{
		public int? HitterId {get; set;}
		public int? PitcherId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public int? Month {get; set;}
		public required SwingDbEnums.PitchGroup PitchGroup {get; set;}
		public required int NumSwings {get; set;}
		public required float ValueSwings {get; set;}
		public required float ValuePer100Swings {get; set;}
		public required int NumNonSwings {get; set;}
		public required float ValueNonSwings {get; set;}
		public required float ValuePer100NonSwings {get; set;}

		public SwingResultAggregation Clone()
		{
			return new SwingResultAggregation
			{
				HitterId = this.HitterId,
				PitcherId = this.PitcherId,
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				PitchGroup = this.PitchGroup,
				NumSwings = this.NumSwings,
				ValueSwings = this.ValueSwings,
				ValuePer100Swings = this.ValuePer100Swings,
				NumNonSwings = this.NumNonSwings,
				ValueNonSwings = this.ValueNonSwings,
				ValuePer100NonSwings = this.ValuePer100NonSwings,
			};
		}
	}
}