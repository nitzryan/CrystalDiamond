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
                var players = db.Model_Players
                    .Join(db.Player, mp => mp.MlbId, p => p.MlbId, (mp, p) => p)
                    .Join(db.Player_CareerStatus, p => p.MlbId, pcs => pcs.MlbId, (p, pcs) => new { p, pcs });

                JsonObject json = new();
                JsonArray arr = new();
                foreach (var playerObj in players)
                {
                    var player = playerObj.p;
                    var pcs = playerObj.pcs;

                    var result = db.Player_OrgMap.Where(f => f.MlbId == player.MlbId)
                            .OrderByDescending(f => f.Year)
                            .ThenByDescending(f => f.Month)
                            .ThenByDescending(f => f.Day)
                            .FirstOrDefault();

                    int status = 0;
                    if (pcs.MlbStartYear != null)
                        status = 2;
                    else if (pcs.AgedOut != null || pcs.PlayingGap != null)
                        status = 1;

                    if (result != null && result.ParentOrgId != 0)
                        status += 4;

                    JsonObject p = new()
                    {
                        ["id"] = player.MlbId,
                        ["b"] = player.BirthYear,
                        ["f"] = player.UseFirstName,
                        ["l"] = player.UseLastName,
                        ["o"] = result != null ? result.ParentOrgId : 0,
                        ["s"] = status, // used to rank results so more prevelant are shown first
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
