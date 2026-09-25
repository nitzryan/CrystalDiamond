using ModelDb;
using SiteDb;
using SitePrep;
using UI.Controls.TestRunnerGraphViewer;
using UI.Types;

namespace UI.Controls
{
    public record SeriesInfo
    {
        public required string Name { get; init; }
        public required ScottPlot.Color Color { get; init; }
        public bool IsPlotted { get; set; } = false;
    }
    public record ModelGraphViewerPoint(int Year, int Month, int X,
        SeriesInfo SeriesInfo,
        Output_PlayerWarAggregation? Opwa,
        List<Prediction_HitterStats>? Phs,
        List<Prediction_PitcherStats>? Pps);

    public record PlotArgs(
            int Id,
            string Name,
            Func<ModelGraphViewerPoint, bool> ResultValid,
            Func<ModelGraphViewerPoint, double> Result,
            double GraphSoftMax,
            string YAxisName,
            Func<ModelGraphViewerPoint, IReadOnlyList<DetailRow>> DetailRows
        );

    public partial class ModelGraphViewer : UserControl
    {
        // Definition for the dropdown and selection logic
        private readonly List<PlotArgs> PlotArgsList = [
            new PlotArgs(0, "Prospect WAR", f => f.Opwa != null, f => f.Opwa!.War, 20, "WAR", DetailViews.War),
            new PlotArgs(1, "MLB PA", f => f.Phs != null, f => Math.Round(f.Phs![0].Pa), 600, "PA", DetailViews.Hitter),
            new PlotArgs(2, "MLB IP", f => f.Pps != null, f => Math.Round((f.Pps![0].Outs_SP + f.Pps![0].Outs_RP) / 3), 200, "IP", DetailViews.Pitcher)
        ];

        private PlotArgs currentPlotArgs;

        private void ResetOutputSelectionComboBox(List<ModelGraphViewerPoint> points)
        {
            outputSelectionComboBox.SelectedValueChanged -= OutputSelectionComboBox_SelectedValueChanged;
            outputSelectionComboBox.DataSource = null;

            List<PlotArgs> options = new();
            foreach (PlotArgs plotArgs in PlotArgsList)
            {
                // Check if this output is valid for these results
                if (points.Any(f => plotArgs.ResultValid(f)))
                {
                    options.Add(plotArgs);
                }
                // Invalid, reset if this selection is currently selected
                else if (plotArgs.Id == currentPlotArgs.Id)
                {
                    currentPlotArgs = PlotArgsList[0];
                }
            }

            outputSelectionComboBox.DisplayMember = "Name";
            outputSelectionComboBox.ValueMember = "Id";
            outputSelectionComboBox.DataSource = options;

            // There should always be at least 1 valid list
            if (outputSelectionComboBox.Items.Count == 0)
                throw new Exception("No valid PlotArgs found");

            
            outputSelectionComboBox.SelectedValue = currentPlotArgs.Id;
            outputSelectionComboBox.SelectedValueChanged += OutputSelectionComboBox_SelectedValueChanged;
        }

        private void OutputSelectionComboBox_SelectedValueChanged(object? sender, EventArgs e)
        {
            if (outputSelectionComboBox.SelectedValue is int id)
                currentPlotArgs = PlotArgsList[id];
            else
                throw new Exception($"OutputSelectionComboBox.SelectedValue isn't int: {outputSelectionComboBox.SelectedValue}");

            PlotResults();

            // Need to select new point; keep at the same index/series if applicable, otherwise deselect
            bool stillValid = SelectedPoint is not null
                && currentPlotArgs.ResultValid(SelectedPoint)
                && CurrentlyPlottedSeries.Contains(SelectedPoint.SeriesInfo);
            SelectPoint(stillValid ? SelectedPoint : null);
        }

        private const float SELECT_RADIUS_PX = 15;
        private const float X_AXIS_MIN_SIZE = 60;
        public static float X_AXIS_OFFSET = 0.5f;

        private readonly List<ModelGraphViewerPoint> modelPoints = [];
        private ScottPlot.Plottables.Marker? selectionMarker = null;

        public ModelGraphViewerPoint? SelectedPoint { get; private set; } = null;
        public event EventHandler<(ModelGraphViewerPoint?, PlotArgs?)>? PointSelected;

        private readonly List<SeriesInfo> series = [];
        public IReadOnlyList<SeriesInfo> CurrentlyPlottedSeries => series.Where(f => f.IsPlotted).ToList();
        public event EventHandler? PlottedSeriesChanged;

