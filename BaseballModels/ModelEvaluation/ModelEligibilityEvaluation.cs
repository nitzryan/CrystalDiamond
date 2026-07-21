using Db;
using System.Globalization;

namespace ModelEvaluation
{
    public class ModelEligibilityEvaluation
    {
        internal static class AnalysisConfig
        {
            public const double AGE_BAND_WIDTH = 1.0;
            public const int MIN_SIGNING_YEAR = 2008;
        }

        internal readonly record struct BucketKey(
            int SigningYear,
            int AgeBand,
            DbEnums.ProspectType ProspectType,
            PlayerType PlayerType);


        internal enum PlayerType
        {
            Hitter,
            Pitcher
        }

        private static List<Model_Players> LoadModelPlayers(int minSigningYear)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            int maxSigningYear = db.Model_Players
                .Where(mp => mp.IsEligible)
        .       Max(mp => mp.SigningYear);

            return db.Model_Players
                .Where(mp => mp.SigningYear >= minSigningYear && mp.SigningYear <= maxSigningYear)
                .Where(mp => !db.Player_CareerStatus
                    .Any(pcs => pcs.MlbId == mp.MlbId && pcs.IgnorePlayer > 0))
                .ToList();
        }

        private static List<PlayerType> GetApplicablePlayerTypes(Model_Players player)
        {
            List<PlayerType> types = new();

            if (player.IsHitter)
            {
                types.Add(PlayerType.Hitter);
            }

            if (player.IsPitcher)
            {
                types.Add(PlayerType.Pitcher);
            }

            if (types.Count == 0)
            {
                throw new InvalidDataException(
                    $"Player {player.MlbId} has neither IsHitter nor IsPitcher set.");
            }

            return types;
        }

        private static BucketKey GetBucketKey(Model_Players player, PlayerType playerType)
        {
            int ageBand = (int)(player.AgeAtSigningYear / AnalysisConfig.AGE_BAND_WIDTH);
            return new BucketKey(
                player.SigningYear,
                ageBand,
                player.ProspectType,
                playerType);
        }

        private static Dictionary<BucketKey, List<Model_Players>> BuildDistribution(
            List<Model_Players> players)
        {
            Dictionary<BucketKey, List<Model_Players>> buckets = new();
            foreach (Model_Players player in players)
            {
                foreach (PlayerType playerType in GetApplicablePlayerTypes(player))
                {
                    BucketKey key = GetBucketKey(player, playerType);
                    if (!buckets.TryGetValue(key, out List<Model_Players>? bucket))
                    {
                        bucket = new List<Model_Players>();
                        buckets[key] = bucket;
                    }
                    bucket.Add(player);
                }
            }
            return buckets;
        }

        internal sealed class BucketResult
        {
            public BucketKey Key { get; init; }
            public int Total { get; init; }
            public int TotalEligible { get; init; }
            public int TotalNonEligible { get; init; }
            public double PercentageEligible { get; init; }
            public double AvgWarEligible { get; init; }
            public double AvgWarNonEligible { get; init; }
            public bool IsSingleCategory { get; init; }
            public int? IneligiblePlayerMlbId { get; init; }
        }

        private static double AverageWar(List<Model_Players> players, PlayerType playerType)
        {
            if (players.Count == 0)
            {
                return -100.0;
            }
            return playerType == PlayerType.Hitter
                ? players.Average(p => p.WarHitter)
                : players.Average(p => p.WarPitcher);
        }

        private static List<BucketResult> AnalyzeImbalance(
            Dictionary<BucketKey, List<Model_Players>> buckets)
        {
            List<BucketResult> results = new();
            foreach ((BucketKey key, List<Model_Players> members) in buckets)
            {
                List<Model_Players> eligible = members.Where(p => p.IsEligible).ToList();
                List<Model_Players> nonEligible = members.Where(p => !p.IsEligible).ToList();
                results.Add(new BucketResult
                {
                    Key = key,
                    Total = members.Count,
                    TotalEligible = eligible.Count,
                    TotalNonEligible = nonEligible.Count,
                    PercentageEligible = (double)eligible.Count / members.Count * 100.0,
                    AvgWarEligible = AverageWar(eligible, key.PlayerType),
                    AvgWarNonEligible = AverageWar(nonEligible, key.PlayerType),
                    IsSingleCategory = eligible.Count == 0 || nonEligible.Count == 0,
                    IneligiblePlayerMlbId = (eligible.Count > 0 && nonEligible.Count > 0)
                        ? nonEligible[0].MlbId
                        : null
                });
            }
            return results;
        }

