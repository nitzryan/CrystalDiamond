using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using ScottPlot;
using System.Linq.Expressions;

namespace DataAquisition.PitchTracking
{
    internal class GenerateFastballBinPlots
    {
        private class PitchLocationHeatmapOptions
        {
            public string OutputDirectory { get; init; } = "PitchLocationHeatmaps";
            public int LevelId { get; init; } = 1;

            // Half of the 0.24ft ball diameter; applied to the top/bottom zone lines.
            public double BallRadius { get; init; } = 0.12;

            // Half plate width (0.708ft) + ball radius. Already ball-adjusted.
            public double ZoneHalfWidth { get; init; } = 0.83;

            // Shift each plot's values so its pitches sum to zero (removes the plot-wide level, e.g. hitter quality).
            public bool ZeroBaselinePerPlot { get; init; } = true;

            // Grid geometry: the zone is exactly CellsAcrossZone cells wide, plus CellsOutsideZone on each side.
            public int CellsAcrossZone { get; init; } = 7;
            public int CellsOutsideZone { get; init; } = 2;   // 2 -> cutoff at +-1.30ft
            public int Rows { get; init; } = 18;              // 18 -> 4.27ft

            public double CellSize => ZoneHalfWidth * 2 / CellsAcrossZone;
            public int Columns => CellsAcrossZone + (2 * CellsOutsideZone);
            public double XMax => Columns * CellSize / 2;
            public double XMin => -XMax;
            public double ZMin => 0.0;
            public double ZMax => ZMin + (Rows * CellSize);

            // Cells with fewer pitches than this are drawn as "no data" rather than a noisy mean.
            public int MinPitchesPerCell { get; init; } = 50;

            // Number of equal-population ZoneBot bins.
            public int ZoneBinCount { get; init; } = 3;
            public float ZoneBotBinHeight { get; init; } = 0.15f;
            public float ZoneTopBinHeight { get; init; } = 0.15f;

            // Lower edge of the first bin. Null = centre the bins on the median value.
            public float? ZoneBotBinStart { get; init; } = null;
            public float? ZoneTopBinStart { get; init; } = null;

            // Plots with fewer pitches than this are not rendered.
            public long MinPitchesPerPlot { get; init; } = 10_000;

            // Rows outside these bounds are treated as bad tracking data and excluded.
            public float ZoneBotValidMin { get; init; } = 1.0f;
            public float ZoneBotValidMax { get; init; } = 2.5f;
            public float ZoneTopValidMin { get; init; } = 2.5f;
            public float ZoneTopValidMax { get; init; } = 4.5f;

            // Colour scale is [-limit, +limit]. Null = derive one shared limit from the data.
            public double? ColorLimit { get; init; } = null;
            public double ColorLimitPercentile { get; init; } = 0.98;

            public int ImageWidth { get; init; } = 800;
            public int ImageHeight { get; init; } = 900;
        }

        public readonly record struct PitchLocationPlotKey(int ZoneBotBin, int ZoneTopBin, Db.DbEnums.PitchClass PitchClass, bool PitIsR);

        public sealed class PitchLocationGrid
        {
            public required double[,] Sum { get; init; }   // [row, column], row 0 = lowest PlateZ
            public required int[,] Count { get; init; }    // [row, column]
            public long TotalPitches { get; set; }
            public double Baseline { get; set; }

            public float ZoneBotMin { get; set; } = float.PositiveInfinity;
            public float ZoneBotMax { get; set; } = float.NegativeInfinity;
            public float ZoneTopMin { get; set; } = float.PositiveInfinity;
            public float ZoneTopMax { get; set; } = float.NegativeInfinity;
        }

        private static IQueryable<PitchData> FilterPitches(IQueryable<PitchData> pitchData, PitchLocationHeatmapOptions options)
        {
            int levelId = options.LevelId;
            float zoneBotMin = options.ZoneBotValidMin;
            float zoneBotMax = options.ZoneBotValidMax;
            float zoneTopMin = options.ZoneTopValidMin;
            float zoneTopMax = options.ZoneTopValidMax;
            float xMin = (float)options.XMin;
            float xMax = (float)options.XMax;
            float zMin = (float)options.ZMin;
            float zMax = (float)options.ZMax;

            return pitchData
                .AsNoTracking() // read-only; avoids change-tracker overhead
                .Where(p => p.LevelId == levelId
                    && p.PitchClass != Db.DbEnums.PitchClass.Other
                    // Drop rows with missing/garbage zone data so they can't define the min/max zone lines.
                    && p.ZoneBot >= zoneBotMin && p.ZoneBot <= zoneBotMax
                    && p.ZoneTop >= zoneTopMin && p.ZoneTop <= zoneTopMax
                    // Only pitches that land inside the plotted area.
                    && p.PlateX >= xMin && p.PlateX < xMax
                    && p.PlateZ >= zMin && p.PlateZ < zMax);
        }

