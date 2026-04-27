namespace ModelDb
{
	public class ModelTrainingHistory_PitchValue
	{
		public required int ModelId {get; set;}
		public required int ModelRun {get; set;}
		public required float LossLocation {get; set;}
		public required float LossStuff {get; set;}
		public required float LossCombined {get; set;}
		public required string Arch {get; set;}

		public ModelTrainingHistory_PitchValue Clone()
		{
			return new ModelTrainingHistory_PitchValue
			{
				ModelId = this.ModelId,
				ModelRun = this.ModelRun,
				LossLocation = this.LossLocation,
				LossStuff = this.LossStuff,
				LossCombined = this.LossCombined,
				Arch = this.Arch,
				
			};
		}
	}
}