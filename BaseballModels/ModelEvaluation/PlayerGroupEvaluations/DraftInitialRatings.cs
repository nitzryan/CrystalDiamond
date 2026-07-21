using Db;
using ModelDb;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ModelEvaluation.PlayerGroupEvaluations
{
    internal class DraftInitialRatings
    {
        // Number of timesteps (0...M) pulled out of Output_PlayerWarAggregation.
        // Guess: predictions are monthly-ish and most players have at least this many rows.
        public const int MAX_TIMESTEPS = 24;

        // Hard y-axis cap for bias plots. Series outside this range are clamped and
        // flagged with off-scale markers rather than being allowed to set the scale.
        public const double BIAS_Y_MIN = -0.8;
        public const double BIAS_Y_MAX = 0.8;

        // Upper bound (inclusive) of each draft pick bucket
        public static readonly int[] BUCKET_UPPER_BOUNDS = [5, 10, 20, 30, 50, 75, 100, 150, 200, 300, 400, 500, 600];
        public sealed class PlayerInfo
        {
            public required int MlbId { get; set; }
            public required bool IsHitter { get; set; }
            public required bool IsCollege { get; set; }
            public required bool IsEligible { get; set; }
            public required int DraftPick { get; set; }
            public required int BucketIndex { get; set; }
            public required float CareerWar { get; set; }
        }
        public readonly record struct GroupKey(int BucketIndex, bool IsHitter, bool IsCollege, bool IsEligible);
        public sealed class GroupResult
        {
            public required GroupKey Key { get; set; }
            public required int PlayerCount { get; set; }
            public required float AvgActualWar { get; set; }       // career WAR (pre) or last pred in window (post)
            public required float[] AvgPredictedWar { get; set; }  // length MAX_TIMESTEPS
            public required float[] Bias { get; set; }             // length MAX_TIMESTEPS
            public required string Label { get; set; }
        }

        public static void Calculate(int modelId, int cutoffYear, string name)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            List<PlayerInfo> players = LoadPlayers(db, cutoffYear);
            Dictionary<int, float[]> predictions = LoadPredictionCurves(modelDb, modelId, players);
            List<GroupResult> groups = AggregateGroups(players, predictions);

            name = DateTime.Now.ToString("yyyyMMMdd").ToUpper() + "_" + name;

            PlotBiasByTimestep(groups, modelId, name);
            PlotBiasByGroup(groups, modelId, timestep: 0, name);
            PlotBiasByGroup(groups, modelId, timestep: MAX_TIMESTEPS - 2, name);

            List<DemoResult> demos = AggregateDemos(players, predictions);
            PlotDemoTotals(demos, modelId, name);
        }

        // Gets PlayerInfo for all players in Model_Players
        private static List<PlayerInfo> LoadPlayers(SqliteDbContext db, int cutoffYear)
        {
            HashSet<int> collegeIds = db.College_Player
                .Select(c => c.MlbId)
                .Distinct()
                .ToHashSet();

            List<Model_Players> raw = db.Model_Players
                .Where(p => p.IsHitter != p.IsPitcher)   // drops both-true and both-false
                .Where(p => p.SigningYear <= cutoffYear)
                .Where(p => p.DraftPick > 0 && p.DraftPick <= BUCKET_UPPER_BOUNDS.Last())
                .ToList();

            List<PlayerInfo> result = [];
            int cutoffMismatch = 0;

            foreach (Model_Players p in raw)
            {
                result.Add(new PlayerInfo
                {
                    MlbId = p.MlbId,
                    IsHitter = p.IsHitter,
                    IsCollege = collegeIds.Contains(p.MlbId),
                    IsEligible = p.IsEligible,
                    DraftPick = p.DraftPick,
                    BucketIndex = GetBucketIndex(p.DraftPick),
                    CareerWar = p.IsEligible ? 
                        (p.IsHitter ? Math.Max(p.WarHitter, 0.0f) : Math.Max(p.WarPitcher, 0.0f)) :
                        float.NaN,
                });
            }

            if (cutoffMismatch > 0)
                Console.WriteLine($"WARNING: {cutoffMismatch} players where IsEligible != (LastProspectYear <= {cutoffYear})");

            return result;
        }

        private static int GetBucketIndex(int draftPick)
        {
            for (int i = 0; i < BUCKET_UPPER_BOUNDS.Length; i++)
            {
                if (draftPick <= BUCKET_UPPER_BOUNDS[i])
                    return i;
            }

            throw new Exception($"No bucket found for draftPick={draftPick}");
        }

        private static Dictionary<int, float[]> LoadPredictionCurves(ModelDbContext modelDb, int modelId, List<PlayerInfo> players)
        {
            Dictionary<int, bool> roleById = players.ToDictionary(p => p.MlbId, p => p.IsHitter);

            List<Output_PlayerWarAggregation> rows = modelDb.Output_PlayerWarAggregation
                .Where(r => r.ModelId == modelId)
                .ToList();

            Dictionary<int, float[]> result = [];

            foreach (IGrouping<int, Output_PlayerWarAggregation> g in rows.GroupBy(r => r.MlbId))
            {
                if (!roleById.TryGetValue(g.Key, out bool isHitter))
                    continue;

                List<float> ordered = g
                    .Where(r => r.IsHitter == isHitter)
                    .OrderBy(r => r.Year)
                    .ThenBy(r => r.Month)
                    .Select(r => r.War)
                    .ToList();

                if (ordered.Count == 0)
                    continue;

                float[] curve = new float[MAX_TIMESTEPS];
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                    curve[t] = t < ordered.Count ? ordered[t] : ordered[^1];

                result[g.Key] = curve;
            }

            return result;
        }

        private static List<GroupResult> AggregateGroups(List<PlayerInfo> players, Dictionary<int, float[]> predictions)
        {
            List<GroupResult> results = [];

            IEnumerable<IGrouping<GroupKey, PlayerInfo>> grouped = players
                .Where(p => predictions.ContainsKey(p.MlbId))
                .GroupBy(p => new GroupKey(p.BucketIndex, p.IsHitter, p.IsCollege, p.IsEligible));

            foreach (IGrouping<GroupKey, PlayerInfo> g in grouped)
            {
                List<PlayerInfo> members = g.ToList();

                double actualSum = 0;
                foreach (PlayerInfo p in members)
                    actualSum += p.IsEligible ? p.CareerWar : predictions[p.MlbId][MAX_TIMESTEPS - 1];
                float avgActual = (float)(actualSum / members.Count);

                float[] avgPred = new float[MAX_TIMESTEPS];
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                {
                    double sum = 0;
                    foreach (PlayerInfo p in members)
                        sum += predictions[p.MlbId][t];
                    avgPred[t] = (float)(sum / members.Count);
                }

                float[] bias = new float[MAX_TIMESTEPS];
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                    bias[t] = ComputeBias(avgActual, avgPred[t]);

                results.Add(new GroupResult
                {
                    Key = g.Key,
                    PlayerCount = members.Count,
                    AvgActualWar = avgActual,
                    AvgPredictedWar = avgPred,
                    Bias = bias,
                    Label = GetGroupLabel(g.Key),
                });
            }

            return results
                .OrderBy(r => r.Key.IsHitter ? 0 : 1)
                .ThenBy(r => r.Key.IsCollege ? 0 : 1)
                .ThenBy(r => r.Key.IsEligible ? 0 : 1)
                .ThenBy(r => r.Key.BucketIndex)
                .ToList();
        }

        private static float ComputeBias(float avgActual, float avgPredicted)
        {
            if (Math.Abs(avgActual) < 1e-4f)
                return float.NaN;

            return (avgPredicted - avgActual) / avgActual;
        }

        private static string GetGroupLabel(GroupKey key)
        {
            int lower = key.BucketIndex == 0 ? 1 : BUCKET_UPPER_BOUNDS[key.BucketIndex - 1] + 1;
            int upper = BUCKET_UPPER_BOUNDS[key.BucketIndex];

            string role = key.IsHitter ? "H" : "P";
            string school = key.IsCollege ? "COL" : "HS";
            string split = key.IsEligible ? "pre" : "post";
            return $"{lower}-{upper} {role} {school} {split}";
        }

        private static readonly ScottPlot.IColormap BUCKET_COLORMAP = new ScottPlot.Colormaps.Turbo();

        private static ScottPlot.Color GetBucketColor(int bucketIndex)
        {
            int maxBucketIndex = BUCKET_UPPER_BOUNDS.Length - 1;
            double fraction = maxBucketIndex == 0 ? 0.5 : (double)bucketIndex / maxBucketIndex;
            fraction = 0.04 + (fraction * (0.96 - 0.04));
            return BUCKET_COLORMAP.GetColor(fraction);
        }

        private static ScottPlot.Color GetRoleSchoolColor(bool isHitter, bool isCollege)
        {
            if (isHitter)
                return isCollege ? ScottPlot.Color.FromHex("#b02418") : ScottPlot.Color.FromHex("#e8873a");

            return isCollege ? ScottPlot.Color.FromHex("#1b3f94") : ScottPlot.Color.FromHex("#3aa8c1");
        }

        private static string GetClipSuffix(double[] ys, double loLimit, double hiLimit)
        {
            double[] finite = ys.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();

            bool hasAbove = finite.Any(v => v > hiLimit);
            bool hasBelow = finite.Any(v => v < loLimit);

            if (!hasAbove && !hasBelow)
                return "";

            List<string> parts = [];
            if (hasAbove)
                parts.Add($"max {finite.Max():F1}");
            if (hasBelow)
                parts.Add($"min {finite.Min():F1}");

            return $" [{string.Join(", ", parts)}]";
        }

        private static void AddOffScaleMarkers(ScottPlot.Plot plot, double[] xs, double[] ys, ScottPlot.Color color, double loLimit, double hiLimit)
        {
            double inset = (hiLimit - loLimit) * 0.025;

            List<double> aboveX = [];
            List<double> belowX = [];

            for (int i = 0; i < ys.Length; i++)
            {
                if (double.IsNaN(ys[i]) || double.IsInfinity(ys[i]))
                    continue;

                if (ys[i] > hiLimit)
                    aboveX.Add(xs[i]);
                else if (ys[i] < loLimit)
                    belowX.Add(xs[i]);
            }

            if (aboveX.Count > 0)
            {
                double[] aboveY = Enumerable.Repeat(hiLimit - inset, aboveX.Count).ToArray();
                ScottPlot.Plottables.Scatter up = plot.Add.Scatter(aboveX.ToArray(), aboveY);
                up.Color = color;
                up.MarkerShape = ScottPlot.MarkerShape.FilledTriangleUp;
                up.MarkerSize = 12;
                up.MarkerLineWidth = 1.5f;
                up.MarkerLineColor = color.Darken(0.4);
                up.LineWidth = 0;
            }

            if (belowX.Count > 0)
            {
                double[] belowY = Enumerable.Repeat(loLimit + inset, belowX.Count).ToArray();
                ScottPlot.Plottables.Scatter down = plot.Add.Scatter(belowX.ToArray(), belowY);
                down.Color = color;
                down.MarkerShape = ScottPlot.MarkerShape.FilledTriangleDown;
                down.MarkerSize = 12;
                down.MarkerLineWidth = 1.5f;
                down.MarkerLineColor = color.Darken(0.4);
                down.LineWidth = 0;
            }
        }

        private static void PlotBiasByTimestep(List<GroupResult> groups, int modelId, string name, int minPlayers = 5)
        {
            Directory.CreateDirectory($"../../../Output/ModelValidation/Bias_{name}/Timestep");

            ScottPlot.MarkerShape[] shapes =
            [
                ScottPlot.MarkerShape.FilledCircle,
                ScottPlot.MarkerShape.FilledSquare,
            ];

            IEnumerable<IGrouping<(bool IsHitter, bool IsCollege, bool IsEligible), GroupResult>> slices = groups
                .Where(g => g.PlayerCount >= minPlayers)
                .GroupBy(g => (g.Key.IsHitter, g.Key.IsCollege, g.Key.IsEligible));

            double[] xs = Enumerable.Range(0, MAX_TIMESTEPS).Select(i => (double)i).ToArray();

            foreach (IGrouping<(bool IsHitter, bool IsCollege, bool IsEligible), GroupResult> slice in slices)
            {
                ScottPlot.Plot plot = new();

                foreach (GroupResult g in slice.OrderBy(g => g.Key.BucketIndex))
                {
                    double[] ys = g.Bias.Select(b => (double)b).ToArray();
                    ScottPlot.Color color = GetBucketColor(g.Key.BucketIndex);

                    ScottPlot.Plottables.Scatter sc = plot.Add.Scatter(xs, ys);
                    sc.Color = color;
                    sc.MarkerShape = shapes[g.Key.BucketIndex % shapes.Length];
                    sc.MarkerSize = 8;
                    sc.MarkerLineWidth = 1.5f;
                    sc.MarkerLineColor = color.Darken(0.4);
                    sc.LineWidth = 1;
                    sc.LegendText = $"{g.Label.Split(' ')[0]} (n={g.PlayerCount})" + GetClipSuffix(ys, BIAS_Y_MIN, BIAS_Y_MAX);

                    AddOffScaleMarkers(plot, xs, ys, color, BIAS_Y_MIN, BIAS_Y_MAX);
                }

                plot.Add.HorizontalLine(0, 1, ScottPlot.Colors.Black);

                ScottPlot.TickGenerators.NumericManual ticks = new();
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                    ticks.AddMajor(t, t.ToString());
                plot.Axes.Bottom.TickGenerator = ticks;
                plot.Axes.Bottom.MinorTickStyle.Length = 0;
                plot.Grid.MinorLineWidth = 0;

                plot.Axes.SetLimitsX(-0.4, MAX_TIMESTEPS - 1 + 0.4);
                plot.Axes.SetLimitsY(BIAS_Y_MIN, BIAS_Y_MAX);

                plot.XLabel("Timestep");
                plot.YLabel("Bias ((pred - actual) / actual)");
                plot.Title($"Model {modelId}: bias vs timestep — " +
                           $"{(slice.Key.IsHitter ? "Hitters" : "Pitchers")}, " +
                           $"{(slice.Key.IsCollege ? "College" : "HS")}, " +
                           $"{(slice.Key.IsEligible ? "pre-cutoff" : "post-cutoff")}");
                plot.ShowLegend(ScottPlot.Edge.Right);

                string file = $"../../../Output/ModelValidation/Bias_{name}/Timestep/m{modelId}_" +
                              $"{(slice.Key.IsHitter ? "H" : "P")}_" +
                              $"{(slice.Key.IsCollege ? "COL" : "HS")}_" +
                              $"{(slice.Key.IsEligible ? "pre" : "post")}.png";
                plot.SavePng(file, 1100, 700);
            }
        }

        private static void PlotBiasByGroup(List<GroupResult> groups, int modelId, int timestep, string name, int minPlayers = 5)
        {
            Directory.CreateDirectory($"../../../Output/ModelValidation/Bias_{name}/Bucket");

            List<GroupResult> usable = groups.Where(g => g.PlayerCount >= minPlayers).ToList();
            if (usable.Count == 0)
                return;

            ScottPlot.Plot plot = new();

            foreach (IGrouping<(bool IsHitter, bool IsCollege, bool IsEligible), GroupResult> series in
                     usable.GroupBy(g => (g.Key.IsHitter, g.Key.IsCollege, g.Key.IsEligible)))
            {
                List<GroupResult> ordered = series.OrderBy(g => g.Key.BucketIndex).ToList();
                double[] xs = ordered.Select(g => (double)g.Key.BucketIndex).ToArray();
                double[] ys = ordered.Select(g => (double)g.Bias[timestep]).ToArray();

                bool isPre = series.Key.IsEligible;
                ScottPlot.Color color = GetRoleSchoolColor(series.Key.IsHitter, series.Key.IsCollege);

                ScottPlot.Plottables.Scatter sc = plot.Add.Scatter(xs, ys);
                sc.Color = color;
                sc.MarkerShape = isPre ? ScottPlot.MarkerShape.FilledCircle : ScottPlot.MarkerShape.OpenSquare;
                sc.MarkerSize = 8;
                sc.LineWidth = 2;
                sc.LinePattern = isPre ? ScottPlot.LinePattern.Solid : ScottPlot.LinePattern.Dashed;
                sc.LegendText = $"{(series.Key.IsHitter ? "Hitter" : "Pitcher")} / " +
                                $"{(series.Key.IsCollege ? "College" : "HS")} / " +
                                $"{(isPre ? "pre" : "post")}" +
                                GetClipSuffix(ys, BIAS_Y_MIN, BIAS_Y_MAX);

                AddOffScaleMarkers(plot, xs, ys, color, BIAS_Y_MIN, BIAS_Y_MAX);
            }

            plot.Add.HorizontalLine(0, 1, ScottPlot.Colors.Black);

            int[] bucketIndexes = usable.Select(g => g.Key.BucketIndex).Distinct().OrderBy(i => i).ToArray();

            ScottPlot.TickGenerators.NumericManual ticks = new();
            foreach (int bucketIndex in bucketIndexes)
            {
                int lower = bucketIndex == 0 ? 1 : BUCKET_UPPER_BOUNDS[bucketIndex - 1] + 1;
                ticks.AddMajor(bucketIndex, $"{lower}-{BUCKET_UPPER_BOUNDS[bucketIndex]}");
            }
            plot.Axes.Bottom.TickGenerator = ticks;
            plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
            plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plot.Axes.Bottom.MinorTickStyle.Length = 0;
            plot.Grid.MinorLineWidth = 0;

            double pad = (BIAS_Y_MAX - BIAS_Y_MIN) * 0.06;
            plot.Axes.SetLimitsX(bucketIndexes.First() - 0.4, bucketIndexes.Last() + 0.4);
            plot.Axes.SetLimitsY(BIAS_Y_MIN - pad, BIAS_Y_MAX + pad);

            plot.XLabel("Draft pick bucket");
            plot.YLabel("Bias ((pred - actual) / actual)");
            plot.Title($"Model {modelId}: bias by draft bucket at timestep {timestep}");
            plot.ShowLegend(ScottPlot.Edge.Right);

            string file = $"../../../Output/ModelValidation/Bias_{name}/Bucket/m{modelId}_t{timestep}.png";
            plot.SavePng(file, 1100, 700);
        }

        // Demo Wide Statistics
        public readonly record struct DemoKey(bool IsHitter, bool IsCollege, bool IsEligible);

        public sealed class DemoResult
        {
            public required DemoKey Key { get; set; }
            public required int PlayerCount { get; set; }
            public required float TotalActualWar { get; set; }
            public required float[] TotalPredictedWar { get; set; }   // length MAX_TIMESTEPS
        }

        private static List<DemoResult> AggregateDemos(List<PlayerInfo> players, Dictionary<int, float[]> predictions)
        {
            List<DemoResult> results = [];

            IEnumerable<IGrouping<DemoKey, PlayerInfo>> grouped = players
                .Where(p => predictions.ContainsKey(p.MlbId))
                .GroupBy(p => new DemoKey(p.IsHitter, p.IsCollege, p.IsEligible));

            foreach (IGrouping<DemoKey, PlayerInfo> g in grouped)
            {
                List<PlayerInfo> members = g.ToList();

                double actualSum = 0;
                foreach (PlayerInfo p in members)
                    actualSum += p.IsEligible ? p.CareerWar : predictions[p.MlbId][MAX_TIMESTEPS - 1];

                float[] totalPred = new float[MAX_TIMESTEPS];
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                {
                    double sum = 0;
                    foreach (PlayerInfo p in members)
                        sum += predictions[p.MlbId][t];
                    totalPred[t] = (float)sum;
                }

                results.Add(new DemoResult
                {
                    Key = g.Key,
                    PlayerCount = members.Count,
                    TotalActualWar = (float)actualSum,
                    TotalPredictedWar = totalPred,
                });
            }

            return results
                .OrderBy(r => r.Key.IsHitter ? 0 : 1)
                .ThenBy(r => r.Key.IsCollege ? 0 : 1)
                .ThenBy(r => r.Key.IsEligible ? 0 : 1)
                .ToList();
        }

        private static void PlotDemoTotals(List<DemoResult> demos, int modelId, string name)
        {
            Directory.CreateDirectory($"../../../Output/ModelValidation/Bias_{name}/DemoTotals");

            double[] xs = Enumerable.Range(0, MAX_TIMESTEPS).Select(i => (double)i).ToArray();

            foreach (DemoResult demo in demos)
            {
                ScottPlot.Plot plot = new();

                double[] ys = demo.TotalPredictedWar.Select(w => (double)w).ToArray();
                ScottPlot.Color color = GetRoleSchoolColor(demo.Key.IsHitter, demo.Key.IsCollege);

                ScottPlot.Plottables.Scatter sc = plot.Add.Scatter(xs, ys);
                sc.Color = color;
                sc.MarkerShape = demo.Key.IsEligible ? ScottPlot.MarkerShape.FilledCircle : ScottPlot.MarkerShape.OpenSquare;
                sc.MarkerSize = 8;
                sc.LineWidth = 2;
                sc.LinePattern = demo.Key.IsEligible ? ScottPlot.LinePattern.Solid : ScottPlot.LinePattern.Dashed;
                sc.LegendText = "Predicted total WAR";

                if (demo.Key.IsEligible)
                {
                    ScottPlot.Plottables.HorizontalLine actualLine = plot.Add.HorizontalLine(demo.TotalActualWar);
                    actualLine.Color = ScottPlot.Colors.Black;
                    actualLine.LineWidth = 1;
                    actualLine.LinePattern = ScottPlot.LinePattern.Dotted;
                    actualLine.LegendText = demo.Key.IsEligible ? "Actual total WAR" : "Total WAR at t=11 (target)";
                }

                ScottPlot.TickGenerators.NumericManual ticks = new();
                for (int t = 0; t < MAX_TIMESTEPS; t++)
                    ticks.AddMajor(t, t.ToString());
                plot.Axes.Bottom.TickGenerator = ticks;
                plot.Axes.Bottom.MinorTickStyle.Length = 0;
                plot.Grid.MinorLineWidth = 0;

                plot.Axes.SetLimitsX(-0.4, MAX_TIMESTEPS - 1 + 0.4);

                double maxY = Math.Max(ys.Max(), demo.TotalActualWar);
                plot.Axes.SetLimitsY(0, maxY * 1.1);

                plot.XLabel("Timestep");
                plot.YLabel("Total WAR");
                plot.Title($"Model {modelId}: demo total WAR vs timestep — " +
                           $"{(demo.Key.IsHitter ? "Hitters" : "Pitchers")}, " +
                           $"{(demo.Key.IsCollege ? "College" : "HS")}, " +
                           $"{(demo.Key.IsEligible ? "pre-cutoff" : "post-cutoff")} " +
                           $"(n={demo.PlayerCount})");
                plot.ShowLegend(ScottPlot.Edge.Right);

                string file = $"../../../Output/ModelValidation/Bias_{name}/DemoTotals/m{modelId}_" +
                              $"{(demo.Key.IsHitter ? "H" : "P")}_" +
                              $"{(demo.Key.IsCollege ? "COL" : "HS")}_" +
                              $"{(demo.Key.IsEligible ? "pre" : "post")}.png";
                plot.SavePng(file, 1100, 700);
            }
        }
    }
}
