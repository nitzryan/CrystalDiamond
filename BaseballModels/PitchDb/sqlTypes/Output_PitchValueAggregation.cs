namespace PitchDb
{
	public class Output_PitchValueAggregation
	{
		public required int Model {get; set;}
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required float AbsValue {get; set;}
		public required float StuffOnly {get; set;}
		public required float LocationOnly {get; set;}
		public required float Combined {get; set;}

		public Output_PitchValueAggregation Clone()
		{
			return new Output_PitchValueAggregation
			{
				Model = this.Model,
				GameId = this.GameId,
				PitchId = this.PitchId,
				AbsValue = this.AbsValue,
				StuffOnly = this.StuffOnly,
				LocationOnly = this.LocationOnly,
				Combined = this.Combined,
				
			};
		}
	}
}