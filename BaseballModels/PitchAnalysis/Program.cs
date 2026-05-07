namespace PitchAnalysis
{
    internal class Program
    {
        const int END_YEAR = 2026;
        const bool FORCE_REFRESH = false;

        static void Main(string[] args)
        {
            YearDeviations.Update(END_YEAR, FORCE_REFRESH);
            PitchAggregation.Update();

            PitcherAggregator.CreateStats();


        }
    }
}
