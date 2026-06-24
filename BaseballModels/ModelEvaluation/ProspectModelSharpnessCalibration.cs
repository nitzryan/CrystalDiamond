using Db;
using ModelDb;
using ShellProgressBar;
using System.Globalization;
using System.Linq.Expressions;

namespace ModelEvaluation
{
    internal class ProspectModelSharpnessCalibration
    {
        // Stub: Represents a single observation for PIT/Brier purposes -
        // the actual bucket the player's WAR fell into, and the predicted probability vector
        private class PitRecord
        {
            public required int MlbId { get; set; }
            public required int ActualBucket { get; set; }
            public required float ActualWar { get; set; }
            public required float[] PredictedProbs { get; set; } // War0...War6
            public required float PredictedWar { get; set; }
        }

        private record GroupResult(
            string IsHitter,
            string ProspectType,
            string WarBucket,
            int RecordCount,
            int PlayerCount,
            double AlphaScore,
            double BrierScore,
            double PitBias,
            double WarBias
        );

        public static void Update()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            float[] warBucketMaxes = [1f, 3f, 5f, 10f, float.PositiveInfinity];
            string[] warBucketLabels = ["0to1", "1to3", "3to5", "5to10", "10plus"];
            string reportFileName = "../../../Output/Prospect_PIT/report.csv";

            Dictionary<int, string> prospectTypeLabels = new()
            {
                { 1, "DRFT" },
                { 2, "UDFA" },
                { 3, "INTL" }
            };

            var allPlayersDb = db.Model_Players.Where(p => p.IsEligible).ToList();

            List<GroupResult> results = [];

            using (ProgressBar progressBar = new ProgressBar(2 * (prospectTypeLabels.Count + 1), "Prospect Model Calibration Stats"))
            {
                foreach (bool isHitter in new[] { true, false })
                {
                    string hitterLabel = isHitter ? "H" : "P";

                    Expression<Func<ModelDb.Output_PlayerWarAggregation, bool>> outputFilter =
                        o => o.IsHitter == isHitter && o.Model == 1;

                    var players = allPlayersDb
                        .Where(p => isHitter ? p.IsHitter == 1 : p.IsPitcher == 1)
                        .ToList();

                    // Build prospect type groups including "All"
                    var prospectTypeGroups = players
                        .Select(p => p.ProspectType).Distinct().OrderBy(x => x)
                        .Select(pt => (Label: prospectTypeLabels[pt], Players: players.Where(p => p.ProspectType == pt).ToList()))
                        .Append(("All", players))
                        .ToList();

                    foreach (var (prospectTypeLabel, prospectPlayers) in prospectTypeGroups)
                    {
                        List<PitRecord> allPitRecords = BuildPitRecords(modelDb, prospectPlayers, outputFilter, isHitter);

                        var warGroups = allPitRecords
                            .GroupBy(r => GetOPVAWarBucket(r.PredictedWar, warBucketMaxes))
                            .ToDictionary(g => g.Key, g => g.ToList());

                        var warBucketGroups = new List<(string Label, List<PitRecord> Records)>();
                        for (int wb = 0; wb < warBucketMaxes.Length; wb++)
                        {
                            if (warGroups.TryGetValue(wb, out var filtered) && filtered.Count > 0)
                                warBucketGroups.Add((warBucketLabels[wb], filtered));
                        }
                        warBucketGroups.Add(("All", allPitRecords));

                        foreach (var (warBucket, records) in warBucketGroups)
                        {
                            if (records.Count == 0) continue;

                            int playerCount = records.Select(r => r.MlbId).Distinct().Count();
                            string groupLabel = $"{hitterLabel} {prospectTypeLabel} {warBucket} (N={records.Count}, Players={playerCount})";
                            string pitPlotFileBase = $"../../../Output/Prospect_PIT/{hitterLabel}_{prospectTypeLabel}_{warBucket}_pit_diagram";

                            double[] pitValues = ComputePitValues(records);
                            double[] pitHistogram = ComputePitHistogram(pitValues);
                            double[] classProportions = ComputeClassProportions(records);
                            PlotPitDiagram(pitHistogram, pitValues, classProportions, pitPlotFileBase, groupLabel);

                            double alphaScore = ComputeAlphaScore(pitHistogram);
                            double brierScore = ComputeBrierScore(records);
                            double pitBias = ComputeBiasScore(pitValues);
                            double warBias = ComputeWarBias(records);

                            results.Add(new GroupResult(
                                hitterLabel, prospectTypeLabel, warBucket,
                                records.Count, playerCount,
                                alphaScore, brierScore,
                                pitBias, warBias
                            ));
                        }

                        progressBar.Tick();
                    }
                }

            }

