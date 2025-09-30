namespace Db
{
	public class Output_HitterStats
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int LevelId {get; set;}
		public required int ModelIdx {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float ISO {get; set;}
		public required float HR {get; set;}
		public required float BB {get; set;}
		public required float K {get; set;}

		public Output_HitterStats Clone()
		{
			return new Output_HitterStats
			{
				MlbId = this.MlbId,
				Model = this.Model,
				LevelId = this.LevelId,
				ModelIdx = this.ModelIdx,
				AVG = this.AVG,
				OBP = this.OBP,
				ISO = this.ISO,
				HR = this.HR,
				BB = this.BB,
				K = this.K,
				
			};
		}
	}
}