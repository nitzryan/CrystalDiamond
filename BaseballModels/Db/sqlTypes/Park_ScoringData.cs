namespace Db
{
	public class Park_ScoringData
	{
		public required int TeamId {get; set;}
		public required int Year {get; set;}
		public required int LeagueId {get; set;}
		public required int LevelId {get; set;}
		public required int HomePa {get; set;}
		public required int HomeOuts {get; set;}
		public required int HomeRuns {get; set;}
		public required int HomeHRs {get; set;}
		public required int AwayPa {get; set;}
		public required int AwayOuts {get; set;}
		public required int AwayRuns {get; set;}
		public required int AwayHRs {get; set;}

		public Park_ScoringData Clone()
		{
			return new Park_ScoringData
			{
				TeamId = this.TeamId,
				Year = this.Year,
				LeagueId = this.LeagueId,
				LevelId = this.LevelId,
				HomePa = this.HomePa,
				HomeOuts = this.HomeOuts,
				HomeRuns = this.HomeRuns,
				HomeHRs = this.HomeHRs,
				AwayPa = this.AwayPa,
				AwayOuts = this.AwayOuts,
				AwayRuns = this.AwayRuns,
				AwayHRs = this.AwayHRs,
				
			};
		}
	}
}