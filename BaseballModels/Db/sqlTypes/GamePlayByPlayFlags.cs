namespace Db
{
	public class GamePlayByPlayFlags
	{
		public required int EventId {get; set;}
		public required DbEnums.GameFlags Flag {get; set;}

		public GamePlayByPlayFlags Clone()
		{
			return new GamePlayByPlayFlags
			{
				EventId = this.EventId,
				Flag = this.Flag,
				
			};
		}
	}
}