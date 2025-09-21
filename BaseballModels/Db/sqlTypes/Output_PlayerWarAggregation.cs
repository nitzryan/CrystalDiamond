namespace Db
{
	public class Output_PlayerWarAggregation
	{
		public required int MlbId {get; set;}
		public required int Model {get; set;}
		public required int IsHitter {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Prob0 {get; set;}
		public required float Prob1 {get; set;}
		public required float Prob2 {get; set;}
		public required float Prob3 {get; set;}
		public required float Prob4 {get; set;}
		public required float Prob5 {get; set;}
		public required float Prob6 {get; set;}

		public Output_PlayerWarAggregation Clone()
		{
			return new Output_PlayerWarAggregation
			{
				MlbId = this.MlbId,
				Model = this.Model,
				IsHitter = this.IsHitter,
				Year = this.Year,
				Month = this.Month,
				Prob0 = this.Prob0,
				Prob1 = this.Prob1,
				Prob2 = this.Prob2,
				Prob3 = this.Prob3,
				Prob4 = this.Prob4,
				Prob5 = this.Prob5,
				Prob6 = this.Prob6,
				
			};
		}
	}
}