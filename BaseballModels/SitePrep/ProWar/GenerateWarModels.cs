using Microsoft.EntityFrameworkCore;
using ModelDb;
using ShellProgressBar;
using SiteDb;
using System.Buffers.Binary;

namespace SitePrep.ProWar
{
    internal class GenerateWarModels
    {
        private record WarModelData(int MlbId, int ModelId, int Year, int Month, float[][] Probs, float[] War)
        {
            public (int MlbId, int ModelId, int Year, int Month) Key => (MlbId, ModelId, Year, Month);
        }

        private const int NUM_YEARS = 7;
        private const int NUM_BUCKETS = 8;
        private const int YEAR_CUTOFF = 2015; // Only aggregation rows with Year > YEAR_CUTOFF are saved
        private const int BATCH_SIZE = 10000;

        private static WarModelData HitterData(Output_HitterMlbWarAggregation f)
        {
            return new WarModelData(f.MlbId, f.ModelId, f.Year, f.Month,
                new float[][]
                {
                new[] { f.War_Yr0_Bkt0, f.War_Yr0_Bkt1, f.War_Yr0_Bkt2, f.War_Yr0_Bkt3, f.War_Yr0_Bkt4, f.War_Yr0_Bkt5, f.War_Yr0_Bkt6, f.War_Yr0_Bkt7 },
                new[] { f.War_Yr1_Bkt0, f.War_Yr1_Bkt1, f.War_Yr1_Bkt2, f.War_Yr1_Bkt3, f.War_Yr1_Bkt4, f.War_Yr1_Bkt5, f.War_Yr1_Bkt6, f.War_Yr1_Bkt7 },
                new[] { f.War_Yr2_Bkt0, f.War_Yr2_Bkt1, f.War_Yr2_Bkt2, f.War_Yr2_Bkt3, f.War_Yr2_Bkt4, f.War_Yr2_Bkt5, f.War_Yr2_Bkt6, f.War_Yr2_Bkt7 },
                new[] { f.War_Yr3_Bkt0, f.War_Yr3_Bkt1, f.War_Yr3_Bkt2, f.War_Yr3_Bkt3, f.War_Yr3_Bkt4, f.War_Yr3_Bkt5, f.War_Yr3_Bkt6, f.War_Yr3_Bkt7 },
                new[] { f.War_Yr4_Bkt0, f.War_Yr4_Bkt1, f.War_Yr4_Bkt2, f.War_Yr4_Bkt3, f.War_Yr4_Bkt4, f.War_Yr4_Bkt5, f.War_Yr4_Bkt6, f.War_Yr4_Bkt7 },
                new[] { f.War_Yr5_Bkt0, f.War_Yr5_Bkt1, f.War_Yr5_Bkt2, f.War_Yr5_Bkt3, f.War_Yr5_Bkt4, f.War_Yr5_Bkt5, f.War_Yr5_Bkt6, f.War_Yr5_Bkt7 },
                new[] { f.War_Yr6_Bkt0, f.War_Yr6_Bkt1, f.War_Yr6_Bkt2, f.War_Yr6_Bkt3, f.War_Yr6_Bkt4, f.War_Yr6_Bkt5, f.War_Yr6_Bkt6, f.War_Yr6_Bkt7 },
                },
                new[] { f.War0, f.War1, f.War2, f.War3, f.War4, f.War5, f.War6 });
        }

