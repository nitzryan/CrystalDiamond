namespace PitchDb
{
	public class Output_PitchValue
	{
		public required int Model {get; set;}
		public required int GameId {get; set;}
		public required int PitchId {get; set;}
		public required int ModelRun {get; set;}
		public required int Year {get; set;}
		public required int LevelId {get; set;}
		public required int MlbId {get; set;}
		public required float LocationCalledStrike {get; set;}
		public required float LocationBall {get; set;}
		public required float LocationHBP {get; set;}
		public required float LocationSwing {get; set;}
		public required float LocationWhiff {get; set;}
		public required float LocationFoul {get; set;}
		public required float LocationInPlay {get; set;}
		public required float LocationInPlayExpected {get; set;}
		public required float StuffCalledStrike {get; set;}
		public required float StuffBall {get; set;}
		public required float StuffHBP {get; set;}
		public required float StuffSwing {get; set;}
		public required float StuffWhiff {get; set;}
		public required float StuffFoul {get; set;}
		public required float StuffInPlay {get; set;}
		public required float StuffInPlayExpected {get; set;}
		public required float CombinedCalledStrike {get; set;}
		public required float CombinedBall {get; set;}
		public required float CombinedHBP {get; set;}
		public required float CombinedSwing {get; set;}
		public required float CombinedWhiff {get; set;}
		public required float CombinedFoul {get; set;}
		public required float CombinedInPlay {get; set;}
		public required float CombinedInPlayExpected {get; set;}

		public Output_PitchValue Clone()
		{
			return new Output_PitchValue
			{
				Model = this.Model,
				GameId = this.GameId,
				PitchId = this.PitchId,
				ModelRun = this.ModelRun,
				Year = this.Year,
				LevelId = this.LevelId,
				MlbId = this.MlbId,
				LocationCalledStrike = this.LocationCalledStrike,
				LocationBall = this.LocationBall,
				LocationHBP = this.LocationHBP,
				LocationSwing = this.LocationSwing,
				LocationWhiff = this.LocationWhiff,
				LocationFoul = this.LocationFoul,
				LocationInPlay = this.LocationInPlay,
				LocationInPlayExpected = this.LocationInPlayExpected,
				StuffCalledStrike = this.StuffCalledStrike,
				StuffBall = this.StuffBall,
				StuffHBP = this.StuffHBP,
				StuffSwing = this.StuffSwing,
				StuffWhiff = this.StuffWhiff,
				StuffFoul = this.StuffFoul,
				StuffInPlay = this.StuffInPlay,
				StuffInPlayExpected = this.StuffInPlayExpected,
				CombinedCalledStrike = this.CombinedCalledStrike,
				CombinedBall = this.CombinedBall,
				CombinedHBP = this.CombinedHBP,
				CombinedSwing = this.CombinedSwing,
				CombinedWhiff = this.CombinedWhiff,
				CombinedFoul = this.CombinedFoul,
				CombinedInPlay = this.CombinedInPlay,
				CombinedInPlayExpected = this.CombinedInPlayExpected,
				
			};
		}
	}
}