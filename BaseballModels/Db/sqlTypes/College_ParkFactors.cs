namespace Db
{
	public class College_ParkFactors
	{
		public required int TeamId {get; set;}
		public required int Year {get; set;}
		public required float RunFactor {get; set;}

		public College_ParkFactors Clone()
		{
			return new College_ParkFactors
			{
				TeamId = this.TeamId,
				Year = this.Year,
				RunFactor = this.RunFactor,
			};
		}
	}
}