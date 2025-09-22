namespace Db
{
	public class Model_PitcherStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Age {get; set;}
		public required int BF {get; set;}
		public required int InjStatus {get; set;}
		public required int TrainMask {get; set;}
		public required float MonthFrac {get; set;}
		public required float LevelId {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
		public required float GBPercRatio {get; set;}
		public required float ERARatio {get; set;}
		public required float FIPRatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float HRPercRatio {get; set;}
		public required float BBPercRatio {get; set;}
		public required float KPercRatio {get; set;}

		public Model_PitcherStats Clone()
		{
			return new Model_PitcherStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Age = this.Age,
				BF = this.BF,
				InjStatus = this.InjStatus,
				TrainMask = this.TrainMask,
				MonthFrac = this.MonthFrac,
				LevelId = this.LevelId,
				ParkRunFactor = this.ParkRunFactor,
				ParkHRFactor = this.ParkHRFactor,
				GBPercRatio = this.GBPercRatio,
				ERARatio = this.ERARatio,
				FIPRatio = this.FIPRatio,
				WOBARatio = this.WOBARatio,
				HRPercRatio = this.HRPercRatio,
				BBPercRatio = this.BBPercRatio,
				KPercRatio = this.KPercRatio,
				
			};
		}
	}
}