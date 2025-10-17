namespace SiteDb
{
	public class HitterWarRank
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required string Position {get; set;}
		public required float War {get; set;}
		public required int RankWar {get; set;}
		public required float Pa {get; set;}

		public HitterWarRank Clone()
		{
			return new HitterWarRank
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				Year = this.Year,
				Month = this.Month,
				TeamId = this.TeamId,
				Position = this.Position,
				War = this.War,
				RankWar = this.RankWar,
				Pa = this.Pa,
				
			};
		}
	}
}