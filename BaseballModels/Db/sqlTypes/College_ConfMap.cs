namespace Db
{
	public class College_ConfMap
	{
		public required int Id {get; set;}
		public required string Name {get; set;}

		public College_ConfMap Clone()
		{
			return new College_ConfMap
			{
				Id = this.Id,
				Name = this.Name,
			};
		}
	}
}