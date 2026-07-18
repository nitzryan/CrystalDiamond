namespace ModelDb
{
	public class Output_PlayerWar
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public required int ModelRun {get; set;}
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

		public Output_PlayerWar Clone()
		{
			return new Output_PlayerWar
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				ModelRun = this.ModelRun,
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
				
			};
		}
	}
}