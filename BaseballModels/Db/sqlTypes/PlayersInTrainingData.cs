namespace Db
{
	public class PlayersInTrainingData
	{
		public required int MlbId {get; set;}
		public required int ModelIdx {get; set;}

		public PlayersInTrainingData Clone()
		{
			return new PlayersInTrainingData
			{
				MlbId = this.MlbId,
				ModelIdx = this.ModelIdx,
			};
		}
	}
}