using Db;
using System.Text.Json;

namespace DataAquisition
{
    internal class UpdateParents
    {
        public static async Task<bool> Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Update minor league parents
                db.Team_OrganizationMap.RemoveRange(
                    db.Team_OrganizationMap.Where(f => f.Year == year)
                );
                db.SaveChanges();

                HttpClient httpClient = new();
                foreach (var sportId in Constants.SPORT_IDS)
                {
                    if (sportId == 1) // SportId 1 is majors, which are the parents
                        continue;
                    if (sportId == 17) // Not a sportId in MLBs system
                        continue;

                    HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/teams?sportIds={sportId}&season={year}");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Getting Org Maps for {year}: {response.StatusCode}");
                    }

                    // Go Through teams
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument json = JsonDocument.Parse(responseBody);
                    if (!json.RootElement.TryGetProperty("teams", out JsonElement teams)) // Not all levels have teams for all years
                        continue;

                    var teamsArray = teams.EnumerateArray();
                    foreach (var team in teamsArray)
                    {
                        int teamId = team.GetProperty("id").GetInt32();
                        try
                        {
                            int parentId = team.GetProperty("parentOrgId").GetInt32();
                            db.Team_OrganizationMap.Add(new Team_OrganizationMap
                            {
                                TeamId = teamId,
                                ParentOrgId = parentId,
                                Year = year
                            });
                        }
                        catch (Exception e) { } // Some VSL teams were multiple orgs, so no parentId
                    }
                }
                db.SaveChanges();

                // Insert parents
                if (db.Team_Parents.Any())
                    return true;

                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://statsapi.mlb.com/api/v1/teams?sportIds=1");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Getting Team Parents: {response.StatusCode}");
                    }

                    // Go Through teams
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument json = JsonDocument.Parse(responseBody);
                    var teamsArray = json.RootElement.GetProperty("teams").EnumerateArray();
                    foreach (var team in teamsArray)
                    {
                        int teamId = team.GetProperty("id").GetInt32();
                        string name = team.GetProperty("name").GetString() ?? throw new Exception($"Failed to get property 'name' for {teamId}");
                        string abbr = team.GetProperty("abbreviation").GetString() ?? throw new Exception($"Failed to get property 'abbreviation' for {teamId}");
                        db.Team_Parents.Add(new Team_Parents
                        {
                            Id = teamId,
                            Name = name,
                            Abbr = abbr
                        });
                    }
                    db.SaveChanges();
                }
                

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in UpdateParents");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }
        }
    }
}
