namespace SiteDb
{
	public partial class ModelDraftPickValues
	{
		public required int Pick {get; set;}
		public required float WarHitter {get; set;}
		public required float WarPitcher {get; set;}

		public ModelDraftPickValues Clone()
		{
			return new ModelDraftPickValues
			{
				Pick = this.Pick,
				WarHitter = this.WarHitter,
				WarPitcher = this.WarPitcher,
			};
		}
	}
}