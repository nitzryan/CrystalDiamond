namespace ModelDb
{
	public class Output_College_HitterAggregation
	{
		public required int TbcId {get; set;}
		public required int Model {get; set;}
		public required int Year {get; set;}
		public required float Draft0 {get; set;}
		public required float Draft1 {get; set;}
		public required float Draft2 {get; set;}
		public required float Draft3 {get; set;}
		public required float Draft4 {get; set;}
		public required float Draft5 {get; set;}
		public required float Draft6 {get; set;}
		public required float Draft {get; set;}
		public required float War0 {get; set;}
		public required float War1 {get; set;}
		public required float War2 {get; set;}
		public required float War3 {get; set;}
		public required float War4 {get; set;}
		public required float War5 {get; set;}
		public required float War6 {get; set;}
		public required float War {get; set;}
		public required float Off0 {get; set;}
		public required float Off1 {get; set;}
		public required float Off2 {get; set;}
		public required float Off3 {get; set;}
		public required float Off4 {get; set;}
		public required float Off5 {get; set;}
		public required float Off6 {get; set;}
		public required float OffNone {get; set;}
		public required float Def0 {get; set;}
		public required float Def1 {get; set;}
		public required float Def2 {get; set;}
		public required float Def3 {get; set;}
		public required float Def4 {get; set;}
		public required float Def5 {get; set;}
		public required float Def6 {get; set;}
		public required float DefNone {get; set;}
		public required float Pa0 {get; set;}
		public required float Pa1 {get; set;}
		public required float Pa2 {get; set;}
		public required float Pa3 {get; set;}
		public required float Pa4 {get; set;}
		public required float Pa5 {get; set;}
		public required float Pa6 {get; set;}
		public required float ProbC {get; set;}
		public required float Prob1B {get; set;}
		public required float Prob2B {get; set;}
		public required float Prob3B {get; set;}
		public required float ProbSS {get; set;}
		public required float ProbLF {get; set;}
		public required float ProbCF {get; set;}
		public required float ProbRF {get; set;}
		public required float ProbDH {get; set;}

		public Output_College_HitterAggregation Clone()
		{
			return new Output_College_HitterAggregation
			{
				TbcId = this.TbcId,
				Model = this.Model,
				Year = this.Year,
				Draft0 = this.Draft0,
				Draft1 = this.Draft1,
				Draft2 = this.Draft2,
				Draft3 = this.Draft3,
				Draft4 = this.Draft4,
				Draft5 = this.Draft5,
				Draft6 = this.Draft6,
				Draft = this.Draft,
				War0 = this.War0,
				War1 = this.War1,
				War2 = this.War2,
				War3 = this.War3,
				War4 = this.War4,
				War5 = this.War5,
				War6 = this.War6,
				War = this.War,
				Off0 = this.Off0,
				Off1 = this.Off1,
				Off2 = this.Off2,
				Off3 = this.Off3,
				Off4 = this.Off4,
				Off5 = this.Off5,
				Off6 = this.Off6,
				OffNone = this.OffNone,
				Def0 = this.Def0,
				Def1 = this.Def1,
				Def2 = this.Def2,
				Def3 = this.Def3,
				Def4 = this.Def4,
				Def5 = this.Def5,
				Def6 = this.Def6,
				DefNone = this.DefNone,
				Pa0 = this.Pa0,
				Pa1 = this.Pa1,
				Pa2 = this.Pa2,
				Pa3 = this.Pa3,
				Pa4 = this.Pa4,
				Pa5 = this.Pa5,
				Pa6 = this.Pa6,
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