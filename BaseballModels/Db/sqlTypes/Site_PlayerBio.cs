namespace Db
{
	public class Site_PlayerBio
	{
		public required int Id {get; set;}
		public required string Position {get; set;}
		public required int IsPitcher {get; set;}
		public required int IsHitter {get; set;}
		public required int HasModel {get; set;}
		public required int ParentId {get; set;}
		public required int LevelId {get; set;}
		public required string Status {get; set;}
		public required int DraftPick {get; set;}
		public required string DraftRound {get; set;}
		public required int DraftBonus {get; set;}
		public required int SigningYear {get; set;}

		public Site_PlayerBio Clone()
		{
			return new Site_PlayerBio
			{
				Id = this.Id,
				Position = this.Position,
				IsPitcher = this.IsPitcher,
				IsHitter = this.IsHitter,
				HasModel = this.HasModel,
				ParentId = this.ParentId,
				LevelId = this.LevelId,
				Status = this.Status,
				DraftPick = this.DraftPick,
				DraftRound = this.DraftRound,
				DraftBonus = this.DraftBonus,
				SigningYear = this.SigningYear,
			};
		}
	}
}