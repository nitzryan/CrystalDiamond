namespace DataAquisition
{
    internal class Program
    {
        const int START_YEAR = 2005;
        const int END_YEAR = 2025;
        const int END_MONTH = 9;

        const bool UPDATE_COLLEGE_DATA = false;
        const bool FULL_REFRESH = false;
        const bool DATA_UPDATE = false;
        const bool STATCAST_ONLY_UPDATE = true;

        static async Task Main(string[] args)
        {
            List<int> years = [.. Enumerable.Range(START_YEAR, END_YEAR - START_YEAR + 1)];
            List<int> collegeYears = [.. Enumerable.Range(2002, END_YEAR - 2002 + 1)];
            List<int> months = [4, 5, 6, 7, 8, 9];

            #pragma warning disable CS0162
            if (DATA_UPDATE)
            {
                years = [END_YEAR];
                months = [END_MONTH];
            }

            bool isFullYearUpdate = END_MONTH == 9;

            if ((DATA_UPDATE && isFullYearUpdate) || FULL_REFRESH)
            {
                await FangraphsData.Update(years);
            }

            //Player could be drafted in 2004 and not play until 2005 or later
            if (FULL_REFRESH)
            {
                await DraftResults.Update(2004);
                await PlayerUpdate.DraftOnly(2004);
            }

            if (DATA_UPDATE || FULL_REFRESH)
            {
                foreach (int year in years)
                {
                    while (!await DraftResults.Update(year))
                    { }
                    while (!await PlayerUpdate.Update(year))
                    { }
                    while (!await GameLogUpdate.Update(year, year == END_YEAR))
                    { }
                    while (!await FielderGameLog.Update(year, year == END_YEAR))
                    { }
                    while (!await GetPlayByPlay.Update(year))
                    { }
                    GetPlayByPlayFlags.UpdateFlags(year);
                    ParkFactorUpdate.Update(year, false);
                    CalculateLeagueStats.Update(year);

                    foreach (int month in months)
                    {
                        CreateLevelGameCounts.Update(year, month);
                        CalculateLeagueBaselines.Update(year, month);
                        CalculateMonthStats.Update(year, month);
                        CalculateMonthRatios.Update(year, month);
                        CalculateMonthBaserunning.Update(year, month);
                        CalculateMonthFielding.Update(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }

                    CalculateAnnualStats.Update(year);
                    CalculateAnnualWRC.Update(year);
                    ScaleFieldingStats.Update(year);

                    foreach (int month in months)
                    {
                        CalculateAnnualWRC.UpdateMonthRatiosWRC(year, month);
                        CalculateMonthWar.Update(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }

                    while (!await UpdateParents.Update(year))
                    { }
                }
            }
            
            if ((END_MONTH == 9 && DATA_UPDATE) || FULL_REFRESH)
            {
                UpdateServiceTime.Update();
            }

            if (DATA_UPDATE || FULL_REFRESH)
            {
                UpdateCareers.Update(years);
                ModelPlayers.Update();
                ModelPlayerWar.Update();

                while (!await TransactionLog.Update())
                { }

                UpdatePlayerOrgMap.Update();

                while (!await ModelMonthStats.Update(END_YEAR, months.Last()))
                { }

                Model_MonthValue.Update();

                while (!await GetLeagues.Update())
                { }

                while (!await SitePlayerBio.Update(END_YEAR))
                { }

                // 1 Year trailing stats
                foreach (var year in years)
                {
                    foreach (var month in months)
                    {
                        if (year == years.Last() || (year == (years.Last() - 1) && month > END_MONTH))
                            break;

                        Model_RawStats.UpdateRawStats(year, month);
                    }
                }
            }

            ////////// Statcast Data //////////
            if (STATCAST_ONLY_UPDATE || DATA_UPDATE || FULL_REFRESH)
            {
                foreach (var year in years)
                {
                    while (!await PitchData.Update(year, year == years.Last()))
                    { }

                    
                }
            }
            

            ////////// College Model //////////
            if (UPDATE_COLLEGE_DATA)
            {
                College.InsertCollegeHitterStats();
                College.InsertCollegePitcherStats();
                College.DataCleanup();
                College.FixDraftedMissingMLBIds();
                College.HandleTwoWayDraftedPlayers();
                foreach (var year in collegeYears)
                {
                    // Covid-Year interrupted, don't use data that exists
                    if (year == 2020)
                        continue;

                    College.UpdateConfStrength(year);
                    await College.GetParkFactors(year);
                    College.CreateConfAverages(year);
                }
                College.CreateHitterModelStats();
                College.CreatePitcherModelStats();
                College.CreatePlayerGaps();

                // Create pro playing-time data
                College.CreateCollegeHittersProData(END_YEAR);
                College.CreateCollegePitchersProData(END_YEAR);
            }

            #pragma warning disable CS0162
        }
    }
}