        private static WarModelData PitcherSPData(Output_PitcherMlbWarAggregation f)
        {
            return new WarModelData(f.MlbId, f.ModelId, f.Year, f.Month,
                new float[][]
                {
                new[] { f.WarSP_Yr0_Bkt0, f.WarSP_Yr0_Bkt1, f.WarSP_Yr0_Bkt2, f.WarSP_Yr0_Bkt3, f.WarSP_Yr0_Bkt4, f.WarSP_Yr0_Bkt5, f.WarSP_Yr0_Bkt6, f.WarSP_Yr0_Bkt7 },
                new[] { f.WarSP_Yr1_Bkt0, f.WarSP_Yr1_Bkt1, f.WarSP_Yr1_Bkt2, f.WarSP_Yr1_Bkt3, f.WarSP_Yr1_Bkt4, f.WarSP_Yr1_Bkt5, f.WarSP_Yr1_Bkt6, f.WarSP_Yr1_Bkt7 },
                new[] { f.WarSP_Yr2_Bkt0, f.WarSP_Yr2_Bkt1, f.WarSP_Yr2_Bkt2, f.WarSP_Yr2_Bkt3, f.WarSP_Yr2_Bkt4, f.WarSP_Yr2_Bkt5, f.WarSP_Yr2_Bkt6, f.WarSP_Yr2_Bkt7 },
                new[] { f.WarSP_Yr3_Bkt0, f.WarSP_Yr3_Bkt1, f.WarSP_Yr3_Bkt2, f.WarSP_Yr3_Bkt3, f.WarSP_Yr3_Bkt4, f.WarSP_Yr3_Bkt5, f.WarSP_Yr3_Bkt6, f.WarSP_Yr3_Bkt7 },
                new[] { f.WarSP_Yr4_Bkt0, f.WarSP_Yr4_Bkt1, f.WarSP_Yr4_Bkt2, f.WarSP_Yr4_Bkt3, f.WarSP_Yr4_Bkt4, f.WarSP_Yr4_Bkt5, f.WarSP_Yr4_Bkt6, f.WarSP_Yr4_Bkt7 },
                new[] { f.WarSP_Yr5_Bkt0, f.WarSP_Yr5_Bkt1, f.WarSP_Yr5_Bkt2, f.WarSP_Yr5_Bkt3, f.WarSP_Yr5_Bkt4, f.WarSP_Yr5_Bkt5, f.WarSP_Yr5_Bkt6, f.WarSP_Yr5_Bkt7 },
                new[] { f.WarSP_Yr6_Bkt0, f.WarSP_Yr6_Bkt1, f.WarSP_Yr6_Bkt2, f.WarSP_Yr6_Bkt3, f.WarSP_Yr6_Bkt4, f.WarSP_Yr6_Bkt5, f.WarSP_Yr6_Bkt6, f.WarSP_Yr6_Bkt7 },
                },
                new[] { f.WarSP0, f.WarSP1, f.WarSP2, f.WarSP3, f.WarSP4, f.WarSP5, f.WarSP6 });
        }

        private static WarModelData PitcherRPData(Output_PitcherMlbWarAggregation f)
        {
            return new WarModelData(f.MlbId, f.ModelId, f.Year, f.Month,
                new float[][]
                {
                new[] { f.WarRP_Yr0_Bkt0, f.WarRP_Yr0_Bkt1, f.WarRP_Yr0_Bkt2, f.WarRP_Yr0_Bkt3, f.WarRP_Yr0_Bkt4, f.WarRP_Yr0_Bkt5, f.WarRP_Yr0_Bkt6, f.WarRP_Yr0_Bkt7 },
                new[] { f.WarRP_Yr1_Bkt0, f.WarRP_Yr1_Bkt1, f.WarRP_Yr1_Bkt2, f.WarRP_Yr1_Bkt3, f.WarRP_Yr1_Bkt4, f.WarRP_Yr1_Bkt5, f.WarRP_Yr1_Bkt6, f.WarRP_Yr1_Bkt7 },
                new[] { f.WarRP_Yr2_Bkt0, f.WarRP_Yr2_Bkt1, f.WarRP_Yr2_Bkt2, f.WarRP_Yr2_Bkt3, f.WarRP_Yr2_Bkt4, f.WarRP_Yr2_Bkt5, f.WarRP_Yr2_Bkt6, f.WarRP_Yr2_Bkt7 },
                new[] { f.WarRP_Yr3_Bkt0, f.WarRP_Yr3_Bkt1, f.WarRP_Yr3_Bkt2, f.WarRP_Yr3_Bkt3, f.WarRP_Yr3_Bkt4, f.WarRP_Yr3_Bkt5, f.WarRP_Yr3_Bkt6, f.WarRP_Yr3_Bkt7 },
                new[] { f.WarRP_Yr4_Bkt0, f.WarRP_Yr4_Bkt1, f.WarRP_Yr4_Bkt2, f.WarRP_Yr4_Bkt3, f.WarRP_Yr4_Bkt4, f.WarRP_Yr4_Bkt5, f.WarRP_Yr4_Bkt6, f.WarRP_Yr4_Bkt7 },
                new[] { f.WarRP_Yr5_Bkt0, f.WarRP_Yr5_Bkt1, f.WarRP_Yr5_Bkt2, f.WarRP_Yr5_Bkt3, f.WarRP_Yr5_Bkt4, f.WarRP_Yr5_Bkt5, f.WarRP_Yr5_Bkt6, f.WarRP_Yr5_Bkt7 },
                new[] { f.WarRP_Yr6_Bkt0, f.WarRP_Yr6_Bkt1, f.WarRP_Yr6_Bkt2, f.WarRP_Yr6_Bkt3, f.WarRP_Yr6_Bkt4, f.WarRP_Yr6_Bkt5, f.WarRP_Yr6_Bkt6, f.WarRP_Yr6_Bkt7 },
                },
                new[] { f.WarRP0, f.WarRP1, f.WarRP2, f.WarRP3, f.WarRP4, f.WarRP5, f.WarRP6 });
        }

