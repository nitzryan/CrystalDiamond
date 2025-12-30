namespace Db
{
	public class Level_GameCounts
	{
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int MaxPA {get; set;}

		public Level_GameCounts Clone()
		{
			return new Level_GameCounts
			{
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				MaxPA = this.MaxPA,
			};
		}
	}
}