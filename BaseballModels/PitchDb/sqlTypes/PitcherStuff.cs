namespace PitchDb
{
	public partial class PitcherStuff
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelId {get; set;}
		public required int GameId {get; set;}
		public required Db.DbEnums.PitchType PitchType {get; set;}
		public required Db.DbEnums.PitchScenario Scenario {get; set;}
		public required int NumPitches {get; set;}
		public required int NumStuffPitches {get; set;}
		public required float CountLeverage {get; set;}
		public required float PitchPlus {get; set;}
		public required float ActualPlus {get; set;}
		public required float StuffPlus {get; set;}
		public required float LocationPlus {get; set;}
		public required float SituationPlus {get; set;}
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
				ModelId = this.ModelId,
				GameId = this.GameId,
				PitchType = this.PitchType,
				Scenario = this.Scenario,
				NumPitches = this.NumPitches,
				NumStuffPitches = this.NumStuffPitches,
				CountLeverage = this.CountLeverage,
				PitchPlus = this.PitchPlus,
				ActualPlus = this.ActualPlus,
				StuffPlus = this.StuffPlus,
				LocationPlus = this.LocationPlus,
				SituationPlus = this.SituationPlus,
				Vel = this.Vel,
				BreakHoriz = this.BreakHoriz,
				BreakVert = this.BreakVert,
			};
		}
	}
}