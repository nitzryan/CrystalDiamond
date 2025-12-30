namespace Db
{
	public class Player_Pitcher_YearAdvanced
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required int BF {get; set;}
		public required int Outs {get; set;}
		public required float SPPerc {get; set;}
		public required float GBRatio {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float KPerc {get; set;}
		public required float BBPerc {get; set;}
		public required int HR {get; set;}
		public required float WOBA {get; set;}

		public Player_Pitcher_YearAdvanced Clone()
		{
			return new Player_Pitcher_YearAdvanced
			{
				MlbId = this.MlbId,
				LevelId = this.LevelId,
				Year = this.Year,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				BF = this.BF,
				Outs = this.Outs,
				SPPerc = this.SPPerc,
				GBRatio = this.GBRatio,
				ERA = this.ERA,
				FIP = this.FIP,
				KPerc = this.KPerc,
				BBPerc = this.BBPerc,
				HR = this.HR,
				WOBA = this.WOBA,
			};
		}
	}
}