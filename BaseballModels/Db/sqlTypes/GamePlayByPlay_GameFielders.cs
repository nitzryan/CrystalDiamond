namespace Db
{
	public class GamePlayByPlay_GameFielders
	{
		public required int GameId {get; set;}
		public required bool IsHome {get; set;}
		public required int IdP {get; set;}
		public required int IdC {get; set;}
		public required int Id1B {get; set;}
		public required int Id2B {get; set;}
		public required int Id3B {get; set;}
		public required int IdSS {get; set;}
		public required int IdLF {get; set;}
		public required int IdCF {get; set;}
		public required int IdRF {get; set;}
		public required string SubList {get; set;}

		public GamePlayByPlay_GameFielders Clone()
		{
			return new GamePlayByPlay_GameFielders
			{
				GameId = this.GameId,
				IsHome = this.IsHome,
				IdP = this.IdP,
				IdC = this.IdC,
				Id1B = this.Id1B,
				Id2B = this.Id2B,
				Id3B = this.Id3B,
				IdSS = this.IdSS,
				IdLF = this.IdLF,
				IdCF = this.IdCF,
				IdRF = this.IdRF,
				SubList = this.SubList,
			};
		}
	}
}