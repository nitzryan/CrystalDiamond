namespace Db
{
	public class LeagueAverageAge
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float HitterAge {get; set;}
		public required float PitcherAge {get; set;}

		public LeagueAverageAge Clone()
		{
			return new LeagueAverageAge
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				Month = this.Month,
				HitterAge = this.HitterAge,
				PitcherAge = this.PitcherAge,
			};
		}
	}
}