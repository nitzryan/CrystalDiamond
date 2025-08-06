namespace Db
{
	public class Player_CareerStatus
	{
		public required int MlbId {get; set;}
		public required int IsPitcher {get; set;}
		public required int IsHitter {get; set;}
		public int? IsActive {get; set;}
		public int? ServiceReached {get; set;}
		public int? MlbStartYear {get; set;}
		public int? MlbRookieYear {get; set;}
		public int? MlbRookieMonth {get; set;}
		public int? ServiceEndYear {get; set;}
		public int? ServiceLapseYear {get; set;}
		public int? CareerStartYear {get; set;}
		public int? CareerStartMonth {get; set;}
		public int? AgedOut {get; set;}
		public int? IgnorePlayer {get; set;}
		public int? HighestLevelPitcher {get; set;}
		public int? HighestLevelHitter {get; set;}

		public Player_CareerStatus Clone()
		{
			return new Player_CareerStatus
			{
				MlbId = this.MlbId,
				IsPitcher = this.IsPitcher,
				IsHitter = this.IsHitter,
				IsActive = this.IsActive,
				ServiceReached = this.ServiceReached,
				MlbStartYear = this.MlbStartYear,
				MlbRookieYear = this.MlbRookieYear,
				MlbRookieMonth = this.MlbRookieMonth,
				ServiceEndYear = this.ServiceEndYear,
				ServiceLapseYear = this.ServiceLapseYear,
				CareerStartYear = this.CareerStartYear,
				CareerStartMonth = this.CareerStartMonth,
				AgedOut = this.AgedOut,
				IgnorePlayer = this.IgnorePlayer,
				HighestLevelPitcher = this.HighestLevelPitcher,
				HighestLevelHitter = this.HighestLevelHitter,
				
			};
		}
	}
}