            ReportResults(results, reportFileName);
        }

        // Joins filtered OPVA rows against the loaded players, pairing each prediction
        // with the actual WAR bucket derived from the player's career WAR.
        // Every OPVA row for a player is kept (each timestep's prediction is a separate observation).
        private static List<PitRecord> BuildPitRecords(
            ModelDbContext modelDb,
            List<Db.Model_Players> players,
            Expression<Func<ModelDb.Output_PlayerWarAggregation, bool>> outputFilter,
            bool isHitter)
        {
            // Build lookup: MlbId -> actual bucket
            Dictionary<int, (int Bucket, float War)> playerLookup = players.ToDictionary(
                p => p.MlbId,
                p => (Constants.GetBucket(isHitter ? p.WarHitter : p.WarPitcher), isHitter ? p.WarHitter : p.WarPitcher)
            );

            List<PitRecord> records = [];

            foreach (var o in modelDb.Output_PlayerWarAggregation.Where(outputFilter))
            {
                if (!playerLookup.TryGetValue(o.MlbId, out var actual))
                    continue;

                float[] predictedProbs = [o.War0, o.War1, o.War2, o.War3, o.War4, o.War5, o.War6];

                // Guard against bucket count mismatches between the model output and Constants.BUCKET_CUTOFFS
                if (predictedProbs.Length != Constants.BUCKET_CUTOFFS.Count)
                    throw new InvalidOperationException(
                        $"Predicted probability count ({predictedProbs.Length}) does not match Constants.BUCKET_CUTOFFS.Count ({Constants.BUCKET_CUTOFFS.Count}). Update the War0..War6 mapping above.");

                records.Add(new PitRecord
                {
                    MlbId = o.MlbId,
                    ActualBucket = actual.Bucket,
                    ActualWar = actual.War,
                    PredictedProbs = predictedProbs,
                    PredictedWar = o.War
                });
            }

            return records;
        }

        // Assigns an OPVA row to a war bucket based on its predicted War value.
        // Bucket i contains values where War <= bucketMaxes[i] (and > bucketMaxes[i-1]).
        private static int GetOPVAWarBucket(float war, float[] bucketMaxes)
        {
            for (int i = 0; i < bucketMaxes.Length; i++)
                if (war <= bucketMaxes[i]) return i;
            return bucketMaxes.Length - 1;
        }

        // Computes randomized PIT values for discrete distributions.
        // For each observation, the PIT value is drawn uniformly from [F(k-1), F(k)]
        // where F is the predicted CDF and k is the actual bucket.
        private static double[] ComputePitValues(List<PitRecord> pitRecords)
        {
            Random rng = new(42);
            double[] pitValues = new double[pitRecords.Count];

            for (int j = 0; j < pitRecords.Count; j++)
            {
                var record = pitRecords[j];

                float cdfUpper = 0f;
                for (int i = 0; i <= record.ActualBucket; i++)
                    cdfUpper += record.PredictedProbs[i];

                float cdfLower = cdfUpper - record.PredictedProbs[record.ActualBucket];

                pitValues[j] = cdfLower + (rng.NextDouble() * (cdfUpper - cdfLower));
                pitValues[j] = Math.Clamp(pitValues[j], 0.0, 1.0 - 1e-10);
            }

            return pitValues;
        }

        // Bins pre-computed PIT values into a histogram.
        // Returns proportions normalized so that values sum to 1.
        private static double[] ComputePitHistogram(double[] pitValues, int numBins = 10)
        {
            double[] histogram = new double[numBins];

            if (pitValues.Length == 0)
                return histogram;

            foreach (double pit in pitValues)
            {
                int bin = (int)(pit * numBins);
                histogram[bin]++;
            }

            for (int i = 0; i < numBins; i++)
                histogram[i] /= pitValues.Length;

            return histogram;
        }

