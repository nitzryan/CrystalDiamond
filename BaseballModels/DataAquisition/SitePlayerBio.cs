using Db;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class SitePlayerBio
    {
        public static async Task<bool> Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Site_PlayerBio.RemoveRange(db.Site_PlayerBio);
                db.SaveChanges();

                HttpClient httpClient = new();

                var players = db.Player.Where(f => f.SigningYear >= 2005);
                using (ProgressBar progressBar = new ProgressBar(players.Count(), $"Creating Site Player Bios"))
                {
                    foreach (var player in players)
                    {
                        bool isHitter = !player.Position.Equals("P");
                        bool isPitcher = !player.Position.Equals("H");
                        var mp = db.Model_Players.Where(f => f.MlbId == player.MlbId);
                        bool hasModel = mp.Any();

                        var transactions = db.Transaction_Log.Where(f => f.MlbId == player.MlbId)
                            .OrderByDescending(f => f.Year)
                            .ThenByDescending(f => f.Month)
                            .ThenByDescending(f => f.Day);

                        int parentId = transactions.Any() ? transactions.First().ParentOrgId : 0;
                        int levelId = -1;

                        if (isHitter)
                        {
                            var ya = db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == player.MlbId && f.Year >= year - 1);
                            if (ya.Any())
                                levelId = ya.OrderByDescending(f => f.LevelId).First().LevelId;
                            else
                                levelId = -1;
                        }
                        else {
                            var ya = db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == player.MlbId && f.Year >= year - 1);
                            if (ya.Any())
                                levelId = ya.OrderByDescending(f => f.LevelId).First().LevelId;
                            else
                                levelId = -1;
                        }

                        string position = "";
                        if (isHitter)
                        {
                            Func<List<int>, Player_Hitter_GameLog, List<int>> PositionAggregation = (l, gl) =>
                            {
                                List<int> gamesAtPosition = [l[0], l[1], l[2], l[3], l[4], l[5], l[6], l[7], l[8], l[9], l[10], l[11]];
                                gamesAtPosition[gl.Position]++;
                                return gamesAtPosition;
                            };

                            List<int> initList = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

                            var phya = db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == player.MlbId).OrderByDescending(f => f.Year).FirstOrDefault();
                            int playerLastYear = phya != null ? phya.Year : year;
                            var games = db.Player_Hitter_GameLog.Where(f => f.MlbId == player.MlbId && f.Year >= playerLastYear - 1 && f.Position < 12)
                                .AsEnumerable();
                            
                            if (games.Any())
                            {
                                var gamesAtPosition = games.Aggregate(initList, (a, b) => PositionAggregation(a, b));
                                int totalGames = games.Count();
                                List<float> gamesProp = [.. gamesAtPosition.Select(f => (float)f / totalGames)];

                                const float SINGLE_POSITION_CUTOFF = 0.8f;
                                const float MULTI_POSITION_CUTOFF = 0.2f;

                                List<bool> sp = [.. gamesProp.Select(f => f > SINGLE_POSITION_CUTOFF)];
                                List<bool> mps = [.. gamesProp.Select(f => f > MULTI_POSITION_CUTOFF)];
                                // Single Position
                                if (sp[2])
                                    position = "C";
                                else if (sp[3])
                                    position = "1B";
                                else if (sp[4])
                                    position = "2B";
                                else if (sp[5])
                                    position = "3B";
                                else if (sp[6])
                                    position = "SS";
                                else if (sp[7])
                                    position = "LF";
                                else if (sp[8])
                                    position = "CF";
                                else if (sp[9])
                                    position = "RF";
                                else if (gamesProp[7] + gamesProp[8] + gamesProp[9] > SINGLE_POSITION_CUTOFF)
                                    position = "OF";
                                else if (sp[10])
                                    position = "DH";
                                else if (sp[11])
                                    position = "P";
                                else
                                {
                                    if (mps[2])
                                        position += "C/";
                                    if (mps[3])
                                        position += "1B/";
                                    if (mps[4])
                                        position += "2B/";
                                    if (mps[5])
                                        position += "3B/";
                                    if (mps[6])
                                        position += "SS/";
                                    if (mps[7])
                                        position += "LF/";
                                    if (mps[8])
                                        position += "CF/";
                                    if (mps[9])
                                        position += "RF/";
                                    if (mps[10])
                                        position += "DH/";
                                    if (mps[11])
                                        position += "P/";

                                    if (position.Length > 0)
                                        position = position.Substring(0, position.Length - 1);
                                    else
                                        position = "UTIL";
                                }
                            }
                            else 
                            {
                                HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/people/{player.MlbId}");
                                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                                {
                                    throw new Exception($"Getting player={player.MlbId}: {response.StatusCode}");
                                }
                                
                                string responseBody = await response.Content.ReadAsStringAsync();
                                JsonDocument json = JsonDocument.Parse(responseBody);
                                JsonElement person = json.RootElement.GetProperty("people").EnumerateArray().ElementAt(0);

                                position = person.GetProperty("primaryPosition").GetProperty("abbreviation").GetString() ?? 
                                    throw new Exception($"Failed to get position for {player.MlbId}");
                            }
                        } 
                        else {
                            position = "P";
                        }

                        string status = "";
                        if (!mp.Any())
                            status = "NoModel";
                        else if (mp.Single().LastMLBSeason < 10000)
                            status = "PostService";
                        else if (mp.Single().LastProspectYear < 10000)
                            status = "InService";
                        else
                            status = "Prospect";

                        int draftPick = -1;
                        int draftBonus = -1;
                        string draftRound = "";
                        if (player.DraftPick != null && player.SigningYear != null)
                        {
                            Db.Draft_Results dr = db.Draft_Results.Where(f => f.MlbId == player.MlbId
                                && f.Year == player.SigningYear.Value
                                && f.Pick == player.DraftPick.Value).Single();

                            draftPick = dr.Pick;
                            draftRound = dr.Round;
                            draftBonus = dr.Bonus;
                        }

                        db.Site_PlayerBio.Add(new Site_PlayerBio
                        {
                            Id = player.MlbId,
                            Position = position,
                            IsPitcher = isPitcher ? 1 : 0,
                            IsHitter = isHitter ? 1 : 0,
                            HasModel = hasModel ? 1 : 0,
                            ParentId = parentId,
                            LevelId = levelId,
                            Status = status,
                            DraftPick = draftPick,
                            DraftRound = draftRound,
                            DraftBonus = draftBonus,
                            SigningYear = player.SigningYear.Value
                        });

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetPlayersThroughStatsAsync");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
