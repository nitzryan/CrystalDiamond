namespace Db
{
	public class Model_TrainingHistory
	{
		public required string Modelname {get; set;}
		public int Year {get; set;}
		public int Ishitter {get; set;}
		public float Testloss {get; set;}
		public int Numlayers {get; set;}
		public int Hiddensize {get; set;}
		public int Modelidx {get; set;}
	}
}