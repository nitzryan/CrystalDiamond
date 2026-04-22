namespace Db
{
	public class PitchDateAverages
	{
		public required int Year {get; set;}
		public required int Month {get; set;}
		public required float Extension {get; set;}
		public required float FastballVelo {get; set;}
		public required float Fastball4SeamVert {get; set;}
		public required float Fastball4SeamHoriz {get; set;}
		public required int FastballCount {get; set;}
		public required float SinkerVelo {get; set;}
		public required float SinkerVert {get; set;}
		public required float SinkerHoriz {get; set;}
		public required int SinkerCount {get; set;}
		public required float CurveballVelo {get; set;}
		public required float CurveballHoriz {get; set;}
		public required float CurveballVert {get; set;}
		public required int CurveballCount {get; set;}

		public PitchDateAverages Clone()
		{
			return new PitchDateAverages
			{
				Year = this.Year,
				Month = this.Month,
				Extension = this.Extension,
				FastballVelo = this.FastballVelo,
				Fastball4SeamVert = this.Fastball4SeamVert,
				Fastball4SeamHoriz = this.Fastball4SeamHoriz,
				FastballCount = this.FastballCount,
				SinkerVelo = this.SinkerVelo,
				SinkerVert = this.SinkerVert,
				SinkerHoriz = this.SinkerHoriz,
				SinkerCount = this.SinkerCount,
				CurveballVelo = this.CurveballVelo,
				CurveballHoriz = this.CurveballHoriz,
				CurveballVert = this.CurveballVert,
				CurveballCount = this.CurveballCount,
			};
		}
	}
}