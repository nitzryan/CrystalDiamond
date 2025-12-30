namespace Db
{
	public class Output_PitcherStats
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required float Outs_SP {get; set;}
		public required float Outs_RP {get; set;}
		public required float GS {get; set;}
		public required float GR {get; set;}
		public required float ERA {get; set;}
		public required float FIP {get; set;}
		public required float HR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float SP_Perc {get; set;}
		public required float RP_Perc {get; set;}

		public Output_PitcherStats Clone()
		{
			return new Output_PitcherStats
			{
				MlbId = this.MlbId,
				Model = this.Model,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				Outs_SP = this.Outs_SP,
				Outs_RP = this.Outs_RP,
				GS = this.GS,
				GR = this.GR,
				ERA = this.ERA,
				FIP = this.FIP,
				HR = this.HR,
				BB = this.BB,
				HBP = this.HBP,
				K = this.K,
				ParkRunFactor = this.ParkRunFactor,
				SP_Perc = this.SP_Perc,
				RP_Perc = this.RP_Perc,
			};
		}
	}
}