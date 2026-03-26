namespace Db
{
	public class College_TeamMap
	{
		public required int Id {get; set;}
		public required string Name {get; set;}

		public College_TeamMap Clone()
		{
			return new College_TeamMap
			{
				Id = this.Id,
				Name = this.Name,
			};
		}
	}
}