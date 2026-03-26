namespace Db
{
	public class College_ConfHitterAvg
	{
		public required int ConfId {get; set;}
		public required int Year {get; set;}
		public required float H {get; set;}
		public required float H2B {get; set;}
		public required float H3B {get; set;}
		public required float HR {get; set;}
		public required float SB {get; set;}
		public required float CS {get; set;}
		public required float BB {get; set;}
		public required float IBB {get; set;}
		public required float K {get; set;}
		public required float HBP {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float OPS {get; set;}

		public College_ConfHitterAvg Clone()
		{
			return new College_ConfHitterAvg
			{
				ConfId = this.ConfId,
				Year = this.Year,
				H = this.H,
				H2B = this.H2B,
				H3B = this.H3B,
				HR = this.HR,
				SB = this.SB,
				CS = this.CS,
				BB = this.BB,
				IBB = this.IBB,
				K = this.K,
				HBP = this.HBP,
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				OPS = this.OPS,
			};
		}
	}
}