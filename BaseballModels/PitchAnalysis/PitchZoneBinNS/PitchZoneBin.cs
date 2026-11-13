using PitchTrackingDb;

namespace PitchAnalysis.PitchZoneBinNS
{
    public partial class PitchZoneBin
    {
        private readonly PitchZoneBinArgs args;
        private readonly double cellSize;
        private readonly int zoneRows;
        public readonly int gridRows;
        public readonly int gridColumns;
        private readonly double[] columnEdges;
        public PitchZoneGeometry Geometry { get; }

        public static PitchZoneBin DEFAULT = new(new PitchZoneBinArgs()
        {
            ZoneColumns = 7,
            SquareRowsPerSide = 3,
            OutsideColumns = 7,
            OutsideRows = 7,
            Kernel = PitchZoneKernel.Gaussian(2, 1.0),
            OutZoneNeighborCap = null,
            SeparateBorderRing = true,
            NeighborCountRatio = 1,
            YearsPerBin = 4,
            YearDecay = 0.75,
            SplitMiddleRow = true,
            SeparateMiddle = true
        });

        // Pitches from one bin split by cell; NoZone holds everything that fell off the grid.
        private sealed record ZoneBuckets(List<PitchData>[,] Cells, List<PitchData> NoZone);

        // Per-cell totals for one combo, either raw or kernel-smoothed.
        private sealed record CellTotals(double[,] Pitches, double[,] Swings, double[,] ValueSum);

        private enum ZoneRegion { InsideMiddle, InsideCenter, InsideBorder, Edge, Corner, Other }
        private readonly ZoneRegion[,] regions;

        public PitchZoneBin(PitchZoneBinArgs args)
        {
            // Validate
            ArgumentOutOfRangeException.ThrowIfLessThan(args.ZoneColumns, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(args.SquareRowsPerSide);
            ArgumentOutOfRangeException.ThrowIfNegative(args.OutsideColumns);
            ArgumentOutOfRangeException.ThrowIfNegative(args.OutsideRows);
            ArgumentOutOfRangeException.ThrowIfNegative(args.MinPitches);
            ArgumentOutOfRangeException.ThrowIfLessThan(args.YearsPerBin, 1);
            if (args.MinSwingRate is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(args), $"MinSwingRate must be in [0, 1], was {args.MinSwingRate}.");
            }
            if (args.InZoneNeighborCap is < 0 || args.OutZoneNeighborCap is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(args), "Neighbour caps must be non-negative.");
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
            if (args.YearDecay is <= 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(args), $"YearDecay must be in (0, 1], was {args.YearDecay}.");
            }

            this.args = args;
            cellSize = 2 * BaseballConstants.ZoneHalfWidth / args.ZoneColumns;
            zoneRows = (2 * args.SquareRowsPerSide) + (args.SplitMiddleRow ? 2 : 1);
            gridRows = zoneRows + (2 * args.OutsideRows);
            gridColumns = args.ZoneColumns + (2 * args.OutsideColumns);
            columnEdges = BuildColumnEdges();
            regions = BuildRegions();
            Geometry = new PitchZoneGeometry(args.ZoneColumns, args.SquareRowsPerSide, args.SplitMiddleRow, args.OutsideColumns, args.OutsideRows);
        }

        public int YearsPerBin => args.YearsPerBin;

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
    }
}
