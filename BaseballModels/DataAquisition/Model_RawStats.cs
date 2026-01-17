using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class Model_RawStats
    {
        private class TeamLevelStadium
        {
            public required int teamId { get; set; }
            public required int stadiumId { get; set; }
            public required int leagueId { get; set; }
            public required int levelId { get; set; }
            public required float parkFactor { get; set; }
        }

        private static TeamLevelStadium GetTLS(int teamId, int year, SqliteDbContext db, IEnumerable<Player_Hitter_GameLog> yearGames)
        { 
            var teamGames = yearGames.Where(f => f.TeamId == teamId && f.IsHome == 1);
            if (!teamGames.Any())
                return new TeamLevelStadium
                {
                    leagueId = -1,
                    stadiumId = -1,
                    teamId = teamId,
                    parkFactor = -1,
                    levelId = -1
                };

            TeamLevelStadium tls = new()
            {
                teamId = teamId,
                stadiumId = teamGames.Select(f => f.StadiumId).First(),
                leagueId = teamGames.Select(f => f.LeagueId).First(),
                levelId = teamGames.Select(f => f.LevelId).First(),
                parkFactor = 0
            };

            tls.parkFactor = db.Park_Factors.Where(f => f.Year == year && f.StadiumId == tls.stadiumId).Single().RunFactor;

            return tls;
        }

        private static float GetParkFactor(IEnumerable<TeamLevelStadium> thisYearPairs, IEnumerable<TeamLevelStadium> nextYearPairs, int levelIdx, float thisYearFraction, float nextYearFraction)
        {
            var thisStadium = thisYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);
            var nextStadium = nextYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);

            if (!thisStadium.Any() && !nextStadium.Any()) // Short Season A got eliminated
                return -1.0f;

            // Only 1 year of data in stadium, take only that year
            if (!thisStadium.Any())
                return nextStadium.Select(f => f.parkFactor).Average();

            if (!nextStadium.Any())
                return thisStadium.Select(f => f.parkFactor).Average();

            return (thisStadium.Select(f => f.parkFactor).Average() * thisYearFraction) +
                                (nextStadium.Select(f => f.parkFactor).Average() * nextYearFraction);
        }

        private static int[] GetMonthGames(IEnumerable<Team_OrganizationMap> thisYearOrgMap, IEnumerable<Team_OrganizationMap> nextYearOrgMap, IQueryable<Player_Hitter_GameLog> thisYearLogs, IQueryable<Player_Hitter_GameLog> nextYearLogs, int orgId)
        {
            int[] games = { 0,0,0,0,0,0,0,0 };

            foreach (var tm in thisYearOrgMap)
            {
                int teamLevel = thisYearLogs.Where(f => f.TeamId == tm.TeamId).Select(f => f.LevelId).FirstOrDefault();
                if (teamLevel == 0)
                    continue;

                int numGames = thisYearLogs.Where(f => f.TeamId == tm.TeamId).Select(f => f.GameId).Distinct().Count();

                games[Utilities.MlbLevelToModelZeroIndexedLevel(teamLevel)] += numGames;
            }

            foreach (var tm in nextYearOrgMap)
            {
                int teamLevel = nextYearLogs.Where(f => f.TeamId == tm.TeamId).Select(f => f.LevelId).FirstOrDefault();
                if (teamLevel == 0)
                    continue;

                int numGames = nextYearLogs.Where(f => f.TeamId == tm.TeamId).Select(f => f.GameId).Distinct().Count();

                games[Utilities.MlbLevelToModelZeroIndexedLevel(teamLevel)] += numGames;
            }

            games[0] = thisYearLogs.Where(f => f.TeamId == orgId).Select(f => f.GameId).Distinct().Count() +
                nextYearLogs.Where(f => f.TeamId == orgId).Select(f => f.GameId).Distinct().Count();

            return games;
        }

        private static bool OrgLeagueStatus(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.Model_LevelYearGames.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

                // Determine number of games played at each level
                var thisYearGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month > month);
                var nextYearGames = db.Player_Hitter_GameLog.Where(f => f.Year == (year + 1) && f.Month <= month);

                // Get parkFactors for each level
                Model_LevelYearGames lyg = new Model_LevelYearGames
                {
                    Year = year,
                    Month = month,
                    MLB_Games = thisYearGames.Where(f => f.LevelId == 1).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 1).Select(f => f.GameId).Distinct().Count(),
                    AAA_Games = thisYearGames.Where(f => f.LevelId == 11).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 11).Select(f => f.GameId).Distinct().Count(),
                    AA_Games = thisYearGames.Where(f => f.LevelId == 12).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 12).Select(f => f.GameId).Distinct().Count(),
                    HA_Games = thisYearGames.Where(f => f.LevelId == 13).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 13).Select(f => f.GameId).Distinct().Count(),
                    A_Games = thisYearGames.Where(f => f.LevelId == 14).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 14).Select(f => f.GameId).Distinct().Count(),
                    LA_Games = thisYearGames.Where(f => f.LevelId == 15).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 15).Select(f => f.GameId).Distinct().Count(),
                    Rk_Games = thisYearGames.Where(f => f.LevelId == 16).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 16).Select(f => f.GameId).Distinct().Count(),
                    DSL_Games = thisYearGames.Where(f => f.LevelId == 17).Select(f => f.GameId).Distinct().Count() +
                                nextYearGames.Where(f => f.LevelId == 17).Select(f => f.GameId).Distinct().Count(),
                };
                db.Model_LevelYearGames.Add(lyg);

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::OrgLeagueStatus");
                Utilities.LogException(e);
                return false;
            }
        }

        private static bool LeagueBaselines(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.Model_LeagueHittingBaselines.Where(f => f.Year == year && f.Month == month).ExecuteDelete();
                db.Model_LeaguePitchingBaselines.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

                var games = db.Player_Hitter_MonthStats.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var pitchingGames = db.Player_Pitcher_MonthStats.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));

                // Get fraction of games that are in this year vs next
                float thisYearFraction = (9 - month) / 6.0f;
                float nextYearFraction = 1.0f - thisYearFraction;

                var leagueIds = games.Select(f => f.LeagueId).Distinct();
                foreach (var leagueId in leagueIds)
                {
                    var leagueStats = games.Where(f => f.LeagueId == leagueId).Aggregate(Utilities.HitterMonthStatsAggregation);
                    int singles = leagueStats.H - leagueStats.Hit2B - leagueStats.Hit3B - leagueStats.HR;
                    db.Model_LeagueHittingBaselines.Add(new Model_LeagueHittingBaselines
                    {
                        Year = year,
                        Month = month,
                        LeagueId = leagueId,
                        LevelId = Utilities.MlbLevelToModelZeroIndexedLevel(leagueStats.LevelId),
                        Hit1B = Utilities.SafeDivide(singles, leagueStats.PA),
                        Hit2B = Utilities.SafeDivide(leagueStats.Hit2B, leagueStats.PA),
                        Hit3B = Utilities.SafeDivide(leagueStats.Hit3B, leagueStats.PA),
                        HitHR = Utilities.SafeDivide(leagueStats.HR, leagueStats.PA),
                        BB = Utilities.SafeDivide(leagueStats.BB, leagueStats.PA),
                        HBP = Utilities.SafeDivide(leagueStats.HBP, leagueStats.PA),
                        K = Utilities.SafeDivide(leagueStats.K, leagueStats.PA),
                        SB = Utilities.SafeDivide(leagueStats.SB, leagueStats.PA),
                        CS = Utilities.SafeDivide(leagueStats.CS, leagueStats.PA)
                    });

                    // LERP CFIP
                    float thisYearCFIP = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Select(f => f.CFIP).SingleOrDefault();
                    float nextYearCFIP = db.LeagueStats.Where(f => f.Year == year + 1 && f.LeagueId == leagueId).Select(f => f.CFIP).SingleOrDefault();
                    float cfip = 0;
                    if (thisYearCFIP == 0)
                        cfip = nextYearCFIP;
                    else if (nextYearCFIP == 0)
                        cfip = thisYearCFIP;
                    else
                        cfip = (thisYearFraction * thisYearCFIP) + (nextYearFraction * nextYearCFIP);

                    var lps = pitchingGames.Where(f => f.LeagueId == leagueId).Aggregate(Utilities.PitcherMonthStatsAggregation);
                    db.Model_LeaguePitchingBaselines.Add(new Model_LeaguePitchingBaselines
                    {
                        Year = year,
                        Month = month,
                        LeagueId = leagueId,
                        LevelId = Utilities.MlbLevelToModelZeroIndexedLevel(leagueStats.LevelId),
                        ERA = Utilities.SafeDivide(lps.ER * 27, lps.Outs),
                        FIP = Utilities.CalculateFip(cfip, lps.HR, lps.K, lps.BB + lps.HBP, lps.Outs),
                        HR = Utilities.SafeDivide(lps.HR, lps.BattersFaced),
                        BB = Utilities.SafeDivide(lps.BB, lps.BattersFaced),
                        HBP = Utilities.SafeDivide(lps.HBP, lps.BattersFaced),
                        K = Utilities.SafeDivide(lps.K, lps.BattersFaced)
                    });
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::LeagueBaselines");
                Utilities.LogException(e);
                return false;
            }
        }

        const int DEF_BSR_STAT_PA_RATES = 100;

        private static bool HitterPlayerStats(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.Model_HitterLevelStats.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

                var games = db.Player_Hitter_MonthStats.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var leagueBaselines = db.Model_LeagueHittingBaselines.Where(f => f.Year == year && f.Month == month);
                var playerIds = games.Select(f => f.MlbId).Distinct();
                var modelHitterStats = db.Model_HitterStats.Where(f => f.Year == year && f.Month == month);

                int playerIdsCount = playerIds.Count();
                List<Model_HitterLevelStats> output = new(playerIdsCount * 70);

                // Group by player
                foreach(var mlbId in playerIds)
                {
                    // Make sure that Model_HitterStats exists for player, if not skip
                    if (!modelHitterStats.Where(f => f.MlbId == mlbId).Any())
                    {
                        continue;
                    }

                    // Group by League
                    var levelGroups = games.Where(f => f.MlbId == mlbId)
                        .GroupBy(f => f.LevelId);

                    foreach(var lvlG in levelGroups)
                    {
                        // Group by level
                        var leagueGroups = lvlG.GroupBy(f => f.LeagueId);
                        var defStats = db.Player_Fielder_MonthStats.Where(f => f.MlbId == mlbId && ((f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month)) && f.LevelId == lvlG.Key)
                            .Select(f => new { f.ScaledDRAA, f.PosAdjust, f.LevelId }).ToArray();
                        var proFieldingStats = db.Player_MonthlyWar.Where(f => f.MlbId == mlbId && ((f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month)))
                            .Select(f => f.DRAA);
                        int totalPa = leagueGroups.Sum(f => f.Sum(g => g.PA));
                        Model_HitterLevelStats mhls = new Model_HitterLevelStats
                        {
                            MlbId = mlbId,
                            Year = year,
                            Month = month,
                            LevelId = Utilities.MlbLevelToModelZeroIndexedLevel(lvlG.Key),
                            Pa = 0,
                            Hit1B = 1,
                            Hit2B = 1,
                            Hit3B = 1,
                            HitHR = 1,
                            BB = 1,
                            HBP = 1,
                            K = 1,
                            SB = 1,
                            CS = 1,
                            ParkRunFactor = 1,
                            BSR = Utilities.SafeDivide(DEF_BSR_STAT_PA_RATES * db.Player_Hitter_MonthBaserunning.Where(f => f.MlbId == mlbId && ((f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month)) && f.LevelId == lvlG.Key)
                                .Sum(f => f.RBSR), totalPa, 0),
                            DRAA = Utilities.SafeDivide(DEF_BSR_STAT_PA_RATES * (defStats.Where(f => f.LevelId != 1).Sum(f => f.ScaledDRAA) + proFieldingStats.Sum()), totalPa, 0),
                            DPOS = Utilities.SafeDivide(DEF_BSR_STAT_PA_RATES * defStats.Sum(f => f.PosAdjust), totalPa, 0),
                        };

                        // Merge shared league stats together
                        foreach (var statGroup in leagueGroups)
                        {
                            // Get stat rates at this league
                            var stats = statGroup.Aggregate(Utilities.HitterMonthStatsAggregation);
                            var statRates = new Model_LeagueHittingBaselines
                            {
                                Year = -1,
                                Month = -1,
                                LeagueId = -1,
                                LevelId = -1,
                                Hit1B = Utilities.SafeDivide(stats.H - stats.Hit2B - stats.Hit3B - stats.HR, stats.PA, 0),
                                Hit2B = Utilities.SafeDivide(stats.Hit2B, stats.PA, 0),
                                Hit3B = Utilities.SafeDivide(stats.Hit3B, stats.PA, 0),
                                HitHR = Utilities.SafeDivide(stats.HR, stats.PA, 0),
                                BB = Utilities.SafeDivide(stats.BB, stats.PA, 0),
                                HBP = Utilities.SafeDivide(stats.HBP, stats.PA, 0),
                                K = Utilities.SafeDivide(stats.K, stats.PA, 0),
                                SB = Utilities.SafeDivide(stats.SB, stats.PA, 0),
                                CS = Utilities.SafeDivide(stats.CS, stats.PA, 0),
                            };

                            // Adjust to rates of the league
                            Model_LeagueHittingBaselines leagueBaseline = leagueBaselines.Where(f => f.LeagueId == stats.LeagueId).Single();
                            statRates.Hit1B = Utilities.SafeDivide(statRates.Hit1B, leagueBaseline.Hit1B);
                            statRates.Hit2B = Utilities.SafeDivide(statRates.Hit2B, leagueBaseline.Hit2B);
                            statRates.Hit3B = Utilities.SafeDivide(statRates.Hit3B, leagueBaseline.Hit3B);
                            statRates.HitHR = Utilities.SafeDivide(statRates.HitHR, leagueBaseline.HitHR);
                            statRates.BB = Utilities.SafeDivide(statRates.BB, leagueBaseline.BB);
                            statRates.HBP = Utilities.SafeDivide(statRates.HBP, leagueBaseline.HBP);
                            statRates.K = Utilities.SafeDivide(statRates.K, leagueBaseline.K);
                            statRates.SB = Utilities.SafeDivide(statRates.SB, leagueBaseline.SB);
                            statRates.CS = Utilities.SafeDivide(statRates.CS, leagueBaseline.CS);


                            // Combine to level stats based on PA
                            float thisProp = mhls.Pa == 0 ? 1.0f : (float)stats.PA / (mhls.Pa + stats.PA);
                            float otherProp = 1.0f - thisProp;
                            mhls.Pa += stats.PA;
                            mhls.Hit1B = (otherProp * mhls.Hit1B) + (thisProp * statRates.Hit1B);
                            mhls.Hit2B = (otherProp * mhls.Hit2B) + (thisProp * statRates.Hit2B);
                            mhls.Hit3B = (otherProp * mhls.Hit3B) + (thisProp * statRates.Hit3B);
                            mhls.HitHR = (otherProp * mhls.HitHR) + (thisProp * statRates.HitHR);
                            mhls.BB = (otherProp * mhls.BB) + (thisProp * statRates.BB);
                            mhls.HBP = (otherProp * mhls.HBP) + (thisProp * statRates.HBP);
                            mhls.K = (otherProp * mhls.K) + (thisProp * statRates.K);
                            mhls.SB = (otherProp * mhls.SB) + (thisProp * statRates.SB);
                            mhls.CS = (otherProp * mhls.CS) + (thisProp * statRates.CS);
                            mhls.ParkRunFactor = (otherProp * mhls.ParkRunFactor) + (thisProp * stats.ParkRunFactor);
                        }

                        output.Add(mhls);
                    }
                }

                db.BulkInsert(output);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::HitterPlayerStats");
                Utilities.LogException(e);
                return false;
            }
        }

        private static bool PitcherPlayerStats(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.Model_PitcherLevelStats.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

                var games = db.Player_Pitcher_GameLog.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var leagueBaselines = db.Model_LeaguePitchingBaselines.Where(f => f.Year == year && f.Month == month);
                var playerIds = games.Select(f => f.MlbId).Distinct();
                var modelPitcherStats = db.Model_PitcherStats.Where(f => f.Year == year && f.Month == month);

                List<Model_PitcherLevelStats> output = new(playerIds.Count() * 70);
                // Group by player
                foreach (var mlbId in playerIds)
                {
                    // Make sure that Model_PitcherStats exists for player, if not skip
                    if (!modelPitcherStats.Where(f => f.MlbId == mlbId).Any())
                    {
                        continue;
                    }

                    // Group by League
                    var levelGroups = games.Where(f => f.MlbId == mlbId)
                        .GroupBy(f => f.LevelId);

                    foreach (var lvlG in levelGroups)
                    {
                        // Group by level
                        var teamGroups = lvlG.GroupBy(f => new { f.TeamId, f.Year });
                        Model_PitcherLevelStats mhls = new Model_PitcherLevelStats
                        {
                            MlbId = mlbId,
                            Year = year,
                            Month = month,
                            LevelId = Utilities.MlbLevelToModelZeroIndexedLevel(lvlG.Key),
                            Outs_RP = 0,
                            Outs_SP = 0,
                            G = 0,
                            GS = 0,
                            ERA = 1,
                            FIP = 1,
                            HR = 1,
                            BB = 1,
                            HBP = 1,
                            K = 1,
                            ParkRunFactor = 1
                        };
                        int battersFaced = 0;

                        // Merge shared level stats together
                        foreach (var tg in teamGroups)
                        {
                            // Get stat rates at this team
                            var teamStats = tg.Aggregate(Utilities.PitcherGameLogAggregation);
                            var statRates = new Model_LeaguePitchingBaselines
                            {
                                Year = -1,
                                Month = -1,
                                LeagueId = -1,
                                LevelId = -1,
                                ERA = Utilities.SafeDivide(teamStats.ER * 27, teamStats.Outs, teamStats.ER * 27),
                                FIP = Utilities.CalculateFip(db.LeagueStats.Where(f => f.Year == tg.Key.Year && f.LeagueId == teamStats.LeagueId).Single().CFIP, teamStats.HR, teamStats.K, teamStats.BB + teamStats.HBP, teamStats.Outs),
                                HR = Utilities.SafeDivide(teamStats.HR, teamStats.BattersFaced, 0),
                                BB = Utilities.SafeDivide(teamStats.BB, teamStats.BattersFaced, 0),
                                HBP = Utilities.SafeDivide(teamStats.HBP, teamStats.BattersFaced, 0),
                                K = Utilities.SafeDivide(teamStats.K, teamStats.BattersFaced, 0),
                            };

                            // Adjust to rates of the league
                            Model_LeaguePitchingBaselines leagueBaseline = leagueBaselines.Where(f => f.LeagueId == teamStats.LeagueId).Single();
                            statRates.HR = Utilities.SafeDivide(statRates.HR, leagueBaseline.HR);
                            statRates.ERA = Utilities.SafeDivide(statRates.ERA, leagueBaseline.ERA);
                            statRates.FIP = Utilities.SafeDivide(statRates.FIP, leagueBaseline.FIP);
                            statRates.BB = Utilities.SafeDivide(statRates.BB, leagueBaseline.BB);
                            statRates.HBP = Utilities.SafeDivide(statRates.HBP, leagueBaseline.HBP);
                            statRates.K = Utilities.SafeDivide(statRates.K, leagueBaseline.K);

                            // Get parkfactor
                            var stadiumIds = tg.Where(f => f.IsHome == 1).Select(f => f.StadiumId);
                            float parkFactor = stadiumIds.Any() ?
                                db.Park_Factors.Where(f => f.Year == tg.Key.Year && f.StadiumId == stadiumIds.First()).Single().RunFactor :
                                1.0f;

                            // Combine to level stats based on PA
                            float thisProp = battersFaced == 0 ? 1.0f : (float)teamStats.BattersFaced / (battersFaced + teamStats.BattersFaced);
                            float otherProp = 1.0f - thisProp;
                            battersFaced += teamStats.BattersFaced;
                            mhls.ERA = (otherProp * mhls.ERA) + (thisProp * statRates.ERA);
                            mhls.FIP = (otherProp * mhls.FIP) + (thisProp * statRates.FIP);
                            mhls.HR = (otherProp * mhls.HR) + (thisProp * statRates.HR);
                            mhls.BB = (otherProp * mhls.BB) + (thisProp * statRates.BB);
                            mhls.HBP = (otherProp * mhls.HBP) + (thisProp * statRates.HBP);
                            mhls.K = (otherProp * mhls.K) + (thisProp * statRates.K);
                            mhls.ParkRunFactor = (otherProp * mhls.ParkRunFactor) + (thisProp * parkFactor);

                            // Add started/relief stats
                            var startedGames = tg.Where(f => f.Started == 1);
                            var reliefGames = tg.Where(f => f.Started == 0);
                            mhls.G += tg.Count();
                            mhls.GS += startedGames.Count();
                            mhls.Outs_SP += startedGames.Select(f => f.Outs).Sum();
                            mhls.Outs_RP += reliefGames.Select(f => f.Outs).Sum();
                        }

                        output.Add(mhls);
                    }
                }

                db.BulkInsert(output);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::PitcherPlayerStats");
                Utilities.LogException(e);
                return false;
            }
        }

        public static bool UpdateRawStats(int year, int month)
        { 
            try {
                using (ProgressBar progressBar = new(4, $"Calculating Model_RawStats for {year}-{month}"))
                {
                    OrgLeagueStatus(year, month);
                    progressBar.Tick();
                    LeagueBaselines(year, month);
                    progressBar.Tick();
                    HitterPlayerStats(year, month);
                    progressBar.Tick();
                    PitcherPlayerStats(year, month);
                    progressBar.Tick();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in UpdateRawStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
