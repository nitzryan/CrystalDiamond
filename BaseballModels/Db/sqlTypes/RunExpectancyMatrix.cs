namespace Db
{
	public class RunExpectancyMatrix
	{
		public required int Year {get; set;}
		public required int LeagueId {get; set;}
		public required int CountBalls {get; set;}
		public required int CountStrikes {get; set;}
		public required DbEnums.PitchResult Result {get; set;}
		public required float DeltaRuns {get; set;}

		public RunExpectancyMatrix Clone()
		{
			return new RunExpectancyMatrix
			{
				Year = this.Year,
				LeagueId = this.LeagueId,
				CountBalls = this.CountBalls,
				CountStrikes = this.CountStrikes,
				Result = this.Result,
				DeltaRuns = this.DeltaRuns,
			};
		}
	}
}