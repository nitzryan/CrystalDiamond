namespace Db
{
	public class College_ConfPitcherAvg
	{
		public required int ConfId {get; set;}
		public required int Year {get; set;}
		public required float ERA {get; set;}
		public required float H9 {get; set;}
		public required float HR9 {get; set;}
		public required float BB9 {get; set;}
		public required float K9 {get; set;}
		public required float WHIP {get; set;}

		public College_ConfPitcherAvg Clone()
		{
			return new College_ConfPitcherAvg
			{
				ConfId = this.ConfId,
				Year = this.Year,
				ERA = this.ERA,
				H9 = this.H9,
				HR9 = this.HR9,
				BB9 = this.BB9,
				K9 = this.K9,
				WHIP = this.WHIP,
			};
		}
	}
}