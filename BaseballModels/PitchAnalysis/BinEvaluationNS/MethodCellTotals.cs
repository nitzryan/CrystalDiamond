using PitchAnalysis.PitchZoneBinNS;

namespace PitchAnalysis.BinEvaluationNS
{
    internal static partial class BinEvaluation
    {
        // Squared errors for the pitches landing in one cell (or off the grid) of a method's layout
        private sealed class CellErrors
        {
            public double MethodSse { get; set; }
            public double ReferenceSse { get; set; }
            public double BaselineSse { get; set; }
            public double MethodResidualSum { get; set; }
            public long Count { get; set; }
        }
        // All cells of one method, pooled over every year and situation
        private sealed class MethodCellTotals
        {
            public Dictionary<PitchZone, CellErrors> Cells { get; } = new();
            public CellErrors NoZone { get; } = new();

            public void Add(PitchZone? zone, double methodSe, double referenceSe, double baselineSe, double methodResidual)
            {
                CellErrors cell = NoZone;
                if (zone is PitchZone z)
                {
                    if (!Cells.TryGetValue(z, out CellErrors? existing))
                    {
                        existing = new();
                        Cells[z] = existing;
                    }

                    cell = existing;
                }

                cell.MethodSse += methodSe;
                cell.ReferenceSse += referenceSe;
                cell.BaselineSse += baselineSe;
                cell.MethodResidualSum += methodResidual;
                cell.Count++;
            }
        }
    }
}
