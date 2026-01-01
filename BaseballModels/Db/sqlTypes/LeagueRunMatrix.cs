namespace Db
{
	public class LeagueRunMatrix
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required string RunExpDict {get; set;}
		public required string FieldOutcomeDict {get; set;}
		public required string BsrAdv1st3rdSingleDict {get; set;}
		public required string BsrAdv2ndHomeSingleDict {get; set;}
		public required string BsrAdv1stHomeDoubleDict {get; set;}
		public required string BsrAvoidForce2ndDict {get; set;}
		public required string BsrAdv2nd3rdGroundoutDict {get; set;}
		public required string BsrAdv1st2ndFlyoutDict {get; set;}
		public required string BsrAdv2nd3rdFlyoutDict {get; set;}
		public required string BsrAdv3rdHomeFlyoutDict {get; set;}
		public required string DoublePlayDict {get; set;}

		public LeagueRunMatrix Clone()
		{
			return new LeagueRunMatrix
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				RunExpDict = this.RunExpDict,
				FieldOutcomeDict = this.FieldOutcomeDict,
				BsrAdv1st3rdSingleDict = this.BsrAdv1st3rdSingleDict,
				BsrAdv2ndHomeSingleDict = this.BsrAdv2ndHomeSingleDict,
				BsrAdv1stHomeDoubleDict = this.BsrAdv1stHomeDoubleDict,
				BsrAvoidForce2ndDict = this.BsrAvoidForce2ndDict,
				BsrAdv2nd3rdGroundoutDict = this.BsrAdv2nd3rdGroundoutDict,
				BsrAdv1st2ndFlyoutDict = this.BsrAdv1st2ndFlyoutDict,
				BsrAdv2nd3rdFlyoutDict = this.BsrAdv2nd3rdFlyoutDict,
				BsrAdv3rdHomeFlyoutDict = this.BsrAdv3rdHomeFlyoutDict,
				DoublePlayDict = this.DoublePlayDict,
			};
		}
	}
}