        public static LeagueBaselineCache leagueBaselineCache = LeagueBaselineCache.Load(Global.db);

        public ModelGraphViewer()
        {
            InitializeComponent();
            Visible = false;
            formsPlot.UserInputProcessor.Disable(); // Prrevent default graph interactions (drag, zoom)

            currentPlotArgs = PlotArgsList[0];

            // One-time configuration that survives Plot.Clear()
            ScottPlot.Plot plot = formsPlot.Plot;
            plot.XLabel("Date");
            plot.Axes.Bottom.TickLabelStyle.Rotation = -90;
            plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plot.Axes.Bottom.MinimumSize = X_AXIS_MIN_SIZE;

            formsPlot.Refresh();
        }

        public void SetResults(IReadOnlyList<ModelResults> results, IReadOnlyList<string> seriesNames)
        {
            if (results.Count != seriesNames.Count)
                throw new ArgumentException("results and seriesLabels must have the same count");

            ClearResults();
            ScottPlot.Plot plot = formsPlot.Plot;

            // Get all (Year,Month) combinations

            List<(int Year, int Month)> warDates = results
                .SelectMany(f => f.ProWar)
                .Select(f => (f.Year, f.Month))
                .Distinct()
                .ToList();
            List<(int Year, int Month)> hitStatDates = results
                .SelectMany(f => f.ProHitStats ?? [])
                .SelectMany(f => f)
                .Select(f => (f.Year, f.Month))
                .Distinct()
                .ToList();
            List<(int Year, int Month)> pitStatDates = results
                .SelectMany(f => f.ProPitStats ?? [])
                .SelectMany(f => f)
                .Select(f => (f.Year, f.Month))
                .Distinct()
                .ToList();
            List<(int Year, int Month)> dates = warDates
                .Concat(hitStatDates)
                .Concat(pitStatDates)
                .Distinct()
                .ToList();

            // Get dates with X indices
            Dictionary<(int Year, int Month), int> xIndex = dates
                .Select((k, i) => (k, i))
                .ToDictionary(t => t.k, t => t.i);

            // Set series
            series.Clear();
            for (int i = 0; i < seriesNames.Count; i++)
            {
                ScottPlot.Color color = plot.Add.Palette.GetColor(i);
                series.Add(new SeriesInfo { Name=seriesNames[i], Color=color });
            }

            // Add points
            modelPoints.Clear();
            for (int i = 0; i < results.Count; i++)
            {
                foreach ((int year, int month) in dates)
                {
                    modelPoints.Add(new ModelGraphViewerPoint(
                        year, month, xIndex[(year, month)],
                        series[i],
                        results[i].ProWar
                            .Where(f => f.Year == year && f.Month == month)
                            .SingleOrDefault(),
                        ConvertHitterStats(results[i].ProHitStats
                            ?.Where(f => f.Count > 0 && f[0].Year == year && f[0].Month == month)
                            .SingleOrDefault(), leagueBaselineCache),
                        ConvertPitcherStats(results[i].ProPitStats
                            ?.Where(f => f.Count > 0 && f[0].Year == year && f[0].Month == month)
                            .SingleOrDefault(), leagueBaselineCache)
                    ));
                }
            }

            ResetOutputSelectionComboBox(modelPoints);
            PlotResults();

            formsPlot.Refresh();
            Visible = true;
        }

        private static List<Prediction_HitterStats>? ConvertHitterStats(List<Output_HitterStatsAggregation>? outputs, LeagueBaselineCache cache)
        {
            if (outputs == null)
                return null;
            List<Prediction_HitterStats> predictions = outputs
                .Select(f => PredictionConverter.ConvertHitter(f, cache))
                .OfType<Prediction_HitterStats>()
                .ToList();
            return predictions.Count > 0 ? predictions : null;
        }

        private static List<Prediction_PitcherStats>? ConvertPitcherStats(List<Output_PitcherStatsAggregation>? outputs, LeagueBaselineCache cache)
        {
            if (outputs == null)
                return null;
            List<Prediction_PitcherStats> predictions = outputs
                .Select(f => PredictionConverter.ConvertPitcher(f, cache))
                .OfType<Prediction_PitcherStats>()
                .ToList();
            return predictions.Count > 0 ? predictions : null;
        }

