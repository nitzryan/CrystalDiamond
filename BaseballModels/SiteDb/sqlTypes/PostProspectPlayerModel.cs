namespace SiteDb
{
	public partial class PostProspectPlayerModel
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelId {get; set;}
		public required bool IsHitter {get; set;}
		public required float War {get; set;}

		public PostProspectPlayerModel Clone()
		{
			return new PostProspectPlayerModel
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				ModelId = this.ModelId,
				IsHitter = this.IsHitter,
				War = this.War,
			};
		}
	}
}