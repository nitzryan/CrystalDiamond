namespace Db
{
	public class College_TeamMap
	{
		public required int TeamId {get; set;}
		public required string Name {get; set;}

		public College_TeamMap Clone()
		{
			return new College_TeamMap
			{
				TeamId = this.TeamId,
				Name = this.Name,
			};
		}
	}
}