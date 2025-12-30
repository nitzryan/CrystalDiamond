namespace Db
{
	public class Model_HitterValue
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float War1Year {get; set;}
		public required float War2Year {get; set;}
		public required float War3Year {get; set;}
		public required float Off1Year {get; set;}
		public required float Off2Year {get; set;}
		public required float Off3Year {get; set;}
		public required float Bsr1Year {get; set;}
		public required float Bsr2Year {get; set;}
		public required float Bsr3Year {get; set;}
		public required float Def1Year {get; set;}
		public required float Def2Year {get; set;}
		public required float Def3Year {get; set;}
		public required float Rep1Year {get; set;}
		public required float Rep2Year {get; set;}
		public required float Rep3Year {get; set;}
		public required int Pa1Year {get; set;}
		public required int Pa2Year {get; set;}
		public required int Pa3Year {get; set;}

		public Model_HitterValue Clone()
		{
			return new Model_HitterValue
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				War1Year = this.War1Year,
				War2Year = this.War2Year,
				War3Year = this.War3Year,
				Off1Year = this.Off1Year,
				Off2Year = this.Off2Year,
				Off3Year = this.Off3Year,
				Bsr1Year = this.Bsr1Year,
				Bsr2Year = this.Bsr2Year,
				Bsr3Year = this.Bsr3Year,
				Def1Year = this.Def1Year,
				Def2Year = this.Def2Year,
				Def3Year = this.Def3Year,
				Rep1Year = this.Rep1Year,
				Rep2Year = this.Rep2Year,
				Rep3Year = this.Rep3Year,
				Pa1Year = this.Pa1Year,
				Pa2Year = this.Pa2Year,
				Pa3Year = this.Pa3Year,
			};
		}
	}
}