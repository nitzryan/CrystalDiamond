using Db;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using OpenQA.Selenium.BiDi.Script;
using ScottPlot;
using SiteDb;

namespace SitePrep.Draft
{
    public enum DraftPickTarget
    {
        Actual,
        Model,
    }

    internal class PickExpectedValueCalibration
    {
        private const int SmoothingWindow = 10;
        private const int TimeSmoothingWindow = 20;
        private const int MaxYearsAfterSigning = 5;
        private const int PlotMaxPick = 650;
        private const int ModelId = 1;

        private static List<Model_Players> LoadPlayers(SqliteDbContext db)
        {
           return db.Model_Players
                .AsNoTracking()
                .Where(x => x.DraftPick < PlotMaxPick && !(x.IsHitter && x.IsPitcher))
                .Join(db.Player_CareerStatus, f => f.MlbId, f => f.MlbId, (mp, pcs) => new { mp, pcs })
                .Where(f => !(f.pcs.IgnorePlayer > 0))
                .Select(f => f.mp)
                .ToList();
        }

        public static void CreateGraph()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Cache from DB
            Dictionary<int, DraftPickValues> draftPickValuesByPick = db.DraftPickValues
                .AsNoTracking()
                .ToDictionary(x => x.Pick);

            List<Model_Players> players = LoadPlayers(db);
            HashSet<int> mlbIds = players.Select(p => p.MlbId).ToHashSet();

            Dictionary<(int MlbId, bool IsHitter), double> initialWarLookup = modelDb.Output_PlayerWarAggregation
                .AsNoTracking()
                .Where(x => x.ModelId == ModelId
                            && x.Year == 0
                            && x.Month == 0
                            && mlbIds.Contains(x.MlbId))
                .Select(x => new { x.MlbId, x.IsHitter, x.War })
                .ToList()
                .ToDictionary(x => (x.MlbId, x.IsHitter), x => (double)x.War);

            // Get ratio of model/actual average for each pick.
            List<(int Pick, int SignRank, bool IsHitter, bool IsEligible, double Ratio)> ratios = players
            .Select(p =>
            {
                DraftPickValues dpv = draftPickValuesByPick[p.DraftPick];
                double modelWar = initialWarLookup[(p.MlbId, p.IsHitter)];
                double expectedWar = p.IsHitter ? dpv.WarHitter : dpv.WarPitcher;
                return (Pick: p.DraftPick, SignRank: p.DraftSignRank, p.IsHitter, p.IsEligible, Ratio: modelWar / expectedWar);
            })
            .ToList();

            var grouped = ratios
                .GroupBy(r => (r.Pick, r.IsHitter, r.IsEligible))
                .Select(g => (g.Key.Pick, g.Key.IsHitter, g.Key.IsEligible, AvgRatio: g.Average(x => x.Ratio)))
                .OrderBy(g => g.Pick)
                .ToList();

            // Get ratio of model/actual average for each Sign Rank.
            List<(int Pick, bool IsHitter, bool IsEligible, double Ratio)> signRankRatios = players
                .Where(p => !p.IsEligible && p.DraftSignRank < 650)
                .Select(p =>
                {
                    DraftPickValues dpv = draftPickValuesByPick[p.DraftSignRank];
                    double modelWar = initialWarLookup[(p.MlbId, p.IsHitter)];
                    double expectedWar = p.IsHitter ? dpv.WarHitter : dpv.WarPitcher;
                    return (Pick: p.DraftSignRank, p.IsHitter, IsEligible: false, Ratio: modelWar / expectedWar);
                })
                .ToList();
            var groupedSignRank = signRankRatios
                .GroupBy(r => (r.Pick, r.IsHitter, r.IsEligible))
                .Select(g => (g.Key.Pick, g.Key.IsHitter, g.Key.IsEligible, AvgRatio: g.Average(x => x.Ratio)))
                .OrderBy(g => g.Pick)
                .ToList();

            // --- Build smoothed series ---
            List<(double X, double Y)> hitterTrained = BuildSmoothedSeries(grouped, true, true, SmoothingWindow);
            List<(double X, double Y)> hitterHeldOut = BuildSmoothedSeries(grouped, true, false, SmoothingWindow);
            List<(double X, double Y)> pitcherTrained = BuildSmoothedSeries(grouped, false, true, SmoothingWindow);
            List<(double X, double Y)> pitcherHeldOut = BuildSmoothedSeries(grouped, false, false, SmoothingWindow);
            List<(double X, double Y)> hitterHeldOutSignRank = BuildSmoothedSeries(groupedSignRank, true, false, SmoothingWindow);
            List<(double X, double Y)> pitcherHeldOutSignRank = BuildSmoothedSeries(groupedSignRank, false, false, SmoothingWindow);
            
