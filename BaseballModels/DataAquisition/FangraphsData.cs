using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System.Text.Json;

namespace DataAquisition
{
    internal class FangraphsData
    {
        private class Date {
            public required int Year { get; set; }
            public required int Month { get; set; }
        }

        private static float IPDoubleToFloat(double ip)
        {
            string ipString = Convert.ToString(ip);
            string[] parts = ipString.Split(".");

            int fullInnings = Convert.ToInt32(parts[0]);
            int partialInnings = parts.Length > 1 ? Convert.ToInt32(parts[1]) : 0;

            float innings = (float)fullInnings + (0.333333333f * partialInnings);
            return innings;
        }

        public static async Task<bool> Main(List<int> years)
        {
            try
            {
                HttpClient httpClient = new();
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                using (ProgressBar progressBar = new(years.Count(), "Getting fangraphs annual data"))
                {
                    foreach (int year in years)
                    {
                        // Don't redo previous years if they exist
                        if (db.Player_YearlyWar.Where(f => f.Year == year).Any() && year != years.Last())
                        {
                            progressBar.Tick();
                            continue;
                        }

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
                            id = f.GetProperty("xMLBAMID").GetInt32(),
                            war = f.GetProperty("WAR").GetDouble(),
                            tbf = (int)f.GetProperty("TBF").GetDouble(),
                            ip = IPDoubleToFloat(f.GetProperty("IP").GetDouble())
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
                            ip = IPDoubleToFloat(f.GetProperty("IP").GetDouble())
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
                                DRAA = 0,
                                DEF = 0,
                                BSR = 0,
                                REP = 0,
                                IP_SP = starterData.Sum(f => f.ip),
                                IP_RP = relieverData.Sum(f => f.ip),
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
                            float draa = 0;
                            if (hitter.TryGetProperty("Fielding", out var fieldElement) && fieldElement.ValueKind == JsonValueKind.Number)
                                draa = (float)fieldElement.GetDouble();

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
                                DRAA = draa,
                                DEF = (float)hitter.GetProperty("Defense").GetDouble(),
                                BSR = (float)hitter.GetProperty("BaseRunning").GetDouble(),
                                REP = (float)hitter.GetProperty("Replacement").GetDouble(),
                                IP_RP = 0,
                                IP_SP = 0,
                            });
                        }

                        db.SaveChanges();
                        progressBar.Tick();
                    }
                }

                List<Date> dates = new();
                List<int> months = [4, 5, 6, 7, 8, 9];
                foreach (var year in years)
                    foreach (var month in months)
                        dates.Add(new Date { Year = year, Month = month });

                using (ProgressBar progressBar = new(dates.Count(), "Getting fangraphs monthly data"))
                {
                    foreach (var date in dates)
                    {
                        int year = date.Year;
                        int month = date.Month;
                        // Don't redo previous years if they exist
                        if (db.Player_MonthlyWar.Where(f => f.Year == year && f.Month == month).Any() && year != years.Last())
                        {
                            progressBar.Tick();
                            continue;
                        }

                        db.Player_MonthlyWar.RemoveRange(db.Player_MonthlyWar.Where(f => f.Year == year && f.Month == month));
                        db.SaveChanges();

                        List<Player_MonthlyWar> playerMonthlyWarList = new();

                        // Get starting pitcher data
                        HttpResponseMessage response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=pit&stats=sta&lg=all&qual=0&season={year}&month={month}&pageitems=10000");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting Starting Pitchers Fangraphs stats: {response.StatusCode}");
                        }
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JsonDocument json = JsonDocument.Parse(responseBody);
                        
                        var startingPitchers = json.RootElement.GetProperty("data").EnumerateArray().Select(f => new
                        {
                            id = f.GetProperty("xMLBAMID").GetInt32(),
                            war = f.GetProperty("WAR").GetDouble(),
                            tbf = (int)f.GetProperty("TBF").GetDouble(),
                            ip = IPDoubleToFloat(f.GetProperty("IP").GetDouble())
                        });

