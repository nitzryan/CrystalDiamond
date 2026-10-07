namespace SiteDb
{
	public partial class TeamDraftPlayer
	{
		public required int ModelId {get; set;}
		public required int MlbId {get; set;}
		public required bool IsHitter {get; set;}
		public required int DraftYear {get; set;}
		public required int DraftPick {get; set;}
		public required int DraftTeamId {get; set;}
		public required float InitialWar {get; set;}
		public float? WarYear1 {get; set;}
		public float? WarYear2 {get; set;}
		public float? WarYear3 {get; set;}
		public float? WarYear4 {get; set;}
		public float? WarYear5 {get; set;}
		public float? WarYear6 {get; set;}
		public required float CurrentWar {get; set;}
		public int? PostEligibleYear {get; set;}

		public TeamDraftPlayer Clone()
		{
			return new TeamDraftPlayer
			{
				ModelId = this.ModelId,
				MlbId = this.MlbId,
				IsHitter = this.IsHitter,
				DraftYear = this.DraftYear,
				DraftPick = this.DraftPick,
				DraftTeamId = this.DraftTeamId,
				InitialWar = this.InitialWar,
				WarYear1 = this.WarYear1,
				WarYear2 = this.WarYear2,
				WarYear3 = this.WarYear3,
				WarYear4 = this.WarYear4,
				WarYear5 = this.WarYear5,
				WarYear6 = this.WarYear6,
				CurrentWar = this.CurrentWar,
				PostEligibleYear = this.PostEligibleYear,
			};
		}
	}
}