namespace Db
{
	public partial class DraftPickValues
	{
		public required int Pick {get; set;}
		public required float WarHitter {get; set;}
		public required float WarPitcher {get; set;}

		public DraftPickValues Clone()
		{
			return new DraftPickValues
			{
				Pick = this.Pick,
				WarHitter = this.WarHitter,
				WarPitcher = this.WarPitcher,
			};
		}
	}
}