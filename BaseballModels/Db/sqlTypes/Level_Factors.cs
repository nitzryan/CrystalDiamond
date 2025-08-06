namespace Db
{
	public class Level_Factors
	{
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required float RunFactor {get; set;}
		public required float HRFactor {get; set;}

		public Level_Factors Clone()
		{
			return new Level_Factors
			{
				LevelId = this.LevelId,
				Year = this.Year,
				RunFactor = this.RunFactor,
				HRFactor = this.HRFactor,
				
			};
		}
	}
}