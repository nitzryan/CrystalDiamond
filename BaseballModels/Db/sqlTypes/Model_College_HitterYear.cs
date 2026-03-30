namespace Db
{
	public class Model_College_HitterYear
	{
		public required int TBCId {get; set;}
		public required int Level {get; set;}
		public required int Year {get; set;}
		public required int ExpYears {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ConfScore {get; set;}
		public required int PA {get; set;}
		public required float H {get; set;}
		public required float H2B {get; set;}
		public required float H3B {get; set;}
		public required float HR {get; set;}
		public required float SB {get; set;}
		public required float CS {get; set;}
		public required float BB {get; set;}
		public required float K {get; set;}
		public required float HBP {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float OPS {get; set;}
		public required float Age {get; set;}
		public required DbEnums.CollegePosition Pos {get; set;}
		public required int Height {get; set;}
		public required int Weight {get; set;}

		public Model_College_HitterYear Clone()
		{
			return new Model_College_HitterYear
			{
				TBCId = this.TBCId,
				Level = this.Level,
				Year = this.Year,
				ExpYears = this.ExpYears,
				ParkRunFactor = this.ParkRunFactor,
				ConfScore = this.ConfScore,
				PA = this.PA,
				H = this.H,
				H2B = this.H2B,
				H3B = this.H3B,
				HR = this.HR,
				SB = this.SB,
				CS = this.CS,
				BB = this.BB,
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