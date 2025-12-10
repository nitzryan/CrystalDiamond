namespace SiteDb
{
	public class PlayerRank
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int IsHitter {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required string Position {get; set;}
		public required float War {get; set;}
		public required int RankWar {get; set;}
		public required int TeamRankWar {get; set;}
		public required int HighestLevel {get; set;}

		public PlayerRank Clone()
		{
			return new PlayerRank
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				Year = this.Year,
				Month = this.Month,
				TeamId = this.TeamId,
				Position = this.Position,
				War = this.War,
				RankWar = this.RankWar,
				TeamRankWar = this.TeamRankWar,
				HighestLevel = this.HighestLevel,
				
			};
		}
	}
}