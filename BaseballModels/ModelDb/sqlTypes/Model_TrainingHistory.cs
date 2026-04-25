namespace ModelDb
{
	public class Model_TrainingHistory
	{
		public required string ModelName {get; set;}
		public required int IsHitter {get; set;}
		public required float TestLoss {get; set;}
		public required float TestLossCollege {get; set;}
		public required int ModelIdx {get; set;}
		public required int ProNumLayers {get; set;}
		public required int ProHiddenSize {get; set;}
		public required int ColNumLayers {get; set;}
		public required int ColHiddenSize {get; set;}

		public Model_TrainingHistory Clone()
		{
			return new Model_TrainingHistory
			{
				ModelName = this.ModelName,
				IsHitter = this.IsHitter,
				TestLoss = this.TestLoss,
				TestLossCollege = this.TestLossCollege,
				ModelIdx = this.ModelIdx,
				ProNumLayers = this.ProNumLayers,
				ProHiddenSize = this.ProHiddenSize,
				ColNumLayers = this.ColNumLayers,
				ColHiddenSize = this.ColHiddenSize,
				
			};
		}
	}
}