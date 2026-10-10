namespace Db
{
	public partial class Model_PitcherWarValues
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Offset {get; set;}
		public required int OutsSP {get; set;}
		public required int OutsRP {get; set;}
		public required float WarSP {get; set;}
		public required float WarRP {get; set;}

		public Model_PitcherWarValues Clone()
		{
			return new Model_PitcherWarValues
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Offset = this.Offset,
				OutsSP = this.OutsSP,
				OutsRP = this.OutsRP,
				WarSP = this.WarSP,
				WarRP = this.WarRP,
			};
		}
	}
}