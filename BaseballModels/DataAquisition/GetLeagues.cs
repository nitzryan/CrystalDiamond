using Db;
using System.Text.Json;

namespace DataAquisition
{
    internal class GetLeagues
    {
        public static async Task<bool> Main()
        {
            try {
                HttpClient httpClient = new();
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Leagues.RemoveRange(db.Leagues);
                db.SaveChanges();

                HttpResponseMessage response = await httpClient.GetAsync("https://statsapi.mlb.com/api/v1/leagues");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"HTTP Error for leagues: {response.StatusCode}");
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(responseBody);
                var leagues = json.RootElement.GetProperty("leagues").EnumerateArray();

                foreach (var league in leagues)
                {
                    db.Leagues.Add(new Leagues
                    {
                        Id = league.GetProperty("id").GetInt32(),
                        Abbr = league.GetProperty("abbreviation").GetString() ?? throw new Exception("abbreviation was not a string"),
                        Name = league.GetProperty("name").GetString() ?? throw new Exception("name was not a string")
                    });
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Leagues");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
