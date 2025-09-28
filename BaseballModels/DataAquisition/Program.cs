namespace DataAquisition
{
    internal class Program
    {
        const int START_YEAR = 2005;
        const int END_YEAR = 2025;
        static async Task Main(string[] args)
        {
            List<int> years = [.. Enumerable.Range(START_YEAR, END_YEAR - START_YEAR + 1)];
            List<int> months = [4, 5, 6, 7, 8, 9];
            //years = [2025];
            //months = [4, 5, 6, 7, 8];
            foreach (int year in years)
            {
                //while (!await DraftResults.Main(year))
                //{ }

                //while (!await PlayerUpdate.Main(year))
                //{ }

                //while (!await GameLogUpdate.Main(year, 3, 10))
                //{ }

                //if (!ParkFactorUpdate.Main(year))
                //    return;

                foreach (int month in months)
                {
                    //if (!CalculateLevelStats.Main(year, month))
                    //    return;

                    //if (!CalculateMonthStats.Main(year, month))
                    //    return;

                    //if (!CalculateMonthRatios.Main(year, month))
                    //    return;
                }

                //if (!CalculateAnnualStats.Main(year))
                //    return;

                //if (!CalculateAnnualOPS.Main(year))
                //    return;

                //while (!await UpdateParents.Main(year))
                //{ }

            }

            if (!UpdateServiceTime.Main())
                return;

            if (!await FangraphsData.Main(years))
                return;

            if (!UpdateCareers.Main(years))
                return;

            if (!ModelPlayers.Main())
                return;

            if (!ModelPlayerWar.Main())
                return;

            //if (!await TransactionLog.Main())
            //    return;

            if (!UpdatePlayerOrgMap.Main())
                return;

            if (!ModelMonthStats.Main(END_YEAR, months.Last()))
                return;

            //while (!await GetLeagues.Main())
            //{ }

            //while (!await SitePlayerBio.Main(END_YEAR))
            //{ }
        }
    }
}
