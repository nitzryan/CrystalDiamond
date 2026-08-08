namespace PitchDb
{
	public class PitchValue
	{
		public required int ModelId {get; set;}
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int PitcherId {get; set;}
		public required float StuffPlus {get; set;}
		public required float StuffRuns {get; set;}
		public required float PitchPlus {get; set;}
		public required float PitchRuns {get; set;}

		public PitchValue Clone()
		{
			return new PitchValue
			{
				ModelId = this.ModelId,
				GameId = this.GameId,
				PitchId = this.PitchId,
				PitcherId = this.PitcherId,
				StuffPlus = this.StuffPlus,
				StuffRuns = this.StuffRuns,
				PitchPlus = this.PitchPlus,
				PitchRuns = this.PitchRuns,
			};
		}
	}
}