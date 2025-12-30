namespace Db
{
	public class Model_OrgLeagueStatus
	{
		public required int OrgId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float MLB_PF {get; set;}
		public required float AAA_PF {get; set;}
		public required float AA_PF {get; set;}
		public required float HA_PF {get; set;}
		public required float A_PF {get; set;}
		public required float LA_PF {get; set;}
		public required float Rk_PF {get; set;}
		public required float DSL_PF {get; set;}
		public required float MLB_MonthsFrac {get; set;}
		public required float AAA_MonthsFrac {get; set;}
		public required float AA_MonthsFrac {get; set;}
		public required float HA_MonthsFrac {get; set;}
		public required float A_MonthsFrac {get; set;}
		public required float LA_MonthsFrac {get; set;}
		public required float Rk_MonthsFrac {get; set;}
		public required float DSL_MonthsFrac {get; set;}

		public Model_OrgLeagueStatus Clone()
		{
			return new Model_OrgLeagueStatus
			{
				OrgId = this.OrgId,
				Year = this.Year,
				Month = this.Month,
				MLB_PF = this.MLB_PF,
				AAA_PF = this.AAA_PF,
				AA_PF = this.AA_PF,
				HA_PF = this.HA_PF,
				A_PF = this.A_PF,
				LA_PF = this.LA_PF,
				Rk_PF = this.Rk_PF,
				DSL_PF = this.DSL_PF,
				MLB_MonthsFrac = this.MLB_MonthsFrac,
				AAA_MonthsFrac = this.AAA_MonthsFrac,
				AA_MonthsFrac = this.AA_MonthsFrac,
				HA_MonthsFrac = this.HA_MonthsFrac,
				A_MonthsFrac = this.A_MonthsFrac,
				LA_MonthsFrac = this.LA_MonthsFrac,
				Rk_MonthsFrac = this.Rk_MonthsFrac,
				DSL_MonthsFrac = this.DSL_MonthsFrac,
			};
		}
	}
}