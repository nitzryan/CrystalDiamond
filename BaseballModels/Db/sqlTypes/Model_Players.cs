namespace Db
{
	public class Model_Players
	{
		public required int MlbId {get; set;}
		public required int IsHitter {get; set;}
		public required int IsPitcher {get; set;}
		public required int SigningYear {get; set;}
		public required int LastProspectYear {get; set;}
		public required int LastProspectMonth {get; set;}
		public required int LastMLBSeason {get; set;}
		public required float AgeAtSigningYear {get; set;}
		public required int DraftPick {get; set;}
		public required int DraftSignRank {get; set;}
		public required int HighestLevelHitter {get; set;}
		public required int HighestLevelPitcher {get; set;}
		public required float WarHitter {get; set;}
		public required float WarPitcher {get; set;}
		public required float PeakWarHitter {get; set;}
		public required float PeakWarPitcher {get; set;}
		public required int TotalPA {get; set;}
		public required int TotalOuts {get; set;}
		public required float RateOff {get; set;}
		public required float RateBsr {get; set;}
		public required float RateDef {get; set;}

		public Model_Players Clone()
		{
			return new Model_Players
			{
				MlbId = this.MlbId,
				IsHitter = this.IsHitter,
				IsPitcher = this.IsPitcher,
				SigningYear = this.SigningYear,
				LastProspectYear = this.LastProspectYear,
				LastProspectMonth = this.LastProspectMonth,
				LastMLBSeason = this.LastMLBSeason,
				AgeAtSigningYear = this.AgeAtSigningYear,
				DraftPick = this.DraftPick,
				DraftSignRank = this.DraftSignRank,
				HighestLevelHitter = this.HighestLevelHitter,
				HighestLevelPitcher = this.HighestLevelPitcher,
				WarHitter = this.WarHitter,
				WarPitcher = this.WarPitcher,
				PeakWarHitter = this.PeakWarHitter,
				PeakWarPitcher = this.PeakWarPitcher,
				TotalPA = this.TotalPA,
				TotalOuts = this.TotalOuts,
				RateOff = this.RateOff,
				RateBsr = this.RateBsr,
				RateDef = this.RateDef,
				
			};
		}
	}
}