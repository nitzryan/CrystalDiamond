using PitchTrackingDb;
using static Db.DbEnums;

namespace PitchAnalysis.PitchZoneBinNS
{
    // Region groups K is fit over. Cells in none of these (far outside the zone) keep the existing smoothing, i.e. K = infinity.
    internal enum KGroup
    {
        InBottomLeft, InBottomMiddle, InBottomRight,
        InMiddleLeft, InMiddleMiddle, InMiddleRight,
        InTopLeft, InTopMiddle, InTopRight,
        Above1, Above2, Above3,
        Below1, Below2, Below3,
        Left1, Left2, Left3,
        Right1, Right2, Right3,
        CornerBottomLeft, CornerBottomRight, CornerTopLeft, CornerTopRight,
    }

    // Scenario dimensions a K fit is pooled over
    internal readonly record struct KFitKey(PitchClass PitchClass, bool HitIsR, int CountBalls, int CountStrikes);

    public partial class PitchZoneBin
    {
        private const int K_OUTSIDE_DEPTH = 3;

        // Region group of a grid cell for K fitting; null outside the grouped area (K = infinity there).
        private KGroup? GetKGroup(int row, int column)
        {
            int zoneBottomRow = args.OutsideRows;
            int zoneTopRow = args.OutsideRows + zoneRows;
            int zoneLeftColumn = args.OutsideColumns;
            int zoneRightColumn = args.OutsideColumns + args.ZoneColumns;

            // Signed distance in cells from the zone on each axis (negative = below/left, 0 = level with the zone)
            int rowGap = row < zoneBottomRow ? row - zoneBottomRow : row >= zoneTopRow ? row - zoneTopRow + 1 : 0;
            int columnGap = column < zoneLeftColumn ? column - zoneLeftColumn : column >= zoneRightColumn ? column - zoneRightColumn + 1 : 0;
            if (rowGap == 0 && columnGap == 0)
            {
                int edgeRows = Math.Min((int)Math.Ceiling(args.KEdgeFraction * zoneRows), (zoneRows - 1) / 2);
                int edgeColumns = Math.Min((int)Math.Ceiling(args.KEdgeFraction * args.ZoneColumns), (args.ZoneColumns - 1) / 2);
                int zoneRow = row - zoneBottomRow;
                int zoneColumn = column - zoneLeftColumn;
                int rowBand = zoneRow < edgeRows ? 0 : zoneRow >= zoneRows - edgeRows ? 2 : 1;
                int columnBand = zoneColumn < edgeColumns ? 0 : zoneColumn >= args.ZoneColumns - edgeColumns ? 2 : 1;
                
                return (rowBand, columnBand) switch
                {
                    (0, 0) => KGroup.InBottomLeft,
                    (0, 1) => KGroup.InBottomMiddle,
                    (0, 2) => KGroup.InBottomRight,
                    (1, 0) => KGroup.InMiddleLeft,
                    (1, 1) => KGroup.InMiddleMiddle,
                    (1, 2) => KGroup.InMiddleRight,
                    (2, 0) => KGroup.InTopLeft,
                    (2, 1) => KGroup.InTopMiddle,
                    (2, 2) => KGroup.InTopRight,
                    _ => throw new InvalidOperationException($"Unexpected zone band ({rowBand}, {columnBand})."),
                };
            }

            if (Math.Abs(rowGap) > 3 || Math.Abs(columnGap) > 3)
            {
                return null;
            }

            if (columnGap == 0)
            {
                return (rowGap > 0, Math.Abs(rowGap)) switch
                {
                    (true, 1) => KGroup.Above1,
                    (true, 2) => KGroup.Above2,
                    (true, 3) => KGroup.Above3,
                    (false, 1) => KGroup.Below1,
                    (false, 2) => KGroup.Below2,
                    (false, 3) => KGroup.Below3,
                    _ => throw new InvalidOperationException($"Unexpected row gap {rowGap}."),
                };
            }

            if (rowGap == 0)
            {
                return (columnGap > 0, Math.Abs(columnGap)) switch
                {
                    (false, 1) => KGroup.Left1,
                    (false, 2) => KGroup.Left2,
                    (false, 3) => KGroup.Left3,
                    (true, 1) => KGroup.Right1,
                    (true, 2) => KGroup.Right2,
                    (true, 3) => KGroup.Right3,
                    _ => throw new InvalidOperationException($"Unexpected column gap {columnGap}."),
                };
            }

            return (rowGap > 0, columnGap > 0) switch
            {
                (false, false) => KGroup.CornerBottomLeft,
                (false, true) => KGroup.CornerBottomRight,
                (true, false) => KGroup.CornerTopLeft,
                (true, true) => KGroup.CornerTopRight,
            };
        }

