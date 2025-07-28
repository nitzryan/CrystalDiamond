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
	}
}