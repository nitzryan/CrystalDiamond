namespace ModelDb
{
	public class Output_College_Hitter
	{
		public required int TbcId {get; set;}
		public required int Model {get; set;}
		public required int ModelIdx {get; set;}
		public required int Year {get; set;}
		public required float Draft0 {get; set;}
		public required float Draft1 {get; set;}
		public required float Draft2 {get; set;}
		public required float Draft3 {get; set;}
		public required float Draft4 {get; set;}
		public required float Draft5 {get; set;}
		public required float Draft6 {get; set;}
		public required float Draft {get; set;}
		public required float ProbC {get; set;}
		public required float Prob1B {get; set;}
		public required float Prob2B {get; set;}
		public required float Prob3B {get; set;}
		public required float ProbSS {get; set;}
		public required float ProbLF {get; set;}
		public required float ProbCF {get; set;}
		public required float ProbRF {get; set;}
		public required float ProbDH {get; set;}

		public Output_College_Hitter Clone()
		{
			return new Output_College_Hitter
			{
				TbcId = this.TbcId,
				Model = this.Model,
				ModelIdx = this.ModelIdx,
				Year = this.Year,
				Draft0 = this.Draft0,
				Draft1 = this.Draft1,
				Draft2 = this.Draft2,
				Draft3 = this.Draft3,
				Draft4 = this.Draft4,
				Draft5 = this.Draft5,
				Draft6 = this.Draft6,
				Draft = this.Draft,
				ProbC = this.ProbC,
				Prob1B = this.Prob1B,
				Prob2B = this.Prob2B,
				Prob3B = this.Prob3B,
				ProbSS = this.ProbSS,
				ProbLF = this.ProbLF,
				ProbCF = this.ProbCF,
				ProbRF = this.ProbRF,
				ProbDH = this.ProbDH,
				
			};
		}
	}
}