            // Get Min/Max values
            List<double> allY = hitterTrained
                .Concat(hitterHeldOut)
                .Concat(pitcherTrained)
                .Concat(pitcherHeldOut)
                .Concat(hitterHeldOutSignRank)
                .Concat(pitcherHeldOutSignRank)
                .Select(p => p.Y)
                .ToList();
            double maxX = ratios.Max(r => r.Pick);
            double minY = allY.Min();
            double maxY = allY.Max();

            Plot plot = new();

            PlotSeries(plot, hitterTrained, "Hitter - In Training", new Color("#0F766E"));
            PlotSeries(plot, hitterHeldOut, "Hitter - NIT", new Color("#14B8A6")); 
            PlotSeries(plot, hitterHeldOutSignRank, "Hitter - NIT (Sign Rank)", new Color("#99F6E4"));
            PlotSeries(plot, pitcherTrained, "Pitcher - In Training", new Color("#9A3412"));
            PlotSeries(plot, pitcherHeldOut, "Pitcher - NIT", new Color("#F59E0B"));
            PlotSeries(plot, pitcherHeldOutSignRank, "Pitcher - NIT (Sign Rank)", new Color("#FDE68A"));
            plot.Add.HorizontalLine(1);

            plot.Axes.SetLimits(0, maxX + 1, minY - 0.1, maxY + 0.1);

            plot.Title("Draft Pick Calibration");
            plot.XLabel("Draft Pick");
            plot.YLabel("Model WAR (Pick 0) / Historical Draft Pick WAR");
            plot.ShowLegend();

            plot.SavePng("../../../Draft/DraftPickCalibration.png", 1600, 900);
        }

        private static List<(double X, double Y)> BuildSmoothedSeries(
            IEnumerable<(int Pick, bool IsHitter, bool IsEligible, double AvgRatio)> averages,
            bool isHitter,
            bool isEligible,
            int windowSize)
        {
            List<(int Pick, double AvgRatio)> points = averages
                .Where(a => a.IsHitter == isHitter && a.IsEligible == isEligible)
                .OrderBy(a => a.Pick)
                .Select(a => (a.Pick, a.AvgRatio))
                .ToList();

            int halfWindow = windowSize / 2;

            return points
                .Select(p => (
                    X: (double)p.Pick,
                    Y: points
                        .Where(q => Math.Abs(q.Pick - p.Pick) <= halfWindow)
                        .Average(q => q.AvgRatio)))
                .ToList();
        }

        private static void PlotSeries(Plot plot, List<(double X, double Y)> points, string label, Color color)
        {
            if (points.Count == 0)
            {
                return;
            }

            var scatter = plot.Add.Scatter(
                points.Select(p => p.X).ToArray(),
                points.Select(p => p.Y).ToArray());

            scatter.LineWidth = 2;
            scatter.MarkerSize = 0;
            scatter.Color = color;
            scatter.LegendText = label;
        }

        // Expected WAR by pick from either the actual or model table
        private static Dictionary<int, (float WarHitter, float WarPitcher)> LoadExpectedValues(
            SqliteDbContext db,
            SiteDbContext siteDb,
            DraftPickTarget source)
        {
            return source == DraftPickTarget.Actual
                ? db.DraftPickValues.AsNoTracking().ToDictionary(x => x.Pick, x => (x.WarHitter, x.WarPitcher))
                : siteDb.ModelDraftPickValues.AsNoTracking().ToDictionary(x => x.Pick, x => (x.WarHitter, x.WarPitcher));
        }

        // Tracks calibration at initialization and at SigningYear+1..+5 for players not in training
        public static void CreateTimeGraph(DraftPickTarget source)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

            Dictionary<int, (float WarHitter, float WarPitcher)> expectedByPick = LoadExpectedValues(db, siteDb, source);

            int maxDataYear = modelDb.Output_PlayerWarAggregation
                .Where(x => x.ModelId == ModelId)
                .Max(x => x.Year);

