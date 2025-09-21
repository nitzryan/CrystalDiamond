namespace SiteDb
{
	public class HomeData
	{
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int RankType {get; set;}
		public required int ModelId {get; set;}
		public required int MlbId {get; set;}
		public required string Data {get; set;}
		public required int Rank {get; set;}

		public HomeData Clone()
		{
			return new HomeData
			{
				Year = this.Year,
				Month = this.Month,
				RankType = this.RankType,
				ModelId = this.ModelId,
				MlbId = this.MlbId,
				Data = this.Data,
				Rank = this.Rank,
				
			};
		}
	}
}