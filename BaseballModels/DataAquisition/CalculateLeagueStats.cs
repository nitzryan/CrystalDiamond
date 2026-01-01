using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class CalculateLeagueStats
    {
        private static float GetAverageEventValue(GameScenarioDict runExpectancyDict, IEnumerable<GamePlayByPlay> events, PBP_Events e)
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

        private static float GetAverageEventValue(GameScenarioDict runExpectancyDict, IEnumerable<GamePlayByPlay> events)
        {
            int numEvents = events.Count();
            if (numEvents == 0)
                return 0;
            float runningTotal = 0;
            foreach (var evnt in events)
            {
                runningTotal += runExpectancyDict[new GameScenarios { occupancy = evnt.EndBaseOccupancy, outs = evnt.EndOuts }];
                runningTotal -= runExpectancyDict[new GameScenarios { occupancy = evnt.StartBaseOccupancy, outs = evnt.StartOuts }];
                runningTotal += evnt.RunsScored;
            }

            return runningTotal / numEvents;
        }

        private static DoublePlayResult GetDoublePlayResult(GameScenarioDict runExpectancyDict, IEnumerable<GamePlayByPlay> events, DoublePlayScenario scenario)
        {
            events = PBP_Utilities.GetDoublePlayOpportunities(events);

            // Get expectancy and count of each event;
            var dpEvents = events.Where(f => f.EndOuts - f.StartOuts >= 2);
            var noOutEvents = events.Where(f => f.EndOuts == f.StartOuts);
            var leadOnlyEvents = events.Where(f => (f.EndOuts - f.StartOuts == 1) && (f.Run1stOutcome == 0 || f.Run2ndOutcome == 0 || f.Run3rdOutcome == 0));
            var hitterOnlyEvents = events.Where(f => (f.EndOuts - f.StartOuts == 1) && f.Run1stOutcome != 0 && f.Run2ndOutcome != 0 && f.Run3rdOutcome != 0);

            int dpCount = dpEvents.Count();
            int noCount = noOutEvents.Count();
            int leadCount = leadOnlyEvents.Count();
            int hitterCount = hitterOnlyEvents.Count();
            
            float dpExpectancy = GetAverageEventValue(runExpectancyDict, dpEvents);
            float noOutExpectancy = GetAverageEventValue(runExpectancyDict, noOutEvents);
            float leadOnlyExpectany = GetAverageEventValue(runExpectancyDict, leadOnlyEvents);
            float hitterOnlyExpectancy = GetAverageEventValue(runExpectancyDict, hitterOnlyEvents);

            // Adjust all so that the expected value is 0, since eval is of fielders not hitters

            int totalEvents = dpCount + noCount + leadCount + hitterCount;
            float avgExpectancy = ((dpExpectancy * dpCount) + 
                                    (noOutExpectancy * noCount) + 
                                    (leadOnlyExpectany * leadCount) + 
                                    (hitterOnlyExpectancy * hitterCount)) / totalEvents;

            dpExpectancy -= avgExpectancy;
            noOutExpectancy -= avgExpectancy;
            leadOnlyExpectany -= avgExpectancy;
            hitterOnlyExpectancy -= avgExpectancy;

            return new DoublePlayResult
            {
                ProbsLeadingOnly = (float)leadCount / totalEvents,
                ProbsDP = (float)dpCount / totalEvents,
                ProbsHitterOnly = (float)hitterCount / totalEvents,
                ProbsNeither = (float)noCount / totalEvents,
                RunsDP = dpExpectancy / 2, // Gives expectancy for each player, 2 players get adjusted
                RunsHitter = hitterOnlyExpectancy / 2,
                RunsLeading = leadOnlyExpectany / 2,
                RunsNeither = noOutExpectancy / 2,
                NumOccurences = totalEvents
            };

        }

        private static BaserunningResult GetBaserunningResult(GameScenarioDict runExpectancyDict, IEnumerable<GamePlayByPlay> events, int startBase, int targetBase)
        {
            IEnumerable<GamePlayByPlay> advances, stays, outs;

            if (startBase == 1)
            {
                advances = events.Where(f => f.Run1stOutcome >= targetBase);
                stays = events.Where(f => f.Run1stOutcome < targetBase && f.Run1stOutcome > 0);
                outs = events.Where(f => f.Run1stOutcome == 0);
            } 
            else if (startBase == 2)
            {
                advances = events.Where(f => f.Run2ndOutcome >= targetBase);
                stays = events.Where(f => f.Run2ndOutcome < targetBase && f.Run2ndOutcome > 0);
                outs = events.Where(f => f.Run2ndOutcome == 0);
            }
            else if (startBase == 3)
            {
                advances = events.Where(f => f.Run3rdOutcome >= targetBase);
                stays = events.Where(f => f.Run3rdOutcome < targetBase && f.Run3rdOutcome > 0);
                outs = events.Where(f => f.Run3rdOutcome == 0);
            }
            else
            {
                throw new Exception($"Unexpected startBase in GetBaserunningResult: {startBase}");
            }
            

            int numAdvances = advances.Count();
            int numStays = stays.Count();
            int numOuts = outs.Count();
            int totalCount = numAdvances + numStays + numOuts;

            float advancesRuns = GetAverageEventValue(runExpectancyDict, advances);
            float staysRuns = GetAverageEventValue(runExpectancyDict, stays);
            float outsRuns = GetAverageEventValue(runExpectancyDict, outs);

            // Get total value and adjust all so the mean is 0
            float totalValue = (advancesRuns * numAdvances) + (staysRuns * numStays) + (outsRuns * numOuts);
            float totalValuePer = totalValue / totalCount;
            advancesRuns -= totalValuePer;
            staysRuns -= totalValuePer;
            outsRuns -= totalValuePer;

            if (numAdvances == 0)
                advancesRuns = 0;
            if (numStays == 0)
                staysRuns = 0;
            if (numOuts == 0)
                outsRuns = 0;

            if (totalCount == 0)
                totalCount = 1;

            return new BaserunningResult
            {
                ProbAdvance = (float)numAdvances / totalCount,
                ProbStay = (float)numStays / totalCount,
                ProbOut = (float)numOuts / totalCount,
                RunsAdvance = advancesRuns,
                RunsStay = staysRuns,
                RunsOut = outsRuns,
                NumOccurences = numAdvances + numStays + numOuts,
            };
        }

        public static bool Main(int year)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.LeagueStats.Where(f => f.Year == year).ExecuteDelete();
                db.LeagueRunMatrix.Where(f => f.Year == year).ExecuteDelete();

                // Get leagues, with combined Major Leagues
                var leagues = new List<int>() { 1 }.Concat(db.Player_Hitter_GameLog.Where(f => f.Year == year)
                    .Select(f => f.LeagueId).Distinct().ToArray());
                

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

                        // Get PBP data for year
                        var leaguePBP = league == 1 ?
                            db.GamePlayByPlay.Where(f => (f.Year == year) && (f.LeagueId == 103 || f.LeagueId == 104) && f.EventFlag == GameFlags.Valid).ToArray() :
                            db.GamePlayByPlay.Where(f => (f.Year == year) && f.LeagueId == league && f.EventFlag == GameFlags.Valid).ToArray();

                        // Determine the run expectancy of every out/base pairing
                        GameScenarioDict runExpectancyDict = new();
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

                        // Get GIDP run values
                        var dpOpportunities = PBP_Utilities.GetDoublePlayOpportunities(leaguePBP);
                        int gidpOpportunities = dpOpportunities.Count();
                        int gidpOccurances = dpOpportunities.Count(f => f.Result.HasFlag(PBP_Events.GIDP));
                        float probGIDP = (float)gidpOccurances / (float)gidpOpportunities;
                        float runGIDP = GetAverageEventValue(runExpectancyDict, dpOpportunities, PBP_Events.GIDP) - wOuts;

                        // Get league hitting stats
                        var leagueHittingStats = (league == 1 ? 
                            yearGames.Where(f => f.LeagueId == 103 || f.LeagueId == 104) : 
                            yearGames.Where(f => f.LeagueId == league)).Aggregate(Utilities.HitterGameLogAggregation);

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
                        var leagueStats = league == 1 ?
                            db.Player_Pitcher_GameLog.Where(f => f.Year == year && (f.LeagueId == 103 || f.LeagueId == 104)) : 
                            db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.LeagueId == league);
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
                        Player_Hitter_GameLog lps = (league == 1 ?
                            db.Player_Hitter_GameLog.Where(f => f.Year == year && (f.LeagueId == 103 || f.LeagueId == 104) && f.Position != 1) : 
                            db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league && f.Position != 1))
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
                            RunGIDP = runGIDP,
                            ProbGIDP = probGIDP,
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

                        // Calculate League Run Matrix
                        if (league != 103 && league != 104)
                        {
                            // Calculate Fielding Outcome Dict
                            FieldingDict fieldingDict = new();
                            var leaguePBPFieldingGroupings = leaguePBP.Where(f => f.EventFlag == GameFlags.Valid).GroupBy(f => PBP_TypeConversions.GetFieldingScenario(f));
                            foreach (var fg in leaguePBPFieldingGroupings)
                            {
                                if (fg.Key == null)
                                    continue;

                                var madePlays = fg.Where(f => (f.Result & PBP_HIT_EVENT) == 0);
                                var missedPlays = fg.Where(f => (f.Result & PBP_HIT_EVENT) != 0);
                                int[] madePlaysCount = [0, 0, 0, 0, 0, 0, 0, 0, 0];
                                foreach (var play in madePlays)
                                {
                                    madePlaysCount[(int)play.HitZone - 1]++;
                                }

                                int totalPlays = madePlays.Count() + missedPlays.Count();
                                float[] probMake = [.. madePlaysCount.Select(f => (float)f / totalPlays)];
                                float probMissed = 1.0f - probMake.Sum();

                                float madeValue = GetAverageEventValue(runExpectancyDict, madePlays);
                                float missedValue = GetAverageEventValue(runExpectancyDict, missedPlays);

                                float totalValue = (madeValue * madePlays.Count()) + (missedValue * missedPlays.Count());

                                // Need to adjust so that the expected value for fielding is 0
                                float valueOvershoot = totalValue / totalPlays;
                                madeValue -= valueOvershoot;
                                missedValue -= valueOvershoot;

                                if (totalPlays == 0 || madePlays.Count() == 0 || missedPlays.Count() == 0)
                                {
                                    fieldingDict.Add(fg.Key, new FieldingResults
                                    {
                                        NumOccurences = 0,
                                        ProbMakeWhenMade = [0, 0, 0, 0, 0, 0, 0, 0, 0],
                                        ProbMiss = 1,
                                        RunsMake = 0,
                                        RunsMiss = 0
                                    });
                                    continue;
                                }

                                // Normalize the probMake matrix so it adds
                                probMake = probMake.Select(f => f / (1.0f - probMissed)).ToArray();

                                fieldingDict.Add(fg.Key, new FieldingResults
                                {
                                    ProbMakeWhenMade = probMake,
                                    ProbMiss = probMissed,
                                    RunsMake = madeValue,
                                    RunsMiss = missedValue,
                                    NumOccurences = totalPlays,
                                });
                            }

                            // Calcualate baserunning outcome dicts
                            BaserunningDict BsrAdv1st3rdSingleDict = new();
                            BaserunningDict BsrAdv2ndHomeSingleDict = new();
                            BaserunningDict BsrAdv1stHomeDoubleDict = new();
                            BaserunningDict BsrAvoidForce2ndDict = new();
                            BaserunningDict BsrAdv1st2ndFlyoutDict = new();
                            BaserunningDict BsrAdv2nd3rdFlyoutDict = new();
                            BaserunningDict BsrAdv3rdHomeFlyoutDict = new();
                            BaserunningDict BsrAdv2nd3rdGroundoutDict = new();
                            var leaguePBPBaserunningGroupings = leaguePBP.GroupBy(f => PBP_TypeConversions.GetBaserunningScenario(f));
                            foreach (var fg in leaguePBPBaserunningGroupings)
                            {
                                if (fg.Key == null)
                                    continue;

                                BsrAdv1st3rdSingleDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_1stTo3rdOnSingle_Opportunities(fg), 1, 3));
                                BsrAdv2ndHomeSingleDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_2ndToHomeOnSingle_Opportunities(fg), 2, 4));
                                BsrAdv1stHomeDoubleDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_1stToHomeOnDouble_Opportunities(fg), 1, 4));
                                BsrAvoidForce2ndDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.Avoid_1stToSecondForceout_Opportunities(fg), 1, 2));
                                BsrAdv1st2ndFlyoutDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_1stTo2ndOnFlyout_Opportunities(fg), 1, 2));
                                BsrAdv2nd3rdFlyoutDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_2ndTo3rdOnFlyout_Opportunities(fg), 2, 3));
                                BsrAdv3rdHomeFlyoutDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_3rdToHomeOnFlyout_Opportunities(fg), 3, 4));
                                BsrAdv2nd3rdGroundoutDict.Add(fg.Key, GetBaserunningResult(runExpectancyDict, PBP_Utilities.GetAdvance_2ndTo3rdOnGroundout_Opportunities(fg), 2, 3));
                            }

                            // Calculate Double Play Turned Dict
                            DoublePlayDict doublePlayDict = new();
                            var leaguePBP_DPGroupings = leaguePBP.GroupBy(f => PBP_TypeConversions.GetDoublePlayScenario(f));
                            foreach (var fg in leaguePBP_DPGroupings)
                            {
                                if (fg.Key == null)
                                    continue;

                                doublePlayDict.Add(fg.Key, GetDoublePlayResult(runExpectancyDict, fg, fg.Key));
                            }

                            db.LeagueRunMatrix.Add(new LeagueRunMatrix
                            {
                                LeagueId = league,
                                Year = year,
                                RunExpDict = LeagueRunMatrixDicts.Serialize(runExpectancyDict),
                                FieldOutcomeDict = LeagueRunMatrixDicts.Serialize(fieldingDict),
                                BsrAdv1st3rdSingleDict = LeagueRunMatrixDicts.Serialize(BsrAdv1st3rdSingleDict),
                                BsrAdv2ndHomeSingleDict = LeagueRunMatrixDicts.Serialize(BsrAdv2ndHomeSingleDict),
                                BsrAdv1stHomeDoubleDict = LeagueRunMatrixDicts.Serialize(BsrAdv1stHomeDoubleDict),
                                BsrAvoidForce2ndDict = LeagueRunMatrixDicts.Serialize(BsrAvoidForce2ndDict),
                                BsrAdv1st2ndFlyoutDict = LeagueRunMatrixDicts.Serialize(BsrAdv1st2ndFlyoutDict),
                                BsrAdv2nd3rdFlyoutDict = LeagueRunMatrixDicts.Serialize(BsrAdv2nd3rdFlyoutDict),
                                BsrAdv3rdHomeFlyoutDict = LeagueRunMatrixDicts.Serialize(BsrAdv3rdHomeFlyoutDict),
                                BsrAdv2nd3rdGroundoutDict = LeagueRunMatrixDicts.Serialize(BsrAdv2nd3rdGroundoutDict),
                                DoublePlayDict = LeagueRunMatrixDicts.Serialize(doublePlayDict),
                            });

                            // Need to save so MLB can be retrieved by AL/NL
                            if (league == 1)
                                db.SaveChanges();
                        }
                        else 
                        {
                            // For AL and NL, just copy total MLB data
                            LeagueRunMatrix mlbLeagueRunMatrix = db.LeagueRunMatrix.Where(f => f.Year == year && f.LeagueId == 1).Single();
                            db.LeagueRunMatrix.Add(new LeagueRunMatrix
                            {
                                LeagueId = league,
                                Year = year,
                                RunExpDict = mlbLeagueRunMatrix.RunExpDict,
                                FieldOutcomeDict = mlbLeagueRunMatrix.FieldOutcomeDict,
                                BsrAdv1st3rdSingleDict = mlbLeagueRunMatrix.BsrAdv1st3rdSingleDict,
                                BsrAdv2ndHomeSingleDict = mlbLeagueRunMatrix.BsrAdv2ndHomeSingleDict,
                                BsrAdv1stHomeDoubleDict = mlbLeagueRunMatrix.BsrAdv1stHomeDoubleDict,
                                BsrAvoidForce2ndDict = mlbLeagueRunMatrix.BsrAvoidForce2ndDict,
                                BsrAdv1st2ndFlyoutDict = mlbLeagueRunMatrix.BsrAdv1st2ndFlyoutDict,
                                BsrAdv2nd3rdFlyoutDict = mlbLeagueRunMatrix.BsrAdv2nd3rdFlyoutDict,
                                BsrAdv3rdHomeFlyoutDict = mlbLeagueRunMatrix.BsrAdv3rdHomeFlyoutDict,
                                BsrAdv2nd3rdGroundoutDict = mlbLeagueRunMatrix.BsrAdv2nd3rdGroundoutDict,
                                DoublePlayDict = mlbLeagueRunMatrix.DoublePlayDict,
                            });
                        }


                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
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
