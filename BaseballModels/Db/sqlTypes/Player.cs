namespace Db
{
	public class Player
	{
		public required int Mlbid {get; set;}
		public int? Fangraphsid {get; set;}
		public required int Birthyear {get; set;}
		public required int Birthmonth {get; set;}
		public required int Birthdate {get; set;}
		public int? Draftpick {get; set;}
		public int? Draftbonus {get; set;}
		public int? Signingyear {get; set;}
		public int? Signingmonth {get; set;}
		public int? Signingdate {get; set;}
		public int? Signingbonus {get; set;}
		public required string Bats {get; set;}
		public required string Throws {get; set;}
		public int? Isretired {get; set;}
		public required string Usefirstname {get; set;}
		public required string Uselastname {get; set;}
	}
}