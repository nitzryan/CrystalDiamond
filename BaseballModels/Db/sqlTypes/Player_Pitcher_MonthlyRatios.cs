namespace Db
{
	public class Player_Pitcher_MonthlyRatios
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required float SPPerc {get; set;}
		public required float GBPercRatio {get; set;}
		public required float ERARatio {get; set;}
		public required float FIPRatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float HRPercRatio {get; set;}
		public required float BBPercRatio {get; set;}
		public required float KPercRatio {get; set;}

		public Player_Pitcher_MonthlyRatios Clone()
		{
			return new Player_Pitcher_MonthlyRatios
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				SPPerc = this.SPPerc,
				GBPercRatio = this.GBPercRatio,
				ERARatio = this.ERARatio,
				FIPRatio = this.FIPRatio,
				WOBARatio = this.WOBARatio,
				HRPercRatio = this.HRPercRatio,
				BBPercRatio = this.BBPercRatio,
				KPercRatio = this.KPercRatio,
				
			};
		}
	}
}