namespace SiteDb
{
	public class PitcherWarRank
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required float SpWar {get; set;}
		public required float SpIP {get; set;}
		public required float RpWar {get; set;}
		public required float RpIP {get; set;}
		public int? SpRank {get; set;}
		public int? RpRank {get; set;}

		public PitcherWarRank Clone()
		{
			return new PitcherWarRank
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				Year = this.Year,
				Month = this.Month,
				TeamId = this.TeamId,
				SpWar = this.SpWar,
				SpIP = this.SpIP,
				RpWar = this.RpWar,
				RpIP = this.RpIP,
				SpRank = this.SpRank,
				RpRank = this.RpRank,
				
			};
		}
	}
}