        // Takes the expected amount of each class and the actual amount of each class
        // Returns how many times each class group is found in the model relative to actual
        private static double[] ComputeClassProportions(List<PitRecord> pitRecords)
        {
            List<double> classProportions = [];
            for (int i = 0; i < Constants.BUCKET_CUTOFFS.Count; i++)
            {
                int actualClassMembers = pitRecords.Where(f => f.ActualBucket == i).Count();
                float modelClassMembers = pitRecords.Sum(f => f.PredictedProbs[i]);

                if (actualClassMembers == 0)
                {
                    classProportions.Add(1);
                    continue;
                }

                classProportions.Add(modelClassMembers / actualClassMembers);
            }

            return classProportions.ToArray();
        }

        // Saves two PIT diagnostic plots:
        //   {fileBase}_hist.png - Histogram showing observed/expected ratio per bin (1.0 = perfect calibration)
        //   {fileBase}_cdf.png  - Empirical CDF of PIT values vs. the uniform diagonal
        private static void PlotPitDiagram(double[] pitHistogram, double[] pitValues, double[] classProportions, string fileBase, string groupLabel)
        {
            int numBins = pitHistogram.Length;
            double uniform = 1.0 / numBins;

            // ---- Histogram: proportion of expected (observed / uniform) ----
            {
                double[] binCenters = Enumerable.Range(0, numBins)
                    .Select(i => (i + 0.5) / numBins)
                    .ToArray();

                double[] ratios = pitHistogram.Select(h => h / uniform).ToArray();

                ScottPlot.Plot plt = new();

                var bars = plt.Add.Bars(binCenters, ratios);
                bars.Bars.ForEach(b => b.Size = 1.0 / numBins * 0.9);

                var refLine = plt.Add.HorizontalLine(1.0);
                refLine.LinePattern = ScottPlot.LinePattern.Dashed;
                refLine.Color = ScottPlot.Colors.Red;
                refLine.LineWidth = 2;

                plt.Title($"PIT Histogram (Obs/Exp) - {groupLabel}");
                plt.XLabel("Probability Integral Transform");
                plt.YLabel("Ratio to Uniform");
                plt.Axes.SetLimitsX(0, 1);

                plt.SavePng($"{fileBase}_hist.png", 800, 600);
            }

            // ---- CDF: empirical CDF of PIT values vs. theoretical uniform diagonal ----
            {
                double[] sorted = pitValues.OrderBy(v => v).ToArray();
                int n = sorted.Length;

                // Empirical CDF: step function, (sorted[i], (i+1)/n)
                double[] empiricalY = Enumerable.Range(0, n)
                    .Select(i => (i + 1.0) / n)
                    .ToArray();

                ScottPlot.Plot plt = new();

                plt.Add.ScatterLine(sorted, empiricalY);

                // Uniform reference: diagonal from (0,0) to (1,1)
                var diag = plt.Add.Line(0, 0, 1, 1);
                diag.LinePattern = ScottPlot.LinePattern.Dashed;
                diag.Color = ScottPlot.Colors.Red;
                diag.LineWidth = 2;

                plt.Title($"PIT CDF vs. Uniform - {groupLabel}");
                plt.XLabel("Probability Integral Transform");
                plt.YLabel("Cumulative Probability");
                plt.Axes.SetLimitsX(0, 1);
                plt.Axes.SetLimitsY(0, 1);

                plt.SavePng($"{fileBase}_cdf.png", 800, 600);
            }

            // ---- Proportion of each class, model / actual ----
            {
                double[] bucketValues = Enumerable.Range(0, Constants.BUCKET_CUTOFFS.Count).Select(f => (double)f).ToArray();

                ScottPlot.Plot plt = new();

                var bars = plt.Add.Bars(bucketValues, classProportions);
                bars.Bars.ForEach(b => b.Size = 1.0 / Constants.BUCKET_CUTOFFS.Count * 0.9);

                var refLine = plt.Add.HorizontalLine(1.0);
                refLine.LinePattern = ScottPlot.LinePattern.Dashed;
                refLine.Color = ScottPlot.Colors.Red;
                refLine.LineWidth = 2;

                plt.Title($"PIT Histogram (Obs/Exp) - {groupLabel}");
                plt.XLabel("WAR Buckets");
                plt.YLabel("Ratio of Model to Actual");
                //plt.Axes.SetLimitsX(0, 1);

                plt.SavePng($"{fileBase}_prop.png", 800, 600);
            }
        }


