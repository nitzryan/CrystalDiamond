namespace PitchAnalysis
{
    internal class Program
    {
        const int START_YEAR = 2017;
        const int END_YEAR = 2026;
        const int END_MONTH = 4;
        const bool FORCE_REFRESH = true;

        static void Main(string[] args)
        {
            //PitchAggregation.Update();
            //YearDeviations.Update(END_YEAR, FORCE_REFRESH);
            //PitchStatcastOutput.Update();
            //PitcherAggregator.CreateStats();
            for (int year = START_YEAR; year <= END_YEAR; year++)
            {
                for (int month = 4; month <= 9; month++)
                {
                    if (year == END_YEAR && month > END_MONTH)
                        continue;

                    //MonthStats.Update(month, year);
                }
                ModelViewerMinMax.Update(year, year == END_YEAR || FORCE_REFRESH);
            }

            //NullMonthStats.Update();
        }
    }
}
