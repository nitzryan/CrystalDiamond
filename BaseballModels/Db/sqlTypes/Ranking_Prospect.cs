namespace Db
{
	public class Ranking_Prospect
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelIdx {get; set;}
		public required int IsHitter {get; set;}
		public required int Rank {get; set;}

		public Ranking_Prospect Clone()
		{
			return new Ranking_Prospect
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				ModelIdx = this.ModelIdx,
				IsHitter = this.IsHitter,
				Rank = this.Rank,
				
			};
		}
	}
}