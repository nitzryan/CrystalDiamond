namespace ModelDb
{
	public class ModelId
	{
		public required int Id {get; set;}
		public required string ModelName {get; set;}

		public ModelId Clone()
		{
			return new ModelId
			{
				Id = this.Id,
				ModelName = this.ModelName,
				
			};
		}
	}
}