        // Computes the alpha reliability index from a PIT histogram.
        // This measures how close the PIT distribution is to uniform (i.e., how well-calibrated the model is).
        //
        // α = 1 - Δ / Δ_max
        //
        // where:
        //   Δ     = Σ |f_k - 1/K|       (total absolute deviation of bin proportions from uniform)
        //   Δ_max = 2 * (1 - 1/K)       (maximum possible deviation, when all mass is in a single bin)
        //   K     = number of bins
        //   f_k   = observed proportion in bin k
        //
        // α = 1.0 means perfectly uniform (ideal calibration)
        // α = 0.0 means all observations fell into a single bin (worst calibration)
        private static double ComputeAlphaScore(double[] pitHistogram)
        {
            int K = pitHistogram.Length;
            if (K <= 1)
                return double.NaN;

            double uniform = 1.0 / K;

            double delta = 0.0;
            for (int i = 0; i < K; i++)
                delta += Math.Abs(pitHistogram[i] - uniform);

            double deltaMax = 2.0 * (1.0 - uniform);

            return 1.0 - (delta / deltaMax);
        }

        // Computes the multi-class Brier score.
        // For each observation, compares the predicted probability vector against a one-hot vector
        // of the actual outcome. The score is the mean squared error across all observations and buckets.
        // Lower is better; 0 = perfect, 2 = worst possible.
        private static double ComputeBrierScore(List<PitRecord> pitRecords)
        {
            if (pitRecords.Count == 0)
                throw new Exception("No members of PitRecords in ComputeBrierScore");

            int numBuckets = Constants.BUCKET_CUTOFFS.Count;
            double sum = 0.0;
            foreach (var record in pitRecords)
            {
                for (int i = 0; i < numBuckets; i++)
                {
                    // indicator is 1 for the bucket the actual WAR fell into, 0 otherwise
                    double indicator = (record.ActualBucket == i) ? 1.0 : 0.0;
                    double diff = record.PredictedProbs[i] - indicator;
                    sum += diff * diff;
                }
            }
            // Average over all observations
            return sum / pitRecords.Count;
        }

        // Computes directional bias from PIT values.
        // bias = mean(PIT) - 0.5
        //   positive bias -> model tends to under-predict (actuals land in upper tail of predictions)
        //   negative bias -> model tends to over-predict (actuals land in lower tail of predictions)
        //   0 -> no systematic directional bias
        private static double ComputeBiasScore(double[] pitValues)
        {
            if (pitValues.Length == 0)
                return double.NaN;

            return pitValues.Average() - 0.5;
        }

        // Computes mean directional bias in WAR units.
        // bias = mean(actualWar - predictedWar)
        //   positive -> model tends to under-predict WAR (actuals higher than predicted)
        //   negative -> model tends to over-predict WAR (actuals lower than predicted)
        private static double ComputeWarBias(List<PitRecord> pitRecords)
        {
            if (pitRecords.Count == 0)
                return double.NaN;

            return pitRecords.Average(r => r.ActualWar - r.PredictedWar);
        }

        private static void ReportResults(List<GroupResult> results, string outputFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFileName)!);

            using var writer = new StreamWriter(outputFileName);
            writer.WriteLine("IsHitter,ProspectType,WarBucket,RecordCount,PlayerCount,AlphaScore,BrierScore,PitBias,WarBias");
            foreach (var r in results)
            {
                writer.WriteLine($"{r.IsHitter},{r.ProspectType},{r.WarBucket},{r.RecordCount},{r.PlayerCount},{r.AlphaScore:F4},{r.BrierScore:F4},{r.PitBias:F4},{r.WarBias:F4}");
            }
        }
    }
}
