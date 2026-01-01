namespace Db
{
	public class Player_Hitter_YearBaserunning
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int LevelId {get; set;}
		public required int LeagueId {get; set;}
		public required int TeamId {get; set;}
		public required float RSB {get; set;}
		public required float RSBNorm {get; set;}
		public required float RUBR {get; set;}
		public required float RGIDP {get; set;}
		public required float RBSR {get; set;}
		public required int TimesOnFirst {get; set;}
		public required int TimesOnBase {get; set;}

		public Player_Hitter_YearBaserunning Clone()
		{
			return new Player_Hitter_YearBaserunning
			{
				MlbId = this.MlbId,
				Year = this.Year,
				LevelId = this.LevelId,
				LeagueId = this.LeagueId,
				TeamId = this.TeamId,
				RSB = this.RSB,
				RSBNorm = this.RSBNorm,
				RUBR = this.RUBR,
				RGIDP = this.RGIDP,
				RBSR = this.RBSR,
				TimesOnFirst = this.TimesOnFirst,
				TimesOnBase = this.TimesOnBase,
			};
		}
	}
}