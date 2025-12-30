namespace Db
{
	public class Team_Parents
	{
		public required int Id {get; set;}
		public required string Abbr {get; set;}
		public required string Name {get; set;}

		public Team_Parents Clone()
		{
			return new Team_Parents
			{
				Id = this.Id,
				Abbr = this.Abbr,
				Name = this.Name,
			};
		}
	}
}