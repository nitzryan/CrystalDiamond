using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using static Db.DbEnums;

namespace PitchAnalysis
{
    using YearTotalsDict = System.Collections.Generic.Dictionary<int, YearTotals>;
    internal sealed class YearTotals(int methodCount)
    {
        public double[] Sse { get; } = new double[methodCount];
        public long Count { get; set; }
    }

    internal static class BinEvaluation
    {
        private const string PLOT_DIRECTORY = "BinEvaluationPlots";
        private const int IMAGE_WIDTH = 2000;
        private const int PLOT_HEIGHT = 800;
        private const int MIN_EVAL_YEAR = 2022;

        private readonly record struct SituationKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR, int Balls, int Strikes);
        private readonly record struct PitchSplitKey(Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR, int Balls, int Strikes);

        // A method's prediction for every cell of a situation, plus pitches that land outside the grid
        private sealed record SituationPrediction(double[,] Cells, bool[,] Valid);

        private const string BASELINE_NAME = "Situation Mean";
        private static readonly PitchZoneBinArgs ZONE_SMALLER = new()
        {
            ZoneColumns = 5,
            SquareRowsPerSide = 2,
            OutsideColumns = 5,
            OutsideRows = 5,
            Kernel=null,
            InZoneNeighborCap=null,
        };
        private static readonly PitchZoneBinArgs ZONE_BIGGER = new()
        {
            ZoneColumns = 7,
            SquareRowsPerSide = 3,
            OutsideColumns = 7,
            OutsideRows = 7,
            Kernel=null,
            OutZoneNeighborCap=null,
        };

        private sealed record Method(string Name, PitchZoneBin Bin);

