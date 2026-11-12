using PitchTrackingDb;

namespace PitchAnalysis
{
    public static class BaseballConstants
    {
        public const double BallDiameter = 0.24;
        public const double BallRadius = BallDiameter / 2;
        public const double PlateWidth = 17.0 / 12.0;

        // Furthest a ball centre can be from the plate centre while still touching the zone (~0.828ft).
        public const double ZoneHalfWidth = (PlateWidth / 2) + BallRadius;
    }

    // Square, odd-sized weight grid centred on a cell. Offsets are in cells, not feet,
    // so borrowing vertically through the (taller) middle row reaches physically further.
    public sealed class PitchZoneKernel
    {
        private readonly double[,] weights;   // normalised so the centre is 1

        // Cells reached in each direction from the centre: (size - 1) / 2.
        internal int Radius { get; }

        public PitchZoneKernel(double[,] weights)
        {
            ArgumentNullException.ThrowIfNull(weights);
            int size = weights.GetLength(0);
            if (size != weights.GetLength(1) || size % 2 == 0)
            {
                throw new ArgumentException(
                    $"Kernel must be square with an odd size, was {weights.GetLength(0)}x{weights.GetLength(1)}.", nameof(weights));
            }

            foreach (double weight in weights)
            {
                if (double.IsNaN(weight) || weight < 0)
                {
                    throw new ArgumentException($"Kernel weights must be non-negative, found {weight}.", nameof(weights));
                }
            }

            Radius = size / 2;
            double centre = weights[Radius, Radius];
            if (centre <= 0)
            {
                throw new ArgumentException("The centre weight must be positive so a cell always counts its own pitches.", nameof(weights));
            }

            // Normalise so the centre is 1; the smoothed mean is unaffected, but the effective count stays comparable to MinPitches.
            this.weights = new double[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    this.weights[row, column] = weights[row, column] / centre;
                }
            }
        }

        // Weight for the cell rowOffset/columnOffset away from the centre; both must be within [-Radius, Radius].
        internal double Weight(int rowOffset, int columnOffset)
        {
            return weights[rowOffset + Radius, columnOffset + Radius];
        }

        // Constructor helpers
        // 3x3 plus shape: the cell at 1, the four edge-adjacent cells at neighbourWeight, corners at 0.
        public static PitchZoneKernel Adjacent(double neighbourWeight)
        {
            return new PitchZoneKernel(new double[,]
            {
                { 0, neighbourWeight, 0 },
                { neighbourWeight, 1, neighbourWeight },
                { 0, neighbourWeight, 0 },
            });
        }

        // (2 * radius + 1) square kernel with weight exp(-d^2 / (2 * sigma^2)), d measured in cells; the centre is 1.
        public static PitchZoneKernel Gaussian(int radius, double sigma)
        {
            int size = (2 * radius) + 1;
            double[,] weights = new double[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    int rowOffset = row - radius;
                    int columnOffset = column - radius;
                    double distanceSquared = (rowOffset * rowOffset) + (columnOffset * columnOffset);
                    weights[row, column] = Math.Exp(-distanceSquared / (2 * sigma * sigma));
                }
            }

