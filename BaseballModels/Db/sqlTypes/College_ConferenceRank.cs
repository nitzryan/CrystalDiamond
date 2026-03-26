namespace Db
{
	public class College_ConferenceRank
	{
		public required int ConfId {get; set;}
		public required int Year {get; set;}
		public required float ISR {get; set;}

		public College_ConferenceRank Clone()
		{
			return new College_ConferenceRank
			{
				ConfId = this.ConfId,
				Year = this.Year,
				ISR = this.ISR,
			};
		}
	}
}