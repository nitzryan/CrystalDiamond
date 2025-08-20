using Db;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SitePrep
{
    internal class OrgMap
    {
        public static bool Main()
        {
            try{
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Parents
                JsonObject json = new();
                JsonObject parents = new();
                foreach (var parent in db.Team_Parents)
                {
                    JsonObject el = new()
                    {
                        ["abbr"] = parent.Abbr,
                        ["name"] = parent.Name
                    };
                    parents.Add(Convert.ToString(parent.Id), el);
                }
                json.Add("parents", parents);

                // Children
                JsonObject children = new();
                var ids = db.Team_OrganizationMap.AsEnumerable().DistinctBy(f => f.TeamId).Select(f => f.TeamId);
                foreach (int id in ids)
                {
                    var tom = db.Team_OrganizationMap.Where(f => f.TeamId == id).OrderBy(f => f.Year);
                    JsonArray parentYears = [];
                    foreach (var t in tom)
                    {
                        parentYears.Add(new JsonObject { ["year"] = t.Year, ["parent"] = t.ParentOrgId });
                    }

                    JsonObject el = new()
                    {
                        ["parents"] = parentYears
                    };
                    children.Add(Convert.ToString(id), el);
                }
                json.Add("children", children);

                // Leagues
                JsonObject leagues = new();
                foreach (var league in db.Leagues)
                {
                    JsonObject el = new()
                    {
                        ["abbr"] = league.Abbr,
                        ["name"] = league.Name,
                    };
                    leagues.Add(Convert.ToString(league.Id), el);
                }
                json.Add("leagues", leagues);

                using var fileStream = new FileStream(Constants.SITE_ASSET_FOLDER + "map.json.gz", FileMode.Create);
                using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                using var writer = new Utf8JsonWriter(gzipStream, new JsonWriterOptions { Indented = false });
                JsonSerializer.Serialize(writer, json);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in TeamMap");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
