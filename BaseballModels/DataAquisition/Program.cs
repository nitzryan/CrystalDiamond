namespace DataAquisition
{
    internal class Program
    {
        const int START_YEAR = 2021;
        const int END_YEAR = 2024;
        static async Task Main(string[] args)
        {
            List<int> years = [.. Enumerable.Range(START_YEAR, END_YEAR - START_YEAR+1)];
            List<int> months = [4, 5, 6, 7, 8, 9];
            foreach (int year in years)
            {
                if (!await PlayerUpdate.Main(year))
                    return;

                if (!await GameLogUpdate.Main(year, 3, 10))
                    return;

                if (!ParkFactorUpdate.Main(year))
                    return;

                foreach (int month in months)
                {
                    if (!CalculateLevelStats.Main(year, month))
                        return;

                    if (!CalculateMonthStats.Main(year, month))
                        return;

                    if (!CalculateMonthRatios.Main(year, month))
                        return;
                }

                if (!CalculateAnnualStats.Main(year))
                    return;

                if (!await UpdateParents.Main(year))
                    return;

            }

            //if (!UpdateServiceTime.Main())
            //    return;

            //if (!UpdateCareers.Main(years))
            //    return;

            //foreach (int year in years)
            //{
            //    foreach (int month in months)
            //    {
                    
            //    }
            //}
        }
    }
}
