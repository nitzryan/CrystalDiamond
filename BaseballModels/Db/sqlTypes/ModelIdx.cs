namespace Db
{
	public class ModelIdx
	{
		public required int Id {get; set;}
		public required string PitcherModelName {get; set;}
		public required string HitterModelName {get; set;}
		public required string ModelName {get; set;}

		public ModelIdx Clone()
		{
			return new ModelIdx
			{
				Id = this.Id,
				PitcherModelName = this.PitcherModelName,
				HitterModelName = this.HitterModelName,
				ModelName = this.ModelName,
				
			};
		}
	}
}