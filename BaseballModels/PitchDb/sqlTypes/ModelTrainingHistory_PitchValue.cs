namespace PitchDb
{
	public class ModelTrainingHistory_PitchValue
	{
		public required int ModelId {get; set;}
		public required int ModelRun {get; set;}
		public required float LossLocationResult {get; set;}
		public required float LossLocationSwing {get; set;}
		public required float LossLocationInplay {get; set;}
		public required float LossStuffResult {get; set;}
		public required float LossStuffSwing {get; set;}
		public required float LossStuffInplay {get; set;}
		public required float LossCombinedResult {get; set;}
		public required float LossCombinedSwing {get; set;}
		public required float LossCombinedInplay {get; set;}
		public required string Arch {get; set;}

		public ModelTrainingHistory_PitchValue Clone()
		{
			return new ModelTrainingHistory_PitchValue
			{
				ModelId = this.ModelId,
				ModelRun = this.ModelRun,
				LossLocationResult = this.LossLocationResult,
				LossLocationSwing = this.LossLocationSwing,
				LossLocationInplay = this.LossLocationInplay,
				LossStuffResult = this.LossStuffResult,
				LossStuffSwing = this.LossStuffSwing,
				LossStuffInplay = this.LossStuffInplay,
				LossCombinedResult = this.LossCombinedResult,
				LossCombinedSwing = this.LossCombinedSwing,
				LossCombinedInplay = this.LossCombinedInplay,
				Arch = this.Arch,
				
			};
		}
	}
}