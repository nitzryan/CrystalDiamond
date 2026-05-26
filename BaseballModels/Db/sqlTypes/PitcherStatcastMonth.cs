namespace Db
{
	public class PitcherStatcastMonth
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Stuff {get; set;}
		public required float Pitch {get; set;}
		public required float Actual {get; set;}
		public required int NumPitches {get; set;}
		public float? StuffFastball {get; set;}
		public float? PitchFastball {get; set;}
		public float? ActFastball {get; set;}
		public required int NumFastballs {get; set;}
		public float? StuffBreaking {get; set;}
		public float? PitchBreaking {get; set;}
		public float? ActBreaking {get; set;}
		public required int NumBreaking {get; set;}
		public float? StuffChangeup {get; set;}
		public float? PitchChangeup {get; set;}
		public float? ActChangeup {get; set;}
		public required int NumChangeup {get; set;}

		public PitcherStatcastMonth Clone()
		{
			return new PitcherStatcastMonth
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				Stuff = this.Stuff,
				Pitch = this.Pitch,
				Actual = this.Actual,
				NumPitches = this.NumPitches,
				StuffFastball = this.StuffFastball,
				PitchFastball = this.PitchFastball,
				ActFastball = this.ActFastball,
				NumFastballs = this.NumFastballs,
				StuffBreaking = this.StuffBreaking,
				PitchBreaking = this.PitchBreaking,
				ActBreaking = this.ActBreaking,
				NumBreaking = this.NumBreaking,
				StuffChangeup = this.StuffChangeup,
				PitchChangeup = this.PitchChangeup,
				ActChangeup = this.ActChangeup,
				NumChangeup = this.NumChangeup,
			};
		}
	}
}