namespace SiteDb
{
	public class PlayerModel
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public required string ProbsWar {get; set;}
		public int? RankWar {get; set;}
		public required bool TrainingBias {get; set;}
		public required DbEnums.TimestepQuality TimestepQuality {get; set;}

		public PlayerModel Clone()
		{
			return new PlayerModel
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				ProbsWar = this.ProbsWar,
				RankWar = this.RankWar,
				TrainingBias = this.TrainingBias,
				TimestepQuality = this.TimestepQuality,
				
			};
		}
	}
}