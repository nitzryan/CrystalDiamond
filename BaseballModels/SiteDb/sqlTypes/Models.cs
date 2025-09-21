namespace SiteDb
{
	public class Models
	{
		public required int ModelId {get; set;}
		public required string Name {get; set;}

		public Models Clone()
		{
			return new Models
			{
				ModelId = this.ModelId,
				Name = this.Name,
				
			};
		}
	}
}