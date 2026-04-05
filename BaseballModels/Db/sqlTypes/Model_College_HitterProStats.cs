namespace Db
{
	public class Model_College_HitterProStats
	{
		public required int TBCId {get; set;}
		public required float PercC {get; set;}
		public required float Perc1B {get; set;}
		public required float Perc2B {get; set;}
		public required float Perc3B {get; set;}
		public required float PercSS {get; set;}
		public required float PercLF {get; set;}
		public required float PercCF {get; set;}
		public required float PercRF {get; set;}
		public required float PercDH {get; set;}
		public required float DEF {get; set;}
		public required float MLB_WAR {get; set;}
		public required int DefOuts {get; set;}
		public required int MLB_PA {get; set;}
		public required int YearsSinceDraft {get; set;}

		public Model_College_HitterProStats Clone()
		{
			return new Model_College_HitterProStats
			{
				TBCId = this.TBCId,
				PercC = this.PercC,
				Perc1B = this.Perc1B,
				Perc2B = this.Perc2B,
				Perc3B = this.Perc3B,
				PercSS = this.PercSS,
				PercLF = this.PercLF,
				PercCF = this.PercCF,
				PercRF = this.PercRF,
				PercDH = this.PercDH,
				DEF = this.DEF,
				MLB_WAR = this.MLB_WAR,
				DefOuts = this.DefOuts,
				MLB_PA = this.MLB_PA,
				YearsSinceDraft = this.YearsSinceDraft,
			};
		}
	}
}