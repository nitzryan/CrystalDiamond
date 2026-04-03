namespace ModelDb
{
	public class Model_TrainingHistory_College
	{
		public required string ModelName {get; set;}
		public required int IsHitter {get; set;}
		public required float TestLoss {get; set;}
		public required int ModelIdx {get; set;}
		public required int NumLayers {get; set;}
		public required int HiddenSize {get; set;}

		public Model_TrainingHistory_College Clone()
		{
			return new Model_TrainingHistory_College
			{
				ModelName = this.ModelName,
				IsHitter = this.IsHitter,
				TestLoss = this.TestLoss,
				ModelIdx = this.ModelIdx,
				NumLayers = this.NumLayers,
				HiddenSize = this.HiddenSize,
				
			};
		}
	}
}