            List<Model_Players> players = LoadPlayers(db)
                .Where(p => !p.IsEligible && p.SigningYear + MaxYearsAfterSigning <= maxDataYear)
                .ToList();

            HashSet<int> mlbIds = players.Select(p => p.MlbId).ToHashSet();

            // Per player, rows sorted by time so the latest row at or before a cutoff is easy to find
            Dictionary<(int MlbId, bool IsHitter), List<(int Year, int Month, double War)>> warTimeline = modelDb.Output_PlayerWarAggregation
                .Where(x => x.ModelId == ModelId && mlbIds.Contains(x.MlbId))
                .Select(x => new { x.MlbId, x.IsHitter, x.Year, x.Month, x.War })
                .ToList()
                .GroupBy(x => (x.MlbId, x.IsHitter))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Year)
                          .ThenBy(x => x.Month)
                          .Select(x => (x.Year, x.Month, War: (double)x.War))
                          .ToList());

            // Horizon 0 = initialization, horizon N = SigningYear + N
            List<(int Pick, bool IsHitter, int Horizon, double Ratio)> ratios = players
                .SelectMany(p =>
                {
                    var expected = expectedByPick[p.DraftPick];
                    double expectedWar = p.IsHitter ? expected.WarHitter : expected.WarPitcher;
                    var timeline = warTimeline[(p.MlbId, p.IsHitter)];

                    var results = new List<(int Pick, bool IsHitter, int Horizon, double Ratio)>
                    {
                        (p.DraftPick, p.IsHitter, 0, timeline.First(r => r.Year == 0 && r.Month == 0).War / expectedWar)
                    };

                    for (int n = 1; n <= MaxYearsAfterSigning; n++)
                    {
                        double war = timeline.Last(r => r.Year <= p.SigningYear + n).War;
                        results.Add((p.DraftPick, p.IsHitter, n, war / expectedWar));
                    }

                    return results;
                })
                .ToList();

            string prefix = source == DraftPickTarget.Actual ? "Actual" : "Model";

            SaveHorizonPlot(ratios, true, $"../../../Draft/DraftPickCalibrationTime_{prefix}_Hitter.png");
            SaveHorizonPlot(ratios, false, $"../../../Draft/DraftPickCalibrationTime_{prefix}_Pitcher.png");
        }

        private static void SaveHorizonPlot(
            List<(int Pick, bool IsHitter, int Horizon, double Ratio)> ratios,
            bool isHitter,
            string path)
        {
            string[] labels = { "Initial", "Y+1", "Y+2", "Y+3", "Y+4", "Y+5" };
            Color[] colors =
            {
                new Color("#BDD7E7"),
                new Color("#9ECAE1"),
                new Color("#6BAED6"),
                new Color("#4292C6"),
                new Color("#2171B5"),
                new Color("#08519C"),
            };

            var horizonSeries = new List<(string Label, Color Color, List<(double X, double Y)> Points)>();

            for (int horizon = 0; horizon <= MaxYearsAfterSigning; horizon++)
            {
                var averages = ratios
                    .Where(r => r.Horizon == horizon)
                    .GroupBy(r => (r.Pick, r.IsHitter))
                    .Select(g => (g.Key.Pick, g.Key.IsHitter, IsEligible: false, AvgRatio: g.Average(x => x.Ratio)))
                    .ToList();

                horizonSeries.Add((labels[horizon], colors[horizon], BuildSmoothedSeries(averages, isHitter, false, TimeSmoothingWindow)));
            }

            double[] allY = horizonSeries
                .SelectMany(s => s.Points)
                .Select(p => p.Y)
                .ToArray();

            double maxX = PlotMaxPick - 1;

            Plot plot = new();

            foreach (var series in horizonSeries)
            {
                PlotSeries(plot, series.Points, series.Label, series.Color);
            }

            plot.Add.HorizontalLine(1);

            plot.Axes.SetLimits(0, maxX + 1, allY.Min() - 0.1, allY.Max() + 0.1);

            plot.Title($"{(isHitter ? "Hitter" : "Pitcher")} Calibration Over Time (Not In Training)");
            plot.XLabel("Draft Pick");
            plot.YLabel("Model WAR / Historical Draft Pick WAR");
            plot.ShowLegend();

            plot.SavePng(path, 1600, 900);
        }
    }
}
