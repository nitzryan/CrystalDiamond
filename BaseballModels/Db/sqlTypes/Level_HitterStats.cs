namespace Db
{
	public class Level_HitterStats
	{
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int AB {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WOBA {get; set;}
		public required float HRPerc {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required float SBRate {get; set;}
		public required float SBPerc {get; set;}

		public Level_HitterStats Clone()
		{
			return new Level_HitterStats
			{
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				AB = this.AB,
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				ISO = this.ISO,
				WOBA = this.WOBA,
				HRPerc = this.HRPerc,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				SBRate = this.SBRate,
				SBPerc = this.SBPerc,
				
			};
		}
	}
}