using Db;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class GameLogUpdate
    {
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
                var games = json.RootElement.GetProperty("stats").EnumerateArray().First().GetProperty("splits").EnumerateArray();

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

                    var stats = game.GetProperty("stats");
                    var positions = game.GetProperty("positionsPlayed").EnumerateArray();

                    Player_Hitter_GameLog gl = new()
                    {
                        GameId = 0,
                        MlbId = id,
                        Day = Convert.ToInt32(gamedate[2]),
                        Month = gameMonth,
                        Year = Convert.ToInt32(gamedate[0]),
                        AB = stats.GetProperty("atBats").GetInt32(),
                        H = stats.GetProperty("hits").GetInt32(),
                        Hit2B = stats.GetProperty("doubles").GetInt32(),
                        Hit3B = stats.GetProperty("tripes").GetInt32(),
                        HR = stats.GetProperty("homeRuns").GetInt32(),
                        K = stats.GetProperty("strikeOuts").GetInt32(),
                        BB = stats.GetProperty("baseOnBalls").GetInt32(),
                        SB = stats.GetProperty("stolenBases").GetInt32(),
                        CS = stats.GetProperty("caughtStealing").GetInt32(),
                        HBP = stats.GetProperty("hitByPitch").GetInt32(),
                        Position = positions.Any() ? positions.First().GetProperty("code").GetInt32() : 10,
                        Level = game.GetProperty("sport").GetProperty("id").GetInt32(),
                        TeamId = game.GetProperty("team").GetProperty("id").GetInt32(),
                        LeagueId = game.GetProperty("league").GetProperty("id").GetInt32(),
                        HomeTeamId = game.GetProperty("idHome").GetBoolean() ? 
                            game.GetProperty("team").GetProperty("id").GetInt32() :
                            game.GetProperty("opponent").GetProperty("id").GetInt32(),
                    };
                    log.Add(gl);
                }
            } catch (Exception) // No games, so exit
            {
                return log;
            }
            
            return log;
        }

        private static async Task<bool> GetPlayerLogsAsync(SqliteDbContext db, HttpClient httpClient, int year, int month)
        {
            // Get stats for each level
            ProgressBar mainProgressBar = new(Constants.SPORT_IDS.Count, "Hitter Game Log Levels");
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
                var stats = json.RootElement.GetProperty("stats").EnumerateArray();

                // Loop through each id, getting stats
                using (ChildProgressBar childProgressBar = mainProgressBar.Spawn(stats.Count(), "Hitters at level"))
                {
                    foreach (var stat in stats)
                    {
                        int id = stat.GetProperty("id").GetInt32();
                        // Make sure player data hasn't been entered already
                        if (db.Player_Hitter_GameLog.Any(f => f.MlbId == id && f.Year == year && f.Month == month))
                        {
                            childProgressBar.Tick();
                            continue;
                        }

                        try
                        {
                            int startMonth = month == 4 ? 0 : month;
                            int endMonth = month == 9 ? 12 : month;
                            List<Player_Hitter_GameLog> gameLogs = await GetPlayer_Hitter_GameLogsAsync(id, httpClient, startMonth, endMonth, year);
                            db.Player_Hitter_GameLog.AddRange(gameLogs);
                        }
                        catch (Exception)
                        {
                            // TODO: Write errors to file, could just be network misses
                            Console.WriteLine("Hello");
                        }

                        childProgressBar.Tick();
                    }
                }
                db.SaveChanges();
            }

            return true;
        }

        public static async Task<bool> Main(SqliteDbContext db, int year, int month)
        {
            HttpClient httpClient = new();
            try {
                await GetPlayerLogsAsync(db, httpClient, year, month);
            } catch (Exception e)
            {
                Console.WriteLine("Error getting hitter game logs");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
