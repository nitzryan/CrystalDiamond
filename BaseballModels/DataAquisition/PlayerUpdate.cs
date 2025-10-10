using Db;
using HtmlAgilityPack;
using ShellProgressBar;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataAquisition
{
    internal class PlayerUpdate
    {
        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts;

        private static async Task<(Player?, string?)> Get_Player_Async(int id, HttpClient httpClient)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{id}?hydrate=currentTeam,team,stats(group=[hitting,pitching],type=[yearByYear](team(league)),leagueListId=mlb_milb)&site=en");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting player={id}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            try
            {
                JsonElement person = json.RootElement.GetProperty("people").EnumerateArray().ElementAt(0);
                return (GetPlayerFromJson(person), null);

            }
            catch (Exception e)
            {
                return(null, ($"Exception for id={id}: {e.Message}"));
            }
        }

        private static async Task<(List<Player>, List<string>)> Get_Player_Thread_Function(IEnumerable<int> ids, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            List<Player> playersToInsert = new();
            List<string> errors = new();

            HttpClient httpClient = new();
            IProgress<float> progress = progressBar.AsProgress<float>();
            foreach (var id in ids)
            {
                // Multiple attempts to get player
                for (var i = 0; i < 3; i++)
                {
                    try {
                        var result = await Get_Player_Async(id, httpClient);
                        if (result.Item1 != null)
                            playersToInsert.Add(result.Item1);
                        else if (result.Item2 != null)
                            errors.Add(result.Item2);
                        break;
                    } catch (Exception)
                    {
                        Thread.Sleep(1000);
                    }
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return (playersToInsert, errors);
        }

        static private Player GetPlayerFromJson(JsonElement person)
        {
            int id = person.GetProperty("id").GetInt32();
            string useFirstName = person.GetProperty("useName").GetString() ?? throw new Exception("No useName found");
            string useLastName = person.GetProperty("useLastName").GetString() ?? throw new Exception("No useLastName found");
            string bats = person.GetProperty("batSide").GetProperty("code").GetString() ?? throw new Exception("No batSide found");
            string throws = person.GetProperty("pitchHand").GetProperty("code").GetString() ?? throw new Exception("No pitchHand found");
            string birthdateFormatted = person.GetProperty("birthDate").GetString() ?? throw new Exception("No birthDate found");
            string[] birthdate = birthdateFormatted.Split("-");
            
            // Parse position code to hitter (H), Pitcher (P), Both (TWP)
            string position = person.GetProperty("primaryPosition").GetProperty("code").GetString() ?? throw new Exception("No Position Code Found");
            try {
                int code = Convert.ToInt32(position);
                if (code == 1)
                    position = "P";
                else
                    position = "H";
            } catch (Exception) // Two-Way players have code "Y"
            {
                if (position == "Y")
                    position = "TWP";
                else
                    position = "H";
            }

            // Parse to get first year of data
            int? startYear = null;
            int? signingMonth = null;
            try
            {
                int sy = 10000;
                var statsArray = person.GetProperty("stats").EnumerateArray();
                foreach (var sa in statsArray)
                {
                    int testStartYear = Convert.ToInt32(sa.GetProperty("splits")
                    .EnumerateArray().First()
                    .GetProperty("season").ToString());
                    if (testStartYear < sy)
                        sy = testStartYear;
                }

                startYear = sy;
                signingMonth = 1;
            }
            catch (Exception) { }

            Player player = new()
            {
                MlbId = id,
                BirthYear = Convert.ToInt32(birthdate[0]),
                BirthMonth = Convert.ToInt32(birthdate[1]),
                BirthDate = Convert.ToInt32(birthdate[2]),
                Position = position,
                Bats = bats,
                Throws = throws,
                UseFirstName = useFirstName,
                UseLastName = useLastName,
                SigningYear = startYear,
                SigningMonth = signingMonth
            };

            return player;
        }

        static private async Task<bool> GetPlayersThroughDraftAsync(SqliteDbContext db, HttpClient httpClient, int year)
        {
            db.ChangeTracker.Clear();
            List<int> playersAdded = new List<int>();
        
            // Get Data
            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/draft/{year}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting draft data for {year}: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            JsonElement.ArrayEnumerator rounds = json.RootElement.GetProperty("drafts").GetProperty("rounds").EnumerateArray();

            // Add/modify pick by pick
            using (ProgressBar progressBar = new(rounds.Count(), $"Adding players from {year} draft, assigning draft pick values"))
            {
                foreach (JsonElement round in rounds)
                {
                    JsonElement.ArrayEnumerator picks = round.GetProperty("picks").EnumerateArray();
                    foreach (JsonElement pick in picks)
                    {
                        if (!pick.TryGetProperty("person", out JsonElement person))
                        {
                            continue;
                        }
                        int id = person.GetProperty("id").GetInt32();

                        var dbPlayer = db.Player.Where(f => f.MlbId == id);
                        if (dbPlayer.Any()) // Player Exists, edit required properties
                        {
                            Player p = dbPlayer.First();
                            p.DraftPick = pick.GetProperty("pickNumber").GetInt32();

                            // Draft was in June 2005-2020, July 2021+
                            if (year < 2021)
                                p.SigningMonth = 6;
                            else
                                p.SigningMonth = 7;
                            p.SigningYear = year;
                        }
                        else { // Player doesn't exist (Single() fails) so add player
                            try
                            {
                                Player player = GetPlayerFromJson(person);
                                player.DraftPick = pick.GetProperty("pickNumber").GetInt32();

                                // Check to see if player did not sign
                                if (!db.Draft_Results.Any(f => f.Year == year && f.MlbId == id && f.Signed == 1))
                                    continue;

                                // MLB API sometimes has duplicates, so check it hasn't already been added
                                if (playersAdded.Contains(id))
                                {
                                    continue;
                                }
                                playersAdded.Add(id);

                                // Draft was in June 2005-2020, July 2021+
                                if (year < 2021)
                                    player.SigningMonth = 6;
                                else
                                    player.SigningMonth = 7;
                                player.SigningYear = year;

                                db.Player.Add(player);
                            }
                            catch (Exception) { } // Some players dont have data and should just be ignored
                                                  // Often these players didn't sign and only have their name known
                        }
                    }

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
            return true;
        }

        // Outputs list of ids to add that doesn't have duplicates, and none are in db
        // Meant to be fed into CreatePlayersAsync
        static private async Task<List<int>> GetPlayersThroughStatsAsync(SqliteDbContext db, HttpClient httpClient, int year)
        {
            db.ChangeTracker.Clear();
            List<int> playersToInsert = new List<int>();

            using (ProgressBar progressBar = new ProgressBar(Constants.SPORT_IDS.Count - 1, $"Getting PlayerIds for {year}"))
            {
                for (int i = 0; i < Constants.SPORT_IDS.Count - 1; i++) // Different levels, ignore last because it is a different level in MLB's sytem
                {
                    int sportId = Constants.SPORT_IDS.ElementAt(i);

                    // Seperate stats for hitting and pithcing
                    List<string> positions = new(["hitting", "pitching"]);
                    foreach (string position in positions)
                    {
                        // Get all stats this level/year
                        HttpResponseMessage response = await httpClient.GetAsync($"https://bdfed.stitch.mlbinfra.com/bdfed/stats/player?stitch_env=prod&season={year}&sportId={sportId}&stats=season&group={position}&gameType=R&limit=5000&offset=0&sortStat=homeRuns&order=desc");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting player stats for {position}: {response.StatusCode}");
                        }

                        // Extract list of playerIds
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JsonDocument json = JsonDocument.Parse(responseBody);
                        JsonElement jsonPlayers = json.RootElement.GetProperty("stats");
                        foreach (JsonElement player in jsonPlayers.EnumerateArray())
                        {
                            int leagueId = player.GetProperty("leagueId").GetInt32();
                            if (leagueId == Constants.MEXICAN_LEAGUE_ID) // Found in some years AAA stats, but not affiliated ball
                                continue;

                            int playerId = player.GetProperty("playerId").GetInt32();

                            // Make sure database doesn't contain player, and that hasn't already been added (has hitting and pitching stats this year)
                            if (!db.Player.Any(f => f.MlbId == playerId) && !playersToInsert.Contains(playerId))
                                playersToInsert.Add(playerId);
                        }
                    }
                    progressBar.Tick();
                }
            }
            
            return playersToInsert;
        }

        // Takes in list of ids, gets player data for each id
        // Assumes ids not in db, and that no duplicate ids in list
        static private async Task<bool> CreatePlayersAsync(SqliteDbContext db, HttpClient httpClient, List<int> ids, int year)
        {
            StreamWriter file = File.CreateText(Constants.DATA_AQ_DIRECTORY + $"Logs/PlayerUpdate-{year}.txt");
            using (ProgressBar progressBar = new ProgressBar(ids.Count, $"Retrieving Player Data for {year}"))
            {
                // Check for empty (Covid year)
                if (ids.Count == 0)
                    return true;

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in ids
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                List<Task<(List<Player>, List<string>)>> tasks = new(NUM_THREADS);
                progress_bar_thread = 0;
                thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                for (int i = 0; i < NUM_THREADS; i++)
                {
                    if (i >= id_partitions.Count())  // No more partitions left to give out
                        break;
                    
                    int idx = i;

                    Task<(List<Player>, List<string>)> t = Get_Player_Thread_Function(id_partitions.ElementAt(i), idx, progressBar, ids.Count);
                    tasks.Add(t);
                }

                foreach(var task in tasks)
                {
                    var result = await task;
                    db.AddRange(result.Item1);
                    foreach (string error in result.Item2)
                        file.WriteLine(error);

                    progress_bar_thread++;
                }

                db.SaveChanges();
                db.ChangeTracker.Clear();
                file.Close();

                return true;
            }
        }

        // Calls all necessary subfuncttions
        public static async Task<bool> Main(int year)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            if (!db.Player.Any(f => f.SigningYear == year && f.DraftPick != null))
            {
                // Look at current years draft
                try
                {
                    await GetPlayersThroughDraftAsync(db, httpClient, year);
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed GetPlayersThroughDraftAsync");
                    Utilities.LogException(e);
                    return false;
                }
            }
            

            // Look at all players that have stats in affiliated ball in the selected year
            List<int> playersToInsert;
            try {
                playersToInsert = await GetPlayersThroughStatsAsync(db, httpClient, year);
            } catch(Exception e)
            {
                Console.WriteLine("Error in GetPlayersThroughStatsAsync");
                Utilities.LogException(e);
                return false;
            }

            bool playersInserted = await CreatePlayersAsync(db, httpClient, playersToInsert, year);

            return playersInserted;
        }

        public static async Task<bool> DraftOnly(int year)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            if (!db.Player.Any(f => f.SigningYear == year && f.DraftPick != null))
            {
                // Look at current years draft
                try
                {
                    await GetPlayersThroughDraftAsync(db, httpClient, year);
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed GetPlayersThroughDraftAsync");
                    Utilities.LogException(e);
                    return false;
                }
            }

            return true;
        }
    }
}
