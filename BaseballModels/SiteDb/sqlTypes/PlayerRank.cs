namespace SiteDb
{
	public class PlayerRank
	{
		public required int MlbId {get; set;}
		public required int ModelName {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float War {get; set;}
		public required int TeamId {get; set;}
		public required string Position {get; set;}
		public required string Name {get; set;}
		public required int Rank {get; set;}
		public required int TeamRank {get; set;}

		public PlayerRank Clone()
		{
			return new PlayerRank
			{
				MlbId = this.MlbId,
				ModelName = this.ModelName,
				Year = this.Year,
				Month = this.Month,
				War = this.War,
				TeamId = this.TeamId,
				Position = this.Position,
				Name = this.Name,
				Rank = this.Rank,
				TeamRank = this.TeamRank,
				
			};
		}
	}
}