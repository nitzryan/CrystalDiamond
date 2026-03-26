namespace Db
{
	public class College_HitterStats
	{
		public required int TBCId {get; set;}
		public required int Year {get; set;}
		public required int Level {get; set;}
		public required int TeamId {get; set;}
		public required int ConfId {get; set;}
		public required int ExpYears {get; set;}
		public required int AB {get; set;}
		public required int PA {get; set;}
		public required int H {get; set;}
		public required int H2B {get; set;}
		public required int H3B {get; set;}
		public required int HR {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}
		public required int BB {get; set;}
		public required int IBB {get; set;}
		public required int K {get; set;}
		public required int HBP {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float OPS {get; set;}
		public required float Age {get; set;}
		public required DbEnums.CollegePosition Pos {get; set;}
		public required int Height {get; set;}
		public required int Weight {get; set;}

		public College_HitterStats Clone()
		{
			return new College_HitterStats
			{
				TBCId = this.TBCId,
				Year = this.Year,
				Level = this.Level,
				TeamId = this.TeamId,
				ConfId = this.ConfId,
				ExpYears = this.ExpYears,
				AB = this.AB,
				PA = this.PA,
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
				Age = this.Age,
				Pos = this.Pos,
				Height = this.Height,
				Weight = this.Weight,
			};
		}
	}
}