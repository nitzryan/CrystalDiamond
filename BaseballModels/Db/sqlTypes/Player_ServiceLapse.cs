namespace Db
{
	public class Player_ServiceLapse
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}

		public Player_ServiceLapse Clone()
		{
			return new Player_ServiceLapse
			{
				MlbId = this.MlbId,
				Year = this.Year,
				
			};
		}
	}
}