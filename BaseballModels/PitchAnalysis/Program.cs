namespace PitchAnalysis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PitchAggregation.Update();

            PitcherAggregator.CreateStats();
        }
    }
}
