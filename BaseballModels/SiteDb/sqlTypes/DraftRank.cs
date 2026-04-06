namespace SiteDb
{
	public class DraftRank
	{
		public required int TbcId {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public required int Year {get; set;}
		public required bool IsEligible {get; set;}
		public required int RankEligible {get; set;}
		public required float Value {get; set;}

		public DraftRank Clone()
		{
			return new DraftRank
			{
				TbcId = this.TbcId,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				Year = this.Year,
				IsEligible = this.IsEligible,
				RankEligible = this.RankEligible,
				Value = this.Value,
				
			};
		}
	}
}