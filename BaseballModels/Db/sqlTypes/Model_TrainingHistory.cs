namespace Db
{
	public class Model_TrainingHistory
	{
		public required string ModelName {get; set;}
		public required int Year {get; set;}
		public required int IsHitter {get; set;}
		public required float TestLoss {get; set;}
		public required int NumLayers {get; set;}
		public required int HiddenSize {get; set;}
		public required int ModelIdx {get; set;}

		public Model_TrainingHistory Clone()
		{
			return new Model_TrainingHistory
			{
				ModelName = this.ModelName,
				Year = this.Year,
				IsHitter = this.IsHitter,
				TestLoss = this.TestLoss,
				NumLayers = this.NumLayers,
				HiddenSize = this.HiddenSize,
				ModelIdx = this.ModelIdx,
				
			};
		}
	}
}