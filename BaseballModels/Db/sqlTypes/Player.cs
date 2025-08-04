namespace Db
{
	public class Player
	{
		public required int MlbId {get; set;}
		public int? FangraphsId {get; set;}
		public required string Position {get; set;}
		public required int BirthYear {get; set;}
		public required int BirthMonth {get; set;}
		public required int BirthDate {get; set;}
		public int? DraftPick {get; set;}
		public int? DraftBonus {get; set;}
		public int? SigningYear {get; set;}
		public int? SigningMonth {get; set;}
		public int? SigningDate {get; set;}
		public int? SigningBonus {get; set;}
		public required string Bats {get; set;}
		public required string Throws {get; set;}
		public int? IsRetired {get; set;}
		public required string UseFirstName {get; set;}
		public required string UseLastName {get; set;}
	}
}