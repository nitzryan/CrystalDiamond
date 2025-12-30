namespace Db
{
	public class Leagues
	{
		public required int Id {get; set;}
		public required string Abbr {get; set;}
		public required string Name {get; set;}

		public Leagues Clone()
		{
			return new Leagues
			{
				Id = this.Id,
				Abbr = this.Abbr,
				Name = this.Name,
			};
		}
	}
}