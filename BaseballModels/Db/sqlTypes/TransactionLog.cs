namespace Db
{
	public class TransactionLog
	{
		public int TransactionId {get; set;}
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Day {get; set;}
		public required int ToIL {get; set;}
		public required int ParentOrgId {get; set;}

		public TransactionLog Clone()
		{
			return new TransactionLog
			{
				TransactionId = this.TransactionId,
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Day = this.Day,
				ToIL = this.ToIL,
				ParentOrgId = this.ParentOrgId,
				
			};
		}
	}
}