        private static byte[] PackProbs(float[][] probs)
        {
            if (probs.Length != NUM_YEARS)
                throw new Exception($"PackProbs: expected {NUM_YEARS} years, got {probs.Length}");

            byte[] blob = new byte[NUM_YEARS * NUM_BUCKETS * 2];
            for (int yr = 0; yr < NUM_YEARS; yr++)
            {
                if (probs[yr].Length != NUM_BUCKETS)
                    throw new Exception($"PackProbs: expected {NUM_BUCKETS} buckets in year {yr}, got {probs[yr].Length}");

                for (int bkt = 0; bkt < NUM_BUCKETS; bkt++)
                {
                    float p = probs[yr][bkt];
                    if (float.IsNaN(p))
                        throw new Exception($"PackProbs: NaN probability at year {yr}, bucket {bkt}");

                    ushort q = (ushort)MathF.Round(Math.Clamp(p, 0f, 1f) * 65535f);
                    BinaryPrimitives.WriteUInt16LittleEndian(blob.AsSpan((yr * NUM_BUCKETS + bkt) * 2), q);
                }
            }
            return blob;
        }

        private static byte[] PackRanks(ushort[] ranks)
        {
            if (ranks.Length != NUM_YEARS)
                throw new Exception($"PackRanks: expected {NUM_YEARS} ranks, got {ranks.Length}");

            byte[] blob = new byte[NUM_YEARS * 2];
            for (int yr = 0; yr < NUM_YEARS; yr++)
                BinaryPrimitives.WriteUInt16LittleEndian(blob.AsSpan(yr * 2), ranks[yr]);
            return blob;
        }

        private static Dictionary<(int MlbId, int ModelId, int Year, int Month), ushort[]> ComputeRanks(List<WarModelData> rows)
        {
            var ranks = rows.ToDictionary(f => f.Key, f => new ushort[NUM_YEARS]);

            foreach (var group in rows.GroupBy(f => (f.ModelId, f.Year, f.Month)))
            {
                List<WarModelData> members = group.ToList();
                if (members.Count > ushort.MaxValue)
                    throw new Exception($"ComputeRanks: {members.Count} players in model {group.Key.ModelId} for {group.Key.Month}-{group.Key.Year} do not fit in uint16");

                for (int yr = 0; yr < NUM_YEARS; yr++)
                {
                    ushort rank = 1;
                    foreach (WarModelData row in members.OrderByDescending(f => f.War[yr]).ThenBy(f => f.MlbId))
                    {
                        ranks[row.Key][yr] = rank;
                        rank++;
                    }
                }
            }

            return ranks;
        }

        private static void CheckBucketMonths(IEnumerable<int> timestepMonths, IEnumerable<int> averageMonths, string tableName)
        {
            HashSet<int> available = averageMonths.ToHashSet();
            List<int> missing = timestepMonths
                .Where(m => m != 9)
                .Append(0)
                .Distinct()
                .Where(m => !available.Contains(m))
                .OrderBy(m => m)
                .ToList();

            if (missing.Count > 0)
                throw new Exception($"{tableName} has no row for month(s) {string.Join(",", missing)}");
        }

        public static void HitterModels()
        {
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Bucket averages: rewritten and committed immediately
            siteDb.HitterMlbWarBucketAverages.ExecuteDelete();
            var averages = modelDb.SingleYearHitterBucketAverages.AsNoTracking().ToList();
            foreach (var avg in averages)
            {
                siteDb.Add(new HitterMlbWarBucketAverages
                {
                    Month = avg.Month > 0 ? avg.Month : 1, // 0 hits AUTOINCREMENT
                    War0 = avg.War0,
                    War1 = avg.War1,
                    War2 = avg.War2,
                    War3 = avg.War3,
                    War4 = avg.War4,
                    War5 = avg.War5,
                    War6 = avg.War6,
                    War7 = avg.War7,
                });
            }
            siteDb.SaveChanges();

            // Load model data in primary key order
            List<WarModelData> rows = modelDb.Output_HitterMlbWarAggregation
                .AsNoTracking()
                .Where(f => f.Year > YEAR_CUTOFF)
                .OrderBy(f => f.MlbId).ThenBy(f => f.ModelId).ThenBy(f => f.Year).ThenBy(f => f.Month)
                .AsEnumerable()
                .Select(HitterData)
                .ToList();

            CheckBucketMonths(rows.Select(f => f.Month), averages.Select(f => f.Month), "SingleYearHitterBucketAverages");
            var ranks = ComputeRanks(rows);

            // Model table: emptied now, then filled in one transaction so a failure leaves it empty
            siteDb.HitterMlbWarModel.ExecuteDelete();
            using var transaction = siteDb.Database.BeginTransaction();
            using (ProgressBar progressBar = new ProgressBar(rows.Count, "Generating Hitter MLB WAR Models"))
            {
                int pending = 0;
                foreach (WarModelData row in rows)
                {
                    siteDb.Add(new HitterMlbWarModel
                    {
                        MlbId = row.MlbId,
                        ModelId = row.ModelId,
                        Year = row.Year,
                        Month = row.Month,
                        ProbsWar = PackProbs(row.Probs),
                        RanksWar = PackRanks(ranks[row.Key]),
                    });

                    pending++;
                    if (pending == BATCH_SIZE)
                    {
                        siteDb.SaveChanges();
                        siteDb.ChangeTracker.Clear();
                        pending = 0;
                    }
                    progressBar.Tick();
                }
            }
            siteDb.SaveChanges();
            transaction.Commit();
        }

