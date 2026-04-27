namespace PitchDb
{
	public class PitcherStuff
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int GameId {get; set;}
		public required Db.DbEnums.PitchType PitchType {get; set;}
		public required DbEnums.Scenario Scenario {get; set;}
		public required int NumPitches {get; set;}
		public required float ValueStuff {get; set;}
		public required float ValueLoc {get; set;}
		public required float ValueCombined {get; set;}
		public required float Vel {get; set;}
		public required float BreakHoriz {get; set;}
		public required float BreakVert {get; set;}

		public PitcherStuff Clone()
		{
			return new PitcherStuff
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				GameId = this.GameId,
				PitchType = this.PitchType,
				Scenario = this.Scenario,
				NumPitches = this.NumPitches,
				ValueStuff = this.ValueStuff,
				ValueLoc = this.ValueLoc,
				ValueCombined = this.ValueCombined,
				Vel = this.Vel,
				BreakHoriz = this.BreakHoriz,
				BreakVert = this.BreakVert,
				
			};
		}
	}
}