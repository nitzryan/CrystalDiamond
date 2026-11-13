using PitchTrackingDb;
using static Db.DbEnums;

namespace PitchAnalysis.PitchZoneBinNS
{
    public partial class PitchZoneBin
    {
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

            // Exact ball center , for reading position against a boundary.
            var center = plot.Add.Circle(pitch.PlateX, pitch.PlateZ, 0.015);
            center.FillStyle.Color = ScottPlot.Colors.Black;
            center.LineStyle.Color = ScottPlot.Colors.Black;

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
        public void PlotZoneCounts(IEnumerable<PitchData> pitches, string label, string outputPath, float displayZoneTop = 3.4f, float displayZoneBot = 1.55f, int imageSize = 1000, PitchClass? pitchClass = null)
        {
            List<PitchData> pitchList = [.. pitches];
            ZoneBuckets buckets = BucketPitches(pitchList);
            int targetYear = pitchList.Count == 0 ? 0 : pitchList.Max(p => p.Year);
            CellTotals effective = ApplyKernel(SumCells(buckets, targetYear), args.SeparateBorderRing);

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

                    double centerX = (columnEdges[column] + columnEdges[column + 1]) / 2;
                    double centerZ = (rowEdges[row] + rowEdges[row + 1]) / 2;
                    string swingRate = effectivePitches == 0 ? "-" : $"{100.0 * effectiveSwings / effectivePitches:F0}% sw";
                    var text = plot.Add.Text($"{effectivePitches:N1}\n{percent:F1}%\n{swingRate}", centerX, centerZ);
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

                    double centerX = (columnEdges[column] + columnEdges[column + 1]) / 2;
                    double centerZ = (rowEdges[row] + rowEdges[row + 1]) / 2;
                    var text = plot.Add.Text(cellText, centerX, centerZ);
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

        // Baseline to add the gridlines with the hard strikezone
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
    }
}
