using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using ShellProgressBar;
using static Db.DbEnums;
using PitchAnalysis.PitchZoneBinNS;

namespace PitchAnalysis.BinEvaluationNS
{
    using YearTotalsDict = Dictionary<int, YearTotals>;
    internal class YearTotals(int methodCount)
    {
        public double[] Sse { get; } = new double[methodCount];
        public long Count { get; set; }
    }

    internal static partial class BinEvaluation
    {
        private const string PLOT_DIRECTORY = "BinEvaluationPlots";
        private const int IMAGE_WIDTH = 2000;
        private const int PLOT_HEIGHT = 800;
        private const int MIN_EVAL_YEAR = 2022;

        private readonly record struct SituationKey(int Year, PitchClass PitchClass, bool PitIsR, bool HitIsR, int Balls, int Strikes);

        // Everything accumulated for one scenario (evaluation year x pitch class x handedness x count)
        private class ScenarioTotals
        {
            public YearTotals All { get; } = new(METHODS.Length + 1);
            public YearTotals InZone { get; } = new(METHODS.Length + 1);
            public YearTotals OutZone { get; } = new(METHODS.Length + 1);
            public MethodCellTotals[] Cells { get; } = [.. METHODS.Select(_ => new MethodCellTotals())];
        }
        // Scenario-level plots: smaller cells, so a lower count floor and wider colour scales than the pooled plots
        private const int MIN_SCENARIO_PITCHES = 100;

        // A method's prediction for every cell of a situation, plus pitches that land outside the grid
        private record SituationPrediction(double[,] Cells, bool[,] Valid);
        
        public static void Evaluate()
        {
            using PitchTrackingDbContext ptDb = new(Connection.PITCHTRACK_DB_READONLY_OPTIONS);
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
    }
}
