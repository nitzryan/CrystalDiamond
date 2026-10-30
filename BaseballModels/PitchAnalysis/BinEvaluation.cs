using DataAquisition.PitchTracking;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;

using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace PitchAnalysis
{
    internal static class BinEvaluation
    {
        private const string PLOT_DIRECTORY = "BinEvaluationPlots";
        private static readonly int[] EXCLUDED_BIN_YEARS = [2020];   // bins built from these years aren't evaluated (60-game season)
        private const int IMAGE_WIDTH = 2000;
        private const int PLOT_HEIGHT = 800;
        private const int GRID = PitchBinCompetitiveness.PitchZoneLayout.GridSize;
        private const int MIN_EVAL_YEAR = 2022;

        private readonly record struct SituationKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes);

        // Everything stored for one situation in PZC + PZNC (all cells, not only valid ones)
        private sealed class SituationCells
        {
            public int[,] Count { get; } = new int[GRID, GRID];
            public float[,] Ev { get; } = new float[GRID, GRID];
            public bool[,] Valid { get; } = new bool[GRID, GRID];   // IsCompetitive && HasEnoughSample
            public int PoolCount { get; set; }
            public float PoolEv { get; set; }


            public int[,] Swing { get; } = new int[GRID, GRID];
            public int OffGridCount { get; set; }       // pitches with no zone (derived: pool minus stored-invalid cells)
            public double OffGridSum { get; set; }
        }

        // A method's prediction for every cell of a situation, plus pitches that land outside the grid
        private sealed record SituationPrediction(double[,] Cells, double OffGrid);
        private sealed record Method(string Name, Func<SituationCells, SituationPrediction> Build);

        private sealed class YearTotals(int methodCount)
        {
            public double[] Sse { get; } = new double[methodCount];
            public long Count { get; set; }
        }

        private static readonly Method BASELINE = new("Situation Mean", SituationMean);
        private static readonly Method[] METHODS =
        [
            new("Current Bins", CurrentBins),
            new("Kernel 0.375 Outside", s => AdjacentKernel(s, 0.375, outsideOnly: true)),
            new("Kernel 0.375 All", s => AdjacentKernel(s, 0.375, outsideOnly: false)),
            new("Effective 0.25", s => EffectiveKernel(s, 0.25)),
            new("Effective 0.375", s => EffectiveKernel(s, 0.375)),
            new("Effective 0.5", s => EffectiveKernel(s, 0.5)),
        ];

        public static void Evaluate()
        {
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            Dictionary<SituationKey, SituationCells> situations = LoadSituations(ptDb);
            Console.WriteLine($"Loaded {situations.Count} Situations");

            Method[] methods = [BASELINE, .. METHODS];
            Dictionary<SituationKey, SituationPrediction>[] predictions = methods
                .Select(m => situations.ToDictionary(kv => kv.Key, kv => m.Build(kv.Value)))
                .ToArray();

            // Score every MLB pitch on the previous year's bins (same filter PZC was built with)
            Dictionary<int, YearTotals> totals = new();
            long skipped = 0;

            var pitchData = ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.Year >= MIN_EVAL_YEAR && f.LevelId == 1 && f.PitchClass != Db.DbEnums.PitchClass.Other)
                .Select(f => new {
                    f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                    f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot, f.RunValueSmoothedHitter
                });
            foreach (var p in pitchData)
            {
                int binYear = p.Year - 1;
                if (EXCLUDED_BIN_YEARS.Contains(binYear))
                    continue;
                SituationKey key = new(binYear, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike);
                if (!situations.ContainsKey(key))
                {
                    skipped++;   // situation never occurred in the prior year; Update() also leaves these null
                    continue;
                }

                PitchBinCompetitiveness.PitchZone? zone = PitchBinCompetitiveness.GetZone(p.PlateX, p.PlateZ, p.ZoneTop, p.ZoneBot);
                if (!totals.TryGetValue(p.Year, out YearTotals? yearTotals))
                {
                    yearTotals = new(methods.Length);
                    totals[p.Year] = yearTotals;
                }
                for (int m = 0; m < methods.Length; m++)
                {
                    SituationPrediction prediction = predictions[m][key];
                    double predicted = zone is PitchBinCompetitiveness.PitchZone z
                        ? prediction.Cells[z.Row, z.Column]
                        : prediction.OffGrid;
                    double error = predicted - p.RunValueSmoothedHitter;
                    yearTotals.Sse[m] += error * error;
                }
                yearTotals.Count++;
            }
            Console.WriteLine($"Scored {totals.Values.Sum(t => t.Count)} pitches, skipped {skipped} with no prior-year situation");

            Directory.CreateDirectory(PLOT_DIRECTORY);
            PlotMseReduction(methods, totals, Path.Combine(PLOT_DIRECTORY, "MseReduction.png"));
        }

        private static Dictionary<SituationKey, SituationCells> LoadSituations(PitchTrackingDbContext ptDb)
        {
            Dictionary<SituationKey, SituationCells> situations = new();
            var pitchZoneCompetitive = ptDb.PitchZoneCompetitive
                .AsNoTracking()
            .Select(f => new
            {
                f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes, f.ZoneRow, f.ZoneCol,
                f.PitchCount, f.SwingCount, f.ExpectedValueHitter, f.IsCompetitive, f.HasEnoughSample
            });

            foreach (var f in pitchZoneCompetitive)
            {
                SituationKey key = new(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes);
                if (!situations.TryGetValue(key, out SituationCells? s))
                {
                    s = new();
                    situations[key] = s;
                }
                s.Count[f.ZoneRow, f.ZoneCol] = f.PitchCount;
                s.Swing[f.ZoneRow, f.ZoneCol] = f.SwingCount;
                s.Ev[f.ZoneRow, f.ZoneCol] = f.ExpectedValueHitter;
                s.Valid[f.ZoneRow, f.ZoneCol] = f.IsCompetitive && f.HasEnoughSample;
            }

            // PZNC and PZC are written for the same situations, so every PZNC row has a PZC entry
            foreach (var f in ptDb.PitchZoneNonCompetitive.AsNoTracking())
            {
                SituationCells s = situations[new SituationKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes)];
                s.PoolCount = f.PitchCount;
                s.PoolEv = f.ExpectedValueHitter;
            }

            // Pool = off-grid pitches + stored-invalid cells, so off-grid = pool minus those cells
            foreach (SituationCells s in situations.Values)
            {
                long invalidCount = 0;
                double invalidSum = 0;
                for (int r = 0; r < GRID; r++)
                    for (int c = 0; c < GRID; c++)
                        if (!s.Valid[r, c])
                        {
                            invalidCount += s.Count[r, c];
                            invalidSum += s.Count[r, c] * (double)s.Ev[r, c];
                        }
                s.OffGridCount = (int)(s.PoolCount - invalidCount);
                s.OffGridSum = s.PoolCount * (double)s.PoolEv - invalidSum;
            }
            return situations;
        }

        // Mean of every pitch in the situation: valid cells + pool (the pool already holds off-grid and non-valid cells)
        private static SituationPrediction SituationMean(SituationCells s)
        {
            double sum = s.PoolCount * (double)s.PoolEv;
            long count = s.PoolCount;
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                    if (s.Valid[r, c])
                    {
                        sum += s.Count[r, c] * (double)s.Ev[r, c];
                        count += s.Count[r, c];
                    }
            double mean = sum / count;

            double[,] cells = new double[GRID, GRID];
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                    cells[r, c] = mean;
            return new(cells, mean);
        }

        // Valid bin value, otherwise the pooled non-competitive value.
        // An empty pool (essentially never) falls back to the situation mean so every method scores the same pitches.
        private static SituationPrediction CurrentBins(SituationCells s)
        {
            double pool = s.PoolCount > 0 ? s.PoolEv : SituationMean(s).OffGrid;
            double[,] cells = new double[GRID, GRID];
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                    cells[r, c] = s.Valid[r, c] ? s.Ev[r, c] : pool;
            return new(cells, pool);
        }

        // Kernel value for valid cells only; validity and the pool are unchanged from CurrentBins
        private static SituationPrediction AdjacentKernel(SituationCells s, double neighbourWeight, bool outsideOnly)
        {
            SituationPrediction current = CurrentBins(s);
            double[,] cells = (double[,])current.Cells.Clone();
            var (sum, weight, _) = SmoothCells(s, neighbourWeight);
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                    if (s.Valid[r, c] && !(outsideOnly && new PitchBinCompetitiveness.PitchZone(r, c).IsInStrikeZone))
                        cells[r, c] = sum[r, c] / weight[r, c];
            return new(cells, current.OffGrid);
        }

        private static void PlotMseReduction(Method[] methods, Dictionary<int, YearTotals> totals, string path)
        {
            // Columns: each evaluation year, then all years pooled
            List<(string Label, double[] Sse, long Count)> columns = totals
                .OrderBy(kv => kv.Key)
                .Select(kv => (kv.Key.ToString(), kv.Value.Sse, kv.Value.Count))
                .ToList();
            columns.Add(("All",
                Enumerable.Range(0, methods.Length).Select(m => totals.Values.Sum(t => t.Sse[m])).ToArray(),
                totals.Values.Sum(t => t.Count)));

            using ScottPlot.Plot plot = new();
            ScottPlot.TickGenerators.NumericManual ticks = new();
            for (int c = 0; c < columns.Count; c++)
                ticks.AddMajor(c, $"{columns[c].Label}\nn={columns[c].Count:N0}");

            // methods[0] is the baseline; every other method gets a bar per column
            ScottPlot.Palettes.Category10 palette = new();
            int compared = methods.Length - 1;
            double barWidth = 0.8 / compared;
            for (int m = 1; m < methods.Length; m++)
            {
                List<ScottPlot.Bar> bars = new();
                for (int c = 0; c < columns.Count; c++)
                {
                    double reduction = 100 * (1 - columns[c].Sse[m] / columns[c].Sse[0]);
                    double position = c + (m - 1 - (compared - 1) / 2.0) * barWidth;
                    bars.Add(new ScottPlot.Bar
                    {
                        Position = position,
                        Value = reduction,
                        Size = barWidth * 0.9,
                        FillColor = palette.GetColor(m - 1),
                    });
                    var label = plot.Add.Text($"{reduction:F2}%", position, reduction);
                    label.LabelFontSize = 16;
                    label.LabelRotation = -90;
                    label.LabelAlignment = ScottPlot.Alignment.MiddleLeft;
                    label.OffsetY = -4;
                }
                var barPlot = plot.Add.Bars(bars);
                barPlot.LegendText = methods[m].Name;
            }

            double allBaselineMse = columns[^1].Sse[0] / columns[^1].Count;
            plot.Axes.Bottom.TickGenerator = ticks;
            plot.ShowLegend();
            plot.Title($"Out-of-Sample MSE Reduction vs Prior-Year {methods[0].Name} (all-years baseline MSE={allBaselineMse:G4})");
            plot.XLabel("Evaluation Year (bins from year − 1)");
            plot.YLabel("MSE Reduction (%)");
            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }

        // Kernel sums for every cell: itself (weight 1) + 4 adjacent cells (weight w), never crossing the zone border
        private static (double[,] Sum, double[,] Weight, double[,] Swings) SmoothCells(SituationCells s, double neighbourWeight)
        {
            double[,] sum = new double[GRID, GRID];
            double[,] weight = new double[GRID, GRID];
            double[,] swings = new double[GRID, GRID];
            for (int r = 0; r < GRID; r++)
            {
                for (int c = 0; c < GRID; c++)
                {
                    PitchBinCompetitiveness.PitchZone cell = new(r, c);
                    foreach ((int nr, int nc, double w) in new[]
                        { (r, c, 1.0), (r - 1, c, neighbourWeight), (r + 1, c, neighbourWeight), (r, c - 1, neighbourWeight), (r, c + 1, neighbourWeight) })
                    {
                        if (nr < 0 || nr >= GRID || nc < 0 || nc >= GRID)
                            continue;
                        if (new PitchBinCompetitiveness.PitchZone(nr, nc).IsInStrikeZone != cell.IsInStrikeZone)
                            continue;   // never borrow across the strike-zone border
                        sum[r, c] += w * s.Count[nr, nc] * (double)s.Ev[nr, nc];
                        weight[r, c] += w * s.Count[nr, nc];
                        swings[r, c] += w * s.Swing[nr, nc];
                    }
                }
            }
            return (sum, weight, swings);
        }

        // Kernel value for every cell, with validity decided from the kernel's effective sample (count and swings)
        // instead of the cell's own. The pool is rebuilt from off-grid pitches + cells invalid under this rule.
        private static SituationPrediction EffectiveKernel(SituationCells s, double neighbourWeight)
        {
            var (sum, weight, swings) = SmoothCells(s, neighbourWeight);
            bool[,] valid = new bool[GRID, GRID];
            double poolSum = s.OffGridSum;
            long poolCount = s.OffGridCount;
            for (int r = 0; r < GRID; r++)
            {
                for (int c = 0; c < GRID; c++)
                {
                    long effCount = (long)Math.Round(weight[r, c]);
                    long effSwings = (long)Math.Round(swings[r, c]);
                    valid[r, c] = PitchBinCompetitiveness.HasValidSampleSize(effCount, PitchBinCompetitiveness.MIN_PITCHES)
                        && PitchBinCompetitiveness.IsCompetitive(effCount, effSwings, PitchBinCompetitiveness.MIN_SWING_RATE);
                    if (!valid[r, c])
                    {
                        poolSum += s.Count[r, c] * (double)s.Ev[r, c];
                        poolCount += s.Count[r, c];
                    }
                }
            }
            double pool = poolCount > 0 ? poolSum / poolCount : SituationMean(s).OffGrid;

            double[,] cells = new double[GRID, GRID];
            for (int r = 0; r < GRID; r++)
                for (int c = 0; c < GRID; c++)
                    cells[r, c] = valid[r, c] ? sum[r, c] / weight[r, c] : pool;
            return new(cells, pool);
        }
    }
}
