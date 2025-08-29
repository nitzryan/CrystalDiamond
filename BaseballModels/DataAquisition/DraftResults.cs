using Db;
using HtmlAgilityPack;
using ShellProgressBar;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataAquisition
{
    internal class DraftResults
    {
        public static async Task<bool> Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                if (db.Draft_Results.Any(f => f.Year == year))
                    return true;
            
                HttpClient httpClient = new();

                // Get status of whether pick was signed or not
                List<(int, bool, int)> pickStatus = new(); // PickNumber, Signed, Bonus

                List<int> rounds;
                if (year < 2012)
                    rounds = [.. Enumerable.Range(1, 50)];
                else if (year < 2020)
                    rounds = [.. Enumerable.Range(1, 40)];
                else if (year == 2020)
                    rounds = [.. Enumerable.Range(1, 5)];
                else
                    rounds = [.. Enumerable.Range(1, 20)];

                using (ProgressBar progressBar = new(rounds.Count(), $"Getting Signing Status for {year} draft"))
                {
                    foreach (int round in rounds)
                    {
                        // Get page
                        HttpResponseMessage response = await httpClient.GetAsync($"https://www.baseball-reference.com/draft/index.fcgi?year_ID={year}&draft_round={round}&draft_type=junreg&query_type=year_round");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting BR Draft Round={round} for Year={year}: {response.StatusCode}");
                        }
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Create HTML doc
                        var doc = new HtmlDocument();
                        doc.LoadHtml(responseBody);

                        // Get table
                        var table = doc.DocumentNode.SelectSingleNode("//*[@id='draft_stats']");
                        var rows = table.SelectNodes(".//tbody//tr");
                        foreach (var row in rows)
                        {
                            int pickNum = Convert.ToInt32(row.SelectSingleNode(".//td[@data-stat='overall_pick']").InnerText);
                            string signed = row.SelectSingleNode(".//td[@data-stat='signed']").InnerText;
                            string bonus = row.SelectSingleNode(".//td[@data-stat='bonus']").InnerText;

                            int bonusAmount = bonus == "" ? -1 : Convert.ToInt32(Regex.Replace(bonus, "[$,]", ""));
                            bool didSign = signed == "Y";

                            // Error for duplicate with Matthew Long, different values for didSign
                            // Need to exclude value where he did not sign (he actually signed)
                            if (year == 2008 && pickNum == 708 && !didSign)
                                continue;

                            var item = (pickNum, didSign, bonusAmount);
                            if (!pickStatus.Contains(item))
                                pickStatus.Add(item);
                        }

                        Thread.Sleep(5000); // Otherwise BR will throw error for TooManyRequests
                        progressBar.Tick();
                    }
                }

                // Get data through mlb
                List<(int, string, int)> pickData = new();

                // Add/modify pick by pick
                using (ProgressBar progressBar = new(rounds.Count(), $"Adding players from {year} draft, assigning draft pick values"))
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/draft/{year}");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Getting draft data for {year}: {response.StatusCode}");
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument json = JsonDocument.Parse(responseBody);
                    JsonElement.ArrayEnumerator roundsJson = json.RootElement.GetProperty("drafts").GetProperty("rounds").EnumerateArray();
                    foreach (JsonElement round in roundsJson)
                    {
                        JsonElement.ArrayEnumerator picks = round.GetProperty("picks").EnumerateArray();
                        string roundString = round.GetProperty("round").GetString() ?? throw new Exception("No 'round' found for draft");
                        foreach (JsonElement pick in picks)
                        {
                            int id = -1;
                            if (!pick.TryGetProperty("person", out JsonElement person))
                            {
                                continue;
                            } else {
                                id = person.GetProperty("id").GetInt32();
                            }

                            int draftPick = pick.GetProperty("pickNumber").GetInt32();
                            if (!pickData.Any(f => f.Item1 == draftPick))
                                pickData.Add((draftPick, roundString, id));
                        }

                        progressBar.Tick();
                    }
                }

                // Combine
                var combined = pickStatus.Join(pickData, ps => ps.Item1, pd => pd.Item1, (ps, pd) => new Draft_Results
                {
                    Year = year,
                    Pick = ps.Item1,
                    Round = pd.Item2,
                    MlbId = pd.Item3,
                    Signed = ps.Item2 ? 1 : 0,
                    Bonus = ps.Item3,
                    BonusRank = -1
                }).OrderByDescending(f => f.Bonus).ThenBy(f => f.Pick);

                int bonusRank = 1;
                foreach (var c in combined)
                {
                    int br = bonusRank;
                    c.BonusRank = br;
                    bonusRank++;
                    db.Draft_Results.Add(c);
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in DraftResults");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
