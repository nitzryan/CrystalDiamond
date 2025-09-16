namespace SiteDb
{
	public class HomeDataType
	{
		public required int Type {get; set;}
		public required string Name {get; set;}

		public HomeDataType Clone()
		{
			return new HomeDataType
			{
				Type = this.Type,
				Name = this.Name,
				
			};
		}
	}
}