        private static string FormatRow(BucketResult row)
        {
            double signingAge = row.Key.AgeBand * AnalysisConfig.AGE_BAND_WIDTH;

            return string.Join(",",
                row.Key.ProspectType,
                row.Key.SigningYear,
                signingAge.ToString("F1", CultureInfo.InvariantCulture),
                row.Key.PlayerType,
                row.Total,
                row.TotalEligible,
                row.TotalNonEligible,
                row.PercentageEligible.ToString("F2", CultureInfo.InvariantCulture),
                row.AvgWarEligible.ToString("F2", CultureInfo.InvariantCulture),
                row.AvgWarNonEligible.ToString("F2", CultureInfo.InvariantCulture),
                row.IsSingleCategory,
                row.IneligiblePlayerMlbId?.ToString() ?? "");
        }

        private static void ReportResults(List<BucketResult> results)
        {
            WriteReport(
                results.Where(r => r.Key.PlayerType == PlayerType.Hitter).ToList(),
                "Hitter",
                "reportHitter.csv");
            WriteReport(
                results.Where(r => r.Key.PlayerType == PlayerType.Pitcher).ToList(),
                "Pitcher",
                "reportPitcher.csv");
        }

        private static void WriteReport(List<BucketResult> results, string heading, string fileName)
        {
            const string header =
                "ProspectType,SigningYear,SigningAge,PlayerType,Total,EligibleCount," +
                "IneligibleCount,EligiblePercent,AvgWarEligible,AvgWarIneligible," +
                "AllSameEligibility,IneligiblePlayer";
            List<BucketResult> ordered = results
                .OrderBy(r => r.Key.ProspectType)
                .ThenBy(r => r.Key.SigningYear)
                .ThenBy(r => r.Key.AgeBand)
                .ToList();
            List<string> lines = new() { header };
            Console.WriteLine(heading);
            for (int i = 0; i < ordered.Count; i++)
            {
                BucketResult row = ordered[i];
                if (i > 0)
                {
                    BucketResult prev = ordered[i - 1];
                    if (row.Key.ProspectType != prev.Key.ProspectType)
                    {
                        lines.Add(string.Empty);
                        lines.Add(string.Empty);
                    }
                    else if (row.Key.SigningYear != prev.Key.SigningYear)
                    {
                        lines.Add(string.Empty);
                    }
                }
                lines.Add(FormatRow(row));
                if (!row.IsSingleCategory)
                {
                    double age = row.Key.AgeBand * AnalysisConfig.AGE_BAND_WIDTH;
                    Console.WriteLine(
                        $"ProspectType {row.Key.ProspectType}, {row.Key.SigningYear}, " +
                        $"Age {age:F1} -> Total: {row.Total}, Eligible: {row.PercentageEligible:F2}%");
                }
            }
            string outputDir = Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "Output", "EligibleVerification");
            Directory.CreateDirectory(outputDir);
            File.WriteAllLines(Path.Combine(outputDir, fileName), lines);
        }


        // For this code, the custom eligibility requirements need to be re-implemented in DataAquisition.Utilities.GetEligibilityMask
        // The report from this is used to set the boundaries used by that file to get an unbiased sample
        public static void Update()
        {
            List<Model_Players> players = LoadModelPlayers(AnalysisConfig.MIN_SIGNING_YEAR);
            Dictionary<BucketKey, List<Model_Players>> buckets = BuildDistribution(players);
            List<BucketResult> results = AnalyzeImbalance(buckets);
            ReportResults(results);
        }
    }
}
