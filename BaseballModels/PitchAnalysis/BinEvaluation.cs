using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.Support.UI;
using PitchTrackingDb;
using ShellProgressBar;
using System.Diagnostics;
using System;
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

        private readonly record struct SituationKey(int Year, PitchClass PitchClass, bool PitIsR, bool HitIsR, int Balls, int Strikes);

        // Everything accumulated for one scenario (evaluation year x pitch class x handedness x count)
        private sealed class ScenarioTotals
        {
            public YearTotals All { get; } = new(METHODS.Length + 1);
            public YearTotals InZone { get; } = new(METHODS.Length + 1);
            public YearTotals OutZone { get; } = new(METHODS.Length + 1);
            public MethodCellTotals[] Cells { get; } = [.. METHODS.Select(_ => new MethodCellTotals())];
        }
        // Scenario-level plots: smaller cells, so a lower count floor and wider colour scales than the pooled plots
        private const int MIN_SCENARIO_PITCHES = 100;

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

            new("Gaussian", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0) })),

            new("Gaussian Border", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true })),

            new("CountCap", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=false, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            new("Border CountCap", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            
            new("Border CountCap Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true })),
            new("Border CountCap Split-Middle 2Yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true, YearsPerBin = 2 })),
            new("Border CountCap Split-Middle 3Yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true, YearsPerBin = 3 })),

            new("Border CountCap 2yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 2 })),
            new("Border CountCap 3yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 3 })),

            new("Fade InZone 4_1000", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 1000, OutZoneSmoothingCutoff = 500 })),

            new("Border CountCap x1 4yr d0.75", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75 })),

            new("Border CountCap x1 3yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 3, YearDecay = 0.75, SplitMiddleRow=true })),
            new("Border CountCap x1 4yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow=true })),
            new("Border CountCap x1 5yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 5, YearDecay = 0.75, SplitMiddleRow=true })),

            new("4yr d0.75 Border Split", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow = true })),
            new("4yr d0.75 Border Split Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow = true, SeparateMiddle = true })),
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

            Dictionary<SituationKey, ScenarioTotals> allScenarios = new();

            PitchClass[] pitchClasses = [PitchClass.Fastball, PitchClass.Breaking, PitchClass.Changeup, PitchClass.Other];
            using ProgressBar progressBar = new(pitchClasses.Length, "Pitch classes");
            foreach (var pitchClass in pitchClasses)
            {
                // Load Data
                List<PitchData> classPitches = pitchClass == PitchClass.Other ?
                    pitches : 
                    pitches.Where(f => f.PitchClass == pitchClass).ToList();

                (YearTotalsDict all, YearTotalsDict inZone, YearTotalsDict outZone, MethodCellTotals[] cellTotals, Dictionary<SituationKey, ScenarioTotals> scenarios) = GetYearTotals(classPitches, means, predictions);

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

                // Scenario breakdowns only make sense per pitch class
                if (pitchClass != PitchClass.Other)
                {
                    PlotScenarios(dir, names, scenarios, progressBar);
                    WriteScenarioSummary(dir, names, scenarios);

                    foreach ((SituationKey key, ScenarioTotals scenario) in scenarios)
                    {
                        allScenarios[key] = scenario;
                    }
                }

                progressBar.Tick();
            }

            WriteSituationSummary(Path.Combine(PLOT_DIRECTORY, "SituationSummary.csv"), names, allScenarios);
        }

        // Writes the full plot set for every scenario into {pitchClass}/{year}/{balls}-{strikes}/{RHP|LHP}_{RHH|LHH}/
        private static void PlotScenarios(string classDir, string[] names, Dictionary<SituationKey, ScenarioTotals> scenarios, IProgressBar progress)
        {
            List<KeyValuePair<SituationKey, ScenarioTotals>> toPlot = scenarios
                .Where(pair => pair.Value.All.Count >= MIN_SCENARIO_PITCHES && pair.Value.All.Sse[0] != 0)
                .ToList();

            using ChildProgressBar scenarioBar = progress.Spawn(toPlot.Count, "Scenarios");
            Stopwatch stopwatch = Stopwatch.StartNew();
            object tickLock = new();
            int completed = 0;

            Parallel.ForEach(toPlot,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                pair =>
            {
                (SituationKey key, ScenarioTotals scenario) = pair;

                string pitStr = key.PitIsR ? "RHP" : "LHP";
                string hitStr = key.HitIsR ? "RHH" : "LHH";
                string directory = Path.Combine(classDir, $"{key.Balls}-{key.Strikes}", $"{pitStr}_{hitStr}", key.Year.ToString());
                Directory.CreateDirectory(directory);

                string label = $"{key.Year} {key.PitchClass} {pitStr} vs {hitStr} {key.Balls}-{key.Strikes}";
                (string Name, YearTotals Totals)[] regions =
                [
                    ("All", scenario.All),
                    ("In-Zone", scenario.InZone),
                    ("Out-Zone", scenario.OutZone),
                ];
                foreach ((string regionName, YearTotals totals) in regions)
                {
                    // A region can be empty or constant in a small scenario, which would divide by zero
                    if (totals.Count == 0 || totals.Sse[0] == 0)
                    {
                        continue;
                    }
                    string file = regionName == "All" ? "MseReduction.png" : $"MseReduction_{regionName.Replace("-", "")}.png";
                    PlotMseReduction(names, new Dictionary<int, YearTotals> { [key.Year] = totals },
                        $"{label} {regionName}", Path.Combine(directory, file), includePooled: false);
                }

                //PlotCellBias(directory, scenario.Cells, MIN_SCENARIO_BIAS_CELL_PITCHES, SCENARIO_BIAS_SCALE);
                PlotBiasSummary(directory, scenario.Cells);

                lock (tickLock)
                {
                    completed++;
                    TimeSpan estimatedTotal = TimeSpan.FromTicks(stopwatch.Elapsed.Ticks * toPlot.Count / completed);
                    scenarioBar.Tick(completed, estimatedTotal, $"{key.Year} {key.PitchClass} {key.Balls}-{key.Strikes} {pitStr}_{hitStr}");
                }
            });
        }

        private static (YearTotalsDict all, YearTotalsDict inZone, YearTotalsDict outZone, MethodCellTotals[] cellTotals, Dictionary<SituationKey, ScenarioTotals> scenarios) 
            GetYearTotals(List<PitchData> pitches, Dictionary<SituationKey, double> means, Dictionary<SituationKey, SituationPrediction>[] predictions)
        {
            YearTotalsDict totals = new();
            YearTotalsDict inZoneTotals = new();
            YearTotalsDict outZoneTotals = new();
            MethodCellTotals[] cellTotals = [.. METHODS.Select(_ => new MethodCellTotals())];
            Dictionary<SituationKey, ScenarioTotals> scenarioTotals = new();

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

                    SituationKey scenarioKey = new(p.Year, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike);
                    if (!scenarioTotals.TryGetValue(scenarioKey, out ScenarioTotals? scenario))
                    {
                        scenario = new();
                        scenarioTotals[scenarioKey] = scenario;
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
                    bool inStrikeZone = METHODS[REFERENCE_METHOD].Bin.IsInStrikeZone(zones[REFERENCE_METHOD]!.Value);
                    YearTotals regionYear = inStrikeZone ? inZoneYear : outZoneYear;
                    YearTotals scenarioRegion = inStrikeZone ? scenario.InZone : scenario.OutZone;
                    regionYear.Sse[0] += baselineError * baselineError;
                    scenario.All.Sse[0] += baselineError * baselineError;
                    scenarioRegion.Sse[0] += baselineError * baselineError;

                    double referenceError = predicted[REFERENCE_METHOD] - p.RunValueSmoothedHitter;

                    for (int m = 0; m < METHODS.Length; m++)
                    {
                        double error = predicted[m] - p.RunValueSmoothedHitter;
                        double mse = error * error;
                        yearTotals.Sse[m + 1] += mse;
                        regionYear.Sse[m + 1] += mse;
                        scenario.All.Sse[m + 1] += mse;
                        scenarioRegion.Sse[m + 1] += mse;

                        cellTotals[m].Add(zones[m], mse, referenceError * referenceError, baselineError * baselineError, -error);
                        scenario.Cells[m].Add(zones[m], mse, referenceError * referenceError, baselineError * baselineError, -error);
                    }

                    yearTotals.Count++;
                    regionYear.Count++;
                    scenario.All.Count++;
                    scenarioRegion.Count++;
                }
            }

            return (totals, inZoneTotals, outZoneTotals, cellTotals, scenarioTotals);
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
            int firstLoadedYear = MIN_EVAL_YEAR - METHODS.Max(m => m.Bin.YearsPerBin);
            return ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.LevelId == 1 
                    && f.PitchClass != PitchClass.Other 
                    && f.Year >= firstLoadedYear);
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

        private static void PlotMseReduction(string[] names, Dictionary<int, YearTotals> totals, string regionLabel, string path, bool includePooled = true)
        {
            // Columns: each evaluation year, then all years pooled
            List<(string Label, double[] Sse, long Count)> columns = totals
                .OrderBy(kv => kv.Key)
                .Select(kv => (kv.Key.ToString(), kv.Value.Sse, kv.Value.Count))
                .ToList();
            if (includePooled)
            {
                columns.Add(("All",
                    Enumerable.Range(0, names.Length).Select(m => totals.Values.Sum(t => t.Sse[m])).ToArray(),
                    totals.Values.Sum(t => t.Count)));
            }

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
            string baselineLabel = includePooled ? "all-years baseline MSE" : "baseline MSE";
            plot.Title($"{regionLabel}: Out-of-Sample MSE Reduction vs Prior-Year {names[0]} ({baselineLabel}={allBaselineMse:G4})");
            plot.XLabel("Evaluation Year (bins from year − 1)");
            plot.YLabel("MSE Reduction (%)");
            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }

        private static Dictionary<SituationKey, SituationPrediction> BuildPredictions(PitchZoneBin bin, List<PitchData> binPitches, Dictionary<SituationKey, double> situationMeans)
        {
            var (competitiveRows, nonCompetitiveRows) = bin.BuildCompetitiveRows(binPitches, MIN_EVAL_YEAR - 1);

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

        private static void PlotCellBias(string dir, MethodCellTotals[] cellTotals, int minCellPitches = 500, double maxAbsValue = 1.0)
        {
            for (int m = 0; m < METHODS.Length; m++)
            {
                Dictionary<PitchZone, (double Value, string Text)> cells = new();
                foreach ((PitchZone zone, CellErrors errors) in cellTotals[m].Cells)
                {
                    if (errors.Count < minCellPitches)
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
                    maxAbsValue: maxAbsValue);
            }
        }

        private static void PlotCellReductions(string dir, MethodCellTotals[] cellTotals, double maxAbsValue = 5.0)
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
                    maxAbsValue: maxAbsValue);
            }
        }

        // One row per scenario (no size threshold) with MSE reduction vs the baseline for every method, so
        // scenarios can be sorted by size against how each method does.
        private static void WriteScenarioSummary(string classDir, string[] names, Dictionary<SituationKey, ScenarioTotals> scenarios)
        {
            static double Reduction(YearTotals totals, int method)
            {
                return totals.Count == 0 || totals.Sse[0] == 0
                    ? double.NaN
                    : 100 * (1 - (totals.Sse[method] / totals.Sse[0]));
            }

            List<string> lines = [];
            List<string> header = ["Year", "Balls", "Strikes", "Hands", "N", "InZoneN", "OutZoneN", "BaselineMse"];
            for (int m = 1; m < names.Length; m++)
            {
                header.Add($"{names[m]} All %");
                header.Add($"{names[m]} In %");
                header.Add($"{names[m]} Out %");
            }
            lines.Add(string.Join(",", header));

            foreach ((SituationKey key, ScenarioTotals scenario) in scenarios
                .OrderBy(kv => kv.Key.Year)
                .ThenBy(kv => kv.Key.Balls)
                .ThenBy(kv => kv.Key.Strikes)
                .ThenBy(kv => kv.Key.PitIsR)
                .ThenBy(kv => kv.Key.HitIsR))
            {
                if (scenario.All.Count == 0)
                {
                    continue;
                }
                List<string> row =
                [
                    key.Year.ToString(),
                    key.Balls.ToString(),
                    key.Strikes.ToString(),
                    $"{(key.PitIsR ? "RHP" : "LHP")}_{(key.HitIsR ? "RHH" : "LHH")}",
                    scenario.All.Count.ToString(),
                    scenario.InZone.Count.ToString(),
                    scenario.OutZone.Count.ToString(),
                    (scenario.All.Sse[0] / scenario.All.Count).ToString("G5"),
                ];
                for (int m = 1; m < names.Length; m++)
                {
                    row.Add(Reduction(scenario.InZone, m).ToString("F3"));
                    row.Add(Reduction(scenario.OutZone, m).ToString("F3"));
                }
                lines.Add(string.Join(",", row));
            }

            File.WriteAllLines(Path.Combine(classDir, "ScenarioSummary.csv"), lines);
        }

        // One row per (pitch class, count, handedness, region) with years pooled, listing the best methods by MSE reduction vs baseline.
        private static void WriteSituationSummary(string path, string[] names, Dictionary<SituationKey, ScenarioTotals> scenarios)
        {
            const int ELEMENTS_TO_SHOW = 3;

            List<string> lines = [];
            List<string> header = ["PitchClass", "Balls", "Strikes", "Hands", "Region", "N", "BaselineMse"];
            for (int rank = 1; rank <= ELEMENTS_TO_SHOW; rank++)
            {
                header.Add($"Best{rank} Method");
                header.Add($"Best{rank} MSE Reduction %");
            }
            lines.Add(string.Join(",", header));

            (string Region, Func<ScenarioTotals, YearTotals> Select)[] regions =
            [
                ("In-Zone", s => s.InZone),
                ("Out-Zone", s => s.OutZone),
            ];

            var situations = scenarios
                .GroupBy(pair => (pair.Key.PitchClass, pair.Key.PitIsR, pair.Key.HitIsR, pair.Key.Balls, pair.Key.Strikes))
                .OrderBy(g => g.Key.PitchClass)
                .ThenBy(g => g.Key.Balls)
                .ThenBy(g => g.Key.Strikes)
                .ThenBy(g => g.Key.PitIsR)
                .ThenBy(g => g.Key.HitIsR);

            foreach (var situation in situations)
            {
                string hands = $"{(situation.Key.PitIsR ? "RHP" : "LHP")}_{(situation.Key.HitIsR ? "RHH" : "LHH")}";

                foreach ((string region, Func<ScenarioTotals, YearTotals> select) in regions)
                {
                    // Pool every year's totals for this situation and region
                    double[] sse = new double[names.Length];
                    long count = 0;
                    foreach (KeyValuePair<SituationKey, ScenarioTotals> pair in situation)
                    {
                        YearTotals totals = select(pair.Value);
                        for (int m = 0; m < names.Length; m++)
                        {
                            sse[m] += totals.Sse[m];
                        }
                        count += totals.Count;
                    }

                    if (count == 0 || sse[0] == 0)
                    {
                        continue;
                    }

                    List<string> row =
                    [
                        situation.Key.PitchClass.ToString(),
                        situation.Key.Balls.ToString(),
                        situation.Key.Strikes.ToString(),
                        hands,
                        region,
                        count.ToString(),
                        (sse[0] / count).ToString("G5"),
                    ];

                    // names[0] is the baseline; rank the rest by lowest pooled SSE
                    foreach (int m in Enumerable.Range(1, names.Length - 1).OrderBy(m => sse[m]).Take(ELEMENTS_TO_SHOW))
                    {
                        row.Add(names[m]);
                        row.Add((100 * (1 - (sse[m] / sse[0]))).ToString("F3"));
                    }

                    lines.Add(string.Join(",", row));
                }
            }

            File.WriteAllLines(path, lines);
        }
    }
}
