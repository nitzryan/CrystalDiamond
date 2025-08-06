namespace Db
{
	public class League_Factors
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required float RunFactor {get; set;}
		public required float HRFactor {get; set;}

		public League_Factors Clone()
		{
			return new League_Factors
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				RunFactor = this.RunFactor,
				HRFactor = this.HRFactor,
				
			};
		}
	}
}