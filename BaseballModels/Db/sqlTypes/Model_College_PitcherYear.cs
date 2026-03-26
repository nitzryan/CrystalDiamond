namespace Db
{
	public class Model_College_PitcherYear
	{
		public required int TBCId {get; set;}
		public required int Level {get; set;}
		public required int ExpYears {get; set;}
		public required float ParkRunFactor {get; set;}
		public required float ConfScore {get; set;}
		public required int G {get; set;}
		public required int GS {get; set;}
		public required int Outs {get; set;}
		public required float ERA {get; set;}
		public required float H9 {get; set;}
		public required float HR9 {get; set;}
		public required float BB9 {get; set;}
		public required float K9 {get; set;}
		public required float WHIP {get; set;}
		public required float Age {get; set;}
		public required int Height {get; set;}
		public required int Weight {get; set;}

		public Model_College_PitcherYear Clone()
		{
			return new Model_College_PitcherYear
			{
				TBCId = this.TBCId,
				Level = this.Level,
				ExpYears = this.ExpYears,
				ParkRunFactor = this.ParkRunFactor,
				ConfScore = this.ConfScore,
				G = this.G,
				GS = this.GS,
				Outs = this.Outs,
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