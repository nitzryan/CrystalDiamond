namespace Db
{
	public class Model_TrainingHistory
	{
		public required string Modelname {get; set;}
		public required int Year {get; set;}
		public required int Ishitter {get; set;}
		public required float Testloss {get; set;}
		public required int Numlayers {get; set;}
		public required int Hiddensize {get; set;}
		public required int Modelidx {get; set;}
	}
}