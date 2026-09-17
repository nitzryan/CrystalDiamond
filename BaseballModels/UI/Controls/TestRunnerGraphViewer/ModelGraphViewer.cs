using ModelDb;
using ScottPlot.WinForms;
using UI.Types;

namespace UI.Controls
{
    public record PlottedPoint(int SeriesIndex, double X, Output_PlayerWarAggregation Row);
    public record SeriesInfo(string Name, Color Color);

    public partial class ModelGraphViewer : UserControl
    {
        private const double MIN_Y_MAX = 20;
        private const float SELECT_RADIUS_PX = 15;
        private const int DEFAULT_START_YEAR = 2021;
        private const int DEFAULT_END_YEAR = 2026;
        private const float X_AXIS_MIN_SIZE = 60;

        private readonly FormsPlot formsPlot;
        private readonly List<PlottedPoint> plottedPoints = [];
        private ScottPlot.Plottables.Marker? selectionMarker = null;

        public PlottedPoint? SelectedPoint { get; private set; } = null;
        public event EventHandler<PlottedPoint?>? PointSelected;

        private readonly List<SeriesInfo> series = [];
        public IReadOnlyList<SeriesInfo> Series => series;

        public ModelGraphViewer()
        {
            formsPlot = new FormsPlot { Dock = DockStyle.Fill };
            formsPlot.UserInputProcessor.Disable();
            formsPlot.MouseDown += FormsPlot_MouseDown;
            Controls.Add(formsPlot);

            // One-time configuration that survives Plot.Clear()
            ScottPlot.Plot plot = formsPlot.Plot;
            plot.Title("Projected WAR");
            plot.XLabel("Season Month");
            plot.YLabel("WAR");
            plot.Axes.Bottom.TickLabelStyle.Rotation = -90;
            plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;
            plot.Axes.Bottom.MinimumSize = X_AXIS_MIN_SIZE;

            ApplyDefaultAxes();
            formsPlot.Refresh();
        }

        public void SetResults(IReadOnlyList<ModelResults> results, IReadOnlyList<string> seriesNames)
        {
            if (results.Count != seriesNames.Count)
                throw new ArgumentException("results and seriesLabels must have the same count");

            ClearResults();
            ScottPlot.Plot plot = formsPlot.Plot;

            // Shared timeline: every (Year, Month) from any result, equally spaced by index
            List<(int Year, int Month)> timeline = results
                .SelectMany(r => r.ProWar)
                .Select(w => (w.Year, w.Month))
                .Distinct()
                .OrderBy(k => k.Year).ThenBy(k => k.Month)
                .ToList();
            Dictionary<(int Year, int Month), int> xIndex = timeline
                .Select((k, i) => (k, i))
                .ToDictionary(t => t.k, t => t.i);

            for (int s = 0; s < results.Count; s++)
                AddSeries(s, seriesNames[s], results[s].ProWar, xIndex);

            // Added last so it draws on top
            selectionMarker = plot.Add.Marker(0, 0, ScottPlot.MarkerShape.OpenCircle, 14, ScottPlot.Colors.Black);
            selectionMarker.IsVisible = false;

            var ticks = new ScottPlot.TickGenerators.NumericManual();
            int? lastYear = null;
            for (int i = 0; i < timeline.Count; i++)
            {
                if (timeline[i].Year == lastYear)
                    continue;
                lastYear = timeline[i].Year;
                ticks.AddMajor(i, lastYear.Value.ToString());
            }
            plot.Axes.Bottom.TickGenerator = ticks;

            double maxWar = results
                .SelectMany(r => r.ProWar)
                .Select(w => (double)w.War)
                .DefaultIfEmpty(0)
                .Max();
            plot.Axes.SetLimitsX(-0.5, Math.Max(timeline.Count - 0.5, 0.5));
            plot.Axes.SetLimitsY(0, Math.Max(MIN_Y_MAX, maxWar));

            formsPlot.Refresh();
        }

        public void ClearResults()
        {
            formsPlot.Plot.Clear();
            plottedPoints.Clear();
            series.Clear();
            selectionMarker = null;
            SelectedPoint = null;
            ApplyDefaultAxes();
            formsPlot.Refresh();
            PointSelected?.Invoke(this, null);
        }

        private void ApplyDefaultAxes()
        {
            ScottPlot.Plot plot = formsPlot.Plot;
            int yearCount = DEFAULT_END_YEAR - DEFAULT_START_YEAR + 1;

            var ticks = new ScottPlot.TickGenerators.NumericManual();
            for (int i = 0; i < yearCount; i++)
                ticks.AddMajor(i, (DEFAULT_START_YEAR + i).ToString());
            plot.Axes.Bottom.TickGenerator = ticks;

            plot.Axes.SetLimitsX(-0.5, yearCount - 0.5);
            plot.Axes.SetLimitsY(0, MIN_Y_MAX);
        }

        private void AddSeries(int seriesIndex, string name,
            List<Output_PlayerWarAggregation> rows,
            Dictionary<(int Year, int Month), int> xIndex)
        {
            ScottPlot.Plot plot = formsPlot.Plot;
            ScottPlot.Color color = plot.Add.Palette.GetColor(seriesIndex);
            series.Add(new SeriesInfo(name, Color.FromArgb(color.A, color.R, color.G, color.B)));

            List<Output_PlayerWarAggregation> ordered = rows
                .OrderBy(r => xIndex[(r.Year, r.Month)])
                .ToList();

            // Split into runs of consecutive timeline indices so missing timesteps leave a gap
            int start = 0;
            for (int i = 1; i <= ordered.Count; i++)
            {
                bool isBreak = i == ordered.Count
                    || xIndex[(ordered[i].Year, ordered[i].Month)] != xIndex[(ordered[i - 1].Year, ordered[i - 1].Month)] + 1;
                if (!isBreak)
                    continue;

                List<Output_PlayerWarAggregation> segment = ordered.GetRange(start, i - start);
                double[] xs = segment.Select(r => (double)xIndex[(r.Year, r.Month)]).ToArray();
                double[] ys = segment.Select(r => (double)r.War).ToArray();

                var scatter = plot.Add.Scatter(xs, ys, color);
                scatter.MarkerSize = 5;
                start = i;
            }

            foreach (Output_PlayerWarAggregation r in ordered)
                plottedPoints.Add(new PlottedPoint(seriesIndex, xIndex[(r.Year, r.Month)], r));
        }

        private void FormsPlot_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || plottedPoints.Count == 0)
                return;

            float scale = formsPlot.DisplayScale;
            var mouse = new ScottPlot.Pixel(e.X * scale, e.Y * scale);
            double bestDistance = SELECT_RADIUS_PX * scale;
            PlottedPoint? nearest = null;

            foreach (PlottedPoint p in plottedPoints)
            {
                ScottPlot.Pixel px = formsPlot.Plot.GetPixel(new ScottPlot.Coordinates(p.X, p.Row.War));
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

        private void SelectPoint(PlottedPoint? point)
        {
            SelectedPoint = point;
            if (selectionMarker is not null)
            {
                selectionMarker.IsVisible = point is not null;
                if (point is not null)
                    selectionMarker.Location = new ScottPlot.Coordinates(point.X, point.Row.War);
            }
            formsPlot.Refresh();
            PointSelected?.Invoke(this, point);
        }
    }
}
