namespace SiteDb
{
	public partial class PitcherMlbWarModel
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required byte[] ProbsWarSP {get; set;}
		public required byte[] ProbsWarRP {get; set;}
		public required byte[] RanksWarSP {get; set;}
		public required byte[] RanksWarRP {get; set;}

		public PitcherMlbWarModel Clone()
		{
			return new PitcherMlbWarModel
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				Year = this.Year,
				Month = this.Month,
				ProbsWarSP = (byte[])this.ProbsWarSP.Clone(),
				ProbsWarRP = (byte[])this.ProbsWarRP.Clone(),
				RanksWarSP = (byte[])this.RanksWarSP.Clone(),
				RanksWarRP = (byte[])this.RanksWarRP.Clone(),
			};
		}
	}
}