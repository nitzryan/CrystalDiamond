namespace SiteDb
{
	public class DraftRank
	{
		public required int TbcId {get; set;}
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public string? Name {get; set;}
		public string? Position {get; set;}
		public required int Year {get; set;}
		public required bool IsEligible {get; set;}
		public required int RankEligible {get; set;}
		public float? WarPre {get; set;}
		public float? WarPost {get; set;}
		public int? DraftPick {get; set;}

		public DraftRank Clone()
		{
			return new DraftRank
			{
				TbcId = this.TbcId,
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				Name = this.Name,
				Position = this.Position,
				Year = this.Year,
				IsEligible = this.IsEligible,
				RankEligible = this.RankEligible,
				WarPre = this.WarPre,
				WarPost = this.WarPost,
				DraftPick = this.DraftPick,
				
			};
		}
	}
}