namespace SiteDb
{
	public class PlayerYearPositions
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int IsHitter {get; set;}
		public required string Position {get; set;}

		public PlayerYearPositions Clone()
		{
			return new PlayerYearPositions
			{
				MlbId = this.MlbId,
				Year = this.Year,
				IsHitter = this.IsHitter,
				Position = this.Position,
				
			};
		}
	}
}