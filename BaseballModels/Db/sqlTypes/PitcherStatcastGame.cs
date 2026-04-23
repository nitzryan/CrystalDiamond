namespace Db
{
	public class PitcherStatcastGame
	{
		public required int MlbId {get; set;}
		public required int GameId {get; set;}
		public required int LevelId {get; set;}
		public required int Year {get; set;}
		public required int Month {get; set;}
		public float? FastballVelo {get; set;}
		public float? FastballBreakHoriz {get; set;}
		public float? FastballBreakInduced {get; set;}
		public float? FastballBreakVert {get; set;}
		public float? SinkerVelo {get; set;}
		public float? SinkerBreakHoriz {get; set;}
		public float? SinkerBreakInduced {get; set;}
		public float? SinkerBreakVert {get; set;}

		public PitcherStatcastGame Clone()
		{
			return new PitcherStatcastGame
			{
				MlbId = this.MlbId,
				GameId = this.GameId,
				LevelId = this.LevelId,
				Year = this.Year,
				Month = this.Month,
				FastballVelo = this.FastballVelo,
				FastballBreakHoriz = this.FastballBreakHoriz,
				FastballBreakInduced = this.FastballBreakInduced,
				FastballBreakVert = this.FastballBreakVert,
				SinkerVelo = this.SinkerVelo,
				SinkerBreakHoriz = this.SinkerBreakHoriz,
				SinkerBreakInduced = this.SinkerBreakInduced,
				SinkerBreakVert = this.SinkerBreakVert,
			};
		}
	}
}