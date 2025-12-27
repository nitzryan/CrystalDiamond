using static System.Net.Mime.MediaTypeNames;

namespace DataAquisition
{
    internal class Program
    {
        const int START_YEAR = 2005;
        const int END_YEAR = 2025;
        const int END_MONTH = 9;
        const bool FORCE_COMPLETE_RESET = false;
        static async Task Main(string[] args)
        {
            List<int> years = [.. Enumerable.Range(START_YEAR, END_YEAR - START_YEAR + 1)];
            List<int> months = [4, 5, 6, 7, 8, 9];
            //years = [2025];
            //months = [9];

            //if (!await FangraphsData.Main(years))
            //    return;

            //Player could be drafted in 2004 and not play until 2005 or later
            //await DraftResults.Main(2004);
            //await PlayerUpdate.DraftOnly(2004);

            foreach (int year in years)
            {
                //while (!await DraftResults.Main(year))
                //{ }

                //while (!await PlayerUpdate.Main(year))
                //{ }

                //while (!await GameLogUpdate.Main(year, 3, 10))
                //{ }

                while (!await GetPlayByPlay.Update(year, year == END_YEAR))
                { }

                //if (!ParkFactorUpdate.Main(year, false))
                //    return;

                //if (!await CalculateLeagueStats.Main(year, false))
                //    return;

                foreach (int month in months)
                {
                    //    if (!CreateLevelGameCounts.Main(year, month))
                    //        return;

                    //if (!CalculateLeagueBaselines.Main(year, month))
                    //    return;

                    //if (!CalculateMonthStats.Main(year, month))
                    //    return;

                    //if (!CalculateMonthRatios.Main(year, month))
                    //    return;

                    if (year == END_YEAR && month == END_MONTH)
                        break;
                }

                //if (!CalculateAnnualStats.Main(year))
                //    return;

                //if (!CalculateAnnualWRC.Main(year))
                //    return;

                foreach (int month in months)
                {
                    //if (!CalculateAnnualWRC.UpdateMonthRatiosWRC(year, month))
                    //    return;

                    //if (year == END_YEAR && month == END_MONTH)
                    //    break;
                }

                //while (!await UpdateParents.Main(year))
                //{ }

            }

            //if (!UpdateServiceTime.Main())
            //    return;

            //if (!UpdateCareers.Main(years))
            //    return;

            //if (!ModelPlayers.Main())
            //    return;

            //if (!ModelPlayerWar.Main())
            //    return;

            //if (!await TransactionLog.Main())
            //    return;

            //if (!UpdatePlayerOrgMap.Main())
            //    return;

            //if (!ModelMonthStats.Main(END_YEAR, months.Last()))
            //    return;

            //if (!Model_MonthValue.Main())
            //    return;

            //while (!await GetLeagues.Main())
            //{ }

            //while (!await SitePlayerBio.Main(END_YEAR))
            //{ }

            // 1 Year trailing stats
            foreach (var year in years)
            {
                foreach (var month in months)
                {
                    //if (year == years.Last() || (year == (years.Last() - 1) && month > END_MONTH))
                    //    break;

                    //if (!Model_RawStats.OrgLeagueStatus(year, month, FORCE_COMPLETE_RESET))
                    //    return;

                    //if (!Model_RawStats.LeagueBaselines(year, month, FORCE_COMPLETE_RESET))
                    //    return;

                    //if (!Model_RawStats.HitterPlayerStats(year, month, FORCE_COMPLETE_RESET))
                    //    return;

                    //if (!Model_RawStats.PitcherPlayerStats(year, month, FORCE_COMPLETE_RESET))
                    //    return;
                }
            }
        }
    }
}
