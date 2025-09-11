namespace Db
{
	public class PlayersInTrainingData
	{
		public required int MlbId {get; set;}
		public required int IsHitter {get; set;}

		public PlayersInTrainingData Clone()
		{
			return new PlayersInTrainingData
			{
				MlbId = this.MlbId,
				IsHitter = this.IsHitter,
				
			};
		}
	}
}