namespace Db
{
	public class Model_College_PitcherProStats
	{
		public required int TBCId {get; set;}
		public required float PercSP {get; set;}
		public required float PercRP {get; set;}
		public required float MLB_WAR {get; set;}
		public required int Outs {get; set;}
		public required int MLB_Outs {get; set;}
		public required int YearsSinceDraft {get; set;}

		public Model_College_PitcherProStats Clone()
		{
			return new Model_College_PitcherProStats
			{
				TBCId = this.TBCId,
				PercSP = this.PercSP,
				PercRP = this.PercRP,
				MLB_WAR = this.MLB_WAR,
				Outs = this.Outs,
				MLB_Outs = this.MLB_Outs,
				YearsSinceDraft = this.YearsSinceDraft,
			};
		}
	}
}