namespace Db
{
	public class Ranking_Prospect
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required string Model {get; set;}
		public required int Rank {get; set;}

		public Ranking_Prospect Clone()
		{
			return new Ranking_Prospect
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Model = this.Model,
				Rank = this.Rank,
				
			};
		}
	}
}