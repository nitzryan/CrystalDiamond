using PitchAnalysis.PitchZoneBinNS;
using ShellProgressBar;
using System.Diagnostics;

namespace PitchAnalysis.BinEvaluationNS
{
    using YearTotalsDict = Dictionary<int, YearTotals>;
    internal static partial class BinEvaluation
    {
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
                        PlotMseReduction(names, new YearTotalsDict { [key.Year] = totals },
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

        private static void PlotMseReduction(string[] names, YearTotalsDict totals, string regionLabel, string path, bool includePooled = true)
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
    }
}