        private static readonly Method[] METHODS =
        [
            new("Current Bins", new PitchZoneBin(ZONE_BIGGER)),

            new("Bigger Gaussian r2 s1_0", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0) })),

            new("Bigger Gaussian r2 s1_0 Border", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true })),

            new("Bigger Gaussian r2 s1.0 CountCap x1", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=false, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            new("Bigger Gaussian r2 s1.0 Border CountCap x1", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            new("Bigger Gaussian r1 s1.0 Border CountCap x1", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(1, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            new("Bigger Gaussian r2 s1.0 Border CountCap x1 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true })),

            new("Fade InZone 3_500 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 500, OutZoneSmoothingCutoff = 250, SplitMiddleRow=true })),
            new("Fade InZone 3_500", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 500, OutZoneSmoothingCutoff = 250 })),
            new("Fade InZone 4_1000", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 1000, OutZoneSmoothingCutoff = 500 })),
            new("Fade InZone 5_2000", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 2000, OutZoneSmoothingCutoff = 1000 })),
        ];

        // Index into METHODS of the bins the per-cell plots are compared against
        private const int REFERENCE_METHOD = 0;
        // Squared errors for the pitches landing in one cell (or off the grid) of a method's layout
        private sealed class CellErrors
        {
            public double MethodSse { get; set; }
            public double ReferenceSse { get; set; }
            public double BaselineSse { get; set; }
            public double MethodResidualSum { get; set; }
            public long Count { get; set; }
        }
        // All cells of one method, pooled over every year and situation
        private sealed class MethodCellTotals
        {
            public Dictionary<PitchZone, CellErrors> Cells { get; } = new();
            public CellErrors NoZone { get; } = new();

            public void Add(PitchZone? zone, double methodSe, double referenceSe, double baselineSe, double methodResidual)
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
                cell.MethodResidualSum += methodResidual;
                cell.Count++;
            }
        }
        
        public static void Evaluate()
        {
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            string[] names = [BASELINE_NAME, .. METHODS.Select(m => m.Name)];

            List<int> evalYears = MlbPitches(ptDb)
                .Where(f => f.Year >= MIN_EVAL_YEAR)
                .Select(f => f.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            List<PitchData> pitches = MlbPitches(ptDb).ToList();
            var (means, predictions) = LoadBinYear(pitches);
            pitches = pitches.Where(f => f.Year >= MIN_EVAL_YEAR).ToList();
            Directory.CreateDirectory(PLOT_DIRECTORY);

            foreach (var pitchClass in new[]{PitchClass.Fastball, PitchClass.Breaking, PitchClass.Changeup, PitchClass.Other})
            {
                // Load Data
                List<PitchData> classPitches = pitchClass == PitchClass.Other ?
                    pitches : 
                    pitches.Where(f => f.PitchClass == pitchClass).ToList();

                (YearTotalsDict all, YearTotalsDict inZone, YearTotalsDict outZone, MethodCellTotals[] cellTotals) = GetYearTotals(classPitches, means, predictions);

                // Create Subdirectory if applicable
                string dir = pitchClass switch
                {
                    PitchClass.Fastball => PLOT_DIRECTORY + "/Fastball",
                    PitchClass.Breaking => PLOT_DIRECTORY + "/Breaking",
                    PitchClass.Changeup => PLOT_DIRECTORY + "/Changeup",
                    _ => PLOT_DIRECTORY
                };
                if (dir != PLOT_DIRECTORY)
                    Directory.CreateDirectory(dir);

                // Create Plots
                PlotMseReduction(names, all, "All", Path.Combine(dir, "MseReduction.png"));
                PlotMseReduction(names, inZone, "In-Zone", Path.Combine(dir, "MseReduction_InZone.png"));
                PlotMseReduction(names, outZone, "Out-Zone", Path.Combine(dir, "MseReduction_OutZone.png"));

                PlotCellReductions(dir, cellTotals);
                PlotCellBias(dir, cellTotals);
                PlotBiasSummary(dir, cellTotals);
            }
        }

        private static (YearTotalsDict all, YearTotalsDict inZone, YearTotalsDict outZone, MethodCellTotals[] cellTotals) 
            GetYearTotals(List<PitchData> pitches, Dictionary<SituationKey, double> means, Dictionary<SituationKey, SituationPrediction>[] predictions)
        {
            YearTotalsDict totals = new();
            YearTotalsDict inZoneTotals = new();
            YearTotalsDict outZoneTotals = new();
            MethodCellTotals[] cellTotals = [.. METHODS.Select(_ => new MethodCellTotals())];

            string[] names = [BASELINE_NAME, .. METHODS.Select(m => m.Name)];
            var pitchYearGroupings = pitches.GroupBy(f => f.Year);
            PitchZone?[] zones = new PitchZone?[METHODS.Length];
            double[] predicted = new double[METHODS.Length];

            foreach (var pitchYears in pitchYearGroupings)
            {
                int binYear = pitchYears.Key - 1;
                var yearPitches = pitchYears.ToList();
                

                YearTotals yearTotals = new(names.Length);
                YearTotals inZoneYear = new(names.Length);
                YearTotals outZoneYear = new(names.Length);

                totals[pitchYears.Key] = yearTotals;
                inZoneTotals[pitchYears.Key] = inZoneYear;
                outZoneTotals[pitchYears.Key] = outZoneYear;

                foreach (var p in yearPitches)
                {
                    SituationKey key = new(binYear, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike);
                    if (!means.TryGetValue(key, out double mean))
                    {
                        // situation never occurred in the prior year
                        continue;
                    }

                    // Only competitive pitches are evaluated: the pitch must land in a cell every method values from its own
                    // bin. Off-grid pitches and pool-fallback cells are thrown out before anything is accumulated.
                    bool competitive = true;
                    for (int m = 0; m < METHODS.Length; m++)
                    {
                        SituationPrediction prediction = predictions[m][key];
                        zones[m] = METHODS[m].Bin.GetZone(p);
                        if (zones[m] is not PitchZone z || !prediction.Valid[z.Row, z.Column])
                        {
                            competitive = false;
                            break;
                        }

                        predicted[m] = prediction.Cells[z.Row, z.Column];
                    }
                    if (!competitive)
                    {
                        continue;
                    }

                    double baselineError = mean - p.RunValueSmoothedHitter;
                    yearTotals.Sse[0] += baselineError * baselineError;

                    // Region comes from the reference method's cell so every method is scored on the same pitches per region
                    YearTotals regionYear = METHODS[REFERENCE_METHOD].Bin.IsInStrikeZone(zones[REFERENCE_METHOD]!.Value)
                        ? inZoneYear
                        : outZoneYear;
                    regionYear.Sse[0] += baselineError * baselineError;

                    double referenceError = predicted[REFERENCE_METHOD] - p.RunValueSmoothedHitter;

                    for (int m = 0; m < METHODS.Length; m++)
                    {
                        double error = predicted[m] - p.RunValueSmoothedHitter;
                        double mse = error * error;
                        yearTotals.Sse[m + 1] += mse;
                        regionYear.Sse[m + 1] += mse;
                        cellTotals[m].Add(zones[m], mse, referenceError * referenceError, baselineError * baselineError, -error);
                    }

                    yearTotals.Count++;
                    regionYear.Count++;
                }
            }

            return (totals, inZoneTotals, outZoneTotals, cellTotals);
        }

        // Count-weighted mean |cell bias| (runs/100 pitches), split by whether the cell is in the strike zone.
        // Equivalent to sum(count * |bias|) / sum(count), since count * |bias| = |sum of residuals|.
        private static void PlotBiasSummary(string dir, MethodCellTotals[] cellTotals)
        {
            double[] inZone = new double[METHODS.Length];
            double[] outZone = new double[METHODS.Length];
            
            for (int m = 0; m < METHODS.Length; m++)
            {
                double inAbs = 0, outAbs = 0;
                long inCount = 0, outCount = 0;
                
                foreach ((PitchZone zone, CellErrors errors) in cellTotals[m].Cells)
                {
                    double absTotal = Math.Abs(errors.MethodResidualSum);
                    
                    if (METHODS[m].Bin.IsInStrikeZone(zone))
                    {
                        inAbs += absTotal;
                        inCount += errors.Count;
                    }
                    else
                    {
                        outAbs += absTotal;
                        outCount += errors.Count;
                    }
                }

                inZone[m] = inCount == 0 ? 0 : 100.0 * inAbs / inCount;
                outZone[m] = outCount == 0 ? 0 : 100.0 * outAbs / outCount;
            }

            using ScottPlot.Plot plot = new();
            ScottPlot.Palettes.Category10 palette = new();

            ScottPlot.TickGenerators.NumericManual ticks = new();
            ticks.AddMajor(0, "In zone");
            ticks.AddMajor(1, "Out of zone");

            double barWidth = 0.8 / METHODS.Length;
            for (int m = 0; m < METHODS.Length; m++)
            {
                List<ScottPlot.Bar> bars = new();
                double[] values = [inZone[m], outZone[m]];
                for (int group = 0; group < values.Length; group++)
                {
                    double position = group + ((m - ((METHODS.Length - 1) / 2.0)) * barWidth);
                    bars.Add(new ScottPlot.Bar
                    {
                        Position = position,
                        Value = values[group],
                        Size = barWidth * 0.9,
                        FillColor = palette.GetColor(m),
                    });

                    var label = plot.Add.Text($"{values[group]:F3}", position, values[group]);
                    label.LabelFontSize = 16;
                    label.LabelRotation = -90;
                    label.LabelAlignment = ScottPlot.Alignment.MiddleLeft;
                    label.OffsetY = -4;
                }

                var barPlot = plot.Add.Bars(bars);
                barPlot.LegendText = METHODS[m].Name;
            }

            plot.Axes.Bottom.TickGenerator = ticks;
            plot.ShowLegend();
            plot.Title("Count-weighted mean |cell bias| (actual - predicted), all years pooled");
            plot.YLabel("Runs per 100 pitches");
            plot.SavePng(Path.Combine(dir, "BiasSummary.png"), IMAGE_WIDTH, PLOT_HEIGHT);
        }

        private static IQueryable<PitchData> MlbPitches(PitchTrackingDbContext ptDb)
        {
            return ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.LevelId == 1 
                    && f.PitchClass != PitchClass.Other 
                    // Need 1 year prior to build bins
                    && f.Year >= MIN_EVAL_YEAR - 1);
        }

        private static (Dictionary<SituationKey, double> Means, Dictionary<SituationKey, SituationPrediction>[] Predictions) LoadBinYear(List<PitchData> binPitches)
        {
            Dictionary<SituationKey, double> means = SituationMeans(binPitches);
            Dictionary<SituationKey, SituationPrediction>[] predictions = METHODS
                .Select(m => BuildPredictions(m.Bin, binPitches, means))
                .ToArray();
            return (means, predictions);
        }

        private static Dictionary<SituationKey, double> SituationMeans(List<PitchData> binPitches)
        {
            return binPitches
                .GroupBy(p => new SituationKey(p.Year, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike))
                .ToDictionary(g => g.Key, g => g.Average(p => (double)p.RunValueSmoothedHitter));
        }

        private static void PlotMseReduction(string[] names, Dictionary<int, YearTotals> totals, string regionLabel, string path)
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
            plot.Title($"{regionLabel}: Out-of-Sample MSE Reduction vs Prior-Year {names[0]} (all-years baseline MSE={allBaselineMse:G4})");
            plot.XLabel("Evaluation Year (bins from year − 1)");
            plot.YLabel("MSE Reduction (%)");
            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }

        private static Dictionary<SituationKey, SituationPrediction> BuildPredictions(PitchZoneBin bin, List<PitchData> binPitches, Dictionary<SituationKey, double> situationMeans)
        {
            var (competitiveRows, nonCompetitiveRows) = bin.BuildCompetitiveRows(binPitches);

            Dictionary<SituationKey, SituationPrediction> predictions = new();
            var situations = competitiveRows
                .GroupBy(f => new SituationKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes));
            foreach (var situation in situations)
            {
                // Every cell has a row, so the largest indices give this bin's grid size.
                int rows = situation.Max(f => f.ZoneRow) + 1;
                int columns = situation.Max(f => f.ZoneCol) + 1;

                double[,] cells = new double[rows, columns];
                bool[,] validCells = new bool[rows, columns];

                foreach (PitchZoneCompetitive cell in situation)
                {
                    bool valid = cell.IsCompetitive && cell.HasEnoughSample;
                    validCells[cell.ZoneRow, cell.ZoneCol] = valid;
                    cells[cell.ZoneRow, cell.ZoneCol] = valid ? cell.ExpectedValueHitter : float.NaN;
                }

                predictions[situation.Key] = new SituationPrediction(cells, validCells);
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

        private static void PlotCellBias(string dir, MethodCellTotals[] cellTotals)
        {
            const int MIN_BIAS_CELL_PITCHES = 500;
            for (int m = 0; m < METHODS.Length; m++)
            {
                Dictionary<PitchZone, (double Value, string Text)> cells = new();
                foreach ((PitchZone zone, CellErrors errors) in cellTotals[m].Cells)
                {
                    if (errors.Count < MIN_BIAS_CELL_PITCHES)
                    {
                        continue;
                    }
                    double bias = 100.0 * errors.MethodResidualSum / errors.Count;
                    cells[zone] = (bias, $"{bias:+0.0000;-0.0000}\n{errors.Count / 1000.0:F1}k");
                }
                (double Value, string Text)? noZone = null;
                CellErrors noZoneErrors = cellTotals[m].NoZone;
                if (noZoneErrors.Count > 0)
                {
                    double noZoneBias = 100.0 * noZoneErrors.MethodResidualSum / noZoneErrors.Count;
                    noZone = (noZoneBias, $"{noZoneBias:+0.0000;-0.0000}\n{noZoneErrors.Count / 1000.0:F1}k");
                }
                METHODS[m].Bin.PlotCellValues(
                    cells,
                    noZone,
                    $"{METHODS[m].Name}: mean bias (actual - predicted)",
                    Path.Combine(dir, $"Bias_{METHODS[m].Name}.png"),
                    maxAbsValue: 1.0);
            }
        }

        private static void PlotCellReductions(string dir, MethodCellTotals[] cellTotals)
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
                    Path.Combine(dir, $"Cells_{METHODS[m].Name}.png"),
                    maxAbsValue: 5.0);
            }
        }

        private const int MIN_GROUP_PITCHES = 2000;
        private static void PlotSplitMseReductions(string[] names, Dictionary<PitchSplitKey, YearTotals> groupTotals)
        {
            ScottPlot.Palettes.Category10 palette = new();   // same colour per method as PlotMseReduction
            foreach ((PitchSplitKey key, YearTotals totals) in groupTotals)
            {
                if (totals.Count < MIN_GROUP_PITCHES || totals.Sse[0] == 0)
                {
                    continue;
                }
                string pitStr = key.PitIsR ? "RHP" : "LHP";
                string hitStr = key.HitIsR ? "RHH" : "LHH";
                string directory = Path.Combine(PLOT_DIRECTORY, key.PitchClass.ToString(), $"{pitStr}_{hitStr}");
                Directory.CreateDirectory(directory);
                using ScottPlot.Plot plot = new();
                for (int m = 1; m < names.Length; m++)
                {
                    double reduction = 100 * (1 - (totals.Sse[m] / totals.Sse[0]));
                    var bars = plot.Add.Bars(new[]
                    {
                        new ScottPlot.Bar
                        {
                            Position = m - 1,
                            Value = reduction,
                            Size = 0.8,
                            FillColor = palette.GetColor(m - 1),
                        },
                    });

                    bars.LegendText = names[m];
                    var label = plot.Add.Text($"{reduction:F2}%", m - 1, reduction);
                    label.LabelFontSize = 16;
                    label.LabelRotation = -90;
                    label.LabelAlignment = ScottPlot.Alignment.MiddleLeft;
                    label.OffsetY = -4;
                }
                plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual();   // bars are identified by the legend
                plot.ShowLegend();
                double baselineMse = totals.Sse[0] / totals.Count;
                plot.Title($"{key.PitchClass} {pitStr} vs {hitStr} {key.Balls}-{key.Strikes} | n={totals.Count:N0} | " +
                    $"MSE reduction vs {names[0]} (baseline MSE={baselineMse:G4})");
                plot.YLabel("MSE Reduction (%)");
                plot.SavePng(Path.Combine(directory, $"{key.Balls}-{key.Strikes}.png"), IMAGE_WIDTH, PLOT_HEIGHT);
            }
        }
    }
}
