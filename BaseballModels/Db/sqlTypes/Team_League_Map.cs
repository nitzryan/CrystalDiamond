namespace Db
{
	public class Team_League_Map
	{
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int Year {get; set;}

		public Team_League_Map Clone()
		{
			return new Team_League_Map
			{
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				Year = this.Year,
			};
		}
	}
}