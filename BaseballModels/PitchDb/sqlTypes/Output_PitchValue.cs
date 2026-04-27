namespace ModelDb
{
	public class Output_PitchValue
	{
		public required int Model {get; set;}
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int ModelRun {get; set;}
		public required float AbsValue {get; set;}
		public required float StuffOnly {get; set;}
		public required float LocationOnly {get; set;}
		public required float Combined {get; set;}

		public Output_PitchValue Clone()
		{
			return new Output_PitchValue
			{
				Model = this.Model,
				GameId = this.GameId,
				PitchId = this.PitchId,
				ModelRun = this.ModelRun,
				AbsValue = this.AbsValue,
				StuffOnly = this.StuffOnly,
				LocationOnly = this.LocationOnly,
				Combined = this.Combined,
				
			};
		}
	}
}