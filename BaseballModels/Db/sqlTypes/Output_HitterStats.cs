namespace Db
{
	public class Output_HitterStats
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int Pa {get; set;}
		public required float Hit1B {get; set;}
		public required float Hit2B {get; set;}
		public required float Hit3B {get; set;}
		public required float HitHR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}
		public required float SB {get; set;}
		public required float CS {get; set;}
		public required float BSR {get; set;}
		public required float DRAA {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float PercC {get; set;}
		public required float Perc1B {get; set;}
		public required float Perc2B {get; set;}
		public required float Perc3B {get; set;}
		public required float PercSS {get; set;}
		public required float PercLF {get; set;}
		public required float PercCF {get; set;}
		public required float PercRF {get; set;}
		public required float PercDH {get; set;}

		public Output_HitterStats Clone()
		{
			return new Output_HitterStats
			{
				MlbId = this.MlbId,
				Model = this.Model,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				Pa = this.Pa,
				Hit1B = this.Hit1B,
				Hit2B = this.Hit2B,
				Hit3B = this.Hit3B,
				HitHR = this.HitHR,
				BB = this.BB,
				HBP = this.HBP,
				K = this.K,
				SB = this.SB,
				CS = this.CS,
				BSR = this.BSR,
				DRAA = this.DRAA,
				ParkRunFactor = this.ParkRunFactor,
				PercC = this.PercC,
				Perc1B = this.Perc1B,
				Perc2B = this.Perc2B,
				Perc3B = this.Perc3B,
				PercSS = this.PercSS,
				PercLF = this.PercLF,
				PercCF = this.PercCF,
				PercRF = this.PercRF,
				PercDH = this.PercDH,
			};
		}
	}
}