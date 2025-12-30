namespace Db
{
	public class Model_LeaguePitchingBaselines
	{
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LeagueId {get; set;}
		public required int LevelId {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float HR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}

		public Model_LeaguePitchingBaselines Clone()
		{
			return new Model_LeaguePitchingBaselines
			{
				Year = this.Year,
				Month = this.Month,
				LeagueId = this.LeagueId,
				LevelId = this.LevelId,
				ERA = this.ERA,
				FIP = this.FIP,
				HR = this.HR,
				BB = this.BB,
				HBP = this.HBP,
				K = this.K,
			};
		}
	}
}