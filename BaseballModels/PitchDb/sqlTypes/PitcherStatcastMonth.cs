namespace PitchDb
{
	public class PitcherStatcastMonth
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required int ModelId {get; set;}
		public required bool IsValid {get; set;}
		public required float Stuff {get; set;}
		public required float Pitch {get; set;}
		public required float Actual {get; set;}
		public required float Smoothed {get; set;}
		public required int NumPitches {get; set;}
		public float? StuffFastball {get; set;}
		public float? PitchFastball {get; set;}
		public float? ActFastball {get; set;}
		public float? SmoothedFastball {get; set;}
		public required int NumFastballs {get; set;}
		public float? StuffBreaking {get; set;}
		public float? PitchBreaking {get; set;}
		public float? ActBreaking {get; set;}
		public float? SmoothedBreaking {get; set;}
		public required int NumBreaking {get; set;}
		public float? StuffChangeup {get; set;}
		public float? PitchChangeup {get; set;}
		public float? ActChangeup {get; set;}
		public float? SmoothedChangeup {get; set;}
		public required int NumChangeup {get; set;}

		public PitcherStatcastMonth Clone()
		{
			return new PitcherStatcastMonth
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Month = this.Month,
				ModelId = this.ModelId,
				IsValid = this.IsValid,
				Stuff = this.Stuff,
				Pitch = this.Pitch,
				Actual = this.Actual,
				Smoothed = this.Smoothed,
				NumPitches = this.NumPitches,
				StuffFastball = this.StuffFastball,
				PitchFastball = this.PitchFastball,
				ActFastball = this.ActFastball,
				SmoothedFastball = this.SmoothedFastball,
				NumFastballs = this.NumFastballs,
				StuffBreaking = this.StuffBreaking,
				PitchBreaking = this.PitchBreaking,
				ActBreaking = this.ActBreaking,
				SmoothedBreaking = this.SmoothedBreaking,
				NumBreaking = this.NumBreaking,
				StuffChangeup = this.StuffChangeup,
				PitchChangeup = this.PitchChangeup,
				ActChangeup = this.ActChangeup,
				SmoothedChangeup = this.SmoothedChangeup,
				NumChangeup = this.NumChangeup,
			};
		}
	}
}