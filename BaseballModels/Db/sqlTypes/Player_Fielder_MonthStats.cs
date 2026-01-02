namespace Db
{
	public class Player_Fielder_MonthStats
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int LevelId {get; set;}
		public required int LeagueId {get; set;}
		public required int TeamId {get; set;}
		public required DbEnums.Position Position {get; set;}
		public required int Chances {get; set;}
		public required int Errors {get; set;}
		public required int ThrowErrors {get; set;}
		public required int Outs {get; set;}
		public required float R_ERR {get; set;}
		public required float R_PM {get; set;}
		public required float PosAdjust {get; set;}
		public required float D_RAA {get; set;}
		public required float ScaledDRAA {get; set;}
		public required float R_GIDP {get; set;}
		public required float R_ARM {get; set;}
		public required float R_SB {get; set;}
		public required int SB {get; set;}
		public required int CS {get; set;}
		public required float R_PB {get; set;}
		public required int PB {get; set;}

		public Player_Fielder_MonthStats Clone()
		{
			return new Player_Fielder_MonthStats
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				LevelId = this.LevelId,
				LeagueId = this.LeagueId,
				TeamId = this.TeamId,
				Position = this.Position,
				Chances = this.Chances,
				Errors = this.Errors,
				ThrowErrors = this.ThrowErrors,
				Outs = this.Outs,
				R_ERR = this.R_ERR,
				R_PM = this.R_PM,
				PosAdjust = this.PosAdjust,
				D_RAA = this.D_RAA,
				ScaledDRAA = this.ScaledDRAA,
				R_GIDP = this.R_GIDP,
				R_ARM = this.R_ARM,
				R_SB = this.R_SB,
				SB = this.SB,
				CS = this.CS,
				R_PB = this.R_PB,
				PB = this.PB,
			};
		}
	}
}