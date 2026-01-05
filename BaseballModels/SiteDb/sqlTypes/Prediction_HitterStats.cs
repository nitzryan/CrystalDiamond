namespace SiteDb
{
	public class Prediction_HitterStats
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required float Pa {get; set;}
		public required float Hit1B {get; set;}
		public required float Hit2B {get; set;}
		public required float Hit3B {get; set;}
		public required float HitHR {get; set;}
		public required float BB {get; set;}
		public required float HBP {get; set;}
		public required float K {get; set;}
		public required float SB {get; set;}
		public required float CS {get; set;}
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
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WRC {get; set;}
		public required float CrOFF {get; set;}
		public required float CrBSR {get; set;}
		public required float CrDEF {get; set;}
		public required float CrDPOS {get; set;}
		public required float CrDRAA {get; set;}
		public required float CrWAR {get; set;}

		public Prediction_HitterStats Clone()
		{
			return new Prediction_HitterStats
			{
				MlbId = this.MlbId,
				Model = this.Model,
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
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				ISO = this.ISO,
				WRC = this.WRC,
				CrOFF = this.CrOFF,
				CrBSR = this.CrBSR,
				CrDEF = this.CrDEF,
				CrDPOS = this.CrDPOS,
				CrDRAA = this.CrDRAA,
				CrWAR = this.CrWAR,
				
			};
		}
	}
}