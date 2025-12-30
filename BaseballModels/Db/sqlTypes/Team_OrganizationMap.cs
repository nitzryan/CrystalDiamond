namespace Db
{
	public class Team_OrganizationMap
	{
		public required int TeamId {get; set;}
		public required int Year {get; set;}
		public required int ParentOrgId {get; set;}

		public Team_OrganizationMap Clone()
		{
			return new Team_OrganizationMap
			{
				TeamId = this.TeamId,
				Year = this.Year,
				ParentOrgId = this.ParentOrgId,
			};
		}
	}
}