namespace SiteDb
{
	public class QualityCode
	{
		public required string Category {get; set;}
		public required DbEnums.TimestepQuality Code {get; set;}
		public required DbEnums.Severity Severity {get; set;}
		public required string Label {get; set;}
		public required string Blurb {get; set;}

		public QualityCode Clone()
		{
			return new QualityCode
			{
				Category = this.Category,
				Code = this.Code,
				Severity = this.Severity,
				Label = this.Label,
				Blurb = this.Blurb,
				
			};
		}
	}
}