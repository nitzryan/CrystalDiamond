using DataAquisition.PitchTracking;
using Db;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ScottPlot;
using SkiaSharp;

namespace PitchAnalysis
{
    internal static class StuffLocationCorrelation
    {
        private const int MIN_PITCHES = 100;              // pitches with a location value per pitcher/pitchType/year
        private const double RUNS_SCALE = 100;            // display as hitter runs per 100 pitches
        private const int RANK_COUNT = 5;
        private const string PLOT_DIRECTORY = "StuffLocationPlots";
        private const int IMAGE_WIDTH = 1200;
        private const int PLOT_HEIGHT = 900;
        private const float TABLE_FONT_SIZE = 15;
        private const int TABLE_LINE_HEIGHT = 22;
        private const int TABLE_PADDING = 20;
        private const int MAX_DISPLAY_POINTS = 50000;     // location-response scatter is subsampled for drawing only

        private readonly record struct GroupKey(int Model, int Year, int MlbId, Db.DbEnums.PitchType PitchType);
        private record DataPoint(GroupKey Key, string Label, double Stuff, double Location);

        // Breaking down by pitches found in bin
        // Bin PitchCount strata for the location-response breakdown: [edge_i, edge_i+1), last is open-ended
        internal static readonly int[] BIN_COUNT_EDGES = [0, 10, 20, 50, 100, 200, 500, 1000, 2000];
        private const int MIN_STRATUM_PITCHES = 1000;     // strata with fewer competitive pitches aren't plotted

        private readonly record struct PitchInfo(Db.DbEnums.PitchType PitchType, Db.DbEnums.PitchClass PitchClass,
            bool PitIsR, bool HitIsR, int Balls, int Strikes, float PlateX, float PlateZ, float ZoneTop, float ZoneBot);
        internal readonly record struct PzcKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes, int Row, int Col);

        public static void GeneratePlots()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            // Pitch type for every pitch in years that have output values
            List<int> years = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .Select(f => f.Year)
                .Distinct()
                .Where(f => f != 2021) // Covid Year lacks data for pitch zones
                .ToList();
            Dictionary<(int, int), PitchInfo> pitchInfoDict = ptDb.PitchData
                .AsNoTracking()
                .Where(f => years.Contains(f.Year) && f.LevelId == 1)
                .Select(f => new {
                    f.GameId, f.PitchId, f.PitchType, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                    f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot
                })
                .ToDictionary(
                    f => (f.GameId, f.PitchId),
                    f => new PitchInfo(f.PitchType, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                                       f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot));
            Console.WriteLine($"Loaded {pitchInfoDict.Count} Pitches");

