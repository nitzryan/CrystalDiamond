namespace Db
{
	public class Player_Hitter_MonthBaserunning
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int LeagueId {get; set;}
		public required float RSB {get; set;}
		public required float RUBR {get; set;}
		public required float RGIDP {get; set;}
		public required int TimesOnFirst {get; set;}
		public required int TimesOnBase {get; set;}

		public Player_Hitter_MonthBaserunning Clone()
		{
			return new Player_Hitter_MonthBaserunning
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				LeagueId = this.LeagueId,
				RSB = this.RSB,
				RUBR = this.RUBR,
				RGIDP = this.RGIDP,
				TimesOnFirst = this.TimesOnFirst,
				TimesOnBase = this.TimesOnBase,
			};
		}
	}
}