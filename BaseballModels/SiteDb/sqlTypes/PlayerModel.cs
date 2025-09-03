namespace SiteDb
{
	public class PlayerModel
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelId {get; set;}
		public required string Probs {get; set;}
		public int? Rank {get; set;}

		public PlayerModel Clone()
		{
			return new PlayerModel
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				ModelId = this.ModelId,
				Probs = this.Probs,
				Rank = this.Rank,
				
			};
		}
	}
}