namespace ModelDb
{
	public class Models_PitchValue
	{
		public required int Id {get; set;}
		public required string Name {get; set;}

		public Models_PitchValue Clone()
		{
			return new Models_PitchValue
			{
				Id = this.Id,
				Name = this.Name,
				
			};
		}
	}
}