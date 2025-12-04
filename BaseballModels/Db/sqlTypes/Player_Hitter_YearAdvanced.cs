namespace Db
{
	public class Player_Hitter_YearAdvanced
	{
		public required int MlbId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int TeamId {get; set;}
		public required int LeagueId {get; set;}
		public required float ParkFactor {get; set;}
		public required int PA {get; set;}
		public required float AVG {get; set;}
		public required float OBP {get; set;}
		public required float SLG {get; set;}
		public required float ISO {get; set;}
		public required float WOBA {get; set;}
		public required float WRC {get; set;}
		public required int HR {get; set;}
		public required float BBPerc {get; set;}
		public required float KPerc {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}

		public Player_Hitter_YearAdvanced Clone()
		{
			return new Player_Hitter_YearAdvanced
			{
				MlbId = this.MlbId,
				LevelId = this.LevelId,
				Year = this.Year,
				TeamId = this.TeamId,
				LeagueId = this.LeagueId,
				ParkFactor = this.ParkFactor,
				PA = this.PA,
				AVG = this.AVG,
				OBP = this.OBP,
				SLG = this.SLG,
				ISO = this.ISO,
				WOBA = this.WOBA,
				WRC = this.WRC,
				HR = this.HR,
				BBPerc = this.BBPerc,
				KPerc = this.KPerc,
				SB = this.SB,
				CS = this.CS,
				
			};
		}
	}
}