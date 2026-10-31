using DataAquisition.PitchTracking;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;

namespace PitchAnalysis
{
    internal static class BinEvaluation
    {
        private const string PLOT_DIRECTORY = "BinEvaluationPlots";
        private static readonly int[] EXCLUDED_BIN_YEARS = [2020];   // bins built from these years aren't evaluated (60-game season)
        private const int IMAGE_WIDTH = 2000;
        private const int PLOT_HEIGHT = 800;
        private const int MIN_EVAL_YEAR = 2022;

        private readonly record struct SituationKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes);

        // A method's prediction for every cell of a situation, plus pitches that land outside the grid
        private sealed record SituationPrediction(double[,] Cells, double OffGrid);

        private sealed class YearTotals(int methodCount)
        {
            public double[] Sse { get; } = new double[methodCount];
            public long Count { get; set; }
        }

        private const string BASELINE_NAME = "Situation Mean";
        private static readonly PitchZoneBinArgs ZONE_SMALLER = new()
        {
            ZoneColumns = 5,
            SquareRowsPerSide = 2,
            OutsideColumns = 5,
            OutsideRows = 5,
            Kernel=null,
        };
        private static readonly PitchZoneBinArgs ZONE_BIGGER = new()
        {
            ZoneColumns = 7,
            SquareRowsPerSide = 3,
            OutsideColumns = 7,
            OutsideRows = 7,
            Kernel=null,
        };

        private sealed record Method(string Name, PitchZoneBin Bin);

