namespace ModelDb
{
	public class Model_TrainingHistory
	{
		public required string ModelName {get; set;}
		public required bool IsHitter {get; set;}
		public required float TestLoss {get; set;}
		public required float TestLossCollege {get; set;}
		public required int ModelRun {get; set;}

		public Model_TrainingHistory Clone()
		{
			return new Model_TrainingHistory
			{
				ModelName = this.ModelName,
				IsHitter = this.IsHitter,
				TestLoss = this.TestLoss,
				TestLossCollege = this.TestLossCollege,
				ModelRun = this.ModelRun,
				
			};
		}
	}
}