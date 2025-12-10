namespace SiteDb
{
	public class TeamRank
	{
		public required int TeamId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int HighestRank {get; set;}
		public required int Top10 {get; set;}
		public required int Top50 {get; set;}
		public required int Top100 {get; set;}
		public required int Top200 {get; set;}
		public required int Top500 {get; set;}
		public required int Rank {get; set;}
		public required float War {get; set;}

		public TeamRank Clone()
		{
			return new TeamRank
			{
				TeamId = this.TeamId,
				ModelId = this.ModelId,
				Year = this.Year,
				Month = this.Month,
				HighestRank = this.HighestRank,
				Top10 = this.Top10,
				Top50 = this.Top50,
				Top100 = this.Top100,
				Top200 = this.Top200,
				Top500 = this.Top500,
				Rank = this.Rank,
				War = this.War,
				
			};
		}
	}
}