namespace Db
{
	public class Model_LevelYearGames
	{
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int MLB_Games {get; set;}
		public required int AAA_Games {get; set;}
		public required int AA_Games {get; set;}
		public required int HA_Games {get; set;}
		public required int A_Games {get; set;}
		public required int LA_Games {get; set;}
		public required int Rk_Games {get; set;}
		public required int DSL_Games {get; set;}

		public Model_LevelYearGames Clone()
		{
			return new Model_LevelYearGames
			{
				Year = this.Year,
				Month = this.Month,
				MLB_Games = this.MLB_Games,
				AAA_Games = this.AAA_Games,
				AA_Games = this.AA_Games,
				HA_Games = this.HA_Games,
				A_Games = this.A_Games,
				LA_Games = this.LA_Games,
				Rk_Games = this.Rk_Games,
				DSL_Games = this.DSL_Games,
			};
		}
	}
}