namespace Db
{
	public class Player_ServiceTime
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int ServiceYear {get; set;}
		public required int ServiceDays {get; set;}

		public Player_ServiceTime Clone()
		{
			return new Player_ServiceTime
			{
				MlbId = this.MlbId,
				Year = this.Year,
				ServiceYear = this.ServiceYear,
				ServiceDays = this.ServiceDays,
				
			};
		}
	}
}