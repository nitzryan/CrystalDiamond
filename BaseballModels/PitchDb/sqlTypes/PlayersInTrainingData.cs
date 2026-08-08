namespace PitchDb
{
	public class PlayersInTrainingData
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int ModelRun {get; set;}
		public required bool IsTrain {get; set;}

		public PlayersInTrainingData Clone()
		{
			return new PlayersInTrainingData
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				Year = this.Year,
				ModelRun = this.ModelRun,
				IsTrain = this.IsTrain,
			};
		}
	}
}