using PitchAnalysis.PitchZoneBinNS;

namespace PitchAnalysis.BinEvaluationNS
{
    internal static partial class BinEvaluation
    {
        private const string BASELINE_NAME = "Situation Mean";
        private static readonly PitchZoneBinArgs ZONE_SMALLER = new()
        {
            ZoneColumns = 5,
            SquareRowsPerSide = 2,
            OutsideColumns = 5,
            OutsideRows = 5,
            Kernel = null,
            InZoneNeighborCap = null,
        };
        private static readonly PitchZoneBinArgs ZONE_BIGGER = new()
        {
            ZoneColumns = 7,
            SquareRowsPerSide = 3,
            OutsideColumns = 7,
            OutsideRows = 7,
            Kernel = null,
            OutZoneNeighborCap = null,
        };

        private sealed record Method(string Name, PitchZoneBin Bin);

        private static readonly Method[] METHODS =
        [
            new("Current Bins", new PitchZoneBin(ZONE_BIGGER)),

            new("Gaussian", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0) })),

            new("Gaussian Border", new PitchZoneBin(ZONE_BIGGER with { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true })),

            new("CountCap", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=false, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),
            new("Border CountCap", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1 })),

            new("Border CountCap Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true })),
            new("Border CountCap Split-Middle 2Yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true, YearsPerBin = 2 })),
            new("Border CountCap Split-Middle 3Yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, SplitMiddleRow=true, YearsPerBin = 3 })),

            new("Border CountCap 2yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 2 })),
            new("Border CountCap 3yr", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 3 })),

            new("Fade InZone 4_1000", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, InZoneSmoothingCutoff = 1000, OutZoneSmoothingCutoff = 500 })),

            new("Border CountCap x1 4yr d0.75", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75 })),

            new("Border CountCap x1 3yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 3, YearDecay = 0.75, SplitMiddleRow=true })),
            new("Border CountCap x1 4yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow=true })),
            new("Border CountCap x1 5yr d0.75 Split-Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null, NeighborCountRatio = 1, YearsPerBin = 5, YearDecay = 0.75, SplitMiddleRow=true })),

            new("4yr d0.75 Border Split", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow = true })),
            new("4yr d0.75 Border Split Middle", new PitchZoneBin(ZONE_BIGGER with
                { Kernel = PitchZoneKernel.Gaussian(2, 1.0), SeparateBorderRing=true, InZoneNeighborCap = null, OutZoneNeighborCap = null,
                  NeighborCountRatio = 1, YearsPerBin = 4, YearDecay = 0.75, SplitMiddleRow = true, SeparateMiddle = true })),
        ];

        // Index into METHODS of the bins the per-cell plots are compared against
        private const int REFERENCE_METHOD = 0;
    }
}
