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

        private bool IsEligibleNeighbor(int row, int column, int sourceRow, int sourceColumn, bool useBorderRing)
        {
            if (useBorderRing)
            {
                return CanBorrow(regions[row, column], regions[sourceRow, sourceColumn]);
            }
            bool cellInZone = IsInStrikeZone(new PitchZone(row, column));
            return IsInStrikeZone(new PitchZone(sourceRow, sourceColumn)) == cellInZone
                && !(regions[row, column] == ZoneRegion.InsideMiddle && regions[sourceRow, sourceColumn] != ZoneRegion.InsideMiddle);
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

        private CellTotals ApplyKernel(CellTotals raw, bool useBorderRing, Dictionary<KGroup, double>? groupK = null)
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

                    // A fitted K replaces the cap for cells in an applied group (K = infinity leaves the neighbors uncapped)
                    if (groupK is not null && GetKGroup(row, column) is KGroup group && groupK.TryGetValue(group, out double groupCap))
                    {
                        cap = double.IsInfinity(groupCap) ? null : groupCap;
                    }

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

                            if (!IsEligibleNeighbor(row, column, sourceRow, sourceColumn, useBorderRing))
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

        // Per scenario-cell deviation from the kernel-weighted mean of its eligible neighbors (cell excluded), over the bin window ending at binYear.
        public List<CellTau> GetCellTau(IEnumerable<PitchData> pitches, int binYear)
        {
            List<CellTau> rows = [];
            if (args.Kernel is not PitchZoneKernel kernel)
            {
                return rows;
            }
            
            var scenarios = GetCellNoise(pitches, binYear)
                .GroupBy(c => (c.PitchClass, c.PitIsR, c.HitIsR, c.CountBalls, c.CountStrikes));
            
            foreach (var scenario in scenarios)
            {
                double[,] counts = new double[gridRows, gridColumns];
                double[,] sums = new double[gridRows, gridColumns];
                double[,] sumsSq = new double[gridRows, gridColumns];
                
                foreach (CellNoise cell in scenario)
                {
                    counts[cell.Row, cell.Column] = cell.PitchCount;
                    sums[cell.Row, cell.Column] = cell.ValueSum;
                    sumsSq[cell.Row, cell.Column] = cell.ValueSumSq;
                }
                
                for (int row = 0; row < gridRows; row++)
                {
                    for (int column = 0; column < gridColumns; column++)
                    {
                        double n = counts[row, column];
                        if (n <= 0)
                        {
                            continue;
                        }
                        
                        double? maxSource = args.NeighborCountRatio is double ratio
                            ? ratio * Math.Max(n, args.NeighborCountFloor)
                            : null;
                        double weightSum = 0, weightedMeans = 0, weightedSampling = 0, weightSquares = 0;
                        double noiseSs = sumsSq[row, column] - (sums[row, column] * sums[row, column] / n);
                        double noiseDf = n - 1;
                        
                        for (int rowOffset = -kernel.Radius; rowOffset <= kernel.Radius; rowOffset++)
                        {
                            for (int columnOffset = -kernel.Radius; columnOffset <= kernel.Radius; columnOffset++)
                            {
                                int sourceRow = row + rowOffset;
                                int sourceColumn = column + columnOffset;
                                bool outside = sourceRow < 0 || sourceRow >= gridRows || sourceColumn < 0 || sourceColumn >= gridColumns;
                                
                                if (outside || (rowOffset == 0 && columnOffset == 0) || counts[sourceRow, sourceColumn] <= 0
                                    || !IsEligibleNeighbor(row, column, sourceRow, sourceColumn, args.SeparateBorderRing))
                                {
                                    continue;
                                }
                                
                                double sourceCount = counts[sourceRow, sourceColumn];
                                double sourceSum = sums[sourceRow, sourceColumn];
                                double weight = kernel.Weight(rowOffset, columnOffset) * Math.Min(sourceCount, maxSource ?? sourceCount);
                                
                                weightSum += weight;
                                weightSquares += weight * weight;
                                weightedMeans += weight * (sourceSum / sourceCount);
                                weightedSampling += weight * weight / sourceCount;
                                
                                noiseSs += sumsSq[sourceRow, sourceColumn] - (sourceSum * sourceSum / sourceCount);
                                noiseDf += sourceCount - 1;
                            }
                        }
                        
                        if (weightSum <= 0 || noiseDf <= 0)
                        {
                            continue;
                        }
                        
                        double noiseVariance = noiseSs / noiseDf;
                        double deviation = (sums[row, column] / n) - (weightedMeans / weightSum);
                        double samplingVariance = noiseVariance * ((1 / n) + (weightedSampling / (weightSum * weightSum)));
                        double tauFactor = 1 + (weightSquares / (weightSum * weightSum));
                        
                        rows.Add(new CellTau(scenario.Key.PitchClass, scenario.Key.PitIsR, scenario.Key.HitIsR, scenario.Key.CountBalls,
                            scenario.Key.CountStrikes, row, column, n, deviation * deviation, samplingVariance, tauFactor, noiseVariance));
                    }
                }
            }
            return rows;
        }
    }
}
