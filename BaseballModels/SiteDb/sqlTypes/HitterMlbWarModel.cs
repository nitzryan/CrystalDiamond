namespace SiteDb
{
	public partial class HitterMlbWarModel
	{
		public required int MlbId {get; set;}
		public required int ModelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required byte[] ProbsWar {get; set;}
		public required byte[] RanksWar {get; set;}

		public HitterMlbWarModel Clone()
		{
			return new HitterMlbWarModel
			{
				MlbId = this.MlbId,
				ModelId = this.ModelId,
				Year = this.Year,
				Month = this.Month,
				ProbsWar = (byte[])this.ProbsWar.Clone(),
				RanksWar = (byte[])this.RanksWar.Clone(),
			};
		}
	}
}