        ///////////// Zone Bins
        private static float GetWeightedMedian(IQueryable<PitchData> pitches, Expression<Func<PitchData, float>> selector)
        {
            var histogram = pitches
                .GroupBy(selector)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderBy(x => x.Value)
                .ToList();

            if (histogram.Count == 0)
            {
                throw new InvalidOperationException("No pitches matched the heatmap filter.");
            }

            long total = histogram.Sum(x => (long)x.Count);
            long running = 0;
            foreach (var entry in histogram)
            {
                running += entry.Count;
                if (running * 2 >= total) // first value at or past the halfway point
                {
                    return entry.Value;
                }
            }

            return histogram[^1].Value;
        }

        private static (float BotStart, float TopStart) GetZoneBinStarts(IQueryable<PitchData> pitches, PitchLocationHeatmapOptions options)
        {
            float halfSpan(float binHeight) => options.ZoneBinCount * binHeight / 2;

            float botStart = options.ZoneBotBinStart
                ?? MathF.Round(GetWeightedMedian(pitches, p => p.ZoneBot) - halfSpan(options.ZoneBotBinHeight), 2);
            float topStart = options.ZoneTopBinStart
                ?? MathF.Round(GetWeightedMedian(pitches, p => p.ZoneTop) - halfSpan(options.ZoneTopBinHeight), 2);

            return (botStart, topStart);
        }

        private static int GetZoneBin(float value, float start, float height, int count)
        {
            int bin = (int)Math.Floor((value - start) / height);
            return bin >= 0 && bin < count ? bin : -1;
        }

        private static Dictionary<PitchLocationPlotKey, PitchLocationGrid> AccumulateGrids(
            IQueryable<PitchData> pitches, float botStart, float topStart, PitchLocationHeatmapOptions options)
        {
            int columns = options.Columns;
            int rows = options.Rows;
            var grids = new Dictionary<PitchLocationPlotKey, PitchLocationGrid>();

            // Throw out zones outside the bin grid before streaming.
            float botEnd = botStart + (options.ZoneBinCount * options.ZoneBotBinHeight);
            float topEnd = topStart + (options.ZoneBinCount * options.ZoneTopBinHeight);

            var projected = pitches
                .Where(p => p.ZoneBot >= botStart && p.ZoneBot < botEnd
                         && p.ZoneTop >= topStart && p.ZoneTop < topEnd)
                .Select(p => new
                {
                    p.PitchClass,
                    p.PitIsR,
                    p.ZoneBot,
                    p.ZoneTop,
                    p.PlateX,
                    p.PlateZ,
                    p.RunValueSmoothedHitter,
                });

            foreach (var pitch in projected) // streamed row-by-row
            {
                int column = (int)Math.Floor((pitch.PlateX - options.XMin) / options.CellSize);
                int row = (int)Math.Floor((pitch.PlateZ - options.ZMin) / options.CellSize);

                // The SQL filter already bounds the location; this only catches float rounding on the far edge.
                if (column < 0 || column >= columns || row < 0 || row >= rows)
                {
                    continue;
                }

                int botBin = GetZoneBin(pitch.ZoneBot, botStart, options.ZoneBotBinHeight, options.ZoneBinCount);
                int topBin = GetZoneBin(pitch.ZoneTop, topStart, options.ZoneTopBinHeight, options.ZoneBinCount);
                if (botBin < 0 || topBin < 0)
                {
                    continue; // float rounding at the SQL boundary
                }

                var key = new PitchLocationPlotKey(botBin, topBin, pitch.PitchClass, pitch.PitIsR);
                if (!grids.TryGetValue(key, out PitchLocationGrid? grid))
                {
                    grid = new PitchLocationGrid { Sum = new double[rows, columns], Count = new int[rows, columns] };
                    grids.Add(key, grid);
                }

                grid.Sum[row, column] += pitch.RunValueSmoothedHitter;
                grid.Count[row, column]++;
                grid.TotalPitches++;

                // Track the extremes of the zone actually present in this plot, for the strike zone lines.
                grid.ZoneBotMin = Math.Min(grid.ZoneBotMin, pitch.ZoneBot);
                grid.ZoneBotMax = Math.Max(grid.ZoneBotMax, pitch.ZoneBot);
                grid.ZoneTopMin = Math.Min(grid.ZoneTopMin, pitch.ZoneTop);
                grid.ZoneTopMax = Math.Max(grid.ZoneTopMax, pitch.ZoneTop);
            }

            return grids;
        }

