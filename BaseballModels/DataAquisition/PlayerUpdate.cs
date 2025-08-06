using Db;
using ShellProgressBar;
using System;
using System.Text.Json;

namespace DataAquisition
{
    internal class PlayerUpdate
    {
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
                position = "TWP";
            }

            // Parse to get first year of data
            int? startYear = null;
            int? signingMonth = null;
            try
            {
                startYear = Convert.ToInt32(person.GetProperty("stats")
                    .EnumerateArray().First()
                    .GetProperty("splits")
                    .EnumerateArray().First()
                    .GetProperty("season").ToString());

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

            for (int i = 0; i < Constants.SPORT_IDS.Count; i++) // Different levels
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
                break;
            }
            
            return playersToInsert;
        }

        // Takes in list of ids, gets player data for each id
        // Assumes ids not in db, and that no duplicate ids in list
        static private async Task<bool> CreatePlayersAsync(SqliteDbContext db, HttpClient httpClient, List<int> playersToInsert)
        {
            using (ProgressBar progressBar = new ProgressBar(playersToInsert.Count, "Retrieving Player Data"))
            {
                foreach (var id in playersToInsert)
                {
                    // Get Player    
                    HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{id}?hydrate=currentTeam,team,stats(type=[yearByYear](team(league)),leagueListId=mlb_milb)&site=en");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Getting player data for {id}: {response.StatusCode}");
                    }

                    try
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JsonDocument json = JsonDocument.Parse(responseBody);
                        JsonElement person = json.RootElement.GetProperty("people").EnumerateArray().ElementAt(0);
                        Player player = GetPlayerFromJson(person);

                        db.Player.Add(player);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception for id={id}: {e.Message}");
                    }

                    progressBar.Tick();
                }

                db.SaveChanges();
                db.ChangeTracker.Clear();

                return true;
            }
        }

        // Calls all necessary subfuncttions
        public static async Task<bool> Main(int year)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            // Look at current years draft
            try
            {
                await GetPlayersThroughDraftAsync(db, httpClient, year);
            } catch (Exception e)
            { 
                Console.WriteLine("failed GetPlayersThroughDraftAsync");
                Utilities.LogException(e);
                return false;
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

            bool playersInserted = await CreatePlayersAsync(db, httpClient, playersToInsert);

            return playersInserted;
        }
    }
}