        public static void PitcherModels()
        {
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Bucket averages: rewritten and committed immediately
            siteDb.PitcherMlbWarBucketAverages.ExecuteDelete();
            var averages = modelDb.SingleYearPitcherBucketAverages.AsNoTracking().ToList();
            foreach (var avg in averages)
            {
                siteDb.Add(new PitcherMlbWarBucketAverages
                {
                    Month = avg.Month > 0 ? avg.Month : 1, // 0 hits AUTOINCREMENT
                    WarSP0 = avg.WarSP0,
                    WarSP1 = avg.WarSP1,
                    WarSP2 = avg.WarSP2,
                    WarSP3 = avg.WarSP3,
                    WarSP4 = avg.WarSP4,
                    WarSP5 = avg.WarSP5,
                    WarSP6 = avg.WarSP6,
                    WarSP7 = avg.WarSP7,
                    WarRP0 = avg.WarRP0,
                    WarRP1 = avg.WarRP1,
                    WarRP2 = avg.WarRP2,
                    WarRP3 = avg.WarRP3,
                    WarRP4 = avg.WarRP4,
                    WarRP5 = avg.WarRP5,
                    WarRP6 = avg.WarRP6,
                    WarRP7 = avg.WarRP7,
                });
            }
            siteDb.SaveChanges();

            // Load model data in primary key order, split into SP and RP records
            List<WarModelData> spRows = new();
            List<WarModelData> rpRows = new();
            var aggregation = modelDb.Output_PitcherMlbWarAggregation
                .AsNoTracking()
                .Where(f => f.Year > YEAR_CUTOFF)
                .OrderBy(f => f.MlbId).ThenBy(f => f.ModelId).ThenBy(f => f.Year).ThenBy(f => f.Month);
            foreach (var f in aggregation)
            {
                spRows.Add(PitcherSPData(f));
                rpRows.Add(PitcherRPData(f));
            }

            CheckBucketMonths(spRows.Select(f => f.Month), averages.Select(f => f.Month), "SingleYearPitcherBucketAverages");
            var ranksSP = ComputeRanks(spRows);
            var ranksRP = ComputeRanks(rpRows);

            // Model table: emptied now, then filled in one transaction so a failure leaves it empty
            siteDb.PitcherMlbWarModel.ExecuteDelete();
            using var transaction = siteDb.Database.BeginTransaction();
            using (ProgressBar progressBar = new ProgressBar(spRows.Count, "Generating Pitcher MLB WAR Models"))
            {
                int pending = 0;
                for (int i = 0; i < spRows.Count; i++)
                {
                    WarModelData sp = spRows[i];
                    WarModelData rp = rpRows[i];
                    if (sp.Key != rp.Key)
                        throw new Exception($"PitcherModels: SP/RP row mismatch at index {i}");

                    siteDb.Add(new PitcherMlbWarModel
                    {
                        MlbId = sp.MlbId,
                        ModelId = sp.ModelId,
                        Year = sp.Year,
                        Month = sp.Month,
                        ProbsWarSP = PackProbs(sp.Probs),
                        ProbsWarRP = PackProbs(rp.Probs),
                        RanksWarSP = PackRanks(ranksSP[sp.Key]),
                        RanksWarRP = PackRanks(ranksRP[rp.Key]),
                    });

                    pending++;
                    if (pending == BATCH_SIZE)
                    {
                        siteDb.SaveChanges();
                        siteDb.ChangeTracker.Clear();
                        pending = 0;
                    }
                    progressBar.Tick();
                }
            }
            siteDb.SaveChanges();
            transaction.Commit();
        }
    }
}
