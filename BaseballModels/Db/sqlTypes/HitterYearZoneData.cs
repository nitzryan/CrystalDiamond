namespace Db
{
	public class HitterYearZoneData
	{
		public required int Year {get; set;}
		public required int MlbId {get; set;}
		public required float ZoneTop {get; set;}
		public required float ZoneBot {get; set;}

		public HitterYearZoneData Clone()
		{
			return new HitterYearZoneData
			{
				Year = this.Year,
				MlbId = this.MlbId,
				ZoneTop = this.ZoneTop,
				ZoneBot = this.ZoneBot,
			};
		}
	}
}