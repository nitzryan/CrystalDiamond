namespace PitchDb
{
	public class ModelTrainingHistory_PitchValue
	{
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int ModelRun {get; set;}
		public required float TestStuffResult {get; set;}
		public required float TestStuffSwing {get; set;}
		public required float TestStuffInplay {get; set;}
		public required float TestCombinedResult {get; set;}
		public required float TestCombinedSwing {get; set;}
		public required float TestCombinedInplay {get; set;}
		public required float ValSeenStuffResult {get; set;}
		public required float ValSeenStuffSwing {get; set;}
		public required float ValSeenStuffInplay {get; set;}
		public required float ValSeenCombinedResult {get; set;}
		public required float ValSeenCombinedSwing {get; set;}
		public required float ValSeenCombinedInplay {get; set;}
		public required float ValUnseenStuffResult {get; set;}
		public required float ValUnseenStuffSwing {get; set;}
		public required float ValUnseenStuffInplay {get; set;}
		public required float ValUnseenCombinedResult {get; set;}
		public required float ValUnseenCombinedSwing {get; set;}
		public required float ValUnseenCombinedInplay {get; set;}

		public ModelTrainingHistory_PitchValue Clone()
		{
			return new ModelTrainingHistory_PitchValue
			{
				ModelId = this.ModelId,
				Year = this.Year,
				ModelRun = this.ModelRun,
				TestStuffResult = this.TestStuffResult,
				TestStuffSwing = this.TestStuffSwing,
				TestStuffInplay = this.TestStuffInplay,
				TestCombinedResult = this.TestCombinedResult,
				TestCombinedSwing = this.TestCombinedSwing,
				TestCombinedInplay = this.TestCombinedInplay,
				ValSeenStuffResult = this.ValSeenStuffResult,
				ValSeenStuffSwing = this.ValSeenStuffSwing,
				ValSeenStuffInplay = this.ValSeenStuffInplay,
				ValSeenCombinedResult = this.ValSeenCombinedResult,
				ValSeenCombinedSwing = this.ValSeenCombinedSwing,
				ValSeenCombinedInplay = this.ValSeenCombinedInplay,
				ValUnseenStuffResult = this.ValUnseenStuffResult,
				ValUnseenStuffSwing = this.ValUnseenStuffSwing,
				ValUnseenStuffInplay = this.ValUnseenStuffInplay,
				ValUnseenCombinedResult = this.ValUnseenCombinedResult,
				ValUnseenCombinedSwing = this.ValUnseenCombinedSwing,
				ValUnseenCombinedInplay = this.ValUnseenCombinedInplay,
			};
		}
	}
}