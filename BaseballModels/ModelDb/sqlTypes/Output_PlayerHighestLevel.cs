namespace ModelDb
{
	public class Output_PlayerHighestLevel
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public required int ModelRun {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float DSL {get; set;}
		public required float CPX {get; set;}
		public required float A_LOW {get; set;}
		public required float A {get; set;}
		public required float A_HIGH {get; set;}
		public required float AA {get; set;}
		public required float AAA {get; set;}
		public required float MLB {get; set;}

		public Output_PlayerHighestLevel Clone()
		{
			return new Output_PlayerHighestLevel
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				ModelRun = this.ModelRun,
				Year = this.Year,
				Month = this.Month,
				DSL = this.DSL,
				CPX = this.CPX,
				A_LOW = this.A_LOW,
				A = this.A,
				A_HIGH = this.A_HIGH,
				AA = this.AA,
				AAA = this.AAA,
				MLB = this.MLB,
				
			};
		}
	}
}