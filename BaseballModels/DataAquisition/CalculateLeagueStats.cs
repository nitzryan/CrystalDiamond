using Db;
using EFCore.BulkExtensions;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class CalculateLeagueStats
    {
        private record GameScenarios
        {
            public required int outs { get; set; }
            public required BaseOccupancy occupancy { get; set; }
        }

        private static float GetAverageEventValue(Dictionary<GameScenarios, float> runExpectancyDict, IEnumerable<GamePlayByPlay> events, PBP_Events e)
        {
            var selectedEvents = events.Where(f => (f.Result & e) != 0);

            int numEvents = selectedEvents.Count();
            float runningTotal = 0;
            foreach (var evnt in selectedEvents)
            {
                runningTotal += runExpectancyDict[new GameScenarios { occupancy = evnt.EndBaseOccupancy, outs = evnt.EndOuts }];
                runningTotal -= runExpectancyDict[new GameScenarios { occupancy = evnt.StartBaseOccupancy, outs = evnt.StartOuts }];
                runningTotal += evnt.RunsScored;
            }

            return runningTotal / numEvents;
        }

        public static bool Main(int year)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.LeagueStats.RemoveRange(db.LeagueStats.Where(f => f.Year == year));
                db.SaveChanges();

                var leagues = db.Player_Hitter_GameLog.Where(f => f.Year == year)
                    .Select(f => f.LeagueId).Distinct();

                var yearGames = db.Player_Hitter_GameLog.Where(f => f.Year == year).ToList();

                using (ProgressBar progressBar = new(leagues.Count(), $"Generating League Stats for {year}"))
                {
                    foreach (int league in leagues)
                    {
                        // Check if league already has data for year
                        if (db.LeagueStats.Any(f => f.Year == year && f.LeagueId == league))
                        {
                            progressBar.Tick();
                            continue;
                        }

                        var leaguePBP = db.GamePlayByPlay.Where(f => f.Year == year && f.LeagueId == league).ToList();

                        // Determine the run expectancy of every out/base pairing
                        Dictionary<GameScenarios, float> runExpectancyDict = new();
                        int[] outs = [0, 1, 2];
                        BaseOccupancy[] occupancies = Enumerable.Range(0, 8).Select(f => (BaseOccupancy)f).ToArray();
                        foreach (var o in outs)
                        {
                            foreach (var occ in occupancies)
                            {
                                var situationEvents = leaguePBP.Where(f => f.StartOuts == o && f.StartBaseOccupancy == occ);
                                int numEvents = situationEvents.Count();
                                int numRuns = situationEvents.Sum(f => f.RunsScoredInningAfterEvent);

                                GameScenarios gs = new() { occupancy = occ, outs = o };
                                runExpectancyDict.Add(gs, (float)numRuns / (float)numEvents);
                            }
                        }

                        // Add in 3 outs being no runs
                        runExpectancyDict.Add(new GameScenarios { occupancy = BaseOccupancy.Empty, outs = 3 }, 0);

                        // Get values for selected events
                        float w1B = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.SINGLE);
                        float w2B = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.DOUBLE);
                        float w3B = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.TRIPLE);
                        float wHR = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.HR);
                        float wBB = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.BB);
                        float wHBP = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.HBP);
                        float runSB = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.SB);
                        float runCS = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.CS);
                        float runErr = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.FIELD_ERROR);
                        float wOuts = GetAverageEventValue(runExpectancyDict, leaguePBP, PBP_Events.GNDOUT | PBP_Events.GIDP | PBP_Events.LINEOUT | PBP_Events.SAC_FLY | PBP_Events.FLYOUT | PBP_Events.FIELDERS_CHOICE | PBP_Events.FIELDERS_CHOICE_OUT);

                        // Adjust values so out is worth 0
                        w1B -= wOuts;
                        w2B -= wOuts;
                        w3B -= wOuts;
                        wHR -= wOuts;
                        wBB -= wOuts;
                        wHBP -= wOuts;
                        // Get league hitting stats
                        var leagueHittingStats = yearGames.Where(f => f.LeagueId == league).Aggregate(Utilities.HitterGameLogAggregation);

                        int singles = leagueHittingStats.H - leagueHittingStats.Hit2B - leagueHittingStats.Hit3B - leagueHittingStats.HR;
                        float wobaAccumulator = ((w1B * singles) + (w2B * leagueHittingStats.Hit2B) + (w3B * leagueHittingStats.Hit3B) + (wHR * leagueHittingStats.HR) + (wBB * leagueHittingStats.BB) + (wHBP * leagueHittingStats.HBP)) / leagueHittingStats.PA;
                        float obp = (float)(leagueHittingStats.H + leagueHittingStats.BB + leagueHittingStats.HBP) / leagueHittingStats.PA;
                        float wobaScale = obp / wobaAccumulator;

                        w1B *= wobaScale;
                        w2B *= wobaScale;
                        w3B *= wobaScale;
                        wHR *= wobaScale;
                        wBB *= wobaScale;
                        wHBP *= wobaScale;


                        // Get runs per win
                        int totalRunsScoredInLeague = leaguePBP.Select(f => f.RunsScored).Sum();
                        int totalInnings = leaguePBP.GroupBy(f => f.GameId).Sum(f => f.Max(g => g.Inning));
                        float runsPerWin = 10.0f * (float)Math.Sqrt((float)totalRunsScoredInLeague / totalInnings);

                        // Calculate Pitching adjustments
                        var leagueStats = db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.LeagueId == league);
                        int leagueHRs = leagueStats.Select(f => f.HR).Sum();
                        int leagueBBs = leagueStats.Select(f => f.BB + f.HBP).Sum();
                        int leagueKs = leagueStats.Select(f => f.K).Sum();
                        int leagueRuns = leagueStats.Select(f => f.R).Sum();
                        int leagueERs = leagueStats.Select(f => f.ER).Sum();
                        int leagueOuts = leagueStats.Select(f => f.Outs).Sum();

                        float leagueRA = (float)leagueRuns / leagueOuts * 27;
                        float leagueERA = (float)leagueERs / leagueOuts * 27;
                        float leagueFIP = Utilities.CalculateFip(0, leagueHRs, leagueKs, leagueBBs, leagueOuts);

                        // Get hitter stats for non-pitchers
                        Player_Hitter_GameLog lps = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league && f.Position != 1)
                            .Aggregate(Utilities.HitterGameLogAggregation);

                        double leaguewRC = (lps.BB * wBB) +
                            (lps.HBP * wHBP) +
                            ((lps.H - lps.Hit2B - lps.Hit3B - lps.HR) * w1B) +
                            (lps.Hit2B * w2B) +
                            (lps.Hit3B * w3B) +
                            (lps.HR * wHR);

                        float leagueHittersWOBA = (float)leaguewRC / lps.PA;

                        db.LeagueStats.Add(new LeagueStats
                        {
                            LeagueId = league,
                            Year = year,
                            AvgWOBA = obp,
                            AvgHitterWOBA = leagueHittersWOBA,
                            WOBAScale = wobaScale,
                            W1B = w1B,
                            W2B = w2B,
                            W3B = w3B,
                            WHR = wHR,
                            WBB = wBB,
                            WHBP = wHBP,
                            RunCS = runCS,
                            RunSB = runSB,
                            RunErr = runErr,
                            RPerPA = (float)totalRunsScoredInLeague / leagueHittingStats.PA,
                            RPerWin = runsPerWin,
                            LeaguePA = db.Player_Hitter_YearAdvanced.Where(f => f.Year == year && f.LeagueId == league)
                                .Select(f => f.PA).Sum(),
                            LeagueGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league)
                                .Select(f => f.GameId).Distinct().Count(),
                            CFIP = leagueERA - leagueFIP,
                            FIPR9Adjustment = leagueRA - leagueERA,
                            LeagueERA = leagueERA,
                        });

                        db.SaveChanges();
                        progressBar.Tick();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateLeagueStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
