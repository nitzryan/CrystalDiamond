namespace SiteDb
{
	public class Player
	{
		public required int MlbId {get; set;}
		public required string FirstName {get; set;}
		public required string LastName {get; set;}
		public required int BirthYear {get; set;}
		public required int BirthMonth {get; set;}
		public required int BirthDate {get; set;}
		public required int StartYear {get; set;}
		public required string Position {get; set;}
		public required string Status {get; set;}
		public required int OrgId {get; set;}
		public int? DraftPick {get; set;}
		public string? DraftRound {get; set;}
		public int? DraftBonus {get; set;}
		public required int IsHitter {get; set;}
		public required int IsPitcher {get; set;}

		public Player Clone()
		{
			return new Player
			{
				MlbId = this.MlbId,
				FirstName = this.FirstName,
				LastName = this.LastName,
				BirthYear = this.BirthYear,
				BirthMonth = this.BirthMonth,
				BirthDate = this.BirthDate,
				StartYear = this.StartYear,
				Position = this.Position,
				Status = this.Status,
				OrgId = this.OrgId,
				DraftPick = this.DraftPick,
				DraftRound = this.DraftRound,
				DraftBonus = this.DraftBonus,
				IsHitter = this.IsHitter,
				IsPitcher = this.IsPitcher,
				
			};
		}
	}
}