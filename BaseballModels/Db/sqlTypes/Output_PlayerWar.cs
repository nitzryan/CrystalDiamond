namespace Db
{
	public class Output_PlayerWar
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float War0 {get; set;}
		public required float War1 {get; set;}
		public required float War2 {get; set;}
		public required float War3 {get; set;}
		public required float War4 {get; set;}
		public required float War5 {get; set;}
		public required float War6 {get; set;}
		public required float War {get; set;}
		public required float Value0 {get; set;}
		public required float Value1 {get; set;}
		public required float Value2 {get; set;}
		public required float Value3 {get; set;}
		public required float Value4 {get; set;}
		public required float Value5 {get; set;}
		public required float Value6 {get; set;}
		public required float Value {get; set;}

		public Output_PlayerWar Clone()
		{
			return new Output_PlayerWar
			{
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				War0 = this.War0,
				War1 = this.War1,
				War2 = this.War2,
				War3 = this.War3,
				War4 = this.War4,
				War5 = this.War5,
				War6 = this.War6,
				War = this.War,
				Value0 = this.Value0,
				Value1 = this.Value1,
				Value2 = this.Value2,
				Value3 = this.Value3,
				Value4 = this.Value4,
				Value5 = this.Value5,
				Value6 = this.Value6,
				Value = this.Value,
				
			};
		}
	}
}