namespace Db
{
	public class Player_CareerStatus
	{
		public required int Mlbid {get; set;}
		public required int Ispitcher {get; set;}
		public required int Ishitter {get; set;}
		public required int Isactive {get; set;}
		public int? Servicereached {get; set;}
		public int? Mlbstartyear {get; set;}
		public int? Mlbrookieyear {get; set;}
		public int? Mlbrookiemonth {get; set;}
		public int? Serviceendyear {get; set;}
		public int? Servicelapseyear {get; set;}
		public required int Careerstartyear {get; set;}
		public int? Agedout {get; set;}
		public int? Ignoreplayer {get; set;}
		public required int Highestlevel {get; set;}
	}
}