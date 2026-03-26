namespace Db
{
	public class College_PitcherStats
	{
		public required int TBCId {get; set;}
		public required int Year {get; set;}
		public required int Level {get; set;}
		public required int TeamId {get; set;}
		public required int ConfId {get; set;}
		public required int ExpYears {get; set;}
		public required int G {get; set;}
		public required int GS {get; set;}
		public required int Outs {get; set;}
		public required int H {get; set;}
		public required int R {get; set;}
		public required int ER {get; set;}
		public required int HR {get; set;}
		public required int BB {get; set;}
		public required int K {get; set;}
		public required int HBP {get; set;}
		public required float ERA {get; set;}
		public required float H9 {get; set;}
		public required float HR9 {get; set;}
		public required float BB9 {get; set;}
		public required float K9 {get; set;}
		public required float WHIP {get; set;}
		public required float Age {get; set;}
		public required int Height {get; set;}
		public required int Weight {get; set;}

		public College_PitcherStats Clone()
		{
			return new College_PitcherStats
			{
				TBCId = this.TBCId,
				Year = this.Year,
				Level = this.Level,
				TeamId = this.TeamId,
				ConfId = this.ConfId,
				ExpYears = this.ExpYears,
				G = this.G,
				GS = this.GS,
				Outs = this.Outs,
				H = this.H,
				R = this.R,
				ER = this.ER,
				HR = this.HR,
				BB = this.BB,
				K = this.K,
				HBP = this.HBP,
				ERA = this.ERA,
				H9 = this.H9,
				HR9 = this.HR9,
				BB9 = this.BB9,
				K9 = this.K9,
				WHIP = this.WHIP,
				Age = this.Age,
				Height = this.Height,
				Weight = this.Weight,
			};
		}
	}
}