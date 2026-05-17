namespace ModelDb
{
	public class PlayersInTrainingData
	{
		public required int MlbId {get; set;}
		public required int TbcId {get; set;}
		public required int ModelIdx {get; set;}
		public required int ModelRun {get; set;}
		public required bool IsHitter {get; set;}
		public required bool IsTrain {get; set;}

		public PlayersInTrainingData Clone()
		{
			return new PlayersInTrainingData
			{
				MlbId = this.MlbId,
				TbcId = this.TbcId,
				ModelIdx = this.ModelIdx,
				ModelRun = this.ModelRun,
				IsHitter = this.IsHitter,
				IsTrain = this.IsTrain,
				
			};
		}
	}
}