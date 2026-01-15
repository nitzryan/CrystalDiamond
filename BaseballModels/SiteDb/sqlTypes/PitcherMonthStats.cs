namespace SiteDb
{
	public class PitcherMonthStats
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required string IP {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float ERAMinus {get; set;}
		public required float FIPMinus {get; set;}
		public required float HR9 {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required float GOPerc {get; set;}

		public PitcherMonthStats Clone()
		{
			return new PitcherMonthStats
			{
				MlbId = this.MlbId,
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				IP = this.IP,
				ERA = this.ERA,
				FIP = this.FIP,
				ERAMinus = this.ERAMinus,
				FIPMinus = this.FIPMinus,
				HR9 = this.HR9,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				GOPerc = this.GOPerc,
				
			};
		}
	}
}