            return new PitchZoneKernel(weights);
        }
    }

    public sealed record PitchZoneBinArgs
    {
        public int ZoneColumns { get; init; } = 7;
        public int SquareRowsPerSide { get; init; } = 3;
        public bool SplitMiddleRow { get; init; } = false;
        public int OutsideColumns { get; init; } = 7;
        public int OutsideRows { get; init; } = 7;
        public int MinPitches { get; init; } = 5;
        public double MinSwingRate { get; init; } = 0.10;
        public PitchZoneKernel? Kernel { get; init; } = PitchZoneKernel.Gaussian(2, 1.0);

        public bool SeparateBorderRing { get; init; } = false;
        public IReadOnlySet<Db.DbEnums.PitchClass>? SeparateBorderRingClasses { get; init; } = null;

        public double? NeighborCountRatio { get; init; } = null;
        public double NeighborCountFloor { get; init; } = 5;
        public double? InZoneSmoothingCutoff { get; init; } = null;
        public double? OutZoneSmoothingCutoff { get; init; } = null;

        // All parameters below this currently are not used in the optimal model, and can potentially be removed.

        // Max total weight (in pitches) the neighbours may add to a cell, chosen by whether the cell is in the strike zone.
        // Ignored when Kernel is null.
        public double? InZoneNeighborCap { get; init; } = 200;
        public double? OutZoneNeighborCap { get; init; } = 15;
        public double? NeighborDecayRatio { get; init; } = 3;

        public double? NeighborTaper { get; init; } = null;
        public bool SoftNeighborCap { get; init; } = false;
        
    }

    // Row 0 = lowest row, Column 0 = left-most column (most negative PlateX).
    // Row/Column are only meaningful alongside the PitchZoneBin (and its args) that produced them.
    public readonly record struct PitchZone(int Row, int Column);

    public sealed class PitchZoneBin
    {
        private readonly PitchZoneBinArgs args;
        private readonly double cellSize;
        private readonly int zoneRows;
        private readonly int gridRows;
        private readonly int gridColumns;
        private readonly double[] columnEdges;

        public static PitchZoneBin DEFAULT = new(new PitchZoneBinArgs()
        {
            ZoneColumns = 7,
            SquareRowsPerSide = 3,
            OutsideColumns = 7,
            OutsideRows = 7,
            Kernel = null,//PitchZoneKernel.Gaussian(2, 1.0),
            OutZoneNeighborCap = null,
            SeparateBorderRing = true,
            SeparateBorderRingClasses = new HashSet<Db.DbEnums.PitchClass> { Db.DbEnums.PitchClass.Fastball },
        });

        // Pitches from one bin split by cell; NoZone holds everything that fell off the grid.
        private sealed record ZoneBuckets(List<PitchData>[,] Cells, List<PitchData> NoZone);

        // Per-cell totals for one combo, either raw or kernel-smoothed.
        private sealed record CellTotals(double[,] Pitches, double[,] Swings, double[,] ValueSum);

        private enum ZoneRegion { InsideCenter, InsideBorder, Edge, Corner, Other }

        private readonly ZoneRegion[,] regions;

        public PitchZoneBin(PitchZoneBinArgs args)
        {
            // Validate
            ArgumentOutOfRangeException.ThrowIfLessThan(args.ZoneColumns, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(args.SquareRowsPerSide);
            ArgumentOutOfRangeException.ThrowIfNegative(args.OutsideColumns);
            ArgumentOutOfRangeException.ThrowIfNegative(args.OutsideRows);
            ArgumentOutOfRangeException.ThrowIfNegative(args.MinPitches);
            if (args.MinSwingRate is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(args), $"MinSwingRate must be in [0, 1], was {args.MinSwingRate}.");
            }
            if (args.InZoneNeighborCap is < 0 || args.OutZoneNeighborCap is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(args), "Neighbour caps must be non-negative.");
            }
            if (args.NeighborDecayRatio is double decayRatio)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(decayRatio, nameof(args.NeighborDecayRatio));
            }
            if (args.NeighborCountRatio is double countRatio)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(countRatio, nameof(args.NeighborCountRatio));
            }
            ArgumentOutOfRangeException.ThrowIfNegative(args.NeighborCountFloor);
            if (args.InZoneSmoothingCutoff is double inZoneCutoff)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inZoneCutoff, nameof(args.InZoneSmoothingCutoff));
            }
            if (args.OutZoneSmoothingCutoff is double outZoneCutoff)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outZoneCutoff, nameof(args.OutZoneSmoothingCutoff));
            }

            this.args = args;
            cellSize = 2 * BaseballConstants.ZoneHalfWidth / args.ZoneColumns;
            zoneRows = (2 * args.SquareRowsPerSide) + (args.SplitMiddleRow ? 2 : 1);
            gridRows = zoneRows + (2 * args.OutsideRows);
            gridColumns = args.ZoneColumns + (2 * args.OutsideColumns);
            columnEdges = BuildColumnEdges();
            regions = BuildRegions();
        }

        ///////////////////////////////////////// PUBLIC /////////////////////////////
        public (List<PitchZoneCompetitive> Competitive, List<PitchZoneNonCompetitive> NonCompetitive) BuildCompetitiveRows(IEnumerable<PitchData> pitches)
        {
            List<PitchZoneCompetitive> competitiveRows = [];
            List<PitchZoneNonCompetitive> nonCompetitiveRows = [];

            var combos = pitches.GroupBy(p => new { p.Year, p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike });
            foreach (var combo in combos)
            {
                ZoneBuckets buckets = BucketPitches(combo);
                CellTotals effective = ApplyKernel(SumCells(buckets), UsesBorderRing(combo.Key.PitchClass));

                // Pitches that will NOT be valued by a competitive bin: no zone, or a bin failing either rule.
                List<PitchData> nonCompetitivePitches = [.. buckets.NoZone];

                // Every cell gets a row, including empty ones, so a missing row never needs interpreting.
                for (int row = 0; row < gridRows; row++)
                {
                    for (int column = 0; column < gridColumns; column++)
                    {
                        List<PitchData> cellPitches = buckets.Cells[row, column];
                        double effectivePitches = effective.Pitches[row, column];
                        double effectiveSwings = effective.Swings[row, column];
                        bool isCompetitive = IsCompetitive(effectivePitches, effectiveSwings);
                        bool hasEnoughSample = HasValidSampleSize(effectivePitches);
                        competitiveRows.Add(new PitchZoneCompetitive
                        {
                            Year = combo.Key.Year,
                            PitchClass = combo.Key.PitchClass,
                            PitIsR = combo.Key.PitIsR,
                            HitIsR = combo.Key.HitIsR,
                            CountBalls = combo.Key.CountBalls,
                            CountStrikes = combo.Key.CountStrike,
                            ZoneRow = row,
                            ZoneCol = column,
                            PitchCount = (float)effectivePitches,
                            SwingCount = (float)effectiveSwings,
                            IsCompetitive = isCompetitive,
                            HasEnoughSample = hasEnoughSample,
                            ExpectedValueHitter = effectivePitches > 0
                                ? (float)(effective.ValueSum[row, column] / effectivePitches)
                                : 0,
                        });

                        if (!isCompetitive || !hasEnoughSample)
                        {
                            nonCompetitivePitches.AddRange(cellPitches);
                        }
                    }
                }

                nonCompetitiveRows.Add(new PitchZoneNonCompetitive
                {
                    Year = combo.Key.Year,
                    PitchClass = combo.Key.PitchClass,
                    PitIsR = combo.Key.PitIsR,
                    HitIsR = combo.Key.HitIsR,
                    CountBalls = combo.Key.CountBalls,
                    CountStrikes = combo.Key.CountStrike,
                    PitchCount = nonCompetitivePitches.Count,
                    ExpectedValueHitter = nonCompetitivePitches.Count > 0
                        ? nonCompetitivePitches.Average(f => f.RunValueSmoothedHitter)
                        : 0,
                });
            }

            return (competitiveRows, nonCompetitiveRows);
        }

        public PitchZone? GetZone(PitchData pitch)
        {
            return GetZone(pitch.PlateX, pitch.PlateZ, pitch.ZoneTop, pitch.ZoneBot);
        }

        public PitchZone? GetZone(float plateX, float plateZ, float zoneTop, float zoneBot)
        {
            return GetZone(plateX, plateZ, BuildRowEdges(zoneTop, zoneBot));
        }

        private PitchZone? GetZone(float plateX, float plateZ, double[] rowEdges)
        {
            int column = FindCell(columnEdges, plateX);
            int row = FindCell(rowEdges, plateZ);
            return column < 0 || row < 0 ? null : new PitchZone(row, column);
        }


        public void PlotPitchZone(PitchData pitch, string outputPath, int imageSize = 900)
        {
            double[] rowEdges = BuildRowEdges(pitch.ZoneTop, pitch.ZoneBot);
            PitchZone? zone = GetZone(pitch.PlateX, pitch.PlateZ, rowEdges);

            using var plot = new ScottPlot.Plot();
            AddZoneGrid(plot, rowEdges);

            string description = "no zone (uncompetitive)";
            if (zone is PitchZone z)
            {
                var cell = plot.Add.Rectangle(columnEdges[z.Column], columnEdges[z.Column + 1], rowEdges[z.Row], rowEdges[z.Row + 1]);
                cell.FillStyle.Color = ScottPlot.Colors.Orange.WithAlpha(0.4);
                cell.LineStyle.Color = ScottPlot.Colors.Orange;
                cell.LineStyle.Width = 2;
                description = $"zone {ZoneId(z)} (row {z.Row}, col {z.Column}), " +
                              (IsInStrikeZone(z) ? "in strike zone" : "outside strike zone");
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
                Math.Min(columnEdges[0], pitch.PlateX - radius) - pad,
                Math.Max(columnEdges[^1], pitch.PlateX + radius) + pad,
                Math.Min(rowEdges[0], pitch.PlateZ - radius) - pad,
                Math.Max(rowEdges[^1], pitch.PlateZ + radius) + pad);
            plot.Axes.SquareUnits();
            plot.SavePng(outputPath, imageSize, imageSize);
        }

        // TODO : This currently has a slight bug where it will give the incorrect counts if the plot checks for pitchClass to determine
        // if it should smooth around the border and UsesBorderRing(null) returns the wrong value.  However, this is currently
        // just used to get a rough idea so it doesn't matter enough to deal with it currently
        public void PlotZoneCounts(IEnumerable<PitchData> pitches, string label, string outputPath, float displayZoneTop = 3.4f, float displayZoneBot = 1.55f, int imageSize = 1000, Db.DbEnums.PitchClass? pitchClass = null)
        {
            ZoneBuckets buckets = BucketPitches(pitches);
            CellTotals effective = ApplyKernel(SumCells(buckets), UsesBorderRing(pitchClass));

            int total = buckets.NoZone.Count;
            int maxCount = 0;
            foreach (List<PitchData> cellPitches in buckets.Cells)
            {
                total += cellPitches.Count;
                maxCount = Math.Max(maxCount, cellPitches.Count);
            }

            double[] rowEdges = BuildRowEdges(displayZoneTop, displayZoneBot);
            using var plot = new ScottPlot.Plot();

            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    List<PitchData> cellPitches = buckets.Cells[row, column];
                    int count = cellPitches.Count;
                    int swings = cellPitches.Count(p => p.HadSwing);
                    double share = maxCount == 0 ? 0 : (double)count / maxCount;
                    double percent = total == 0 ? 0 : 100.0 * count / total;
                    var cell = plot.Add.Rectangle(columnEdges[column], columnEdges[column + 1], rowEdges[row], rowEdges[row + 1]);

                    // Color depending on competitiveness and percentage of pitches
                    double effectivePitches = effective.Pitches[row, column];
                    double effectiveSwings = effective.Swings[row, column];
                    bool competitive = IsCompetitive(effectivePitches, effectiveSwings)
                        && HasValidSampleSize(effectivePitches);
                    cell.FillStyle.Color = competitive
                        ? ScottPlot.Colors.SteelBlue.WithAlpha(0.15 + (0.7 * share))
                        : ScottPlot.Colors.Red.WithAlpha(0.15);
                    cell.LineStyle.Color = ScottPlot.Colors.Gray;
                    cell.LineStyle.Width = 1;

                    double centreX = (columnEdges[column] + columnEdges[column + 1]) / 2;
                    double centreZ = (rowEdges[row] + rowEdges[row + 1]) / 2;
                    string swingRate = effectivePitches == 0 ? "-" : $"{100.0 * effectiveSwings / effectivePitches:F0}% sw";
                    var text = plot.Add.Text($"{effectivePitches:N1}\n{percent:F1}%\n{swingRate}", centreX, centreZ);
                    text.LabelAlignment = ScottPlot.Alignment.MiddleCenter;
                    text.LabelFontSize = 10;
                }
            }

            AddZoneGrid(plot, rowEdges); // drawn last so the zone outline sits on top of the cells

            int noZone = buckets.NoZone.Count;
            int noZoneSwings = buckets.NoZone.Count(p => p.HadSwing);
            double noZonePercent = total == 0 ? 0 : 100.0 * noZone / total;
            string noZoneSwingRate = noZone == 0 ? "-" : $"{100.0 * noZoneSwings / noZone:F0}% sw";
            plot.Title($"{label} | n={total:N0} | no zone: {noZone:N0} ({noZonePercent:F1}%, {noZoneSwingRate})");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            const double pad = 0.1;
            plot.Axes.SetLimits(columnEdges[0] - pad, columnEdges[^1] + pad, rowEdges[0] - pad, rowEdges[^1] + pad);
            plot.Axes.SquareUnits();
            plot.SavePng(outputPath, imageSize, imageSize);
        }

        // One value per cell with diverging colour (green positive, red negative) scaled to the largest |Value|.
        // Cells missing from the dictionary are drawn grey. noZone is shown in the title.
        public void PlotCellValues(IReadOnlyDictionary<PitchZone, (double Value, string Text)> cells,
            (double Value, string Text)? noZone, string label, string outputPath,
            float displayZoneTop = 3.4f, float displayZoneBot = 1.55f, int imageSize = 1000, double? maxAbsValue = null)
        {
            double maxAbs = maxAbsValue ?? (cells.Count == 0 ? 0 : cells.Values.Max(v => Math.Abs(v.Value)));
            double[] rowEdges = BuildRowEdges(displayZoneTop, displayZoneBot);
            using var plot = new ScottPlot.Plot();

            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    var cell = plot.Add.Rectangle(columnEdges[column], columnEdges[column + 1], rowEdges[row], rowEdges[row + 1]);
                    cell.LineStyle.Color = ScottPlot.Colors.Gray;
                    cell.LineStyle.Width = 1;

                    string cellText = "-";
                    cell.FillStyle.Color = ScottPlot.Colors.LightGray.WithAlpha(0.3);
                    if (cells.TryGetValue(new PitchZone(row, column), out (double Value, string Text) entry))
                    {
                        double share = maxAbs == 0 ? 0 : Math.Min(1, Math.Abs(entry.Value) / maxAbs);
                        ScottPlot.Color extreme = entry.Value >= 0 ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;
                        cell.FillStyle.Color = BlendFromNeutral(extreme, share);
                        cellText = entry.Text;
                    }

                    double centreX = (columnEdges[column] + columnEdges[column + 1]) / 2;
                    double centreZ = (rowEdges[row] + rowEdges[row + 1]) / 2;
                    var text = plot.Add.Text(cellText, centreX, centreZ);
                    text.LabelAlignment = ScottPlot.Alignment.MiddleCenter;
                    text.LabelFontSize = 10;
                }
            }

            AddZoneGrid(plot, rowEdges); // drawn last so the zone outline sits on top of the cells

            string noZoneText = noZone.HasValue ? noZone.Value.Text.Replace('\n', ' ') : "-";
            plot.Title($"{label} | no zone: {noZoneText}");
            plot.XLabel("PlateX (ft)");
            plot.YLabel("PlateZ (ft)");

            const double pad = 0.1;
            plot.Axes.SetLimits(columnEdges[0] - pad, columnEdges[^1] + pad, rowEdges[0] - pad, rowEdges[^1] + pad);
            plot.Axes.SquareUnits();
            plot.SavePng(outputPath, imageSize, imageSize);
        }

        // Neutral gray at fraction 0, the full target colour at fraction 1, so near-zero cells read as "about even".
        private static ScottPlot.Color BlendFromNeutral(ScottPlot.Color target, double fraction)
        {
            const byte NEUTRAL = 190;
            fraction = Math.Clamp(fraction, 0, 1);
            byte Mix(byte extreme) => (byte)Math.Round(NEUTRAL + ((extreme - NEUTRAL) * fraction));
            return new ScottPlot.Color(Mix(target.R), Mix(target.G), Mix(target.B), 200);
        }

        ///////////////////////////////////////// Build Zone Rows/Columns /////////////////////////////
        private double[] BuildColumnEdges()
        {
            double[] edges = new double[gridColumns + 1];
            double zoneLeft = -BaseballConstants.ZoneHalfWidth;
            double zoneRight = BaseballConstants.ZoneHalfWidth;
            int zoneLeftIndex = args.OutsideColumns;
            int zoneRightIndex = args.OutsideColumns + args.ZoneColumns;

            // Outside-left edges, ending exactly on the zone's left edge.
            for (int i = 0; i <= args.OutsideColumns; i++)
            {
                edges[i] = zoneLeft - ((args.OutsideColumns - i) * cellSize);
            }

            // Equal-width columns across the zone.
            for (int i = 1; i < args.ZoneColumns; i++)
            {
                edges[zoneLeftIndex + i] = zoneLeft + (i * cellSize);
            }

            // Zone's right edge, then outside-right edges.
            for (int i = 0; i <= args.OutsideColumns; i++)
            {
                edges[zoneRightIndex + i] = zoneRight + (i * cellSize);
            }

            return edges;
        }

        private double[] BuildRowEdges(float zoneTop, float zoneBot)
        {
            double zoneBottomEdge = zoneBot - BaseballConstants.BallRadius;
            double zoneTopEdge = zoneTop + BaseballConstants.BallRadius;
            double middleHeight = (zoneTopEdge - zoneBottomEdge) - (2 * args.SquareRowsPerSide * cellSize);

            double[] edges = new double[gridRows + 1];

            // Outside-below edges, ending exactly on the zone's bottom edge.
            for (int i = 0; i <= args.OutsideRows; i++)
            {
                edges[i] = zoneBottomEdge - ((args.OutsideRows - i) * cellSize);
            }

            // Square rows up from the bottom of the zone.
            for (int i = 1; i <= args.SquareRowsPerSide; i++)
            {
                edges[args.OutsideRows + i] = zoneBottomEdge + (i * cellSize);
            }

            // Split the middle into two equal-height rows; the only edge not set by the square-row loops.
            if (args.SplitMiddleRow && middleHeight >= 0)
            {
                edges[args.OutsideRows + args.SquareRowsPerSide + 1] =
                    zoneBottomEdge + (args.SquareRowsPerSide * cellSize) + (middleHeight / 2);
            }

            // Square rows down from the top of the zone (the gap between these and the ones above is the middle row).
            int topZoneEdgeIndex = args.OutsideRows + zoneRows;
            for (int i = 0; i <= args.SquareRowsPerSide; i++)
            {
                edges[topZoneEdgeIndex - i] = zoneTopEdge - (i * cellSize);
            }

            // Outside-above edges.
            for (int i = 1; i <= args.OutsideRows; i++)
            {
                edges[topZoneEdgeIndex + i] = zoneTopEdge + (i * cellSize);
            }

            // Need to adjust the some edges if there is not a middle bin
            if (middleHeight < 0)
            {
                double midpoint = ((zoneTopEdge - zoneBottomEdge) / 2) + zoneBottomEdge;
                edges[topZoneEdgeIndex - args.SquareRowsPerSide] = midpoint;
                edges[topZoneEdgeIndex - args.SquareRowsPerSide - 1] = midpoint;

                if (args.SplitMiddleRow)
                {
                    edges[topZoneEdgeIndex - args.SquareRowsPerSide - 2] = midpoint;
                }
            }

            return edges;
        }

        private ZoneRegion[,] BuildRegions()
        {
            ZoneRegion[,] result = new ZoneRegion[gridRows, gridColumns];
            int zoneBottomRow = args.OutsideRows;
            int zoneTopRow = args.OutsideRows + zoneRows;
            int zoneLeftColumn = args.OutsideColumns;
            int zoneRightColumn = args.OutsideColumns + args.ZoneColumns;
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    // Cells away from the zone on each axis; 0 when the cell is level with the zone.
                    int rowGap = row < zoneBottomRow ? zoneBottomRow - row
                        : row >= zoneTopRow ? row - zoneTopRow + 1
                        : 0;
                    int columnGap = column < zoneLeftColumn ? zoneLeftColumn - column
                        : column >= zoneRightColumn ? column - zoneRightColumn + 1
                        : 0;
                    if (rowGap == 0 && columnGap == 0)
                    {
                        bool onZoneBoundary = row == zoneBottomRow || row == zoneTopRow - 1
                                                    || column == zoneLeftColumn || column == zoneRightColumn - 1;
                        result[row, column] = onZoneBoundary ? ZoneRegion.InsideBorder : ZoneRegion.InsideCenter;
                    }
                    else if (rowGap <= 1 && columnGap <= 1)
                    {
                        result[row, column] = rowGap == 1 && columnGap == 1 ? ZoneRegion.Corner : ZoneRegion.Edge;
                    }
                    else
                    {
                        result[row, column] = ZoneRegion.Other;
                    }
                }
            }
            return result;
        }

        // Whether a cell of region target may use a cell of region source when smoothing (ring mode only).
        private static bool CanBorrow(ZoneRegion target, ZoneRegion source)
        {
            return target switch
            {
                ZoneRegion.InsideCenter => source is ZoneRegion.InsideBorder or ZoneRegion.InsideCenter,
                ZoneRegion.InsideBorder => source is ZoneRegion.InsideBorder,
                ZoneRegion.Edge => source is ZoneRegion.Edge or ZoneRegion.Corner,
                _ => source is not (ZoneRegion.InsideCenter or ZoneRegion.InsideBorder),
            };
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

        private ZoneBuckets BucketPitches(IEnumerable<PitchData> pitches)
        {
            List<PitchData>[,] cells = new List<PitchData>[gridRows, gridColumns];
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    cells[row, column] = [];
                }
            }

            List<PitchData> noZone = [];
            foreach (PitchData pitch in pitches)
            {
                if (GetZone(pitch) is PitchZone zone)
                {
                    cells[zone.Row, zone.Column].Add(pitch);
                }
                else
                {
                    noZone.Add(pitch);
                }
            }

            return new ZoneBuckets(cells, noZone);
        }

        ///////////////////////////////////////// Rules for Zone Validity /////////////////////////////
        private bool IsCompetitive(double pitchCount, double swingCount)
        {
            return swingCount >= args.MinSwingRate * pitchCount;
        }

        private bool HasValidSampleSize(double pitchCount)
        {
            return pitchCount >= args.MinPitches;
        }

        public bool IsInStrikeZone(PitchZone zone)
        {
            return zone.Row >= args.OutsideRows && zone.Row < args.OutsideRows + zoneRows
                && zone.Column >= args.OutsideColumns && zone.Column < args.OutsideColumns + args.ZoneColumns;
        }

        private int ZoneId(PitchZone zone)
        {
            return (zone.Row * gridColumns) + zone.Column;
        }

        ///////////////////////////////////////// Plotting /////////////////////////////
        private void AddZoneGrid(ScottPlot.Plot plot, double[] rowEdges)
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

            var outline = plot.Add.Rectangle(
                columnEdges[args.OutsideColumns], columnEdges[args.OutsideColumns + args.ZoneColumns],
                rowEdges[args.OutsideRows], rowEdges[args.OutsideRows + zoneRows]);
            outline.FillStyle.Color = ScottPlot.Colors.Transparent;
            outline.LineStyle.Color = ScottPlot.Colors.Black;
            outline.LineStyle.Width = 2.5f;
        }

        ///////////////////////////////////////// Kernel Handling /////////////////////////////
        private CellTotals SumCells(ZoneBuckets buckets)
        {
            double[,] pitches = new double[gridRows, gridColumns];
            double[,] swings = new double[gridRows, gridColumns];
            double[,] valueSum = new double[gridRows, gridColumns];
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    List<PitchData> cellPitches = buckets.Cells[row, column];
                    pitches[row, column] = cellPitches.Count;
                    swings[row, column] = cellPitches.Count(p => p.HadSwing);
                    valueSum[row, column] = cellPitches.Sum(p => (double)p.RunValueSmoothedHitter);
                }
            }

            return new CellTotals(pitches, swings, valueSum);
        }

        private bool UsesBorderRing(Db.DbEnums.PitchClass? pitchClass)
        {
            return args.SeparateBorderRingClasses is { } classes && pitchClass is { } c
                ? classes.Contains(c)
                : args.SeparateBorderRing;
        }

        private CellTotals ApplyKernel(CellTotals raw, bool useBorderRing)
        {
            if (args.Kernel is not PitchZoneKernel kernel)
            {
                return raw;
            }

            double[,] pitches = new double[gridRows, gridColumns];
            double[,] swings = new double[gridRows, gridColumns];
            double[,] valueSum = new double[gridRows, gridColumns];

            
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    bool cellInZone = IsInStrikeZone(new PitchZone(row, column));
                    double? cap = cellInZone ? args.InZoneNeighborCap : args.OutZoneNeighborCap;

                    // Largest effective count any single neighbour may contribute to this cell
                    double ? maxSourcePitches = args.NeighborCountRatio is double countRatio
                        ? countRatio * Math.Max(raw.Pitches[row, column], args.NeighborCountFloor)
                        : null;

                    double neighborPitches = 0, neighborSwings = 0, neighborValue = 0;
                    for (int rowOffset = -kernel.Radius; rowOffset <= kernel.Radius; rowOffset++)
                    {
                        for (int columnOffset = -kernel.Radius; columnOffset <= kernel.Radius; columnOffset++)
                        {
                            int sourceRow = row + rowOffset;
                            int sourceColumn = column + columnOffset;
                            if (sourceRow < 0 || sourceRow >= gridRows || sourceColumn < 0 || sourceColumn >= gridColumns)
                            {
                                continue;
                            }

                            // Check if the cell is eligible for the target to be used in smoothing
                            if (useBorderRing)
                            {
                                if (!CanBorrow(regions[row, column], regions[sourceRow, sourceColumn]))
                                {
                                    continue;
                                }
                            }
                            else if (IsInStrikeZone(new PitchZone(sourceRow, sourceColumn)) != cellInZone)
                            {
                                continue;
                            }

                            double weight = kernel.Weight(rowOffset, columnOffset);
                            if (rowOffset == 0 && columnOffset == 0)
                            {
                                // Centre weight is 1 after normalisation, so the cell always counts fully
                                pitches[row, column] = raw.Pitches[row, column];
                                swings[row, column] = raw.Swings[row, column];
                                valueSum[row, column] = raw.ValueSum[row, column];
                            }
                            else
                            {

                                double sourcePitches = raw.Pitches[sourceRow, sourceColumn];
                                if (maxSourcePitches is double maxSource && sourcePitches > maxSource)
                                {
                                    weight *= maxSource / sourcePitches;
                                }
                                neighborPitches += weight * raw.Pitches[sourceRow, sourceColumn];
                                neighborSwings += weight * raw.Swings[sourceRow, sourceColumn];
                                neighborValue += weight * raw.ValueSum[sourceRow, sourceColumn];
                            }
                        }
                    }

                    double scale = 1;
                    if (cap is double limit && neighborPitches > 0)
                    {
                        scale = args.SoftNeighborCap
                            ? limit / (limit + neighborPitches)
                            : Math.Min(1, limit / neighborPitches);
                    }
                    if (args.NeighborTaper is double taper)
                    {
                        scale *= taper / (taper + raw.Pitches[row, column]);
                    }
                    // Linear hand-off to the cell's own data, reaching zero neighbour influence at the cutoff
                    if ((cellInZone ? args.InZoneSmoothingCutoff : args.OutZoneSmoothingCutoff) is double cutoff)
                    {
                        scale *= Math.Max(0, 1 - (raw.Pitches[row, column] / cutoff));
                    }

                    pitches[row, column] += scale * neighborPitches;
                    swings[row, column] += scale * neighborSwings;
                    valueSum[row, column] += scale * neighborValue;
                }
            }

            return new CellTotals(pitches, swings, valueSum);
        }
    }
}