        public void ClearResults()
        {
            Visible = false;
            formsPlot.Plot.Clear();
            series.Clear();
            selectionMarker = null;
            SelectedPoint = null;
            formsPlot.Refresh();
            PointSelected?.Invoke(this, (null, null));
        }

        private void PlotResults()
        {
            ScottPlot.Plot plot = formsPlot.Plot;

            var validPoints = modelPoints
                .Where(f => currentPlotArgs.ResultValid(f));
            var seriesPoints = validPoints
                .OrderBy(f => f.X)
                .GroupBy(f => f.SeriesInfo);

            // Set Points
            formsPlot.Plot.Clear();
            double maxY = -1;
            double maxX = -1;
            List<(double[] Xs, double[] Ys)> plotted = [];
            foreach (var sp in seriesPoints)
            {
                var xs = sp.Select(f => (double)f.X).ToArray();
                var ys = sp.Select(f => (double)currentPlotArgs.Result(f)).ToArray();

                var xsRounded = xs.Select(f => Math.Round(f, 2)).ToArray(); // TODO : Remove once model entirely runs on Model_HitterStats/Model_PitcherStats
                var ysRounded = ys.Select(f => Math.Round(f, 2)).ToArray();
                if (plotted.Any(p => p.Xs.SequenceEqual(xsRounded) && p.Ys.SequenceEqual(ysRounded)))
                {
                    sp.Key.IsPlotted = false;
                    continue;
                }

                sp.Key.IsPlotted = true;
                plotted.Add((xsRounded, ysRounded));

                maxY = Math.Max(maxY, ys.Max());
                maxX = Math.Max(maxX, xs.Max());

                var scatter = plot.Add.Scatter(xs, ys, sp.Key.Color);
                scatter.MarkerSize = 5;
            }

            PlottedSeriesChanged?.Invoke(this, EventArgs.Empty);

            // Set Axis
            plot.Axes.SetLimitsY(0, Math.Max(maxY, currentPlotArgs.GraphSoftMax));
            plot.Axes.SetLimitsX(-X_AXIS_OFFSET, maxX + X_AXIS_OFFSET);

            plot.Title(currentPlotArgs.Name);
            plot.YLabel(currentPlotArgs.YAxisName);

            // Set Ticks
            var validDates = validPoints
                .Select(f => (f.Year, f.Month, f.X))
                .Distinct()
                .OrderBy(f => f.Year)
                .ThenBy(f => f.Month)
                .GroupBy(f => f.Year);

            var ticks = new ScottPlot.TickGenerators.NumericManual();
            foreach (var vd in validDates)
            {
                int year = vd.First().Year;
                int x = vd.First().X;

                string tickLabel = year == 0 ? "Init" : year.ToString();
                ticks.AddMajor(x, tickLabel);
            }
            plot.Axes.Bottom.TickGenerator = ticks;

            selectionMarker = plot.Add.Marker(0, 0, ScottPlot.MarkerShape.OpenCircle, 14, ScottPlot.Colors.Black);
            selectionMarker.IsVisible = false;

            formsPlot.Refresh();
        }

        private void FormsPlot_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || modelPoints.Count == 0)
                return;

            float scale = formsPlot.DisplayScale;
            var mouse = new ScottPlot.Pixel(e.X * scale, e.Y * scale);
            double bestDistance = SELECT_RADIUS_PX * scale;
            ModelGraphViewerPoint? nearest = null;

            var validPoints = modelPoints
                .Where(f => currentPlotArgs.ResultValid(f) && f.SeriesInfo.IsPlotted);

            foreach (ModelGraphViewerPoint p in validPoints)
            {
                ScottPlot.Pixel px = formsPlot.Plot.GetPixel(new ScottPlot.Coordinates(p.X, currentPlotArgs.Result(p)));
                double dx = px.X - mouse.X;
                double dy = px.Y - mouse.Y;
                double distance = Math.Sqrt((dx * dx) + (dy * dy));
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    nearest = p;
                }
            }

            if (nearest is not null)
                SelectPoint(nearest);
        }

        private void SelectPoint(ModelGraphViewerPoint? point)
        {
            SelectedPoint = point;
            if (selectionMarker is not null)
            {
                selectionMarker.IsVisible = point is not null;
                if (point is not null)
                    selectionMarker.Location = new ScottPlot.Coordinates(point.X, currentPlotArgs.Result(point));
            }
            formsPlot.Refresh();
            PointSelected?.Invoke(this, (point, currentPlotArgs));
        }
    }
}
