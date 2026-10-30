using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PitchTrackingDb;
using ScottPlot;
using ShellProgressBar;
using System.Linq.Expressions;

namespace DataAquisition.PitchTracking
{
    public static class BaseballConstants
    {
        public const double BallDiameter = 0.24;
        public const double BallRadius = BallDiameter / 2;
        public const double PlateWidth = 17.0 / 12.0;

        // Furthest a ball centre can be from the plate centre while still touching the zone (~0.828ft).
        public const double ZoneHalfWidth = PlateWidth / 2 + BallRadius;
    }

    internal class GeneratePitchClassBinPlots
    {
        private class PitchLocationHeatmapOptions
        {
            public string OutputDirectory { get; init; } = "PitchLocationHeatmaps";
            public int LevelId { get; init; } = 1;

            // Shift each plot's values so its pitches sum to zero (removes the plot-wide level, e.g. hitter quality).
            public bool ZeroBaselinePerPlot { get; init; } = false;

            // Hitter-quality split. Pitches to hitters below MinHitterPa are thrown out.
            public int MinHitterPa { get; init; } = 100;
            // Ascending ISO cutoffs; N cutoffs -> N+1 bins. Bin i is [cutoff[i-1], cutoff[i]).
            public float[] IsoCutoffs { get; init; } = [0.14f, 0.20f];

            // Grid geometry: the zone is exactly CellsAcrossZone cells wide, plus CellsOutsideZone on each side.
            public int CellsAcrossZone { get; init; } = 5;
            public int CellsOutsideZone { get; init; } = 2;   // 2 -> cutoff at +-1.30ft
            public int Rows { get; init; } = 16;              // 18 -> 4.27ft

            public double CellSize => BaseballConstants.ZoneHalfWidth * 2 / CellsAcrossZone;
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

        public readonly record struct PitchLocationPlotKey(int ZoneBotBin, int ZoneTopBin, int IsoBin, Db.DbEnums.PitchClass PitchClass, bool PitIsR);

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
                    && p.HitterPa >= options.MinHitterPa
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

        private static int GetIsoBin(float iso, float[] cutoffs)
        {
            int bin = 0;
            while (bin < cutoffs.Length && iso >= cutoffs[bin])
            {
                bin++;
            }
            return bin;
        }

        private static string GetIsoBinLabel(int bin, float[] cutoffs)
        {
            if (bin == 0)
            {
                return $"below-{cutoffs[0]:F3}";
            }
            if (bin == cutoffs.Length)
            {
                return $"{cutoffs[^1]:F3}-up";
            }
            return $"{cutoffs[bin - 1]:F3}-{cutoffs[bin]:F3}";
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
                    p.ISO,
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

                int isoBin = GetIsoBin(pitch.ISO, options.IsoCutoffs);
                var key = new PitchLocationPlotKey(botBin, topBin, isoBin, pitch.PitchClass, pitch.PitIsR);
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
            string baseline = options.ZeroBaselinePerPlot ? $" | base {grid.Baseline:+0.0000;-0.0000}" : "";
            plot.Title($"{key.PitchClass} {hand} | ISO {GetIsoBinLabel(key.IsoBin, options.IsoCutoffs)} | " +
                       $"bot {grid.ZoneBotMin:F2}-{grid.ZoneBotMax:F2} | top {grid.ZoneTopMin:F2}-{grid.ZoneTopMax:F2} | " +
                       $"n={grid.TotalPitches:N0}{baseline}");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            plot.Axes.SetLimits(options.XMin, options.XMax, options.ZMin, options.ZMax);
            plot.Axes.SquareUnits(); // 1ft horizontal == 1ft vertical on screen

            return plot;
        }

