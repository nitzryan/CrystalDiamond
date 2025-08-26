namespace Db
{
	public class Player_OrgMap
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Day {get; set;}
		public required int ParentOrgId {get; set;}

		public Player_OrgMap Clone()
		{
			return new Player_OrgMap
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Day = this.Day,
				ParentOrgId = this.ParentOrgId,
				
			};
		}
	}
}