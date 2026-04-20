namespace ModelDb
{
	public class ModelIdx
	{
		public required int Id {get; set;}
		public required string ModelName {get; set;}

		public ModelIdx Clone()
		{
			return new ModelIdx
			{
				Id = this.Id,
				ModelName = this.ModelName,
				
			};
		}
	}
}