        private static void AddStrikeZone(ScottPlot.Plot plot, PitchLocationGrid grid, PitchLocationHeatmapOptions options)
        {
            double left = -BaseballConstants.ZoneHalfWidth;   // already includes the ball radius
            double right = BaseballConstants.ZoneHalfWidth;

            // Bottom edges move down by a radius, top edges move up by a radius.
            double outerBottom = grid.ZoneBotMin - BaseballConstants.BallRadius;
            double innerBottom = grid.ZoneBotMax - BaseballConstants.BallRadius;
            double innerTop = grid.ZoneTopMin + BaseballConstants.BallRadius;
            double outerTop = grid.ZoneTopMax + BaseballConstants.BallRadius;

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
                string groupDirectory = Path.Combine(options.OutputDirectory, $"{group.Key.PitchClass}_{hand}");
                
                // Still one colour scale per class/hand, so the ISO folders inside it are directly comparable.
                double colorLimit = options.ColorLimit ?? GetSharedColorLimit(group.Select(g => g.Value), options);
                
                foreach (var (key, grid) in group)
                {
                    string isoDirectory = Path.Combine(groupDirectory,
                        $"ISO{key.IsoBin}_{GetIsoBinLabel(key.IsoBin, options.IsoCutoffs)}");
                    Directory.CreateDirectory(isoDirectory);
                    
                    string path = Path.Combine(isoDirectory, $"Bot{key.ZoneBotBin}_Top{key.ZoneTopBin}.png");
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

    public static class PitchBinCompetitiveness
    {
        public static int MIN_PITCHES = 5;
        public static float MIN_SWING_RATE = 0.10f;

        private const string ZoneCompetitivePlotDirectory = "ZoneCompetitivePlots";

        public static class PitchZoneLayout
        {
            public const int ZoneCells = 5;                                   // NxN inside the strike zone
            public const int OutsideCells = 5;                                // rows/columns on each side of the zone
            public const int GridSize = ZoneCells + (2 * OutsideCells);         
            public const int SquareRowsPerSide = 2;                           // square rows at top and bottom of the zone
            public const int MiddleRowIndex = OutsideCells + SquareRowsPerSide; // the variable-height row

            // In-zone cells are square (except the middle row): zone width split into ZoneCells columns.
            public const double ZoneCellSize = 2 * BaseballConstants.ZoneHalfWidth / ZoneCells;

            // Fixed size of cells outside the zone. 3 x 0.19ft reaches +-1.40ft horizontally.
            public const double OutsideCellSize = ZoneCellSize;
        }

        public readonly record struct PitchZone(int Row, int Column)
        {
            // Row 0 = lowest row, Column 0 = left-most column (most negative PlateX).
            public int Id => (Row * PitchZoneLayout.GridSize) + Column;

            public bool IsInStrikeZone =>
                Row >= PitchZoneLayout.OutsideCells && Row < PitchZoneLayout.OutsideCells + PitchZoneLayout.ZoneCells &&
                Column >= PitchZoneLayout.OutsideCells && Column < PitchZoneLayout.OutsideCells + PitchZoneLayout.ZoneCells;

            public bool IsMiddleRow => Row == PitchZoneLayout.MiddleRowIndex;
        }

        private static readonly double[] ColumnEdges = BuildColumnEdges();
        private static double[] BuildColumnEdges()
        {
            double[] edges = new double[PitchZoneLayout.GridSize + 1];
            double zoneLeft = -BaseballConstants.ZoneHalfWidth;
            double zoneRight = BaseballConstants.ZoneHalfWidth;

            // Outside-left edges, ending exactly on the zone's left edge.
            for (int i = 0; i <= PitchZoneLayout.OutsideCells; i++)
            {
                edges[i] = zoneLeft - (PitchZoneLayout.OutsideCells - i) * PitchZoneLayout.OutsideCellSize;
            }

            // Equal-width columns across the zone.
            for (int i = 1; i < PitchZoneLayout.ZoneCells; i++)
            {
                edges[PitchZoneLayout.OutsideCells + i] = zoneLeft + i * PitchZoneLayout.ZoneCellSize;
            }

            // Zone's right edge, then outside-right edges.
            for (int i = 0; i <= PitchZoneLayout.OutsideCells; i++)
            {
                edges[PitchZoneLayout.OutsideCells + PitchZoneLayout.ZoneCells + i] = zoneRight + i * PitchZoneLayout.OutsideCellSize;
            }

            return edges;
        }

        public static double[] BuildRowEdges(float zoneTop, float zoneBot)
        {
            double zoneBottomEdge = zoneBot - BaseballConstants.BallRadius;
            double zoneTopEdge = zoneTop + BaseballConstants.BallRadius;

            double middleHeight = (zoneTopEdge - zoneBottomEdge) - 2 * PitchZoneLayout.SquareRowsPerSide * PitchZoneLayout.ZoneCellSize;
            if (middleHeight <= 0)
            {
                throw new ArgumentException(
                    $"Zone {zoneBot:F2}-{zoneTop:F2} is too short for {2 * PitchZoneLayout.SquareRowsPerSide} square rows.");
            }

            double[] edges = new double[PitchZoneLayout.GridSize + 1];

            // Outside-below edges, ending exactly on the zone's bottom edge.
            for (int i = 0; i <= PitchZoneLayout.OutsideCells; i++)
            {
                edges[i] = zoneBottomEdge - (PitchZoneLayout.OutsideCells - i) * PitchZoneLayout.OutsideCellSize;
            }

            // Square rows up from the bottom of the zone.
            for (int i = 1; i <= PitchZoneLayout.SquareRowsPerSide; i++)
            {
                edges[PitchZoneLayout.OutsideCells + i] = zoneBottomEdge + i * PitchZoneLayout.ZoneCellSize;
            }

            // Square rows down from the top of the zone (the gap between these and the ones above is the middle row).
            int topZoneEdgeIndex = PitchZoneLayout.OutsideCells + PitchZoneLayout.ZoneCells;
            for (int i = 0; i <= PitchZoneLayout.SquareRowsPerSide; i++)
            {
                edges[topZoneEdgeIndex - i] = zoneTopEdge - i * PitchZoneLayout.ZoneCellSize;
            }

            // Outside-above edges.
            for (int i = 1; i <= PitchZoneLayout.OutsideCells; i++)
            {
                edges[topZoneEdgeIndex + i] = zoneTopEdge + i * PitchZoneLayout.OutsideCellSize;
            }

            return edges;
        }

        private static int FindCell(double[] edges, double value)
        {
            if (value < edges[0] || value >= edges[^1])
            {
                return -1;
            }

            int cell = 0;
            while (value >= edges[cell + 1])
            {
                cell++;
            }
            return cell;
        }

        public static PitchZone? GetZone(float plateX, float plateZ, float zoneTop, float zoneBot)
        {
            int column = FindCell(ColumnEdges, plateX);
            int row = FindCell(BuildRowEdges(zoneTop, zoneBot), plateZ);

            return column < 0 || row < 0 ? null : new PitchZone(row, column);
        }

        public static PitchZone? GetZone(PitchData pitch)
        {
            return GetZone(pitch.PlateX, pitch.PlateZ, pitch.ZoneTop, pitch.ZoneBot);
        }

        // Plot the strike zone and the zone bins
        private static void AddZoneGrid(ScottPlot.Plot plot, double[] columnEdges, double[] rowEdges)
        {
            foreach (double x in columnEdges)
            {
                var line = plot.Add.Line(x, rowEdges[0], x, rowEdges[^1]);
                line.LineStyle.Color = ScottPlot.Colors.Gray;
                line.LineStyle.Width = 1;
            }

            foreach (double z in rowEdges)
            {
                var line = plot.Add.Line(columnEdges[0], z, columnEdges[^1], z);
                line.LineStyle.Color = ScottPlot.Colors.Gray;
                line.LineStyle.Width = 1;
            }

            int zoneStart = PitchZoneLayout.OutsideCells;
            int zoneEnd = zoneStart + PitchZoneLayout.ZoneCells;
            var outline = plot.Add.Rectangle(columnEdges[zoneStart], columnEdges[zoneEnd], rowEdges[zoneStart], rowEdges[zoneEnd]);
            outline.FillStyle.Color = ScottPlot.Colors.Transparent;
            outline.LineStyle.Color = ScottPlot.Colors.Black;
            outline.LineStyle.Width = 2.5f;
        }

        // Plot where a single pitch was, and what zone is hit
        public static void PlotPitchZone(PitchData pitch, string outputPath, int imageSize = 900)
        {
            double[] rowEdges = BuildRowEdges(pitch.ZoneTop, pitch.ZoneBot);
            PitchZone? zone = GetZone(pitch);

            using var plot = new ScottPlot.Plot();
            AddZoneGrid(plot, ColumnEdges, rowEdges);

            string description = "no zone (uncompetitive)";
            if (zone is PitchZone z)
            {
                var cell = plot.Add.Rectangle(ColumnEdges[z.Column], ColumnEdges[z.Column + 1], rowEdges[z.Row], rowEdges[z.Row + 1]);
                cell.FillStyle.Color = ScottPlot.Colors.Orange.WithAlpha(0.4);
                cell.LineStyle.Color = ScottPlot.Colors.Orange;
                cell.LineStyle.Width = 2;

                description = $"zone {z.Id} (row {z.Row}, col {z.Column}), " +
                              (z.IsInStrikeZone ? "in strike zone" : "outside strike zone");
            }

            // The ball at true size; with SquareUnits below it renders as a circle.
            double radius = BaseballConstants.BallRadius;
            var ball = plot.Add.Circle(pitch.PlateX, pitch.PlateZ, radius);
            ball.FillStyle.Color = ScottPlot.Colors.Red.WithAlpha(0.5);
            ball.LineStyle.Color = ScottPlot.Colors.Black;
            ball.LineStyle.Width = 1.5f;

            // Exact ball centre, for reading position against a boundary.
            var centre = plot.Add.Circle(pitch.PlateX, pitch.PlateZ, 0.015);
            centre.FillStyle.Color = ScottPlot.Colors.Black;
            centre.LineStyle.Color = ScottPlot.Colors.Black;

            plot.Title($"Game {pitch.GameId} pitch {pitch.PitchId}: {description}");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            // Cover the grid and the ball, whichever is larger, with a little padding.
            const double pad = 0.2;
            plot.Axes.SetLimits(
                Math.Min(ColumnEdges[0], pitch.PlateX - radius) - pad,
                Math.Max(ColumnEdges[^1], pitch.PlateX + radius) + pad,
                Math.Min(rowEdges[0], pitch.PlateZ - radius) - pad,
                Math.Max(rowEdges[^1], pitch.PlateZ + radius) + pad);
            plot.Axes.SquareUnits();

            plot.SavePng(outputPath, imageSize, imageSize);
        }

        public sealed record PitchZoneCounts(long[,] Counts, long[,] Swings, long NoZone, long NoZoneSwings);
        public static PitchZoneCounts CountPitchesPerZone(List<PitchData> pitches)
        {
            long[,] counts = new long[PitchZoneLayout.GridSize, PitchZoneLayout.GridSize];
            long[,] swings = new long[PitchZoneLayout.GridSize, PitchZoneLayout.GridSize];
            long noZone = 0;
            long noZoneSwings = 0;

            var projected = pitches
                .Select(p => new { p.PlateX, p.PlateZ, p.ZoneTop, p.ZoneBot, p.HadSwing });

            foreach (var p in projected)
            {
                int swing = p.HadSwing ? 1 : 0;
                if (GetZone(p.PlateX, p.PlateZ, p.ZoneTop, p.ZoneBot) is PitchZone zone)
                {
                    counts[zone.Row, zone.Column]++;
                    swings[zone.Row, zone.Column] += swing;
                }
                else
                {
                    noZone++;
                    noZoneSwings += swing;
                }
            }

            return new PitchZoneCounts(counts, swings, noZone, noZoneSwings);
        }

        // Rules for valid pitches
        public static bool IsCompetitive(long pitchCount, long swingCount, double minSwingRate)
        {
            return swingCount >= minSwingRate * pitchCount;
        }

        public static bool HasValidSampleSize(long pitchCount,  int minPitches)
        {
            return pitchCount >= minPitches;
        }

        public static void PlotZoneCounts(PitchZoneCounts result, string label, string outputPath,
            int minPitches, double minSwingRate,
            float displayZoneTop = 3.4f, float displayZoneBot = 1.55f, int imageSize = 1000)
        {
            long total = result.NoZone;
            long maxCount = 0;
            foreach (long count in result.Counts)
            {
                total += count;
                maxCount = Math.Max(maxCount, count);
            }

            double[] columnEdges = BuildColumnEdges();
            double[] rowEdges = BuildRowEdges(displayZoneTop, displayZoneBot);

            using var plot = new ScottPlot.Plot();

            for (int row = 0; row < PitchZoneLayout.GridSize; row++)
            {
                for (int column = 0; column < PitchZoneLayout.GridSize; column++)
                {
                    long count = result.Counts[row, column];
                    double share = maxCount == 0 ? 0 : (double)count / maxCount;
                    double percent = total == 0 ? 0 : 100.0 * count / total;

                    var cell = plot.Add.Rectangle(columnEdges[column], columnEdges[column + 1], rowEdges[row], rowEdges[row + 1]);
                    
                    // Color depending on competitiveness and percentage of pitches
                    bool competitive = IsCompetitive(count, result.Swings[row, column], minSwingRate)
                        && HasValidSampleSize(count, minPitches);
                    cell.FillStyle.Color = competitive
                        ? ScottPlot.Colors.SteelBlue.WithAlpha(0.15 + (0.7 * share))
                        : ScottPlot.Colors.Red.WithAlpha(0.15);

                    cell.LineStyle.Color = ScottPlot.Colors.Gray;
                    cell.LineStyle.Width = 1;

                    double centreX = (columnEdges[column] + columnEdges[column + 1]) / 2;
                    double centreZ = (rowEdges[row] + rowEdges[row + 1]) / 2;

                    string swingRate = count == 0 ? "-" : $"{100.0 * result.Swings[row, column] / count:F0}% sw";
                    var text = plot.Add.Text($"{count:N0}\n{percent:F1}%\n{swingRate}", centreX, centreZ);
                    text.LabelAlignment = ScottPlot.Alignment.MiddleCenter;
                    text.LabelFontSize = 10;
                }
            }

            AddZoneGrid(plot, columnEdges, rowEdges); // drawn last so the zone outline sits on top of the cells

            double noZonePercent = total == 0 ? 0 : 100.0 * result.NoZone / total;
            string noZoneSwingRate = result.NoZone == 0 ? "-" : $"{100.0 * result.NoZoneSwings / result.NoZone:F0}% sw";
            plot.Title($"{label} | n={total:N0} | no zone: {result.NoZone:N0} ({noZonePercent:F1}%, {noZoneSwingRate})");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            const double pad = 0.1;
            plot.Axes.SetLimits(columnEdges[0] - pad, columnEdges[^1] + pad, rowEdges[0] - pad, rowEdges[^1] + pad);
            plot.Axes.SquareUnits();

            plot.SavePng(outputPath, imageSize, imageSize);
        }

        private static void GenerateZoneCompetitive(PitchTrackingDbContext pitchDb, IQueryable<PitchData> pitches, int minPitches, double minSwingRate)
        {
            using var deleteTransaction = pitchDb.Database.BeginTransaction();
            pitchDb.PitchZoneCompetitive.ExecuteDelete();
            pitchDb.PitchZoneNonCompetitive.ExecuteDelete();
            deleteTransaction.Commit();

            var combos = pitches
                .Select(p => new { p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike, p.Year })
                .Distinct()
                .OrderBy(f => f.Year)
                .ToList();

            if (Directory.Exists(ZoneCompetitivePlotDirectory))
            {
                Directory.Delete(ZoneCompetitivePlotDirectory, recursive: true);
            }
            Directory.CreateDirectory(ZoneCompetitivePlotDirectory);

            List<PitchZoneCompetitive> rows = new(combos.Count * PitchZoneLayout.GridSize * PitchZoneLayout.GridSize);
            List<PitchZoneNonCompetitive> nonCompetitiveRows = new(combos.Count);

            ProgressBar progressBar = new(combos.Count, $"Generating Pitch Zone Competitive Values");

            // Get each year's pitches so we only need to hit the DB once per year
            List<PitchData> pitchDataCache = [];
            int lastYear = -1;
            foreach (var combo in combos)
            {
                // Check for cache update
                if (combo.Year != lastYear)
                {
                    lastYear = combo.Year;
                    pitchDataCache = pitches
                        .AsNoTracking()
                        .Where(f => f.Year == lastYear)
                        .ToList();
                }

                // Get selected pitches from cache
                var validPitches = pitchDataCache.Where(p =>
                    p.PitchClass == combo.PitchClass &&
                    p.PitIsR == combo.PitIsR &&
                    p.HitIsR == combo.HitIsR &&
                    p.CountBalls == combo.CountBalls &&
                    p.CountStrike == combo.CountStrike)
                    .ToList();
                PitchZoneCounts counts = CountPitchesPerZone(validPitches);

                // Every cell gets a row, including empty ones, so a missing row never needs interpreting.
                for (int row = 0; row < PitchZoneLayout.GridSize; row++)
                {
                    for (int column = 0; column < PitchZoneLayout.GridSize; column++)
                    {
                        long pitchCount = counts.Counts[row, column];
                        long swingCount = counts.Swings[row, column];

                        var currentZone = new PitchZone(row, column);

                        var cellPitches = validPitches
                            .Where(f => GetZone(f) == currentZone)
                            .ToList();

                        rows.Add(new PitchZoneCompetitive
                        {
                            Year = combo.Year,
                            PitchClass = combo.PitchClass,
                            PitIsR = combo.PitIsR,
                            HitIsR = combo.HitIsR,
                            CountBalls = combo.CountBalls,
                            CountStrikes = combo.CountStrike,
                            ZoneRow = row,
                            ZoneCol = column,
                            PitchCount = (int)pitchCount,
                            SwingCount = (int)swingCount,
                            IsCompetitive = IsCompetitive(pitchCount, swingCount, minSwingRate),
                            HasEnoughSample = HasValidSampleSize(pitchCount, minPitches),
                            ExpectedValueHitter = cellPitches.Any() ?
                                cellPitches.Average(f => f.RunValueSmoothedHitter)
                                : 0,
                        });
                    }
                }

                // Pitches that will NOT be valued by a competitive bin: no zone, or a bin failing either rule.
                List<PitchData> nonCompetitivePitches = validPitches
                    .Where(p =>
                        GetZone(p) is not PitchZone zone
                        || !IsCompetitive(counts.Counts[zone.Row, zone.Column], counts.Swings[zone.Row, zone.Column], minSwingRate)
                        || !HasValidSampleSize(counts.Counts[zone.Row, zone.Column], minPitches))
                    .ToList();
                nonCompetitiveRows.Add(new PitchZoneNonCompetitive
                {
                    Year = combo.Year,
                    PitchClass = combo.PitchClass,
                    PitIsR = combo.PitIsR,
                    HitIsR = combo.HitIsR,
                    CountBalls = combo.CountBalls,
                    CountStrikes = combo.CountStrike,
                    PitchCount = nonCompetitivePitches.Count,
                    ExpectedValueHitter = nonCompetitivePitches.Count > 0
                        ? nonCompetitivePitches.Average(f => f.RunValueSmoothedHitter)
                        : 0,
                });

                // Same counts and same rule as the rows above, so the plot matches what gets stored.
                string label = $"{combo.PitchClass}_{combo.Year}_{(combo.PitIsR ? "RHP" : "LHP")}_{(combo.HitIsR ? "RHH" : "LHH")} {combo.CountBalls}-{combo.CountStrike}";
                PlotZoneCounts(counts, label, Path.Combine(ZoneCompetitivePlotDirectory, $"{label}.png"),
                    minPitches, minSwingRate);

                progressBar.Tick();
            }

            using var insertTransaction = pitchDb.Database.BeginTransaction();
            pitchDb.BulkInsert(rows);
            pitchDb.BulkInsert(nonCompetitiveRows);
            insertTransaction.Commit();
        }

        public static void GenerateZoneCompetitivePlotsAndTable()
        {
            PitchTrackingDbContext pitchDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_OPTIONS);

            var pitches = pitchDb.PitchData
                .Where(f => f.Year >= 2017 
                    && f.LevelId == 1
                    && f.PitchClass != Db.DbEnums.PitchClass.Other);

            GenerateZoneCompetitive(pitchDb, pitches, MIN_PITCHES, MIN_SWING_RATE);
        }
    }
}
