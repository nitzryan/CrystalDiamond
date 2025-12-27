using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System.Text.Json;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class GetPlayByPlay
    {
        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];
        private const int EVENTS_PER_GAME = 100;

        private const string PBP_LOG_DIRECTORY = Constants.DATA_AQ_DIRECTORY + "/Logs/GPBP/";

        private static async Task<List<GamePlayByPlay>> GetPBPThreadFunction(IEnumerable<int> gameIds, int thread_idx, ProgressBar progressBar, int totalCount)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<GamePlayByPlay> pbp = new(gameIds.Count() * EVENTS_PER_GAME); // 100 PAs/Game will be a slight overestimate
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int gameId in gameIds)
            {
                try {
                    pbp.AddRange(await GetPlayByPlayAsync(gameId, httpClient, db));
                } catch (Exception e)
                {
                    File.WriteAllText(PBP_LOG_DIRECTORY + $"GetPlayByPlay_{gameId}.txt", e.Message + "\n\n" + e.InnerException + "\n\n" + e.StackTrace);
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / totalCount);
                }
            }

            return pbp;
        }

        private static async Task<List<GamePlayByPlay>> GetPlayByPlayAsync(int gameId, HttpClient httpClient, SqliteDbContext db)
        {
            Player_Hitter_GameLog game = db.Player_Hitter_GameLog.Where(f => f.GameId == gameId).First();

            List<GamePlayByPlay> pbp = new(2 * EVENTS_PER_GAME);

            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/game/{gameId}/playByPlay");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting Game log for id={gameId}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            var allPlaysArray = json.RootElement.GetProperty("allPlays").EnumerateArray();
            var playsByInningArray = json.RootElement.GetProperty("playsByInning").EnumerateArray();

            foreach (var inning in playsByInningArray)
            {
                var top = inning.GetProperty("top").EnumerateArray().Select(f => f.GetInt32());
                var bot = inning.GetProperty("bottom").EnumerateArray().Select(f => f.GetInt32());

                var topInningEvents = GetInningEvents(allPlaysArray, top, game);
                var botInningEvents = GetInningEvents(allPlaysArray, bot, game);

                // Apply the number of runs scored the inning to each event (only during or after the specific event)
                for (int i = 0; i < topInningEvents.Count(); i++)
                {
                    topInningEvents[i].RunsScoredInningAfterEvent = topInningEvents.Skip(i).Sum(f => f.RunsScored);
                }
                for (int i = 0; i < botInningEvents.Count(); i++)
                {
                    botInningEvents[i].RunsScoredInningAfterEvent = botInningEvents.Skip(i).Sum(f => f.RunsScored);
                }

                // Some old MiLB games have broken PBP data.  Check for nonsensical data here
                if (!topInningEvents.Any(f => f.StartOuts >= 3 || f.EndOuts > 3))
                    pbp.AddRange(topInningEvents);

                if (!botInningEvents.Any(f => f.StartOuts >= 3 || f.EndOuts > 3))
                    pbp.AddRange(botInningEvents);
            }

            return pbp;
        }

        private static int JsonElementToBaseInt(JsonElement b)
        {
            if (b.ValueKind == JsonValueKind.Null)
                return -1;

            string b_string = b.ToString();
            if (b_string == "1B")
                return 0;
            if (b_string == "2B")
                return 1;
            if (b_string == "3B")
                return 2;
            if (b_string == "score" || b_string == "4B")
                return 3;

            if (b_string == "0B") // Not sure, definately not coded correctly
                return -1;

            throw new Exception("JsonElementToBaseInt did not find a proper base");
        }

        private static Dictionary<string, PBP_Events> ResultDict = new() {
            {"Single", PBP_Events.SINGLE},
            {"Double", PBP_Events.DOUBLE},
            {"Triple", PBP_Events.TRIPLE},
            {"Home Run", PBP_Events.HR},
            {"Walk", PBP_Events.BB},
            {"Hit By Pitch", PBP_Events.HBP},
            {"Strikeout", PBP_Events.K},
            {"Grounded Into DP", PBP_Events.GIDP},
            {"Pop Out", PBP_Events.POPOUT},
            {"Groundout", PBP_Events.GNDOUT},
            {"Flyout", PBP_Events.FLYOUT},
            {"Field Out", PBP_Events.FLYOUT}, // Not sure on this
            {"Passed Ball", PBP_Events.PB},
            {"Balk", PBP_Events.BALK},
            {"Lineout", PBP_Events.LINEOUT},
            {"Defensive Indiff", PBP_Events.DEF_INDIF},
            {"Wild Pitch", PBP_Events.WILD_PITCH},
            {"Forceout", PBP_Events.FORCEOUT},
            {"Sac Fly", PBP_Events.SAC_FLY},
            {"Runner Out", PBP_Events.RUNNER_OUT},
            {"Bunt Pop Out", PBP_Events.BUNT_POPOUT},
            {"Bunt Lineout", PBP_Events.BUNT_POPOUT},
            {"Intent Walk", PBP_Events.IBB},
            {"Sac Bunt", PBP_Events.SAC_BUNT},
            {"Fielders Choice", PBP_Events.FIELDERS_CHOICE},
            {"Fielders Choice Out", PBP_Events.FIELDERS_CHOICE_OUT},
            {"Bunt Groundout", PBP_Events.BUNT_GROUNDOUT},
            {"Double Play", PBP_Events.FB_DOUBLE_PLAY},
            {"Strikeout Double Play", PBP_Events.K | PBP_Events.CS},
            {"Catcher Interference", PBP_Events.CATCH_INT},
            {"Sac Fly Double Play", PBP_Events.SAC_FLY | PBP_Events.RUNNER_OUT},
            {"Other Advance", PBP_Events.NONE}, // Not sure on this, believe it always comes with other event which takes precedence
            {"Sac Bunt Double Play", PBP_Events.SAC_BUNT | PBP_Events.RUNNER_OUT},
            {"Batter Out", PBP_Events.OTHER},
            {"Triple Play", PBP_Events.TRIPLE_PLAY},
            {"Strikeout Triple Play", PBP_Events.K | PBP_Events.CS}, // Gameid=34270, I want to see this play
            {"Runner Double Play", PBP_Events.RUNNER_OUT},
            {"Disengagement Violation", PBP_Events.DISENGAGEMENT_VIOLATION},
            {"Official Scorer Ruling Pending",PBP_Events.OFFICIAL_SCORER_PENDING},
        };

        private static PBP_Events EventStringToInt(string eventString)
        {
            if (ResultDict.TryGetValue(eventString, out PBP_Events result))
                return result;

            if (eventString.Contains("Error"))
                return PBP_Events.FIELD_ERROR;
            if (eventString.Contains("Stolen"))
                return PBP_Events.SB;
            if (eventString.Contains("Caught"))
                return PBP_Events.CS;
            if (eventString.Contains("Pickoff"))
                return PBP_Events.PICKOFF;

            throw new Exception($"Unexpected event found for EventStringToInt: {eventString}");
        }

        private static PBP_HitTrajectory? ParseHitTrajectory(string trajectoryString)
        {
            if (trajectoryString == "ground_ball")
                return PBP_HitTrajectory.Groundball;
            if (trajectoryString == "fly_ball")
                return PBP_HitTrajectory.Flyball;
            if (trajectoryString == "line_drive")
                return PBP_HitTrajectory.Linedrive;
            if (trajectoryString == "popup")
                return PBP_HitTrajectory.Popup;
            if (trajectoryString == "bunt_grounder")
                return PBP_HitTrajectory.BuntGrounder;
            if (trajectoryString == "bunt_popup")
                return PBP_HitTrajectory.BuntPopup;
            if (trajectoryString == "bunt_line_drive")
                return PBP_HitTrajectory.BuntLinedrive;

            if (trajectoryString == "")
                return null;
            throw new Exception($"Unexpected string found for ParseHitTrajectory: {trajectoryString}");
        }

        private static PBP_HitHardness? ParseHitHardness(string hardnessString)
        {
            if (hardnessString == "soft")
                return PBP_HitHardness.Soft;
            if (hardnessString == "medium")
                return PBP_HitHardness.Medium;
            if (hardnessString == "hard")
                return PBP_HitHardness.Hard;
            if (hardnessString == "")
                return null;

            throw new Exception($"Unexpected string found for ParseHitHardness: {hardnessString}");
        }

        private static BaseOccupancy OccupancyArrayToInt(List<int?> occupancy)
        {
            return (occupancy[0] != null ? BaseOccupancy.B1 : BaseOccupancy.Empty) |
                    (occupancy[1] != null ? BaseOccupancy.B2 : BaseOccupancy.Empty) |
                    (occupancy[2] != null ? BaseOccupancy.B3 : BaseOccupancy.Empty);
        }

        private static GamePlayByPlay GetEmptyEvent(Player_Hitter_GameLog game, int inning, bool topOfInning, int outs, List<int?> currentOccupancy)
        {
            return new GamePlayByPlay
            {
                GameId = game.GameId,
                LeagueId = game.LeagueId,
                Year = game.Year,
                Month = game.Month,
                HitterId = -1,
                PitcherId = -1,
                FielderId = null,
                Run1stId = currentOccupancy[0],
                Run2ndId = currentOccupancy[1],
                Run3rdId = currentOccupancy[2],
                StartOuts = outs,
                Inning = inning,
                IsTop = topOfInning ? 1 : 0,
                StartBaseOccupancy = OccupancyArrayToInt(currentOccupancy),
                EndOuts = outs,
                EndBaseOccupancy = BaseOccupancy.Invalid,
                RunsScored = 0,
                RunsScoredInningAfterEvent = 0,
                Result = PBP_Events.NONE,
                HitZone = null,
                HitHardness = null,
                HitTrajectory = null,
                HitCoordX = null,
                HitCoordY = null,
                Run1stOutcome = null,
                Run2ndOutcome = null,
                Run3rdOutcome = null,
            };
        }

        private static void UpdatePBPForRunner(GamePlayByPlay gpbp, JsonElement runnerElement, List<int?> currentOccupany, List<int?> nextOccupancy)
        {
            var movement = runnerElement.GetProperty("movement");
            int originBase = JsonElementToBaseInt(movement.GetProperty("originBase"));
            int startBase = JsonElementToBaseInt(movement.GetProperty("start"));
            int endBase = JsonElementToBaseInt(movement.GetProperty("end"));

            var isOutElement = movement.GetProperty("isOut");
            bool isOut = isOutElement.ValueKind == JsonValueKind.True;

            // Update Runner
            var runDetails = runnerElement.GetProperty("details");
            int runnerId = runDetails.GetProperty("runner").GetProperty("id").GetInt32();
            if (endBase != -1 && endBase != 3)
                nextOccupancy[endBase] = runnerId;

            // Remove player from base they were on
            if (originBase >= 0 && originBase < 3 && nextOccupancy[originBase] == runnerId)
                nextOccupancy[originBase] = null;
            if (startBase >= 0 && startBase < 3 && nextOccupancy[startBase] == runnerId)
                nextOccupancy[startBase] = null;

            if (isOut)
                gpbp.EndOuts++;

            if (endBase == 3)
            {
                gpbp.RunsScored++;
                if (currentOccupany[0] == runnerId)
                    gpbp.Run1stOutcome = 4;
                else if (currentOccupany[1] == runnerId)
                    gpbp.Run2ndOutcome = 4;
                else if (currentOccupany[2] == runnerId)
                    gpbp.Run3rdOutcome = 4;
            }

                // Get first fielder, used for outfielder arm analysis
                if (gpbp.FielderId == null)
                {
                    if (runnerElement.TryGetProperty("credits", out var credits)) // If no credits in first result, not a situation we want the fielder for anyways
                    {
                        var creditsArray = credits.EnumerateArray();
                        if (creditsArray.Any())
                            if (creditsArray.ElementAt(0).TryGetProperty("player", out var creditsPlayer))
                                if (creditsPlayer.TryGetProperty("id", out var cpid))
                                    gpbp.FielderId = cpid.GetInt32();
                    }
                }
        }

        private static void UpdateHitData(GamePlayByPlay gpbp, JsonElement.ArrayEnumerator playEvents)
        {
            foreach (var pe in playEvents)
            {
                if (pe.TryGetProperty("hitData", out JsonElement hitData))
                {
                    string trajectoryString = "";
                    string hardnessString = "";
                    if (hitData.TryGetProperty("trajectory", out var trajElement))
                        trajectoryString = trajectoryString.ToString();
                    if (hitData.TryGetProperty("hardness", out var hardElement))
                        hardnessString = hardElement.ToString();

                    if (hitData.TryGetProperty("location", out var lc))
                    {
                        gpbp.HitZone = Convert.ToInt32(lc.ToString());
                    }
                    
                    var coordinates = hitData.GetProperty("coordinates");
                    if (coordinates.TryGetProperty("coordX", out var coordX) && coordinates.TryGetProperty("coordY", out var coordY))
                    {
                        gpbp.HitCoordX = coordX.GetSingle();
                        gpbp.HitCoordY = coordY.GetSingle();
                    }
                    
                    gpbp.HitTrajectory = ParseHitTrajectory(trajectoryString);
                    gpbp.HitHardness = ParseHitHardness(hardnessString);

                    if (hitData.TryGetProperty("launchSpeed", out var launchSpeedElement))
                        gpbp.LaunchSpeed = launchSpeedElement.GetSingle();
                    if (hitData.TryGetProperty("launchAngle", out var launchAngleElement))
                        gpbp.LaunchAngle = launchAngleElement.GetSingle();
                    if (hitData.TryGetProperty("totalDistance", out var totalDistElement))
                        gpbp.LaunchDistance = totalDistElement.GetSingle();

                    return;
                }
            }
        }

        private static void SetEndRunnerOutcomes(GamePlayByPlay gpbp, List<int?> currentOccupany, List<int?> nextOccupancy)
        {
            if (gpbp.EndOuts == 3)
                gpbp.EndBaseOccupancy = 0;
        
            if (gpbp.Run1stOutcome == null && currentOccupany[0] != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (nextOccupancy[i] == currentOccupany[0])
                    {
                        gpbp.Run1stOutcome = i + 1;
                        break;
                    }
                }
                if (gpbp.Run1stOutcome == null)
                    gpbp.Run1stOutcome = 0;
            }

            if (gpbp.Run2ndOutcome == null && currentOccupany[1] != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (nextOccupancy[i] == currentOccupany[1])
                    {
                        gpbp.Run2ndOutcome = i + 1;
                        break;
                    }
                }
                if (gpbp.Run2ndOutcome == null)
                    gpbp.Run2ndOutcome = 0;
            }

            if (gpbp.Run3rdOutcome == null && currentOccupany[2] != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (nextOccupancy[i] == currentOccupany[2])
                    {
                        gpbp.Run3rdOutcome = i + 1;
                        break;
                    }
                }
                if (gpbp.Run3rdOutcome == null)
                    gpbp.Run3rdOutcome = 0;
            }
        }

        private static List<GamePlayByPlay> GetInningEvents(JsonElement.ArrayEnumerator allPlaysArray, IEnumerable<int> plays, Player_Hitter_GameLog game)
        {
            List<GamePlayByPlay> pbp = new(plays.Count() + 1);

            // For events, need to track which bases had runners since API won't say anything about them if they dont' move a base
            List<int?> currentOccupancy = [null, null, null];
            List<int?> nextOccupancy = [null, null, null];
            int outs = 0;
            foreach (int playId in plays)
            {
                var play = allPlaysArray.ElementAt(playId);

                var result = play.GetProperty("result");

                // Some games just cut off, so if there is no events don't log the half-inning since it will throw off calculations
                if (!result.TryGetProperty("event", out var resultEventElement))
                {
                    return [];
                }

                var resultEvent = resultEventElement.ToString();
                var about = play.GetProperty("about");
                int inning = about.GetProperty("inning").GetInt32();
                bool topOfInning = about.GetProperty("isTopInning").GetBoolean();

                var pitchIndexes = play.GetProperty("pitchIndex").EnumerateArray();
                int pitchIndex = pitchIndexes.Any() ? pitchIndexes.Last().GetInt32() : -1;

                var runners = play.GetProperty("runners").EnumerateArray();
                int currentPlayIndex = -1;
                GamePlayByPlay gpbp = GetEmptyEvent(game, inning, topOfInning, outs, currentOccupancy);
                for (int i = 0; i < runners.Count(); i++)
                {
                    var r = runners.ElementAt(i);

                    var runDetails = r.GetProperty("details");
                    var runEvent = runDetails.GetProperty("event").ToString();
                    int playIndex = runDetails.GetProperty("playIndex").GetInt32();
                    if (currentPlayIndex == -1)
                        currentPlayIndex = playIndex;

                    // Check if a new preAb event happend
                    if (playIndex != currentPlayIndex)
                    {
                        gpbp.EndBaseOccupancy = OccupancyArrayToInt(nextOccupancy);
                        SetEndRunnerOutcomes(gpbp, currentOccupancy, nextOccupancy);
                        pbp.Add(gpbp);
                        for (int j = 0; j < 3; j++)
                            currentOccupancy[j] = nextOccupancy[j];

                        outs = gpbp.EndOuts;
                        gpbp = GetEmptyEvent(game, inning, topOfInning, outs, currentOccupancy);
                        currentPlayIndex = playIndex;
                    }

                    UpdatePBPForRunner(gpbp, r, currentOccupancy, nextOccupancy);
                    PBP_Events runEventId = EventStringToInt(runEvent);
                    if ((gpbp.Result & runEventId) == 0)
                        gpbp.Result |= runEventId;
                }

                var matchup = play.GetProperty("matchup");
                gpbp.HitterId = matchup.GetProperty("batter").GetProperty("id").GetInt32();
                gpbp.PitcherId = matchup.GetProperty("pitcher").GetProperty("id").GetInt32();

                var playEvents = play.GetProperty("playEvents").EnumerateArray();
                UpdateHitData(gpbp, playEvents);

                gpbp.EndBaseOccupancy = OccupancyArrayToInt(nextOccupancy);
                SetEndRunnerOutcomes(gpbp, currentOccupancy, nextOccupancy);
                pbp.Add(gpbp);
                outs = gpbp.EndOuts;
                for (int i = 0; i < 3; i++)
                    currentOccupancy[i] = nextOccupancy[i];

            }

            return pbp;
        }

        private const int GAMES_TO_DROP_INDEXES = 10000;

        public static async Task<bool> Update(int year, bool updateIndices)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var alreadyLoggedIds = db.GamePlayByPlay.Where(f => f.Year == year).Select(f => f.GameId).Distinct().ToList().Order();
                var gameIds = db.Player_Hitter_GameLog.Where(f => f.Year == year && !alreadyLoggedIds.Contains(f.GameId)).Select(f => f.GameId).Distinct().ToList();

                if (!gameIds.Any())
                    return true;

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in gameIds
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                progress_bar_thread = 0;
                thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];
                List<Task<List<GamePlayByPlay>>> tasks = new(NUM_THREADS);

                if (gameIds.Count() > GAMES_TO_DROP_INDEXES)
                {
                    // Drop indexes to speedup insertion
                    List<string> indexNames = ["idx_PBP_HitterYearMonth", "idx_PBP_Run1stYearMonth", "idx_PBP_Run2ndYearMonth", "idx_PBP_Run3rdYearMonth", "idx_PBP_YearLeagueSituation", "idx_PBP_FielderYearMonth", "idx_PBP_Game"];
                    foreach (var name in indexNames)
                    {
                        try
                        {
                            db.Database.ExecuteSqlRaw($"DROP INDEX {name};");
                        }
                        catch (Exception e)
                        {
                            // Index doesn't exist, so fine
                        }
                    }
                }
                

                // Use transaction to speed up insert
                await using var transaction = await db.Database.BeginTransactionAsync();
                try {
                    // Get data
                    using (ProgressBar progressBar = new(gameIds.Count(), $"Generating PBP Stats for {year}"))
                    {
                        // Send out gameIds for threads to collect data
                        for (int i = 0; i < NUM_THREADS; i++)
                        {
                            if (i >= id_partitions.Count())
                                break;
                            int idx = i;
                            Task<List<GamePlayByPlay>> task = GetPBPThreadFunction(id_partitions.ElementAt(i), idx, progressBar, gameIds.Count());
                            tasks.Add(task);
                        }

                        // Collect data from threads, insert to db
                        List<List<GamePlayByPlay>> gamesList = new();
                        foreach (var task in tasks)
                        {
                            gamesList.Add(await task);
                            progress_bar_thread++;
                        }

                        // Need to wait until all threads are done to start inserting to prevent locking the db
                        foreach (var games in gamesList)
                        {
                            await db.BulkInsertAsync(games);
                        }

                        progressBar.Tick(gameIds.Count());
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                if (updateIndices)
                {
                    try {
                        // Restore indexes
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_HitterYearMonth ON GamePlayByPlay(HitterId, Year, Month);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_Run1stYearMonth ON GamePlayByPlay(Run1stId, Year, Month);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_Run2ndYearMonth ON GamePlayByPlay(Run2ndId, Year, Month);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_Run3rdYearMonth ON GamePlayByPlay(Run3rdId, Year, Month);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_FielderYearMonth ON GamePlayByPlay(FielderId, Year, Month);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_Game ON GamePlayByPlay(GameId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PBP_YearLeagueSituation ON GamePlayByPlay(Year, LeagueId, StartOuts, StartBaseOccupancy);");
                    }
                    catch (Exception)
                    {
                        // Fails if they already exist, which means they don't need to be created
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetPlayByPlay");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
