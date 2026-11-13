namespace PitchAnalysis.PitchZoneBinNS
{
    public sealed record PitchZoneBinArgs
    {
        public int ZoneColumns { get; init; } = 7;
        public int SquareRowsPerSide { get; init; } = 3;
        public bool SplitMiddleRow { get; init; } = false;
        public int OutsideColumns { get; init; } = 7;
        public int OutsideRows { get; init; } = 7;
        public int MinPitches { get; init; } = 5;
        public double MinSwingRate { get; init; } = 0.10;
        public int YearsPerBin { get; init; } = 1;
        public double YearDecay { get; init; } = 1.0;
        public PitchZoneKernel? Kernel { get; init; } = PitchZoneKernel.Gaussian(2, 1.0);

        public bool SeparateBorderRing { get; init; } = false;
        public bool SeparateMiddle { get; init; } = false;

        public double? NeighborCountRatio { get; init; } = null;
        public double NeighborCountFloor { get; init; } = 5;
        public double? InZoneSmoothingCutoff { get; init; } = null;
        public double? OutZoneSmoothingCutoff { get; init; } = null;

        // All parameters below this currently are not used in the optimal model, and can potentially be removed.

        // Max total weight (in pitches) the neighbours may add to a cell, chosen by whether the cell is in the strike zone.
        // Ignored when Kernel is null.
        public double? InZoneNeighborCap { get; init; } = 200;
        public double? OutZoneNeighborCap { get; init; } = 15;
    }
}
