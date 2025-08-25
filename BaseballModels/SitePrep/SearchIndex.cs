using Db;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SitePrep
{
    internal class SearchIndex
    {
        public static bool Main()
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                var players = db.Model_Players.Where(f => f.IsHitter == 1)
                    .Join(db.Player, mp => mp.MlbId, p => p.MlbId, (mp, p) => p);

                JsonObject json = new();
                JsonArray arr = new();
                foreach (var player in players)
                {
                    JsonObject p = new()
                    {
                        ["id"] = player.MlbId,
                        ["f"] = player.UseFirstName,
                        ["l"] = player.UseLastName,
                    };
                    arr.Add(p);
                }
                json.Add("players", arr);

                using var fileStream = new FileStream(Constants.SITE_ASSET_FOLDER + "player_search.json.gz", FileMode.Create);
                using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                using var writer = new Utf8JsonWriter(gzipStream, new JsonWriterOptions { Indented = false });
                JsonSerializer.Serialize(writer, json);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in HitterPage");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
