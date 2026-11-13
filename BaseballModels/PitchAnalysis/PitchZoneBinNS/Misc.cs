using static Db.DbEnums;

namespace PitchAnalysis.PitchZoneBinNS
{
    public static class BaseballConstants
    {
        public const double BallDiameter = 0.24;
        public const double BallRadius = BallDiameter / 2;
        public const double PlateWidth = 17.0 / 12.0;

        // Furthest a ball center can be from the plate center while still touching the zone (~0.828ft).
        public const double ZoneHalfWidth = (PlateWidth / 2) + BallRadius;
    }

    // Many bins share the same geometry, so much of what is done here can be shared between them
    public readonly record struct PitchZoneGeometry(int ZoneColumns, int SquareRowsPerSide, bool SplitMiddleRow, int OutsideColumns, int OutsideRows);
    internal readonly record struct ScenarioKey(PitchClass PitchClass, bool PitIsR, bool HitIsR, int CountBalls, int CountStrike);
    internal sealed class YearCells(int rows, int columns)
    {
        public double[,] Pitches { get; } = new double[rows, columns];
        public double[,] Swings { get; } = new double[rows, columns];
        public double[,] ValueSum { get; } = new double[rows, columns];
        public double NoZonePitches { get; set; }
        public double NoZoneValue { get; set; }
    }

    // Row 0 = lowest row, Column 0 = left-most column (most negative PlateX).
    // Row/Column are only meaningful alongside the PitchZoneBin (and its args) that produced them.
    public readonly record struct PitchZone(int Row, int Column);
}
