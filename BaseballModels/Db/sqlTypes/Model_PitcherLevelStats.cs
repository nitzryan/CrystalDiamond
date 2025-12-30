namespace Db
{
	public class Model_PitcherLevelStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int Outs_SP {get; set;}
		public required int Outs_RP {get; set;}
		public required int G {get; set;}
		public required int GS {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float HR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}
		public required float ParkRunFactor {get; set;}

		public Model_PitcherLevelStats Clone()
		{
			return new Model_PitcherLevelStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				Outs_SP = this.Outs_SP,
				Outs_RP = this.Outs_RP,
				G = this.G,
				GS = this.GS,
				ERA = this.ERA,
				FIP = this.FIP,
				HR = this.HR,
				BB = this.BB,
				HBP = this.HBP,
				K = this.K,
				ParkRunFactor = this.ParkRunFactor,
			};
		}
	}
}