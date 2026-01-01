using Db;
using EFCore.BulkExtensions;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class FielderGameLog
    {
        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static int[] thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

        private const string FIELDER_LOG_DIRECTORY = Constants.DATA_AQ_DIRECTORY + "/Logs/FielderGameLogs/";

        private static async Task<List<Player_Fielder_GameLog>> GetFielderThreadFunction(IEnumerable<int> ids, int year, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<Player_Fielder_GameLog> logs = new(ids.Count() * 120);
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int id in ids)
            {
                // Get games during selected timeframe
                List<Player_Fielder_GameLog>? gameLogs = null;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        gameLogs = await GetFielderGameLogsAsync(id, httpClient, year);
                        break;
                    }
                    catch (Exception e)
                    {
                        File.WriteAllText(FIELDER_LOG_DIRECTORY + $"Log_{id}.txt", e.Message + "\n\n" + e.InnerException + "\n\n" + e.StackTrace);
                    }
                }
                if (gameLogs == null)
                {
                    thread_counts[thread_idx]++;

                    if (thread_idx == progress_bar_thread) // Only update from single thread
                    {
                        int count = thread_counts.Sum();
                        progress.Report(Convert.ToSingle(count) / progressSum);
                    }
                    continue;
                }
                    

                // Make sure that existing games aren't added
                var gamesInDb = db.Player_Fielder_GameLog.Where(f => f.MlbId == id && f.Year == year).Select(f => f.GameId);
                //var monthsInDb = db.Player_Hitter_GameLog.Where(f => f.MlbId == id && f.Year == year).Select(f => f.Month).Distinct();
                gameLogs = gameLogs.Where(f => !gamesInDb.Contains(f.GameId)).ToList();
                logs.AddRange(gameLogs);

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return logs;
        }

        private static async Task<List<Player_Fielder_GameLog>> GetFielderGameLogsAsync(int id, HttpClient httpClient, int year)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{id}/stats?stats=gameLog&leagueListId=mlb_milb&group=fielding&gameType=R&sitCodes=1,2,3,4,5,6,7,8,9,10,11,12&hydrate=team&language=en&season={year}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting fielder stats for year={year}: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);

            // Check for any data
            if (!json.RootElement.TryGetProperty("stats", out var statsElement))
                return new();

            if (!statsElement.EnumerateArray().Any())
                return new();

            if (!statsElement.EnumerateArray().ElementAt(0).TryGetProperty("splits", out var splitsElement))
                return new();

            var splits = splitsElement.EnumerateArray();
            List<Player_Fielder_GameLog> logs = new(splits.Count());

            foreach (var split in splits)
            {
                // Stat elements
                if (!split.TryGetProperty("stat", out var statElement))
                    throw new Exception("Did not find property stat");

                if (!statElement.TryGetProperty("gamesStarted", out var gamesStartedElement))
                    throw new Exception("Did not find property gamesStarted");

                if (!statElement.TryGetProperty("errors", out var errorsElement))
                    throw new Exception("Did not find property errors");

                if (!statElement.TryGetProperty("chances", out var chancesElement))
                    throw new Exception("Did not find property chances");

                if (!statElement.TryGetProperty("throwingErrors", out var throwingErrorsElement))
                    throw new Exception("Did not find property throwingErrors");

                if (!statElement.TryGetProperty("innings", out var inningsElement))
                    throw new Exception("Did not find property innings");

                int[] inningsValues = [.. inningsElement.GetString().Split('.').Select(f => Int32.Parse(f))];

                if (!statElement.TryGetProperty("position", out var positionElement))
                    throw new Exception("Did not find property position");
                if (!positionElement.TryGetProperty("code", out var codeElement))
                    throw new Exception("Did not find property code");

                // Catcher-only stat elements, 0 for all other positions
                int sb = 0;
                int cs = 0;
                int pb = 0;
                if (statElement.TryGetProperty("caughtStealing", out var csElement)
                    && statElement.TryGetProperty("stolenBases", out var sbElement)
                    && statElement.TryGetProperty("passedBall", out var pbElement))
                {
                    sb = sbElement.GetInt32();
                    cs = csElement.GetInt32();
                    pb = pbElement.GetInt32();
                }

                // Team elements
                if (!split.TryGetProperty("team", out var teamElement))
                    throw new Exception("Did not find property team");

                if (!teamElement.TryGetProperty("id", out var teamIdElement))
                    throw new Exception("Did not find property id for team");

                if (!teamElement.TryGetProperty("league", out var leagueElement))
                    throw new Exception("Did not find property team");

                if (!leagueElement.TryGetProperty("id", out var leagueIdElement))
                    throw new Exception("Did not find property id for league");

                // Game elements
                if (!split.TryGetProperty("date", out var dateElement))
                    throw new Exception("Did not find property date");

                int[] date = [.. dateElement.GetString().Split('-').Select(f => Int32.Parse(f))];

                if (!split.TryGetProperty("isHome", out var isHomeElement))
                    throw new Exception("Did not find property isHome");

                if (!split.TryGetProperty("game", out var gameElement) || !gameElement.TryGetProperty("gamePk", out var gamePkElement))
                    throw new Exception("Did not find property game or game/gamePK");

                int leagueId = leagueIdElement.GetInt32();

                if (leagueId == Constants.MEXICAN_LEAGUE_ID)
                    continue;

                // Combine fielding stats from AL/NL to MLB
                // Needed for getting fielding stats in combined games while not splitting other statts that vary by league
                if (leagueId == 103 || leagueId == 104)
                    leagueId = 1; 

                logs.Add(new Player_Fielder_GameLog
                {
                    MlbId = id,
                    GameId = gamePkElement.GetInt32(),
                    LeagueId = leagueId,
                    TeamId = teamIdElement.GetInt32(),
                    Day = date[2],
                    Month = date[1],
                    Year = year,
                    Position = (DbEnums.Position)Int32.Parse(codeElement.GetString()),
                    Outs = (inningsValues[0] * 3) + inningsValues[1],
                    Chances = chancesElement.GetInt32(),
                    Errors = errorsElement.GetInt32(),
                    ThrowErrors = throwingErrorsElement.GetInt32(),
                    Started = gamesStartedElement.GetInt32() == 1,
                    IsHome = isHomeElement.GetBoolean(),
                    SB = sb,
                    CS = cs,
                    PassedBall = pb,
                });
            }

            return logs;
        }

        private static async Task<List<int>> GetPlayerIds(int sportId, int year)
        {
            HttpClient httpClient = new();

            HttpResponseMessage response = await httpClient.GetAsync($"https://bdfed.stitch.mlbinfra.com/bdfed/stats/player?stitch_env=prod&season={year}&sportId={sportId}&stats=season&group=fielding&gameType=R&limit=10000&offset=0&order=desc&playerPool=All");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting fielder stats for year={year}: {response.StatusCode}");
            }

            // Parse JSON to get player Ids
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            var stats = json.RootElement.GetProperty("stats");
            var statsArray = stats.EnumerateArray();

            return statsArray.Select(f => f.GetProperty("playerId").GetInt32()).ToList();
        }

        public static async Task<bool> Update(int year, bool rescan)
        { 
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Get ids
                HttpClient httpClient = new();
                List<int> ids = new(20000);

                using (ProgressBar progressBar = new(Constants.SPORT_IDS_MLBAPI.Count, $"Getting Fielder Ids {year}"))
                {
                    List<Task<List<int>>> getIdsTasks = new(Constants.SPORT_IDS_MLBAPI.Count);
                    foreach (int level in Constants.SPORT_IDS_MLBAPI)
                    {
                        getIdsTasks.Add(GetPlayerIds(level, year));
                    }

                    foreach (var task in getIdsTasks)
                    {
                        ids.AddRange(await task);
                        ids = ids.Distinct().ToList();
                        progressBar.Tick();
                    }
                }
                

                if (!rescan)
                {
                    int[] yearIds = [.. db.Player_Fielder_GameLog.Where(f => f.Year == year).Select(f => f.MlbId).Distinct()];
                    ids = ids.Where(f => !yearIds.Contains(f)).ToList();
                } else {
                    db.BulkDelete(db.Player_Fielder_GameLog.Where(f => f.Year == year));
                }

                if (!ids.Any())
                    return true;

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in ids
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                // Reset shared variables between threads
                progress_bar_thread = 0;
                thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                int idCount = ids.Count();
                // Distribute tasks out into threads
                List<Task<List<Player_Fielder_GameLog>>> tasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new(idCount, $"Getting Fielder Game Logs for {year}"))
                {
                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        if (i >= idCount)
                            continue;
                        int idx = i;
                        Task<List<Player_Fielder_GameLog>> task = GetFielderThreadFunction(id_partitions.ElementAt(i), year, idx, progressBar, idCount);
                        tasks.Add(task);
                    }

                    foreach (var task in tasks)
                    {
                        var games = await task;
                        db.Player_Fielder_GameLog.AddRange(games);
                        progress_bar_thread++; // If thread N completes, move updating progress bar to thread N+1
                    }

                    // Prevent bar finishing below 100%
                    // Can end up there due to race condition in bar-ticking code
                    progressBar.Tick(idCount);
                }

                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in FielderGameLog");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
