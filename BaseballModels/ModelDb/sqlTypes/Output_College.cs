namespace ModelDb
{
	public class Output_College
	{
		public required int TbcId {get; set;}
		public int? MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Draft0 {get; set;}
		public required float Draft1 {get; set;}
		public required float Draft2 {get; set;}
		public required float Draft3 {get; set;}
		public required float Draft4 {get; set;}
		public required float Draft5 {get; set;}
		public required float Draft6 {get; set;}
		public required float Draft {get; set;}

		public Output_College Clone()
		{
			return new Output_College
			{
				TbcId = this.TbcId,
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				Draft0 = this.Draft0,
				Draft1 = this.Draft1,
				Draft2 = this.Draft2,
				Draft3 = this.Draft3,
				Draft4 = this.Draft4,
				Draft5 = this.Draft5,
				Draft6 = this.Draft6,
				Draft = this.Draft,
				
			};
		}
	}
}