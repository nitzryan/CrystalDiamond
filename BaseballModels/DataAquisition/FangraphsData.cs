using Db;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class FangraphsData
    {
        public static async Task<bool> Main(List<int> years)
        {
            try
            {
                HttpClient httpClient = new();
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                using (ProgressBar progressBar = new(years.Count(), "Getting fangraphs data"))
                {
                    foreach (int year in years)
                    {
                        db.Player_YearlyWar.RemoveRange(db.Player_YearlyWar.Where(f => f.Year == year));
                        db.SaveChanges();

                        HttpResponseMessage response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=pit&stats=sta&lg=all&qual=0&season={year}&pageitems=10000");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting Starting Pitchers Fangraphs stats: {response.StatusCode}");
                        }
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JsonDocument json = JsonDocument.Parse(responseBody);
                        var startingPitchers = json.RootElement.GetProperty("data").EnumerateArray().Select(f => new
                        {
                            id=f.GetProperty("xMLBAMID").GetInt32(),
                            war=f.GetProperty("WAR").GetDouble(),
                            tbf=(int)f.GetProperty("TBF").GetDouble(),
                        });

                        response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=pit&stats=rel&lg=all&qual=0&season={year}&pageitems=10000");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting Starting Pitchers Fangraphs stats: {response.StatusCode}");
                        }
                        responseBody = await response.Content.ReadAsStringAsync();
                        json = JsonDocument.Parse(responseBody);
                        var reliefPitchers = json.RootElement.GetProperty("data").EnumerateArray().Select(f => new
                        {
                            id = f.GetProperty("xMLBAMID").GetInt32(),
                            war = f.GetProperty("WAR").GetDouble(),
                            tbf = (int)f.GetProperty("TBF").GetDouble(),
                        });

                        var pitchersIds = startingPitchers.Select(f => f.id).ToList();
                        pitchersIds.AddRange(reliefPitchers.Select(f => f.id));
                        pitchersIds = pitchersIds.Distinct().ToList();
                        foreach (var id in pitchersIds)
                        {
                            var starterData = startingPitchers.Where(f => f.id == id);
                            var relieverData = reliefPitchers.Where(f => f.id == id);
                            db.Player_YearlyWar.Add(new Player_YearlyWar
                            {
                                MlbId = id,
                                Year = year,
                                IsHitter = 0,
                                PA = starterData.Sum(f => f.tbf) + relieverData.Sum(f => f.tbf),
                                WAR_h = 0,
                                WAR_s = starterData.Any() ? (float)starterData.First().war : 0,
                                WAR_r = relieverData.Any() ? (float)relieverData.First().war : 0,
                                OFF = 0,
                                DEF = 0,
                                BSR = 0,
                                REP = 0
                            });
                        }

                        response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=np&stats=bat&lg=all&qual=0&season={year}&pageitems=10000");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting Starting Pitchers Fangraphs stats: {response.StatusCode}");
                        }
                        responseBody = await response.Content.ReadAsStringAsync();
                        json = JsonDocument.Parse(responseBody);
                        var hitters = json.RootElement.GetProperty("data").EnumerateArray();
                        foreach (var hitter in hitters)
                        {
                            
                            db.Player_YearlyWar.Add(new Player_YearlyWar
                            {
                                MlbId = hitter.GetProperty("xMLBAMID").GetInt32(),
                                Year = year,
                                IsHitter = 1,
                                PA = (int)hitter.GetProperty("PA").GetDouble(),
                                WAR_h = (float)hitter.GetProperty("WAR").GetDouble(),
                                WAR_s = 0,
                                WAR_r = 0,
                                OFF = (float)hitter.GetProperty("Batting").GetDouble(),
                                DEF = (float)hitter.GetProperty("Defense").GetDouble(),
                                BSR = (float)hitter.GetProperty("BaseRunning").GetDouble(),
                                REP = (float)hitter.GetProperty("Replacement").GetDouble(),
                            });
                        }

                        db.SaveChanges();
                        progressBar.Tick();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in FangraphsData");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
