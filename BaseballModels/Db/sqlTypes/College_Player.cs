namespace Db
{
	public class College_Player
	{
		public required int TBCId {get; set;}
		public int? MlbId {get; set;}
		public required string FirstName {get; set;}
		public required string LastName {get; set;}
		public required int BirthYear {get; set;}
		public required int BirthMonth {get; set;}
		public required int BirthDay {get; set;}
		public required int DraftOvrPitcher {get; set;}
		public required int DraftOvrHitter {get; set;}
		public required int FirstYear {get; set;}
		public required int LastYear {get; set;}
		public required string Bats {get; set;}
		public required string Throws {get; set;}
		public required bool IsPitcher {get; set;}
		public required bool IsHitter {get; set;}

		public College_Player Clone()
		{
			return new College_Player
			{
				TBCId = this.TBCId,
				MlbId = this.MlbId,
				FirstName = this.FirstName,
				LastName = this.LastName,
				BirthYear = this.BirthYear,
				BirthMonth = this.BirthMonth,
				BirthDay = this.BirthDay,
				DraftOvrPitcher = this.DraftOvrPitcher,
				DraftOvrHitter = this.DraftOvrHitter,
				FirstYear = this.FirstYear,
				LastYear = this.LastYear,
				Bats = this.Bats,
				Throws = this.Throws,
				IsPitcher = this.IsPitcher,
				IsHitter = this.IsHitter,
			};
		}
	}
}