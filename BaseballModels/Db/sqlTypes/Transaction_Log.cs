namespace Db
{
	public class Transaction_Log
	{
		public int TransactionId {get; set;}
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int Day {get; set;}
		public required int ToIL {get; set;}
		public required int ParentOrgId {get; set;}

		public Transaction_Log Clone()
		{
			return new Transaction_Log
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