                        // Get relief pitching data
                        response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=pit&stats=rel&lg=all&qual=0&season={year}&month={month}&pageitems=10000");
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
                            ip = IPDoubleToFloat(f.GetProperty("IP").GetDouble())
                        });

                        // Aggregate Ids
                        var pitchersIds = startingPitchers.Select(f => f.id).ToList();
                        pitchersIds.AddRange(reliefPitchers.Select(f => f.id));
                        pitchersIds = pitchersIds.Distinct().ToList();

                        // Create stats for each pitcher
                        foreach (var id in pitchersIds)
                        {
                            var starterData = startingPitchers.Where(f => f.id == id);
                            var relieverData = reliefPitchers.Where(f => f.id == id);
                            playerMonthlyWarList.Add(new Player_MonthlyWar
                            {
                                MlbId = id,
                                Year = year,
                                Month = month,
                                PA = starterData.Sum(f => f.tbf) + relieverData.Sum(f => f.tbf),
                                WAR_h = 0,
                                WAR_s = starterData.Any() ? (float)starterData.First().war : 0,
                                WAR_r = relieverData.Any() ? (float)relieverData.First().war : 0,
                                OFF = 0,
                                DRAA = 0,
                                DEF = 0,
                                BSR = 0,
                                REP = 0,
                                IP_SP = starterData.Sum(f => f.ip),
                                IP_RP = relieverData.Sum(f => f.ip),
                            });
                        }


                        // Get hitter data
                        response = await httpClient.GetAsync($"https://www.fangraphs.com/api/leaders/major-league/data?pos=np&stats=bat&lg=all&qual=0&season={year}&month={month}&pageitems=10000");
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception($"Getting Starting Pitchers Fangraphs stats: {response.StatusCode}");
                        }
                        responseBody = await response.Content.ReadAsStringAsync();
                        json = JsonDocument.Parse(responseBody);
                        var hitters = json.RootElement.GetProperty("data").EnumerateArray();

                        foreach (var hitter in hitters)
                        {
                            float draa = 0;
                            if (hitter.TryGetProperty("Fielding", out var fieldElement) && fieldElement.ValueKind == JsonValueKind.Number)
                                draa = (float)fieldElement.GetDouble();

                            // Check if player already has pitcher stats
                            int hitterId = hitter.GetProperty("xMLBAMID").GetInt32();
                            if (playerMonthlyWarList.Any(f => f.MlbId == hitterId)) 
                            { // Modify if exists
                                var pmw = playerMonthlyWarList.Single(f => f.MlbId == hitterId);
                                pmw.PA = (int)hitter.GetProperty("PA").GetDouble();
                                pmw.WAR_h = (float)hitter.GetProperty("WAR").GetDouble();
                                pmw.OFF = (float)hitter.GetProperty("Batting").GetDouble();
                                pmw.DRAA = draa;
                                pmw.DEF = (float)hitter.GetProperty("Defense").GetDouble();
                                pmw.BSR = (float)hitter.GetProperty("BaseRunning").GetDouble();
                                pmw.REP = (float)hitter.GetProperty("Replacement").GetDouble();
                            } else 
                            { // Create if doesn't
                                playerMonthlyWarList.Add(new Player_MonthlyWar
                                {
                                    MlbId = hitterId,
                                    Year = year,
                                    Month = month,
                                    PA = (int)hitter.GetProperty("PA").GetDouble(),
                                    WAR_h = (float)hitter.GetProperty("WAR").GetDouble(),
                                    WAR_s = 0,
                                    WAR_r = 0,
                                    OFF = (float)hitter.GetProperty("Batting").GetDouble(),
                                    DRAA = draa,
                                    DEF = (float)hitter.GetProperty("Defense").GetDouble(),
                                    BSR = (float)hitter.GetProperty("BaseRunning").GetDouble(),
                                    REP = (float)hitter.GetProperty("Replacement").GetDouble(),
                                    IP_SP = 0,
                                    IP_RP = 0,
                                });
                            }
                        }

                        db.Player_MonthlyWar.AddRange(playerMonthlyWarList);
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
