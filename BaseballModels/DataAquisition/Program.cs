namespace DataAquisition
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            for (int year = 2005; year <= 2006; year++)
            {
                //if (!await PlayerUpdate.Main(year))
                //    return;

                //if (!await GameLogUpdate.Main(year, 3, 10))
                //    return;

                //if (!ParkFactorUpdate.Main(year))
                //    return;

                for (int month = 4; month <= 9; month++)
                {
                    //if (!CalculateLevelStats.Main(year, month))
                    //    return;

                    //if (!CalculateMonthStats.Main(year, month))
                    //    return;

                    if (!CalculateMonthRatios.Main(year, month))
                        return;
                }
            }
        }
    }
}
