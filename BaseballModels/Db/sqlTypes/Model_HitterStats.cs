namespace Db
{
	public class Model_HitterStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Age {get; set;}
		public required int PA {get; set;}
		public required int InjStatus {get; set;}
		public required float MonthFrac {get; set;}
		public required float LevelId {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
		public required float AVGRatio {get; set;}
		public required float OBPRatio {get; set;}
		public required float ISORatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float SBRateRatio {get; set;}
		public required float SBPercRatio {get; set;}
		public required float HRPercRatio {get; set;}
		public required float BBPercRatio {get; set;}
		public required float KPercRatio {get; set;}
		public required float PercC {get; set;}
		public required float Perc1B {get; set;}
		public required float Perc2B {get; set;}
		public required float Perc3B {get; set;}
		public required float PercSS {get; set;}
		public required float PercLF {get; set;}
		public required float PercCF {get; set;}
		public required float PercRF {get; set;}
		public required float PercDH {get; set;}

		public Model_HitterStats Clone()
		{
			return new Model_HitterStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Age = this.Age,
				PA = this.PA,
				InjStatus = this.InjStatus,
				MonthFrac = this.MonthFrac,
				LevelId = this.LevelId,
				ParkRunFactor = this.ParkRunFactor,
				ParkHRFactor = this.ParkHRFactor,
				AVGRatio = this.AVGRatio,
				OBPRatio = this.OBPRatio,
				ISORatio = this.ISORatio,
				WOBARatio = this.WOBARatio,
				SBRateRatio = this.SBRateRatio,
				SBPercRatio = this.SBPercRatio,
				HRPercRatio = this.HRPercRatio,
				BBPercRatio = this.BBPercRatio,
				KPercRatio = this.KPercRatio,
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