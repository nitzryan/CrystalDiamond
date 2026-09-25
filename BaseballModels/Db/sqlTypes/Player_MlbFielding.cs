namespace Db
{
	public partial class Player_MlbFielding
	{
		public required int MlbId {get; set;}
		public required int Year {get; set;}
		public required DbEnums.Position Position {get; set;}
		public required int Outs {get; set;}
		public required float DRAA {get; set;}

		public Player_MlbFielding Clone()
		{
			return new Player_MlbFielding
			{
				MlbId = this.MlbId,
				Year = this.Year,
				Position = this.Position,
				Outs = this.Outs,
				DRAA = this.DRAA,
			};
		}
	}
}