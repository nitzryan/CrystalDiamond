namespace Db
{
	public class Player_CareerStatus
	{
		public required int MlbId {get; set;}
		public required int IsPitcher {get; set;}
		public required int IsHitter {get; set;}
		public required int IsActive {get; set;}
		public int? ServiceReached {get; set;}
		public int? MlbStartYear {get; set;}
		public int? MlbRookieYear {get; set;}
		public int? MlbRookieMonth {get; set;}
		public int? ServiceEndYear {get; set;}
		public int? ServiceLapseYear {get; set;}
		public required int CareerStartYear {get; set;}
		public int? AgedOut {get; set;}
		public int? IgnorePlayer {get; set;}
		public required int HighestLevel {get; set;}
	}
}