namespace Db
{
	public class Pre05_Players
	{
		public required int MlbId {get; set;}
		public required int CareerStartYear {get; set;}

		public Pre05_Players Clone()
		{
			return new Pre05_Players
			{
				MlbId = this.MlbId,
				CareerStartYear = this.CareerStartYear,
				
			};
		}
	}
}