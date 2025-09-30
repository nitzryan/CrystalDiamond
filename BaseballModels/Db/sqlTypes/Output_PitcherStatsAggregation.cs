namespace Db
{
	public class Output_PitcherStatsAggregation
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int LevelId {get; set;}
		public required float GB {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float HR {get; set;}
		public required float BB {get; set;}
		public required float K {get; set;}
		public required float SP {get; set;}

		public Output_PitcherStatsAggregation Clone()
		{
			return new Output_PitcherStatsAggregation
			{
				MlbId = this.MlbId,
				Model = this.Model,
				LevelId = this.LevelId,
				GB = this.GB,
				ERA = this.ERA,
				FIP = this.FIP,
				HR = this.HR,
				BB = this.BB,
				K = this.K,
				SP = this.SP,
				
			};
		}
	}
}