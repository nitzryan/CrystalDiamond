using Db;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class GameLogUpdate
    {
        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts;

        private static async Task<List<Player_Hitter_GameLog>> Get_Hitter_GameLogs_ThreadFunction(IEnumerable<int> ids, int startMonth, int endMonth, int year, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<Player_Hitter_GameLog> logs = new();
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int id in ids)
            {
                // Get games during selected timeframe
                List<Player_Hitter_GameLog>? gameLogs = null;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        gameLogs = await GetPlayer_Hitter_GameLogsAsync(id, httpClient, startMonth, endMonth, year);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (gameLogs == null)
                    throw new Exception($"Failed 3 times to get Pitcher game logs for {id} in Year={year}");

                // Make sure that existing games aren't added
                var gamesInDb = db.Player_Hitter_GameLog.Where(f => f.MlbId == id && f.Year == year).Select(f => f.GameId);
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

        private static async Task<List<Player_Pitcher_GameLog>> Get_Pitcher_GameLogs_ThreadFunction(IEnumerable<int> ids, int startMonth, int endMonth, int year, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            HttpClient httpClient = new();
            List<Player_Pitcher_GameLog> logs = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int id in ids)
            {
                // Get games during selected timeframe
                List<Player_Pitcher_GameLog>? gameLogs = null;
                for (var i = 0; i < 3; i++)
                {
                    try {
                        gameLogs = await GetPlayer_Pitcher_GameLogsAsync(id, httpClient, startMonth, endMonth, year);
                        break;
                    } catch(Exception)
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (gameLogs == null)
                    throw new Exception($"Failed 3 times to get Pitcher game logs for {id} in Year={year}");

                // Make sure that existing games aren't added
                var gamesInDb = db.Player_Pitcher_GameLog.Where(f => f.MlbId == id && f.Year == year).Select(f => f.GameId).Distinct();
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

        private static async Task<List<Player_Hitter_GameLog>> GetPlayer_Hitter_GameLogsAsync(int id, HttpClient httpClient, int startMonth, int endMonth, int year)
        {
            List<Player_Hitter_GameLog> log = new();

            // Get data
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{id}/stats?stats=gameLog&leagueListId=mlb_milb&group=hitting&gameType=R&sitCodes=1,2,3,4,5,6,7,8,9,10,11,12&hydrate=team&language=en&season={year}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting hitter stats for year={year}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            try {
                var stat = json.RootElement.GetProperty("stats");
                var statArray = stat.EnumerateArray();
                var statFirst = statArray.First();
                var splits = statFirst.GetProperty("splits");
                var games = splits.EnumerateArray();

                // Go through each game
                foreach (var game in games)
                {
                    int leagueId = game.GetProperty("team").GetProperty("league").GetProperty("id").GetInt32();
                    if (leagueId == Constants.MEXICAN_LEAGUE_ID)
                        continue;

                    // Make sure game is in proper month
                    string[] gamedate = game.GetProperty("date").GetString().Split("-");
                    int gameMonth = Convert.ToInt32(gamedate[1]);
                    if (gameMonth < startMonth || gameMonth > endMonth)
                        continue;

                    var stats = game.GetProperty("stat");
                    var positions = game.GetProperty("positionsPlayed").EnumerateArray();

                    Player_Hitter_GameLog gl = new()
                    {
                        GameId = game.GetProperty("game").GetProperty("gamePk").GetInt32(),
                        MlbId = id,
                        Day = Convert.ToInt32(gamedate[2]),
                        Month = gameMonth,
                        Year = Convert.ToInt32(gamedate[0]),
                        AB = stats.GetProperty("atBats").GetInt32(),
                        PA = stats.GetProperty("plateAppearances").GetInt32(),
                        H = stats.GetProperty("hits").GetInt32(),
                        Hit2B = stats.GetProperty("doubles").GetInt32(),
                        Hit3B = stats.GetProperty("triples").GetInt32(),
                        HR = stats.GetProperty("homeRuns").GetInt32(),
                        K = stats.GetProperty("strikeOuts").GetInt32(),
                        BB = stats.GetProperty("baseOnBalls").GetInt32(),
                        SB = stats.GetProperty("stolenBases").GetInt32(),
                        CS = stats.GetProperty("caughtStealing").GetInt32(),
                        HBP = stats.GetProperty("hitByPitch").GetInt32(),
                        Position = positions.Any() ? Convert.ToInt32(positions.First().GetProperty("code").ToString()) : 10,
                        LevelId = game.GetProperty("sport").GetProperty("id").GetInt32(),
                        TeamId = game.GetProperty("team").GetProperty("id").GetInt32(),
                        LeagueId = game.GetProperty("league").GetProperty("id").GetInt32(),
                        HomeTeamId = game.GetProperty("isHome").GetBoolean() ?
                            game.GetProperty("team").GetProperty("id").GetInt32() :
                            game.GetProperty("opponent").GetProperty("id").GetInt32(),
                    };

                    // MLB changed Rk Adv to different LevelId id for some years, so catch here
                    if (gl.LevelId == 5442)
                        gl.LevelId = 16;

                    // Make sure to avoid games with no stats accumulated
                    if (gl.AB == 0 && gl.H == 0 && gl.BB == 0 && gl.SB == 0 && gl.CS == 0 && gl.HBP == 0)
                        continue;

                    // Map DSL/VSL to seperate league
                    if (gl.LeagueId == Constants.DSL_LEAGUE_ID || gl.LeagueId == Constants.VSL_LEAGUE_ID)
                        gl.LevelId = Constants.SPORT_IDS.Last();
                    log.Add(gl);
                }
            } catch (Exception) // No games, so exit
            {
                return log;
            }
            
            return log;
        }

        private static async Task<List<Player_Pitcher_GameLog>> GetPlayer_Pitcher_GameLogsAsync(int id, HttpClient httpClient, int startMonth, int endMonth, int year)
        {
            List<Player_Pitcher_GameLog> log = new();

            // Get data
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{id}/stats?stats=gameLog&leagueListId=mlb_milb&group=pitching&gameType=R&sitCodes=1,2,3,4,5,6,7,8,9,10,11,12&hydrate=team&language=en&season={year}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting hitter stats for year={year}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            try
            {
                var stat = json.RootElement.GetProperty("stats");
                var statArray = stat.EnumerateArray();
                var statFirst = statArray.First();
                var splits = statFirst.GetProperty("splits");
                var games = splits.EnumerateArray();

                // Go through each game
                foreach (var game in games)
                {
                    int leagueId = game.GetProperty("team").GetProperty("league").GetProperty("id").GetInt32();
                    if (leagueId == Constants.MEXICAN_LEAGUE_ID)
                        continue;

                    // Make sure game is in proper month
                    string[] gamedate = game.GetProperty("date").GetString().Split("-");
                    int gameMonth = Convert.ToInt32(gamedate[1]);
                    if (gameMonth < startMonth || gameMonth > endMonth)
                        continue;

                    var stats = game.GetProperty("stat");

                    Player_Pitcher_GameLog gl = new()
                    {
                        GameId = game.GetProperty("game").GetProperty("gamePk").GetInt32(),
                        MlbId = id,
                        Day = Convert.ToInt32(gamedate[2]),
                        Month = gameMonth,
                        Year = Convert.ToInt32(gamedate[0]),
                        BattersFaced = stats.GetProperty("battersFaced").GetInt32(),
                        Started = stats.GetProperty("gamesStarted").GetInt32(),
                        Outs = stats.GetProperty("outs").GetInt32(),
                        R = stats.GetProperty("runs").GetInt32(),
                        ER = stats.GetProperty("earnedRuns").GetInt32(),
                        H = stats.GetProperty("hits").GetInt32(),
                        Hit2B = stats.GetProperty("doubles").GetInt32(),
                        Hit3B = stats.GetProperty("triples").GetInt32(),
                        HR = stats.GetProperty("homeRuns").GetInt32(),
                        K = stats.GetProperty("strikeOuts").GetInt32(),
                        BB = stats.GetProperty("baseOnBalls").GetInt32(),
                        HBP = stats.GetProperty("hitByPitch").GetInt32(),
                        GO = stats.GetProperty("groundOuts").GetInt32(),
                        AO = stats.GetProperty("airOuts").GetInt32(),
                        LevelId = game.GetProperty("sport").GetProperty("id").GetInt32(),
                        TeamId = game.GetProperty("team").GetProperty("id").GetInt32(),
                        LeagueId = game.GetProperty("league").GetProperty("id").GetInt32(),
                        HomeTeamId = game.GetProperty("isHome").GetBoolean() ?
                            game.GetProperty("team").GetProperty("id").GetInt32() :
                            game.GetProperty("opponent").GetProperty("id").GetInt32(),
                    };

                    // MLB changed Rk Adv to different LevelId id for some years, so catch here
                    if (gl.LevelId == 5442)
                        gl.LevelId = 16;

                    // Map DSL/VSL to seperate league
                    if (gl.LeagueId == Constants.DSL_LEAGUE_ID || gl.LeagueId == Constants.VSL_LEAGUE_ID)
                        gl.LevelId = Constants.SPORT_IDS.Last();
                    log.Add(gl);
                }
            }
            catch (Exception) // No games, so exit
            {
                return log;
            }

            return log;
        }

        private static async Task<bool> GetHitterLogsAsync(SqliteDbContext db, HttpClient httpClient, int year, int startMonth, int endMonth)
        {
            // Get stats for each level
            int sport_id_idx = 0;
            foreach(int sportId in Constants.SPORT_IDS)
            {
                // Get hitters that have stats at level
                HttpResponseMessage response = await httpClient.GetAsync($"https://bdfed.stitch.mlbinfra.com/bdfed/stats/player?stitch_env=prod&season={year}&sportId={sportId}&stats=season&group=hitting&gameType=R&limit=5000&offset=0&sortStat=homeRuns&order=desc");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Getting hitter stats for year={year}: {response.StatusCode}");
                }

                // Parse JSON to get player Ids
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(responseBody);
                var stats = json.RootElement.GetProperty("stats");
                var statsArray = stats.EnumerateArray();
                IEnumerable<int> ids = statsArray.Select(f => f.GetProperty("playerId").GetInt32());

                // Check for empty (Covid year)
                if (!ids.Any())
                    continue;

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in ids
                                            group item by j++ % NUM_THREADS into part
                                            select part.AsEnumerable();

                // Distribute tasks out into threads
                List<Task<List<Player_Hitter_GameLog>>> tasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new(ids.Count(), $"Getting Hitter Game Logs for {Constants.SPORT_ID_NAMES[sport_id_idx]} {year}"))
                {
                    progress_bar_thread = 0;
                    thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        int idx = i;
                        Task<List<Player_Hitter_GameLog>> task = Get_Hitter_GameLogs_ThreadFunction(id_partitions.ElementAt(i), startMonth, endMonth, year, idx, progressBar, ids.Count());
                        tasks.Add(task);
                    }

                    foreach (var task in tasks)
                    {
                        var games = await task;
                        db.Player_Hitter_GameLog.AddRange(games);
                        progress_bar_thread++; // If thread N completes, move updating progress bar to thread N+1
                    }
                }

                db.SaveChanges();
                sport_id_idx++;
            }

            return true;
        }

        private static async Task<bool> GetPitcherLogsAsync(SqliteDbContext db, HttpClient httpClient, int year, int startMonth, int endMonth)
        {
            // Get stats for each level
            int sport_id_idx = 0;
            foreach (int sportId in Constants.SPORT_IDS)
            {
                // Get hitters that have stats at level
                HttpResponseMessage response = await httpClient.GetAsync($"https://bdfed.stitch.mlbinfra.com/bdfed/stats/player?stitch_env=prod&season={year}&sportId={sportId}&stats=season&group=pitching&gameType=R&limit=5000&offset=0&sortStat=homeRuns&order=desc");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Getting hitter stats for year={year}: {response.StatusCode}");
                }

                // Parse JSON to get player Ids
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(responseBody);
                var stats = json.RootElement.GetProperty("stats");
                var statsArray = stats.EnumerateArray();
                IEnumerable<int> ids = statsArray.Select(f => f.GetProperty("playerId").GetInt32());

                // Check for empty (Covid year)
                if (!ids.Any())
                    continue;

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in ids
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                // Distribute tasks out into threads
                List<Task<List<Player_Pitcher_GameLog>>> tasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new(ids.Count(), $"Getting Pitcher Game Logs for {Constants.SPORT_ID_NAMES[sport_id_idx]} {year}"))
                {
                    progress_bar_thread = 0;
                    thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        int idx = i;
                        Task<List<Player_Pitcher_GameLog>> task = Get_Pitcher_GameLogs_ThreadFunction(id_partitions.ElementAt(i), startMonth, endMonth, year, idx, progressBar, ids.Count());
                        tasks.Add(task);
                    }

                    foreach (var task in tasks)
                    {
                        var games = await task;
                        db.Player_Pitcher_GameLog.AddRange(games);
                        progress_bar_thread++; // If thread N completes, move updating progress bar to thread N+1
                    }
                }

                db.SaveChanges();
                sport_id_idx++;
            }

            return true;
        }

        public static async Task<bool> Main(int year, int startMonth, int endMonth)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            try
            {
                await GetHitterLogsAsync(db, httpClient, year, startMonth, endMonth);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting hitter game logs");
                Utilities.LogException(e);
                return false;
            }

            try
            {
                await GetPitcherLogsAsync(db, httpClient, year, startMonth, endMonth);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting pitcher game logs");
                Utilities.LogException(e);
                return false;
            }

            return true;
        }
    }
}
