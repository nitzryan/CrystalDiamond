namespace Db
{
	public class LeagueRunMatrix
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required string RunExpDict {get; set;}
		public required string FieldOutcomeDict {get; set;}
		public required string BaserunningDict {get; set;}

		public LeagueRunMatrix Clone()
		{
			return new LeagueRunMatrix
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				RunExpDict = this.RunExpDict,
				FieldOutcomeDict = this.FieldOutcomeDict,
				BaserunningDict = this.BaserunningDict,
				
			};
		}
	}
}