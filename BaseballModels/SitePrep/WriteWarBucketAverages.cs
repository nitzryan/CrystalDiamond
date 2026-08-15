using ModelDb;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SitePrep
{
    internal class WriteWarBucketAverages
    {
        public static void Update()
        {
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Get Values
            WarBucketAverages hb = modelDb.WarBucketAverages
                .Where(f => f.IsHitter).Single();
            WarBucketAverages pb = modelDb.WarBucketAverages
                .Where(f => !f.IsHitter).Single();

            // Write Json
            JsonObject obj = new();
            float[] hitterValues = {0, hb.War1, hb.War2, hb.War3, hb.War4, hb.War5, hb.War6};
            float[] pitcherValues = {0, pb.War1, pb.War2, pb.War3, pb.War4, pb.War5, pb.War6};

            obj["hitter"] = JsonSerializer.SerializeToNode(hitterValues);
            obj["pitcher"] = JsonSerializer.SerializeToNode(pitcherValues);

            // Write to file
            using var fileStream = new FileStream(Constants.SITE_ASSET_FOLDER + "warbuckets.json.gz", FileMode.Create);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new Utf8JsonWriter(gzipStream, new JsonWriterOptions { Indented = false });
            JsonSerializer.Serialize(writer, obj);
        }
    }
}
