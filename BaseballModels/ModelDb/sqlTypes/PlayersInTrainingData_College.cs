namespace ModelDb
{
	public class PlayersInTrainingData_College
	{
		public required int TbcId {get; set;}
		public required int ModelIdx {get; set;}

		public PlayersInTrainingData_College Clone()
		{
			return new PlayersInTrainingData_College
			{
				TbcId = this.TbcId,
				ModelIdx = this.ModelIdx,
				
			};
		}
	}
}