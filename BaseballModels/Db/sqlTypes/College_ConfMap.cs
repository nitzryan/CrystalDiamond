namespace Db
{
	public class College_ConfMap
	{
		public required int ConfId {get; set;}
		public required string Name {get; set;}

		public College_ConfMap Clone()
		{
			return new College_ConfMap
			{
				ConfId = this.ConfId,
				Name = this.Name,
			};
		}
	}
}