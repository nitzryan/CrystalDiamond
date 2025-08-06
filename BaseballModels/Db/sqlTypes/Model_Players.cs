namespace Db
{
	public class Model_Players
	{
		public required int MlbId {get; set;}
		public required int IsHitter {get; set;}
		public required int IsPitcher {get; set;}
		public required int LastProspectYear {get; set;}
		public required int LastProspectMonth {get; set;}
		public required int LastMLBSeason {get; set;}
		public required float AgeAtSigningYear {get; set;}

		public Model_Players Clone()
		{
			return new Model_Players
			{
				MlbId = this.MlbId,
				IsHitter = this.IsHitter,
				IsPitcher = this.IsPitcher,
				LastProspectYear = this.LastProspectYear,
				LastProspectMonth = this.LastProspectMonth,
				LastMLBSeason = this.LastMLBSeason,
				AgeAtSigningYear = this.AgeAtSigningYear,
				
			};
		}
	}
}