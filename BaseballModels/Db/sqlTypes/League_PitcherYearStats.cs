namespace Db
{
	public class League_PitcherYearStats
	{
		public required int LeagueId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float ERA {get; set;}
		public required float RA {get; set;}
		public required float FipConstant {get; set;}
		public required float WOBA {get; set;}
		public required float HRPerc {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required float GOPerc {get; set;}
		public required float Avg {get; set;}
		public required float Iso {get; set;}

		public League_PitcherYearStats Clone()
		{
			return new League_PitcherYearStats
			{
				LeagueId = this.LeagueId,
				Year = this.Year,
				Month = this.Month,
				ERA = this.ERA,
				RA = this.RA,
				FipConstant = this.FipConstant,
				WOBA = this.WOBA,
				HRPerc = this.HRPerc,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				GOPerc = this.GOPerc,
				Avg = this.Avg,
				Iso = this.Iso,
				
			};
		}
	}
}