            // Sample size of every valid bin (the only bins competitive pitches can have been valued with)
            Dictionary<PzcKey, int> binCountDict = ptDb.PitchZoneCompetitive
                .AsNoTracking()
                .Where(f => f.IsCompetitive && f.HasEnoughSample)
                .ToDictionary(
                    f => new PzcKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes, f.ZoneRow, f.ZoneCol),
                    f => f.PitchCount);

            // Competitive pitches per group: (bin value, stuffBinRuns, bin PitchCount); combined = bin + stuffBin
            Dictionary<GroupKey, List<(float Bin, float StuffBin, int BinCount)>> competitivePitches = new();

            // Stream OPVA, accumulating per model/pitcher/pitchType/year.
            // Every row with a stuff value also has a location value, so filtering on location loses nothing.
            Dictionary<GroupKey, (RunningStatistics Stuff, RunningStatistics Loc)> groups = new();
            foreach (var pva in pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .Where(f => f.LocBinRuns != null && f.LevelId == 1 && f.Year != 2021)
                .Select(f => new { f.Model, f.GameId, f.PitchId, f.Year, f.MlbId, f.StuffBinRuns, f.LocBinRuns }))
            {
                PitchInfo pitch = pitchInfoDict[(pva.GameId, pva.PitchId)];
                GroupKey key = new(pva.Model, pva.Year, pva.MlbId, pitch.PitchType);
                if (!groups.TryGetValue(key, out var stats))
                {
                    stats = (new RunningStatistics(), new RunningStatistics());
                    groups[key] = stats;
                }
                stats.Loc.Push(pva.LocBinRuns!.Value);
                if (pva.StuffBinRuns.HasValue)
                {
                    stats.Stuff.Push(pva.StuffBinRuns.Value);
                    if (!competitivePitches.TryGetValue(key, out var pitchList))
                    {
                        pitchList = new();
                        competitivePitches[key] = pitchList;
                    }
                    // Prior-year bin this competitive pitch was valued with; must exist or OPVA is out of sync with PZC
                    if (PitchBinCompetitiveness.GetZone(pitch.PlateX, pitch.PlateZ, pitch.ZoneTop, pitch.ZoneBot)
                        is not PitchBinCompetitiveness.PitchZone zone)
                        throw new InvalidOperationException(
                            $"Competitive pitch {pva.GameId}/{pva.PitchId} has no zone; OPVA is out of sync with PitchZoneCompetitive.");
                    int binCount = binCountDict[new PzcKey(pva.Year - 1, pitch.PitchClass, pitch.PitIsR, pitch.HitIsR,
                        pitch.Balls, pitch.Strikes, zone.Row, zone.Column)];
                    pitchList.Add((pva.LocBinRuns!.Value, pva.StuffBinRuns.Value, binCount));
                }
            }
            Console.WriteLine($"Created {groups.Count} Pitcher/PitchType/Year Groups");

            Dictionary<int, string> names = db.Player
                .AsNoTracking()
                .Select(f => new { f.MlbId, f.UseFirstName, f.UseLastName })
                .ToDictionary(f => f.MlbId, f => $"{f.UseFirstName} {f.UseLastName}");

            // Qualifying datapoints
            List<DataPoint> dataPoints = new();
            foreach (var (key, stats) in groups)
            {
                if (stats.Loc.Count < MIN_PITCHES)
                    continue;
                if (stats.Stuff.Count == 0)
                    throw new InvalidOperationException(
                        $"Model {key.Model}, {names[key.MlbId]} ({key.MlbId}) {key.PitchType} {key.Year}: " +
                        $"{stats.Loc.Count} pitches with a location value but none with a stuff value.");
                dataPoints.Add(new DataPoint(key, $"{names[key.MlbId]} {key.PitchType} {key.Year}",
                    stats.Stuff.Mean * RUNS_SCALE, stats.Loc.Mean * RUNS_SCALE));
            }

            if (Directory.Exists(PLOT_DIRECTORY))
                Directory.Delete(PLOT_DIRECTORY, recursive: true);
            Directory.CreateDirectory(PLOT_DIRECTORY);

            foreach (int model in groups.Keys.Select(k => k.Model).Distinct().OrderBy(m => m))
            {
                // Years with at least one non-null stuff value for this model
                List<int> modelYears = groups
                    .Where(g => g.Key.Model == model && g.Value.Stuff.Count > 0)
                    .Select(g => g.Key.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList();
                List<DataPoint> modelPoints = dataPoints.Where(p => p.Key.Model == model).ToList();

                foreach (int year in modelYears)
                {
                    List<DataPoint> yearPoints = modelPoints.Where(p => p.Key.Year == year).ToList();
                    CreatePlot(yearPoints,
                        $"Model {model} - {year}", Path.Combine(PLOT_DIRECTORY, $"Model{model}_{year}.png"));
                    CreateLocationResponsePlot(yearPoints, competitivePitches,
                        $"Model {model} - {year}", Path.Combine(PLOT_DIRECTORY, $"Model{model}_{year}_LocResponse.png"));
                    CreateBinCountResponsePlot(yearPoints, competitivePitches,
                        $"Model {model} - {year}", Path.Combine(PLOT_DIRECTORY, $"Model{model}_{year}_BinCount.png"));
                }
                CreatePlot(modelPoints,
                    $"Model {model} - All Years", Path.Combine(PLOT_DIRECTORY, $"Model{model}_AllYears.png"));
                CreateLocationResponsePlot(modelPoints, competitivePitches,
                    $"Model {model} - All Years", Path.Combine(PLOT_DIRECTORY, $"Model{model}_AllYears_LocResponse.png"));
                CreateBinCountResponsePlot(modelPoints, competitivePitches,
                    $"Model {model} - All Years", Path.Combine(PLOT_DIRECTORY, $"Model{model}_AllYears_BinCount.png"));
            }
        }

        private static void CreatePlot(List<DataPoint> points, string title, string path)
        {
            if (points.Count < 3)
                throw new InvalidOperationException($"{title}: only {points.Count} qualifying datapoints, cannot fit/test.");

            double[] xs = points.Select(p => p.Stuff).ToArray();
            double[] ys = points.Select(p => p.Location).ToArray();

            (double intercept, double slope) = Fit.Line(xs, ys);
            double r = Correlation.Pearson(xs, ys);

            // One-sided test of H1: correlation < 0 (Pearson t-test, df = n - 2; identical to the slope t-test)
            int df = points.Count - 2;
            double t = r * Math.Sqrt(df / (1 - r * r));
            double pValue = StudentT.CDF(0, 1, df, t);

            using ScottPlot.Plot plot = new();
            var scatter = plot.Add.ScatterPoints(xs, ys);
            scatter.Color = ScottPlot.Colors.SteelBlue.WithAlpha(0.5);
            scatter.MarkerSize = 4;

            double xMin = xs.Min();
            double xMax = xs.Max();
            var trend = plot.Add.Line(xMin, intercept + slope * xMin, xMax, intercept + slope * xMax);
            trend.LineStyle.Color = ScottPlot.Colors.Red;
            trend.LineStyle.Width = 2;

            plot.Title($"{title} | n={points.Count:N0} | slope={slope:F3} | r={r:F3} | one-sided p={pValue:G3}");
            plot.XLabel("Stuff (stuffBinRuns, hitter runs / 100 pitches)");
            plot.YLabel("Location (locBinRuns, hitter runs / 100 pitches)");

            SaveWithRankings(plot, points, path);
        }

        // Values are hitter runs, so lowest = best for the pitcher
        private static List<string> RankingLines(string metric, List<DataPoint> points, Func<DataPoint, double> value)
        {
            List<string> lines = [$"Best {metric} (lowest)"];
            lines.AddRange(points.OrderBy(value).Take(RANK_COUNT)
                .Select((p, i) => $"{i + 1}. {p.Label,-34}{value(p),7:F2}"));
            lines.Add("");
            lines.Add($"Worst {metric} (highest)");
            lines.AddRange(points.OrderByDescending(value).Take(RANK_COUNT)
                .Select((p, i) => $"{i + 1}. {p.Label,-34}{value(p),7:F2}"));
            return lines;
        }

        private static void SaveWithRankings(ScottPlot.Plot plot, List<DataPoint> points, string path)
        {
            List<string>[] columns =
            [
                RankingLines("Stuff", points, p => p.Stuff),
                RankingLines("Location", points, p => p.Location),
            ];
            int tableHeight = 2 * TABLE_PADDING + columns.Max(c => c.Count) * TABLE_LINE_HEIGHT;

            using SKBitmap plotBitmap = SKBitmap.Decode(plot.GetImageBytes(IMAGE_WIDTH, PLOT_HEIGHT, ScottPlot.ImageFormat.Png));
            using SKSurface surface = SKSurface.Create(new SKImageInfo(IMAGE_WIDTH, PLOT_HEIGHT + tableHeight));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(plotBitmap, 0, 0);

            using SKPaint paint = new() { Color = SKColors.Black, IsAntialias = true };
            using SKFont font = new(SKTypeface.FromFamilyName("Consolas"), TABLE_FONT_SIZE);   // monospace so values align
            for (int c = 0; c < columns.Length; c++)
            {
                float x = TABLE_PADDING + c * (IMAGE_WIDTH / columns.Length);
                for (int i = 0; i < columns[c].Count; i++)
                    canvas.DrawText(columns[c][i], x, PLOT_HEIGHT + TABLE_PADDING + (i + 1) * TABLE_LINE_HEIGHT, font, paint);
            }

            using SKImage image = surface.Snapshot();
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, data.ToArray());
        }

        // Within-group slope of CombinedRuns on bin value over competitive pitches.
        // k = 1: combined fully reflects location, no mechanical coupling. k < 1: stuffBinRuns contains -(1-k)*bin.
        private static void CreateLocationResponsePlot(List<DataPoint> points,
            Dictionary<GroupKey, List<(float Bin, float StuffBin, int BinCount)>> competitivePitches, string title, string path)
        {
            // Demean within each pitcher/pitchType/year so only pitch-to-pitch location variation remains
            List<double> xList = new();
            List<double> yList = new();
            foreach (DataPoint p in points)
            {
                List<(float Bin, float StuffBin, int BinCount)> pitches = competitivePitches[p.Key];
                double binMean = pitches.Average(f => (double)f.Bin);
                double combinedMean = pitches.Average(f => (double)f.Bin + f.StuffBin);
                foreach (var(bin, stuffBin, _) in pitches)
                {
                    xList.Add(bin - binMean);
                    yList.Add(bin + stuffBin - combinedMean);
                }
            }
            double[] xs = xList.ToArray();
            double[] ys = yList.ToArray();

            (double intercept, double slope) = Fit.Line(xs, ys);
            double r = Correlation.Pearson(xs, ys);

            using ScottPlot.Plot plot = new();
            int step = Math.Max(1, xs.Length / MAX_DISPLAY_POINTS);
            var scatter = plot.Add.ScatterPoints(
                xs.Where((_, i) => i % step == 0).ToArray(),
                ys.Where((_, i) => i % step == 0).ToArray());
            scatter.Color = ScottPlot.Colors.SteelBlue.WithAlpha(0.15);
            scatter.MarkerSize = 2;

            double xMin = xs.Min();
            double xMax = xs.Max();
            var fit = plot.Add.Line(xMin, intercept + slope * xMin, xMax, intercept + slope * xMax);
            fit.LineStyle.Color = ScottPlot.Colors.Red;
            fit.LineStyle.Width = 2;
            var reference = plot.Add.Line(xMin, xMin, xMax, xMax);   // k = 1
            reference.LineStyle.Color = ScottPlot.Colors.Black;
            reference.LineStyle.Width = 1.5f;
            reference.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;

            plot.Title($"{title} | Location Response k={slope:F3} (dashed = 1) | R²={r * r:F3} | pitches={xs.Length:N0}");
            plot.XLabel("Bin value − group mean (hitter runs / pitch)");
            plot.YLabel("CombinedRuns − group mean (hitter runs / pitch)");

            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }

        // Location response k split by the sample size of the bin each pitch was valued with.
        // If low k is bin noise (attenuation), k should climb toward 1 as bin PitchCount grows.
        private static void CreateBinCountResponsePlot(List<DataPoint> points,
            Dictionary<GroupKey, List<(float Bin, float StuffBin, int BinCount)>> competitivePitches, string title, string path)
        {
            int bucketCount = BIN_COUNT_EDGES.Length;
            List<double>[] xs = Enumerable.Range(0, bucketCount).Select(_ => new List<double>()).ToArray();
            List<double>[] ys = Enumerable.Range(0, bucketCount).Select(_ => new List<double>()).ToArray();

            // Demean within each group AND stratum so every stratum's k is a within-group slope
            foreach (DataPoint p in points)
            {
                foreach (var stratum in competitivePitches[p.Key]
                    .GroupBy(f => Array.FindLastIndex(BIN_COUNT_EDGES, e => f.BinCount >= e)))
                {
                    double binMean = stratum.Average(f => (double)f.Bin);
                    double combinedMean = stratum.Average(f => (double)f.Bin + f.StuffBin);
                    foreach (var (bin, stuffBin, _) in stratum)
                    {
                        xs[stratum.Key].Add(bin - binMean);
                        ys[stratum.Key].Add(bin + stuffBin - combinedMean);
                    }
                }
            }

            using ScottPlot.Plot plot = new();
            ScottPlot.TickGenerators.NumericManual ticks = new();
            List<double> plotXs = new();
            List<double> plotKs = new();
            for (int b = 0; b < bucketCount; b++)
            {
                string range = b + 1 < bucketCount
                    ? $"{BIN_COUNT_EDGES[b]}-{BIN_COUNT_EDGES[b + 1] - 1}"
                    : $"{BIN_COUNT_EDGES[b]}+";
                if (xs[b].Count < MIN_STRATUM_PITCHES)
                {
                    ticks.AddMajor(b, range);
                    continue;
                }

                double[] x = xs[b].ToArray();
                double[] y = ys[b].ToArray();
                var (intercept, k) = Fit.Line(x, y);
                // Slope standard error: residual SE / sqrt(sum of squared demeaned x)
                double kSe = GoodnessOfFit.StandardError(x.Select(v => intercept + k * v), y, 2) / Math.Sqrt(x.Sum(v => v * v));

                ticks.AddMajor(b, $"{range}\nn={x.Length:N0}");
                plotXs.Add(b);
                plotKs.Add(k);
                var label = plot.Add.Text($"{k:F2} ±\n{kSe:F2}", b, k);
                label.LabelFontSize = 16;
                label.LabelAlignment = ScottPlot.Alignment.LowerCenter;
                label.OffsetY = -12;
                label.OffsetX = -24;
            }

            var kLine = plot.Add.Scatter(plotXs.ToArray(), plotKs.ToArray());
            kLine.LineWidth = 2;
            kLine.MarkerSize = 8;
            var reference = plot.Add.Line(-0.5, 1, bucketCount - 0.5, 1);   // k = 1
            reference.LineStyle.Color = ScottPlot.Colors.Black;
            reference.LineStyle.Width = 1.5f;
            reference.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;

            plot.Axes.Bottom.TickGenerator = ticks;
            plot.Title($"{title} | Location Response k by Prior-Year Bin Sample Size (dashed = 1)");
            plot.XLabel("Bin PitchCount");
            plot.YLabel("Within-group k");
            plot.SavePng(path, IMAGE_WIDTH, PLOT_HEIGHT);
        }
    }
}