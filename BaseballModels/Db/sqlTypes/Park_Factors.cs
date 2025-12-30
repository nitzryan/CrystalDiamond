namespace Db
{
	public class Park_Factors
	{
		public required int StadiumId {get; set;}
		public required int LeagueId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required float RunFactor {get; set;}
		public required float HRFactor {get; set;}

		public Park_Factors Clone()
		{
			return new Park_Factors
			{
				StadiumId = this.StadiumId,
				LeagueId = this.LeagueId,
				LevelId = this.LevelId,
				Year = this.Year,
				RunFactor = this.RunFactor,
				HRFactor = this.HRFactor,
			};
		}
	}
}