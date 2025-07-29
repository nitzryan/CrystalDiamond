namespace Db
{
	public class Park_Factors
	{
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required float RunFactor {get; set;}
		public required float HRFactor {get; set;}
	}
}