        // The single place that decides which scenarios share a K fit (currently pitcher handedness is pooled)
        private static KFitKey FitKey(PitchClass pitchClass, bool pitIsR, bool hitIsR, int balls, int strikes)
        {
            return new KFitKey(pitchClass, hitIsR, balls, strikes);
        }

        // Maximum-likelihood theta >= 0 for deviations d_t ~ N(0, theta * Scale_t + BaseVariance_t).
        // Returns 0 when the data show no variation beyond BaseVariance (the maximum sits on the boundary).
        internal static double EstimateVarianceScale(IEnumerable<(double SquaredDeviation, double Scale, double BaseVariance)> observations)
        {
            var data = observations.ToList();
            // Derivative of the log-likelihood in theta: positive below the maximum, negative above it
            double Score(double theta) => data.Sum(o =>
            {
                double variance = Math.Max((theta * o.Scale) + o.BaseVariance, 1e-12);
                return o.Scale * (o.SquaredDeviation - variance) / (variance * variance);
            });
            if (data.Count == 0 || Score(0) <= 0)
            {
                return 0;
            }
            // Past this every term of the score is negative
            double low = 0;
            double high = data.Max(o => o.SquaredDeviation / Math.Max(o.Scale, 1e-12));
            for (int i = 0; i < 60; i++)
            {
                double mid = (low + high) / 2;
                if (Score(mid) > 0)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
            return (low + high) / 2;
        }

        // K for every (fit key, group), from the windows ending at year and the YearsPerBin - 1 years before it
        private Dictionary<KFitKey, Dictionary<KGroup, double>> FitGroupK(IEnumerable<PitchData> pitches, int year)
        {
            Dictionary<(KFitKey, KGroup), List<CellTau>> observations = [];
            for (int age = 0; age < args.YearsPerBin; age++)
            {
                foreach (CellTau cell in GetCellTau(pitches, year - age))
                {
                    if (GetKGroup(cell.Row, cell.Column) is not KGroup group)
                    {
                        continue;
                    }
                    var key = (FitKey(cell.PitchClass, cell.PitIsR, cell.HitIsR, cell.CountBalls, cell.CountStrikes), group);
                    if (!observations.TryGetValue(key, out List<CellTau>? list))
                    {
                        list = [];
                        observations[key] = list;
                    }
                    list.Add(cell);
                }
            }

            Dictionary<KFitKey, Dictionary<KGroup, double>> result = [];
            foreach (((KFitKey fitKey, KGroup group), List<CellTau> cells) in observations)
            {
                if (!result.TryGetValue(fitKey, out Dictionary<KGroup, double>? groups))
                {
                    groups = [];
                    result[fitKey] = groups;
                }

                groups[group] = 1 / EstimateVarianceScale(cells.Select(o => (o.SquaredDeviation, o.NoiseVariance * o.TauFactor, o.SamplingVariance)));
            }
            return result;
        }

        private Dictionary<KGroup, double>? GroupKFor(IEnumerable<PitchData> pitches, ScenarioKey scenario, int year,
    Dictionary<int, Dictionary<KFitKey, Dictionary<KGroup, double>>> cache)
        {
            if (!cache.TryGetValue(year, out Dictionary<KFitKey, Dictionary<KGroup, double>>? yearK))
            {
                yearK = FitGroupK(pitches, year);
                cache[year] = yearK;
            }
            return yearK.GetValueOrDefault(FitKey(scenario.PitchClass, scenario.PitIsR, scenario.HitIsR, scenario.CountBalls, scenario.CountStrike));
        }
    }
}
