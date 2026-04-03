namespace ModelDb
{
	public class ModelIdx_College
	{
		public required int Id {get; set;}
		public required string PitcherModelName {get; set;}
		public required string HitterModelName {get; set;}
		public required string ModelName {get; set;}

		public ModelIdx_College Clone()
		{
			return new ModelIdx_College
			{
				Id = this.Id,
				PitcherModelName = this.PitcherModelName,
				HitterModelName = this.HitterModelName,
				ModelName = this.ModelName,
				
			};
		}
	}
}