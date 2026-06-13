namespace PitchDb
{
	public class PitcherStuff
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Model {get; set;}
		public required int GameId {get; set;}
		public required Db.DbEnums.PitchType PitchType {get; set;}
		public required Db.DbEnums.PitchScenario Scenario {get; set;}
		public required int NumPitches {get; set;}
		public required float ValueActual {get; set;}
		public required float ValueStuff {get; set;}
		public required float ValueCombined {get; set;}
		public required float ActualPlus {get; set;}
		public required float StuffPlus {get; set;}
		public required float PitchPlus {get; set;}
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
				Model = this.Model,
				GameId = this.GameId,
				PitchType = this.PitchType,
				Scenario = this.Scenario,
				NumPitches = this.NumPitches,
				ValueActual = this.ValueActual,
				ValueStuff = this.ValueStuff,
				ValueCombined = this.ValueCombined,
				ActualPlus = this.ActualPlus,
				StuffPlus = this.StuffPlus,
				PitchPlus = this.PitchPlus,
				Vel = this.Vel,
				BreakHoriz = this.BreakHoriz,
				BreakVert = this.BreakVert,
				
			};
		}
	}
}