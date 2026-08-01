using DataAquisition.College;

namespace DataAquisition
{
    internal class Program
    {
        const int START_YEAR = 2005;
        const int END_YEAR = 2026;
        const int END_MONTH = 7;

        const bool UPDATE_COLLEGE_DATA = false;
        const bool FULL_REFRESH = false;
        const bool DATA_UPDATE = false;
        const bool DRAFT_UPDATE = false;
        const bool STATCAST_ONLY_UPDATE = false;

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
                collegeYears = [END_YEAR];
            }

            bool isFullYearUpdate = END_MONTH == 9;

            if ((DATA_UPDATE && isFullYearUpdate) || FULL_REFRESH)
            {
                await PlayerAquisition.FangraphsData.Update(years);
            }

            //Player could be drafted in 2004 and not play until 2005 or later
            if (FULL_REFRESH)
            {
                await PlayerAquisition.DraftResults.Update(2004);
                await PlayerAquisition.PlayerUpdate.DraftOnly(2004);
            }

            if (DATA_UPDATE || FULL_REFRESH)
            {
                foreach (int year in years)
                {
                    if ((FULL_REFRESH && year != END_YEAR) || DRAFT_UPDATE)
                    {
                        while (!await PlayerAquisition.DraftResults.Update(year))
                        { }
                    }

                    while (!await PlayerAquisition.PlayerUpdate.Update(year))
                    { }
                    while (!await GameLog.GameLogUpdate.Update(year, year == END_YEAR, END_YEAR, END_MONTH))
                    { }
                    while (!await FieldingStats.FielderGameLog.Update(year, year == END_YEAR, END_YEAR, END_MONTH))
                    { }
                    while (!await GameLog.GetPlayByPlay.Update(year))
                    { }
                    GameLog.GetPlayByPlayFlags.UpdateFlags(year);
                    LgStats.ParkFactorUpdate.Update(year, year == END_YEAR);
                    LgStats.CalculateLeagueStats.Update(year);

                    foreach (int month in months)
                    {
                        LgStats.CreateLeagueGameCounts.Update(year, month);
                        MonthStats.CalculateMonthStats.Update(year, month);
                        LgStats.CalculateLeagueBaselines.Update(year, month);
                        MonthStats.CalculateMonthStats.UpdateAdvanced(year, month);
                        MonthStats.CalculateMonthRatios.Update(year, month);
                        MonthStats.CalculateMonthBaserunning.Update(year, month);
                        MonthStats.CalculateMonthFielding.Update(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }

                    AnnualStats.CalculateAnnualStats.Update(year);
                    AnnualStats.CalculateAnnualWRC.Update(year);
                    FieldingStats.ScaleFieldingStats.Update(year);

                    foreach (int month in months)
                    {
                        AnnualStats.CalculateAnnualWRC.UpdateMonthRatiosWRC(year, month);
                        MonthStats.CalculateMonthWar.Update(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }

                    while (!await SitePrep.UpdateParents.Update(year))
                    { }
                }
            }

            if ((END_MONTH == 9 && DATA_UPDATE) || FULL_REFRESH)
            {
                ModelStats.UpdateServiceTime.Update();
            }

            ////////// College Model //////////
            if (UPDATE_COLLEGE_DATA)
            {
                if (FULL_REFRESH)
                {
                    College.ReadDataFiles.InsertCollegeHitterStats();
                    College.ReadDataFiles.InsertCollegePitcherStats();
                    College.DataCleanup.Cleanup();
                }

                foreach (var year in collegeYears)
                {
                    // Covid-Year interrupted, don't use data that exists
                    if (year == 2020)
                        continue;

                    if (year > 2025)
                    {
                        College.GetSingleClassData.GetData(year, FULL_REFRESH);
                    }

                    College.TeamData.UpdateConfStrength(year);
                    await College.ColParkFactors.GetParkFactors(year);
                    College.TeamData.CreateConfAverages(year);
                }
                College.ModelStats.CreateHitterModelStats();
                College.ModelStats.CreatePitcherModelStats();
                College.ModelStats.CreatePlayerGaps();

                // Create pro playing-time data
                College.ProData.CreateHittersData(END_YEAR);
                College.ProData.CreatePitchersData(END_YEAR);
            }

            // Need to Set the College_Player draft pick values
            if (DRAFT_UPDATE)
            {
                College.MapDraftedPlayers.Update(END_YEAR);
                SetPlayerDraftPicks.Update(END_YEAR);
            }

            ////////// Model Data //////////
            if (DATA_UPDATE || FULL_REFRESH)
            {
                foreach (var year in years)
                {
                    foreach (var month in months)
                    {
                        ModelStats.LeagueAveragePlayerAge.Update(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }
                }
                
                ModelStats.UpdateCareers.Update(END_MONTH == 9 ? years.Last() : years.Last() - 1);
                ModelStats.ModelPlayers.Update();
                ModelStats.ModelPlayerWar.Update();

                while (!await SitePrep.TransactionLog.Update())
                { }

                SitePrep.UpdatePlayerOrgMap.Update();

                while (!await ModelStats.ModelMonthStats.Update(END_YEAR, months.Last()))
                { }

                ModelStats.Model_MonthValue.Update();

                while (!await LgStats.GetLeagues.Update())
                { }

                while (!await SitePrep.SitePlayerBio.Update(END_YEAR))
                { }

                // 1 Year trailing stats
                foreach (var year in years)
                {
                    foreach (var month in months)
                    {
                        ModelStats.Model_RawStats.UpdateRawStats(year, month);

                        if (year == END_YEAR && month == END_MONTH)
                            break;
                    }
                }
            }

            ////////// Statcast Data //////////
            if (STATCAST_ONLY_UPDATE || DATA_UPDATE || FULL_REFRESH)
            {
                foreach (var year in years)
                {
                    while (!await PitchModeling.PitchData.Update(year, year == years.Last()))
                    { }

                    PitchModeling.PitchValues.UpdateUnsmoothed(year, year == years.Last() || FULL_REFRESH);
                    PitchModeling.PitchValues.UpdateSmoothed(year, year == years.Last() || FULL_REFRESH);
                    PitchModeling.PitchHitterZones.Update(year, year == years.Last() || FULL_REFRESH);

                    PitchModeling.PitchAggregation.CreatePitcherGameBaselines(year);

                    foreach (var month in months)
                    {
                        if (year == END_YEAR && month > END_MONTH)
                            break;

                        PitchModeling.PitchAggregation.CreateLeagueDateAverages(year, month);
                        PitchModeling.HitterStatcastMonths.Update(month, year);
                    }
                }
            }

            #pragma warning disable CS0162
        }
    }
}