        private static readonly Method[] METHODS =
        [
            new("Current Bins", new PitchZoneBin(ZONE_SMALLER)),
            new("Adjacent 0.375", new PitchZoneBin(ZONE_SMALLER with { Kernel = PitchZoneKernel.Adjacent(0.375) })),
            new("Adjacent 0.5", new PitchZoneBin(ZONE_SMALLER with { Kernel = PitchZoneKernel.Adjacent(0.5) })),
            new("Gaussian r1 s1.0", new PitchZoneBin(ZONE_SMALLER with { Kernel = PitchZoneKernel.Gaussian(1, 1.0) })),
            new("Bigger Adjacent 0.25", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Adjacent(0.25) })),
            new("Bigger Adjacent 0.375", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Adjacent(0.375) })),
            new("Bigger Adjacent 0.375 Split", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Adjacent(0.375), SplitMiddleRow=true })),
            new("Bigger Adjacent 0.5", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Adjacent(0.5) })),
            new("Bigger Gaussian r1 s1.0", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(1, 1.0) })),
            new("Bigger Gaussian r2 s1.0", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0) })),
            new("Bigger Gaussian r2 s1.0 Split", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SplitMiddleRow=true })),
            new("Bigger Gaussian r3 s1.0", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(3, 1.0) })),
        ];

        // Index into METHODS of the bins the per-cell plots are compared against
        private const int REFERENCE_METHOD = 0;
        // Squared errors for the pitches landing in one cell (or off the grid) of a method's layout
        private sealed class CellErrors
        {
            public double MethodSse { get; set; }
            public double ReferenceSse { get; set; }
            public double BaselineSse { get; set; }
            public long Count { get; set; }
        }
        // All cells of one method, pooled over every year and situation
        private sealed class MethodCellTotals
        {
            public Dictionary<PitchZone, CellErrors> Cells { get; } = new();
            public CellErrors NoZone { get; } = new();

            public void Add(PitchZone? zone, double methodSe, double referenceSe, double baselineSe)
            {
                CellErrors cell = NoZone;
                if (zone is PitchZone z)
                {
                    if (!Cells.TryGetValue(z, out CellErrors? existing))
                    {
                        existing = new();
                        Cells[z] = existing;
                    }

                    cell = existing;
                }

                cell.MethodSse += methodSe;
                cell.ReferenceSse += referenceSe;
                cell.BaselineSse += baselineSe;
                cell.Count++;
            }
        }

        public static void Evaluate()
        {
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            string[] names = [BASELINE_NAME, .. METHODS.Select(m => m.Name)];
            Dictionary<int, YearTotals> totals = new();
            MethodCellTotals[] cellTotals = [.. METHODS.Select(_ => new MethodCellTotals())];
            PitchZone?[] zones = new PitchZone?[METHODS.Length];
            double[] predicted = new double[METHODS.Length];
            long skipped = 0;

            List<int> evalYears = MlbPitches(ptDb)
                .Where(f => f.Year >= MIN_EVAL_YEAR)
                .Select(f => f.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Score every MLB pitch on the previous year's bins
            foreach (int evalYear in evalYears)
            {
                int binYear = evalYear - 1;
                if (EXCLUDED_BIN_YEARS.Contains(binYear))
                {
                    continue;
                }

                var (means, predictions) = LoadBinYear(ptDb, binYear);
                YearTotals yearTotals = new(names.Length);
                totals[evalYear] = yearTotals;

                foreach (PitchData p in MlbPitches(ptDb).Where(f => f.Year == evalYear))
                {
                    SituationKey key = new(binYear, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike);
                    if (!means.TryGetValue(key, out double mean))
                    {
                        skipped++;   // situation never occurred in the prior year; Update() also leaves these null
                        continue;
                    }

                    double baselineError = mean - p.RunValueSmoothedHitter;
                    yearTotals.Sse[0] += baselineError * baselineError;

                    for (int m = 0; m < METHODS.Length; m++)
                    {
                        SituationPrediction prediction = predictions[m][key];
                        zones[m] = METHODS[m].Bin.GetZone(p);
                        predicted[m] = zones[m] is PitchZone z ? prediction.Cells[z.Row, z.Column] : prediction.OffGrid;
                    }

                    double referenceError = predicted[REFERENCE_METHOD] - p.RunValueSmoothedHitter;

                    for (int m = 0; m < METHODS.Length; m++)
                    {
                        double error = predicted[m] - p.RunValueSmoothedHitter;
                        yearTotals.Sse[m + 1] += error * error;
                        cellTotals[m].Add(zones[m], error * error, referenceError * referenceError, baselineError * baselineError);
                    }

                    yearTotals.Count++;
                }
            }

            Console.WriteLine($"Scored {totals.Values.Sum(t => t.Count)} pitches, skipped {skipped} with no prior-year situation");
            Directory.CreateDirectory(PLOT_DIRECTORY);
            PlotMseReduction(names, totals, Path.Combine(PLOT_DIRECTORY, "MseReduction.png"));
            PlotCellReductions(cellTotals);
        }

        private static IQueryable<PitchData> MlbPitches(PitchTrackingDbContext ptDb)
        {
            return ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.LevelId == 1 && f.PitchClass != Db.DbEnums.PitchClass.Other);
        }

        private static (Dictionary<SituationKey, double> Means, Dictionary<SituationKey, SituationPrediction>[] Predictions) LoadBinYear(PitchTrackingDbContext ptDb, int binYear)
        {
            List<PitchData> binPitches = MlbPitches(ptDb).Where(f => f.Year == binYear).ToList();
            Dictionary<SituationKey, double> means = SituationMeans(binPitches);
            Dictionary<SituationKey, SituationPrediction>[] predictions = METHODS
                .Select(m => BuildPredictions(m.Bin, binPitches, means))
                .ToArray();
            Console.WriteLine($"Built {means.Count} situations from {binPitches.Count:N0} pitches in {binYear}");
            return (means, predictions);
        }

        private static Dictionary<SituationKey, double> SituationMeans(List<PitchData> binPitches)
        {
            return binPitches
                .GroupBy(p => new SituationKey(p.Year, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike))
                .ToDictionary(g => g.Key, g => g.Average(p => (double)p.RunValueSmoothedHitter));
        }

        private static void PlotMseReduction(string[] names, Dictionary<int, YearTotals> totals, string path)
        {
            // Columns: each evaluation year, then all years pooled
            List<(string Label, double[] Sse, long Count)> columns = totals
                .OrderBy(kv => kv.Key)
                .Select(kv => (kv.Key.ToString(), kv.Value.Sse, kv.Value.Count))
                .ToList();
            columns.Add(("All",
                Enumerable.Range(0, names.Length).Select(m => totals.Values.Sum(t => t.Sse[m])).ToArray(),
                totals.Values.Sum(t => t.Count)));

            using ScottPlot.Plot plot = new();
            ScottPlot.TickGenerators.NumericManual ticks = new();
            for (int c = 0; c < columns.Count; c++)
                ticks.AddMajor(c, $"{columns[c].Label}\nn={columns[c].Count:N0}");

            // methods[0] is the baseline; every other method gets a bar per column
            ScottPlot.Palettes.Category10 palette = new();
            int compared = names.Length - 1;
            double barWidth = 0.8 / compared;
            for (int m = 1; m < names.Length; m++)
            {
                List<ScottPlot.Bar> bars = new();
                for (int c = 0; c < columns.Count; c++)
                {
                    double reduction = 100 * (1 - (columns[c].Sse[m] / columns[c].Sse[0]));
                    double position = c + ((m - 1 - ((compared - 1) / 2.0)) * barWidth);
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
                barPlot.LegendText = names[m];
            }

            double allBaselineMse = columns[^1].Sse[0] / columns[^1].Count;
            plot.Axes.Bottom.TickGenerator = ticks;
            plot.ShowLegend();
            plot.Title($"Out-of-Sample MSE Reduction vs Prior-Year {names[0]} (all-years baseline MSE={allBaselineMse:G4})");
            plot.XLabel("Evaluation Year (bins from year − 1)");
            plot.YLabel("MSE Reduction (%)");
            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }

        private static Dictionary<SituationKey, SituationPrediction> BuildPredictions(PitchZoneBin bin, List<PitchData> binPitches, Dictionary<SituationKey, double> situationMeans)
        {
            var (competitiveRows, nonCompetitiveRows) = bin.BuildCompetitiveRows(binPitches);

            // An empty pool (essentially never) falls back to the situation mean so every method scores the same pitches.
            Dictionary<SituationKey, double> pools = new();
            foreach (PitchZoneNonCompetitive f in nonCompetitiveRows)
            {
                SituationKey key = new(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes);
                pools[key] = f.PitchCount > 0 ? f.ExpectedValueHitter : situationMeans[key];
            }

            Dictionary<SituationKey, SituationPrediction> predictions = new();
            var situations = competitiveRows
                .GroupBy(f => new SituationKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes));
            foreach (var situation in situations)
            {
                double pool = pools[situation.Key];

                // Every cell has a row, so the largest indices give this bin's grid size.
                double[,] cells = new double[situation.Max(f => f.ZoneRow) + 1, situation.Max(f => f.ZoneCol) + 1];
                foreach (PitchZoneCompetitive cell in situation)
                {
                    bool valid = cell.IsCompetitive && cell.HasEnoughSample;
                    cells[cell.ZoneRow, cell.ZoneCol] = valid ? cell.ExpectedValueHitter : pool;
                }

                predictions[situation.Key] = new SituationPrediction(cells, pool);
            }

            return predictions;
        }

        private static (double Value, string Text)? CellReductionVsReference(CellErrors errors)
        {
            if (errors.Count == 0 || errors.BaselineSse == 0)
            {
                return null;
            }

            double difference = 100 * (errors.ReferenceSse - errors.MethodSse) / errors.BaselineSse;
            return (difference, $"{difference:+0.0;-0.0}%\n{errors.Count / 1000.0:F1}k");
        }

        private static void PlotCellReductions(MethodCellTotals[] cellTotals)
        {
            for (int m = 0; m < METHODS.Length; m++)
            {
                if (m == REFERENCE_METHOD)
                {
                    continue;
                }

                Dictionary<PitchZone, (double Value, string Text)> cells = new();
                foreach ((PitchZone zone, CellErrors errors) in cellTotals[m].Cells)
                {
                    if (CellReductionVsReference(errors) is (double, string) reduction)
                    {
                        cells[zone] = reduction;
                    }
                }

                string label = $"{METHODS[m].Name} vs {METHODS[REFERENCE_METHOD].Name}: MSE reduction difference (%)";
                METHODS[m].Bin.PlotCellValues(
                    cells,
                    CellReductionVsReference(cellTotals[m].NoZone),
                    label,
                    Path.Combine(PLOT_DIRECTORY, $"Cells_{METHODS[m].Name}.png"));
            }
        }
    }
}
