namespace Db
{
	public class Model_PitcherStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Age {get; set;}
		public required int BF {get; set;}
		public required float Level {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ParkHRFactor {get; set;}
		public required float GBPercRatio {get; set;}
		public required float ERARatio {get; set;}
		public required float FIPRatio {get; set;}
		public required float WOBARatio {get; set;}
		public required float HrPercRatio {get; set;}
		public required float BbPercRatio {get; set;}
		public required float KPercRatio {get; set;}
	}
}