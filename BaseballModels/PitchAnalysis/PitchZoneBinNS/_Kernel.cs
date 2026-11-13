using PitchTrackingDb;

namespace PitchAnalysis.PitchZoneBinNS
{
    public partial class PitchZoneBin
    {
        // Whether a cell of region target may use a cell of region source when smoothing (ring mode only).
        private static bool CanBorrow(ZoneRegion target, ZoneRegion source)
        {
            return target switch
            {
                ZoneRegion.InsideMiddle => source is ZoneRegion.InsideMiddle,
                ZoneRegion.InsideCenter => source is ZoneRegion.InsideBorder or ZoneRegion.InsideCenter or ZoneRegion.InsideMiddle,
                ZoneRegion.InsideBorder => source is ZoneRegion.InsideBorder,
                ZoneRegion.Edge => source is ZoneRegion.Edge or ZoneRegion.Corner,
                _ => source is not (ZoneRegion.InsideMiddle or ZoneRegion.InsideCenter or ZoneRegion.InsideBorder),
            };
        }

        private double YearWeight(int targetYear, int pitchYear)
        {
            return Math.Pow(args.YearDecay, targetYear - pitchYear);
        }

        private CellTotals SumCells(ZoneBuckets buckets, int targetYear)
        {
            double[,] pitches = new double[gridRows, gridColumns];
            double[,] swings = new double[gridRows, gridColumns];
            double[,] valueSum = new double[gridRows, gridColumns];
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    List<PitchData> cellPitches = buckets.Cells[row, column];
                    pitches[row, column] = cellPitches.Sum(p => YearWeight(targetYear, p.Year));
                    swings[row, column] = cellPitches.Where(p => p.HadSwing).Sum(p => YearWeight(targetYear, p.Year));
                    valueSum[row, column] = cellPitches.Sum(p => YearWeight(targetYear, p.Year) * p.RunValueSmoothedHitter);
                }
            }

            return new CellTotals(pitches, swings, valueSum);
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
                    double? maxSourcePitches = args.NeighborCountRatio is double countRatio
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
                            else if (IsInStrikeZone(new PitchZone(sourceRow, sourceColumn)) != cellInZone
                                || (regions[row, column] == ZoneRegion.InsideMiddle && regions[sourceRow, sourceColumn] != ZoneRegion.InsideMiddle))
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
                        scale = Math.Min(1, limit / neighborPitches);
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