        private static double GetSharedColorLimit(IEnumerable<PitchLocationGrid> grids, PitchLocationHeatmapOptions options)
        {
            var magnitudes = new List<double>();
            foreach (PitchLocationGrid grid in grids)
            {
                for (int row = 0; row < options.Rows; row++)
                {
                    for (int column = 0; column < options.Columns; column++)
                    {
                        int count = grid.Count[row, column];
                        if (count >= options.MinPitchesPerCell) // only cells that will actually be drawn
                        {
                            magnitudes.Add(Math.Abs(grid.Sum[row, column] / count));
                        }
                    }
                }
            }

            if (magnitudes.Count == 0)
            {
                return 0.1; // nothing drawable; any non-zero limit avoids a degenerate range
            }

            magnitudes.Sort();
            int index = Math.Min(magnitudes.Count - 1, (int)(options.ColorLimitPercentile * magnitudes.Count));
            return Math.Max(magnitudes[index], 1e-6);
        }

        private static double[,] BuildIntensities(PitchLocationGrid grid, double colorLimit, PitchLocationHeatmapOptions options)
        {
            int rows = options.Rows;
            int columns = options.Columns;
            var intensities = new double[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int count = grid.Count[row, column];
                    double value = count >= options.MinPitchesPerCell
                        ? Math.Clamp(grid.Sum[row, column] / count, -colorLimit, colorLimit)
                        : double.NaN;

                    // Flip vertically: accumulator row 0 (lowest pitch) goes to the last image row (bottom).
                    intensities[rows - 1 - row, column] = value;
                }
            }

            return intensities;
        }

        private static ScottPlot.Plot BuildPlot(
            PitchLocationPlotKey key, PitchLocationGrid grid, double colorLimit, PitchLocationHeatmapOptions options)
        {
            var plot = new ScottPlot.Plot();

            var heatmap = plot.Add.Heatmap(BuildIntensities(grid, colorLimit, options));
            heatmap.Colormap = new ScottPlot.Colormaps.Balance();                    // diverging: blue (low) -> white -> red (high)
            heatmap.ManualRange = new ScottPlot.Range(-colorLimit, colorLimit);     // same symmetric scale on every plot
            heatmap.NaNCellColor = ScottPlot.Colors.DarkGray;                       // "not enough pitches" cells

            // Position = centres of the corner cells, so inset the outer extents by half a cell.
            double halfCell = options.CellSize / 2;
            heatmap.CellAlignment = ScottPlot.Alignment.MiddleCenter;
            heatmap.Position = new ScottPlot.CoordinateRect(
                options.XMin + halfCell, options.XMax - halfCell,
                options.ZMin + halfCell, options.ZMax - halfCell);
            heatmap.Update(); // regenerate the bitmap after all styling changes

            var colorBar = plot.Add.ColorBar(heatmap);
            colorBar.Label = "Mean RunValueSmoothedHitter";

            // Added after the heatmap so the lines are drawn on top of it.
            AddStrikeZone(plot, grid, options);

            string hand = key.PitIsR ? "RHP" : "LHP";
            plot.Title($"{key.PitchClass} {hand} | bot {grid.ZoneBotMin:F2}-{grid.ZoneBotMax:F2} | " +
                       $"top {grid.ZoneTopMin:F2}-{grid.ZoneTopMax:F2} | n={grid.TotalPitches:N0} | " +
                       $"base {grid.Baseline:+0.0000;-0.0000}");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            plot.Axes.SetLimits(options.XMin, options.XMax, options.ZMin, options.ZMax);
            plot.Axes.SquareUnits(); // 1ft horizontal == 1ft vertical on screen

            return plot;
        }

