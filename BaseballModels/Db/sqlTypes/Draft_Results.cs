namespace Db
{
	public class Draft_Results
	{
		public required int Year {get; set;}
		public required int Pick {get; set;}
		public required string Round {get; set;}
		public required int MlbId {get; set;}
		public required int Signed {get; set;}
		public required int Bonus {get; set;}
		public required int BonusRank {get; set;}

		public Draft_Results Clone()
		{
			return new Draft_Results
			{
				Year = this.Year,
				Pick = this.Pick,
				Round = this.Round,
				MlbId = this.MlbId,
				Signed = this.Signed,
				Bonus = this.Bonus,
				BonusRank = this.BonusRank,
				
			};
		}
	}
}