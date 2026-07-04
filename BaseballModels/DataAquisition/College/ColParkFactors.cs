using Db;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace DataAquisition.College
{
    internal class ColParkFactors
    {
        public async static Task<bool> GetParkFactors(int year)
        {
            // Remove old data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_ParkFactors.Where(f => f.Year == year).ExecuteDelete();

            List<College_ParkFactors> parkFactors = new();
            // Covid year caused no data for these years
            if (year >= 2020 && year <= 2023)
            {
                var data2019 = db.College_ParkFactors.Where(f => f.Year == 2019);
                foreach (var d in data2019)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = d.TeamId,
                        Year = year,
                        RunFactor = d.RunFactor,
                    });
                }

                db.College_ParkFactors.AddRange(parkFactors);
                return true;
            }

            // Currently doesn't have a 2025 park factors, fallback to 2024
            if (year >= 2025)
            {
                var data2024 = db.College_ParkFactors.Where(f => f.Year == 2024);
                db.College_ParkFactors.Where(f => f.Year == 2019);
                foreach (var d in data2024)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = d.TeamId,
                        Year = year,
                        RunFactor = d.RunFactor,
                    });
                }

                db.College_ParkFactors.AddRange(parkFactors);
                return true;
            }

            // Make GET request
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36"
    );
            int reqYear = year == 2002 ? 2003 : year; // Different req for 2002
            string url = $"http://www.boydsworld.com/data/pf{reqYear}.html";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting College Park Factors for Year={year}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();

            // Get <pre> contents
            var doc = new HtmlDocument();
            doc.LoadHtml(responseBody);
            var preNode = doc.DocumentNode.SelectSingleNode("//pre") ?? throw new Exception($"Failed to find <pre> for College Park Factors for Year={year}");
            string contents = preNode.InnerText;

            // Read through line by line
            List<(int id, float pf)> teamParkFactors = new();

            var lines = contents.Split(new[] { '\r', '\n' });
            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int parkFactor))
                    {
                        // Everything after the first two numbers is the team name
                        string boydTeamName = string.Join(" ", parts, 2, parts.Length - 2);

                        // Boyd has some duplicates (multiple names for same team)(
                        if (ColUtilities.BoydYearSkip(boydTeamName, year))
                            continue;

                        string teamName = ColUtilities.BoydToBaseballCubeTeamName(boydTeamName);

                        try
                        {
                            int teamId = db.College_TeamMap.Where(f => f.Name.Equals(teamName)).Single().TeamId;

                            teamParkFactors.Add((teamId, parkFactor));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Skipped Park Factors for {teamName}");
                        }
                    }
                }
            }

            // Get all teams in a conference for a given year
            var conferenceTeams = db.College_HitterStats.Where(f => f.Year == year)
                .Select(f => new { f.TeamId, f.ConfId })
                .Distinct()
                .GroupBy(f => f.ConfId, f => f.TeamId);

            foreach (var conf in conferenceTeams)
            {
                // Get mean PF for conference
                var confParkFactors = teamParkFactors.Where(f => conf.Contains(f.id));
                float pfPerTeam = confParkFactors.Sum(f => f.pf) / confParkFactors.Count();

                // Add conf-adjusted park factor
                foreach (var team in confParkFactors)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = team.id,
                        Year = year,
                        RunFactor = team.pf / pfPerTeam,
                    });
                }
            }

            db.College_ParkFactors.AddRange(parkFactors);
            db.SaveChanges();

            return true;
        }
    }
}