        private static void AddStrikeZone(ScottPlot.Plot plot, PitchLocationGrid grid, PitchLocationHeatmapOptions options)
        {
            double left = -options.ZoneHalfWidth;   // already includes the ball radius
            double right = options.ZoneHalfWidth;

            // Bottom edges move down by a radius, top edges move up by a radius.
            double outerBottom = grid.ZoneBotMin - options.BallRadius;
            double innerBottom = grid.ZoneBotMax - options.BallRadius;
            double innerTop = grid.ZoneTopMin + options.BallRadius;
            double outerTop = grid.ZoneTopMax + options.BallRadius;

            var segments = new (double X1, double Y1, double X2, double Y2, bool IsOuter)[]
            {
                (left, outerBottom, left, outerTop, true),       // left side
                (right, outerBottom, right, outerTop, true),     // right side
                (left, outerBottom, right, outerBottom, true),   // lowest bottom in this bin
                (left, outerTop, right, outerTop, true),         // highest top in this bin
                (left, innerBottom, right, innerBottom, false),  // highest bottom in this bin
                (left, innerTop, right, innerTop, false),        // lowest top in this bin
            };

            foreach (var segment in segments)
            {
                var line = plot.Add.Line(segment.X1, segment.Y1, segment.X2, segment.Y2);
                line.LineStyle.Color = ScottPlot.Colors.Black;
                line.LineStyle.Width = segment.IsOuter ? 2f : 1.5f;
                line.LineStyle.Pattern = segment.IsOuter ? ScottPlot.LinePattern.Solid : ScottPlot.LinePattern.Dashed;
            }
        }

        private static void ShiftBaselineToZero(PitchLocationGrid grid)
        {
            if (grid.TotalPitches == 0)
            {
                return;
            }

            double total = 0;
            foreach (double cellSum in grid.Sum)
            {
                total += cellSum;
            }
            grid.Baseline = total / grid.TotalPitches;

            // Per-cell: sum of (value - baseline) over the cell's pitches == Sum - baseline * Count.
            for (int row = 0; row < grid.Sum.GetLength(0); row++)
            {
                for (int column = 0; column < grid.Sum.GetLength(1); column++)
                {
                    grid.Sum[row, column] -= grid.Baseline * grid.Count[row, column];
                }
            }
        }

        private static void Generate(IQueryable<PitchData> pitchData, PitchLocationHeatmapOptions options)
        {
            IQueryable<PitchData> pitches = FilterPitches(pitchData, options);
            (float botStart, float topStart) = GetZoneBinStarts(pitches, options);
            Dictionary<PitchLocationPlotKey, PitchLocationGrid> grids = AccumulateGrids(pitches, botStart, topStart, options);
            
            // Only plots that clear the threshold are drawn, and only they contribute to the shared colour scale.
            var plottable = grids.Where(g => g.Value.TotalPitches >= options.MinPitchesPerPlot).ToList();
            
            // Shift results
            if (options.ZeroBaselinePerPlot)
            {
                foreach (var (_, grid) in plottable)
                {
                    ShiftBaselineToZero(grid);
                }
            }

            // Start from an empty folder so plots from a previous config can't linger.
            if (Directory.Exists(options.OutputDirectory))
            {
                Directory.Delete(options.OutputDirectory, recursive: true);
            }
            Directory.CreateDirectory(options.OutputDirectory);

            // One subdirectory, and one colour scale, per pitch class / pitcher hand.
            foreach (var group in plottable.GroupBy(g => (g.Key.PitchClass, g.Key.PitIsR)))
            {
                string hand = group.Key.PitIsR ? "RHP" : "LHP";
                string directory = Path.Combine(options.OutputDirectory, $"{group.Key.PitchClass}_{hand}");
                Directory.CreateDirectory(directory);

                // Shared only within this directory, computed after the baseline shift.
                double colorLimit = options.ColorLimit ?? GetSharedColorLimit(group.Select(g => g.Value), options);
                foreach (var (key, grid) in group)
                {
                    string path = Path.Combine(directory, $"Bot{key.ZoneBotBin}_Top{key.ZoneTopBin}.png");
                    using ScottPlot.Plot plot = BuildPlot(key, grid, colorLimit, options);
                    plot.SavePng(path, options.ImageWidth, options.ImageHeight);
                }
            }
        }

        public static void Update()
        {
            PitchTrackingDbContext pitchDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_OPTIONS);

            Generate(pitchDb.PitchData, new PitchLocationHeatmapOptions());
        }
    }
}
