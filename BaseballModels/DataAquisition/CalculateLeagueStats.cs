using Db;
using ShellProgressBar;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DataAquisition
{
    internal class CalculateLeagueStats
    {
        private class BaseState
        {
            public required int outs;
            public required int occupancy;

            public override bool Equals(object? obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                BaseState other = (BaseState)obj;
                return outs == other.outs && occupancy == other.occupancy;
            }

            public override int GetHashCode()
            {
                return System.HashCode.Combine(outs, occupancy);
            }
        }
    
        private class HitterEvent
        {
            public required BaseState inputState;
            public required BaseState outputState;
            public required int outcome; // 0 out, 1-4 1B/4hr, 5 BB, 6 HBP
            public required int runsScoredAfter; // Includes this event also
            public required int runsScoredThisEvent;
            public required float changedRunExpectancy;
        }

        private const int NUM_OUTCOMES = 7;

        private class ApiEvent
        {
            public required BaseState inputState;
            public required BaseState outputState;
            public required int outcome;
            public required int inning;
            public required bool topOfInning;
            public required int scoredThisEvent;
            public required int score;
        }

        private static async Task<(IEnumerable<HitterEvent>, int)> GetHitterEventsAsync(int gameId, HttpClient httpClient)
        {
            // Get data
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/game/{gameId}/winProbability");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting Game log for id={gameId}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            var results = json.RootElement.EnumerateArray();

            // For events, need to track which bases had runners since API won't say anything about them if they dont' move a base
            List<bool> runnersOnBase = [false, false, false];

            // Iterate through events to extract useful data
            List<ApiEvent> apiEvents = new();
            apiEvents.Capacity = results.Count();
            foreach (var r in results)
            {
                // Result object
                var result = r.GetProperty("result");
                int aScore = result.GetProperty("awayScore").GetInt32();
                int hScore = result.GetProperty("homeScore").GetInt32();
                string eventType = result.GetProperty("type").GetString() ?? throw new Exception("Unable to get 'type' from result JSON");
                if (eventType != "atBat")
                    throw new Exception($"EventType wasn't 'atBat' in GetHitterEventsAsync: type={eventType}");
                string eventDesc = result.GetProperty("event").GetString() ?? throw new Exception("Unable to get 'event' from result JSON");

                // About object
                var about = r.GetProperty("about");
                int inning = about.GetProperty("inning").GetInt32();
                bool isTopInning = about.GetProperty("isTopInning").GetBoolean();

                // Count object
                var count = r.GetProperty("count");
                int outs = count.GetProperty("outs").GetInt32();

                // Runners object
                var runners = r.GetProperty("runners");
                BaseState inputState = new() { outs = outs, occupancy = 0 };
                BaseState outputState = new() { outs = outs, occupancy = 0 };

                int scoredThisEvent = 0;
                List<bool> runnersUpdated = [false, false, false];
                bool hitEventFound = false;
                foreach (var runner in runners.EnumerateArray())
                {
                    // Movement object
                    var movement = runner.GetProperty("movement");
                    string? originBase = movement.GetProperty("originBase").GetString();
                    string? startBase = movement.GetProperty("start").GetString();
                    string? endBase = movement.GetProperty("end").GetString();

                    // Details object
                    var details = runner.GetProperty("details");

                    string runnerEvent = details.GetProperty("event").GetString() ?? throw new Exception("Unable to get 'event' from runnerEvent JSON");
                    if (!hitEventFound && runnerEvent != eventDesc) // Runner did something before hit event, need to update where runners currently are
                    {
                        // !Assumes that bases are updated in descending order (3rd, then 2nd, than 1st).  

                        // Remove runner from current base
                        if (startBase == "1B")
                            runnersOnBase[0] = false;
                        else if (startBase == "2B")
                            runnersOnBase[1] = false;
                        else if (startBase == "3B")
                            runnersOnBase[2] = false;

                        // Add to destination base
                        if (endBase == "1B")
                            runnersOnBase[0] = true;
                        else if (endBase == "2B")
                            runnersOnBase[1] = true;
                        else if (endBase == "3B")
                            runnersOnBase[2] = true;

                        continue;
                    } else { // Baserunning outs may be recorded as a different event, so need to include those in the end state
                        hitEventFound = true;
                    }

                    // For some dumb reason they move runners 1 base at a time in multiple events, so need to check if this was the base they started the event at
                    if (originBase == startBase)
                    {
                        if (startBase == "1B")
                        {
                            inputState.occupancy += 1;
                            runnersUpdated[0] = true;
                        }
                        else if (startBase == "2B")
                        {
                            inputState.occupancy += 2;
                            runnersUpdated[1] = true;
                        }

                        else if (startBase == "3B")
                        {
                            inputState.occupancy += 4;
                            runnersUpdated[2] = true;
                        }
                    }
                    else
                    { // Player was added to base but then removed, so need to update occupancy
                        if (startBase == "1B")
                            outputState.occupancy -= 1;
                        else if (startBase == "2B")
                            outputState.occupancy -= 2;
                        else if (startBase == "3B")
                            outputState.occupancy -= 4;
                    }


                    // Check where runners ended the event
                    if (endBase == "1B")
                        outputState.occupancy += 1;
                    else if (endBase == "2B")
                        outputState.occupancy += 2;
                    else if (endBase == "3B")
                        outputState.occupancy += 4;
                    else if (endBase == "score")
                        scoredThisEvent++;

                    // Check how many runners got out
                    try { // for Strikeout that isn't caught by catcher, isOut is null
                        bool isOut = movement.GetProperty("isOut").GetBoolean();
                        if (isOut)
                            inputState.outs--;
                    } catch (Exception) { /*Do Nothing */ }
                }

                // Place runners who were on base and not updated
                for (int i = 0; i < 3; i++)
                    if (runnersOnBase[i] & !runnersUpdated[i])
                    {
                        int baseValue = 1 << i;
                        outputState.occupancy += baseValue;
                        inputState.occupancy += baseValue;
                    }


                if (outputState.outs > 3) // Bugged PBP
                    continue;
                    
                // Force base occupancy to empty if 3 outs so don't have to deal with multiple codes (all have 0 run expectancy since inning is over)
                if (outputState.outs >= 3)
                {
                    outputState.occupancy = 0;
                }

                if (inputState.outs == 3)
                {
                    continue; // Final out made on caught stealing
                }

                if (outputState.occupancy > 7 || outputState.occupancy < 0) // Some PBP bug lead to multiple runners on the same base
                {
                    continue;
                }

                runnersOnBase = [(outputState.occupancy & 1) != 0, (outputState.occupancy & 2) != 0, (outputState.occupancy & 4) != 0];


                // Iterate through possible event codes
                int outcomeCode = 0;
                if (eventDesc == "Sac Bunt" || eventDesc == "Intent Walk") // Intentional out/walk, improper for run expectancy analysis to use (run-expectancy suboptimal plays, but often win-expectancy optimal)
                    continue;
                else if (eventDesc == "Single")
                    outcomeCode = 1;
                else if (eventDesc == "Double")
                    outcomeCode = 2;
                else if (eventDesc == "Triple")
                    outcomeCode = 3;
                else if (eventDesc == "Home Run")
                    outcomeCode = 4;
                else if (eventDesc == "Walk")
                    outcomeCode = 5;
                else if (eventDesc == "Hit By Pitch")
                    outcomeCode = 6;

                // Create APIEvents
                apiEvents.Add(new ApiEvent
                {
                    inputState = inputState,
                    outputState = outputState,
                    outcome = outcomeCode,
                    inning = inning,
                    topOfInning = isTopInning,
                    score = isTopInning ? aScore : hScore,
                    scoredThisEvent = scoredThisEvent
                });
            }

            // Find last inning that should be counted (either inning before last of game or 8th inning)
            // Done to avoid having to deal with early game exits messing with data
            int lastInning = Math.Min(apiEvents.Last().inning - 1, 8);

            // Scores at end of top and bottom of inning
            List<int> homeInningScores = [];
            List<int> roadInningScores = [];
            for (int i = 1; i <= lastInning; i++)
            {
                var thisInningApiEvents = apiEvents.Where(f => f.inning == i);
                roadInningScores.Add(thisInningApiEvents.Where(f => f.topOfInning).Select(f => f.score).Max());
                homeInningScores.Add(thisInningApiEvents.Where(f => !f.topOfInning).Select(f => f.score).Max());
            }

            // Go through api events and find the state change between them
            List<HitterEvent> hitterEvents = new();
            hitterEvents.Capacity = apiEvents.Count;
            foreach (ApiEvent apiEvent in apiEvents)
            {
                // Make sure to not check after last inning
                if (apiEvent.inning > lastInning)
                    break;

                // Track changes between event
                hitterEvents.Add(new HitterEvent
                {
                    inputState = apiEvent.inputState,
                    outputState = apiEvent.outputState,
                    outcome = apiEvent.outcome,
                    runsScoredThisEvent = apiEvent.scoredThisEvent,
                    runsScoredAfter = apiEvent.topOfInning ?
                        roadInningScores[apiEvent.inning - 1] - apiEvent.score:
                        homeInningScores[apiEvent.inning - 1] - apiEvent.score,
                    changedRunExpectancy = 0 // Will be filled in later
                });
            }

            return (hitterEvents, lastInning);
        }

        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts;

        private static async Task<(List<HitterEvent>, int)> GetHitterEventsAsyncThreadFunction(IEnumerable<int> ids, int threadIdx, ChildProgressBar progressBar, int progressSum, int year, int league)
        {
            HttpClient httpClient = new();
            List<HitterEvent> data = new();
            int innings = 0;
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int id in ids)
            {
                try {
                    var logs = await GetHitterEventsAsync(id, httpClient);
                    data.AddRange(logs.Item1);
                    innings += logs.Item2;
                } catch (Exception e)
                {
                    File.WriteAllText(Constants.DATA_AQ_DIRECTORY + "/Logs/" + $"CalculateLeagueStats_{year}_{league}_{id}.txt", e.Message);
                }
                

                thread_counts[threadIdx]++; // Allows for tracking progress

                if (threadIdx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return (data, innings);
        }

        public static async Task<bool> Main(int year, bool forceDelete)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                
                // Check if season data is incomplete
                if (forceDelete)
                {
                    db.LeagueStats.RemoveRange(db.LeagueStats);
                    db.SaveChanges();
                    db.ChangeTracker.Clear();
                }

                HttpClient httpClient = new();
                //await GetHitterEventsAsync(33333, httpClient);

                var leagues = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year)
                    .Select(f => f.LeagueId).Distinct();

                using (ProgressBar progressBar = new(leagues.Count(), $"Generating League Stats for {year}"))
                {
                    foreach (int league in leagues)
                    {
                        // Check if league already has data for year
                        if (db.LeagueStats.Any(f => f.Year == year && f.LeagueId == league))
                        {
                            LeagueStats ls = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == league).Single();
                            ls.LeaguePA = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league)
                                .Select(f => f.PA).Sum();
                            ls.LeagueGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league)
                                .Select(f => f.GameId).Distinct().Count();

                            var lleagueStats = db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.LeagueId == league);
                            int lleagueHRs = lleagueStats.Select(f => f.HR).Sum();
                            int lleagueBBs = lleagueStats.Select(f => f.BB + f.HBP).Sum();
                            int lleagueKs = lleagueStats.Select(f => f.K).Sum();
                            int lleagueRuns = lleagueStats.Select(f => f.R).Sum();
                            int lleagueERs = lleagueStats.Select(f => f.ER).Sum();
                            int lleagueOuts = lleagueStats.Select(f => f.Outs).Sum();

                            float lleagueRA = (float)lleagueRuns / lleagueOuts * 27;
                            float lleagueERA = (float)lleagueERs / lleagueOuts * 27;
                            float lleagueFIP = Utilities.CalculateFip(0, lleagueHRs, lleagueKs, lleagueBBs, lleagueOuts);

                            ls.CFIP = lleagueERA - lleagueFIP;
                            ls.FIPR9Adjustment = lleagueRA - lleagueERA;
                            ls.LeagueERA = lleagueERA;
                            db.SaveChanges();

                            progressBar.Tick();
                            continue;
                        }

                        // Get games for league
                        IEnumerable<int> gameIds = db.Player_Hitter_GameLog.Where(f => f.LeagueId == league && f.Year == year)
                            .Select(f => f.GameId).Distinct();

                        if (!gameIds.Any()) // Many leagues in DB aren't valid leagues, or were valid at one point but not now
                        {
                            progressBar.Tick();
                            continue;
                        }
                        
                        gameIds = gameIds.Where(f => f != 37234); // Game is either bugged or not a legal game

                        // Get Events
                        List<HitterEvent> hitterEvents = new(300000); // About 200000 for an entire MLB season, gives some breathing room
                        int totalInnings = 0;

                        // Split ids into groups for tasks
                        int j = 0;
                        IEnumerable<IEnumerable<int>> id_partitions = from item in gameIds
                                                                      group item by j++ % NUM_THREADS into part
                                                                      select part.AsEnumerable();

                        List<Task<(List<HitterEvent>, int)>> tasks = new(NUM_THREADS);
                        using (ChildProgressBar gameChildBar = progressBar.Spawn(gameIds.Count(), $"Getting Game PBP for {db.Leagues.Where(f => f.Id == league).Single().Abbr}"))
                        {
                            progress_bar_thread = 0;
                            thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];
                            for (int i = 0; i < NUM_THREADS; i++)
                            {
                                int idx = i;
                                Task<(List<HitterEvent>, int)> task = GetHitterEventsAsyncThreadFunction(id_partitions.ElementAt(i), idx, gameChildBar, gameIds.Count(), year, league);
                                tasks.Add(task);
                            }

                            foreach (var task in tasks)
                            {
                                (List<HitterEvent> events, int innings) = await task;
                                hitterEvents.AddRange(events);
                                totalInnings += innings;
                                progress_bar_thread++; // Increment which task should update progress bar
                            }
                        }

                        // Find run expectancy and probability for each out/base state
                        Dictionary<BaseState, float> runExpectancyDict = new();
                        IEnumerable<BaseState> baseStates = hitterEvents.OrderBy(f => f.inputState.outs).ThenBy(f => f.inputState.occupancy).Select(f => f.inputState).Distinct();
                        foreach (BaseState bs in baseStates)
                        {
                            var selectedEvents = hitterEvents.Where(f => f.inputState.Equals(bs));
                            int totalRunsScored = selectedEvents.Select(f => f.runsScoredAfter).Sum() + selectedEvents.Select(f => f.runsScoredThisEvent).Sum();
                            float runsExpected = (float)totalRunsScored / selectedEvents.Count();
                            runExpectancyDict.Add(bs, runsExpected);
                        }

                        runExpectancyDict.Add(new BaseState { occupancy = 0, outs = 3 }, 0);

                        foreach (var he in hitterEvents)
                        {
                            float incomeExpectancy = runExpectancyDict[he.inputState];
                            float outcomeExpectancy = runExpectancyDict[he.outputState];
                            float runsAdded = outcomeExpectancy - incomeExpectancy + he.runsScoredThisEvent;
                            he.changedRunExpectancy = runsAdded;
                        }

                        // Get Runs Added by each event type
                        List<float> outcomeRunsAdded = [0, 0, 0, 0, 0, 0, 0];
                        List<int> eventCounts = [0, 0, 0, 0, 0, 0, 0];
                        for (int i = 0; i < NUM_OUTCOMES; i++)
                        {
                            var selectedEvents = hitterEvents.Where(f => f.outcome == i);
                            float totalRunsAdded = selectedEvents.Select(f => f.changedRunExpectancy).Sum();

                            eventCounts[i] = selectedEvents.Count();
                            float runsPerEvent = totalRunsAdded / eventCounts[i];
                            outcomeRunsAdded[i] = runsPerEvent;
                        }

                        // Adjust up so that outs have 0 value
                        for (int i = 1; i < NUM_OUTCOMES; i++)
                        {
                            outcomeRunsAdded[i] -= outcomeRunsAdded[0];
                        }

                        // Multiply by event counts to get average for all events
                        float wobaAccumulator = 0;
                        for (int i = 1; i < NUM_OUTCOMES; i++)
                        {
                            wobaAccumulator += outcomeRunsAdded[i] * eventCounts[i] / hitterEvents.Count;
                        }

                        // Adjust to league OBP
                        float obp = 1.0f - ((float)eventCounts[0] / hitterEvents.Count);
                        float wobaScale = obp / wobaAccumulator;

                        // Get runs per win
                        int totalRunsScoredInLeague = hitterEvents.Select(f => f.runsScoredThisEvent).Sum();
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

                        db.LeagueStats.Add(new LeagueStats
                        {
                            LeagueId = league,
                            Year = year,
                            AvgWOBA = obp,
                            WOBAScale = wobaScale,
                            W1B = outcomeRunsAdded[1] * wobaScale,
                            W2B = outcomeRunsAdded[2] * wobaScale,
                            W3B = outcomeRunsAdded[3] * wobaScale,
                            WHR = outcomeRunsAdded[4] * wobaScale,
                            WBB = outcomeRunsAdded[5] * wobaScale,
                            WHBP = outcomeRunsAdded[6] * wobaScale,
                            RunCS = -0.4f,
                            RunSB = 0.2f,
                            RPerPA = (float)totalRunsScoredInLeague / hitterEvents.Count,
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
