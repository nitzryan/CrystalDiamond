namespace SiteDb
{
	public partial class TeamDraftOverview
	{
		public required int ModelId {get; set;}
		public required int DraftYear {get; set;}
		public required int EvaluationYear {get; set;}
		public required int TeamId {get; set;}
		public required float HitterCapital {get; set;}
		public required float PitcherCapital {get; set;}
		public required float HitterValue {get; set;}
		public required float PitcherValue {get; set;}

		public TeamDraftOverview Clone()
		{
			return new TeamDraftOverview
			{
				ModelId = this.ModelId,
				DraftYear = this.DraftYear,
				EvaluationYear = this.EvaluationYear,
				TeamId = this.TeamId,
				HitterCapital = this.HitterCapital,
				PitcherCapital = this.PitcherCapital,
				HitterValue = this.HitterValue,
				PitcherValue = this.PitcherValue,
			};
		}
	}
}