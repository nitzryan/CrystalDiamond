namespace Db
{
	public class League_GameCounts
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int MaxPA {get; set;}

		public League_GameCounts Clone()
		{
			return new League_GameCounts
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				Month = this.Month,
				MaxPA = this.MaxPA,
			};
		}
	}
}