using PitchTrackingDb;
using System.Collections.Generic;

namespace PitchAnalysis.PitchZoneBinNS
{
    public partial class PitchZoneBin
    {
        // One aggregation per grid geometry, valid only for the pitch collection it was built from
        private sealed record AggregationCacheEntry(WeakReference<IEnumerable<PitchData>> Source, Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> Scenarios);
        private static readonly Dictionary<PitchZoneGeometry, AggregationCacheEntry> AGGREGATION_CACHE = [];
        private static readonly object AGGREGATION_LOCK = new();

        // One scenario-year's weighted window: smoothed-ready cell totals, unweighted counts, and the off-grid pitches
        private sealed record WindowTotals(CellTotals Cells, double[,] RawCounts, double NoZonePitches, double NoZoneWeighted, double NoZoneValue);
        public (List<PitchZoneCompetitive> Competitive, List<PitchZoneNonCompetitive> NonCompetitive) BuildCompetitiveRows(IEnumerable<PitchData> pitches, int minOutputYear = int.MinValue)
        {
            List<PitchZoneCompetitive> competitiveRows = [];
            List<PitchZoneNonCompetitive> nonCompetitiveRows = [];
            Dictionary<int, Dictionary<KFitKey, Dictionary<KGroup, double>>> kByYear = [];

            Dictionary<(ScenarioKey Pooled, int Year), double[,]> hbpRatesCache = [];
            Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> scenarioData = GetScenarioData(pitches);
            foreach ((ScenarioKey scenario, SortedDictionary<int, YearCells> years) in scenarioData)
            {
                // Years below minOutputYear are only history for later windows
                foreach (int year in years.Keys.Where(y => y >= minOutputYear))
                {
                    WindowTotals window = SumWindow(years, year);
                    Dictionary<KGroup, double> ? groupK = args.UseFittedK ? GroupKFor(pitches, scenario, year, kByYear) : null;
                    CellTotals effective = ApplyKernel(window.Cells, args.SeparateBorderRing, groupK);

                    // Get HBP Count, with different counts pooled together
                    ScenarioKey pooledKey = scenario with { CountBalls = 0, CountStrike = 0 };
                    if (!hbpRatesCache.TryGetValue((pooledKey, year), out double[,]? hbpRates))
                    {
                        hbpRates = PooledHbpRates(scenarioData, pooledKey, year);
                        hbpRatesCache[(pooledKey, year)] = hbpRates;
                    }

                    // Pitches that will NOT be valued by a competitive bin: off the grid, or in a bin failing either rule
                    double fallbackCount = window.NoZonePitches;
                    double fallbackWeighted = window.NoZoneWeighted;
                    double fallbackValue = window.NoZoneValue;

                    // Every cell gets a row, including empty ones, so a missing row never needs interpreting.
                    for (int row = 0; row < gridRows; row++)
                    {
                        for (int column = 0; column < gridColumns; column++)
                        {
                            PitchZoneCompetitive cellRow = CreateCompetitiveRow(scenario, year, row, column, effective, hbpRates[row, column]);
                            competitiveRows.Add(cellRow);

                            if (!cellRow.IsCompetitive || !cellRow.HasEnoughSample)
                            {
                                // The cell's own (unsmoothed) pitches fall back to the non-competitive pool
                                fallbackCount += window.RawCounts[row, column];
                                fallbackWeighted += window.Cells.Pitches[row, column];
                                fallbackValue += window.Cells.ValueSum[row, column];
                            }
                        }
                    }

                    nonCompetitiveRows.Add(CreateNonCompetitiveRow(scenario, year, fallbackCount, fallbackWeighted, fallbackValue));
                }
            }

            return (competitiveRows, nonCompetitiveRows);
        }

        // Weighted sum of the raw cell totals for the YearsOfData years ending at targetYear
        private WindowTotals SumWindow(SortedDictionary<int, YearCells> years, int targetYear)
        {
            double[,] pitches = new double[gridRows, gridColumns];
            double[,] swings = new double[gridRows, gridColumns];
            double[,] valueSum = new double[gridRows, gridColumns];
            double[,] rawCounts = new double[gridRows, gridColumns];
            double noZonePitches = 0, noZoneWeighted = 0, noZoneValue = 0;

            for (int age = 0; age < args.YearsPerBin; age++)
            {
                if (!years.TryGetValue(targetYear - age, out YearCells? yearCells))
                {
                    continue;
                }

                double weight = YearWeight(targetYear, targetYear - age);
                for (int row = 0; row < gridRows; row++)
                {
                    for (int column = 0; column < gridColumns; column++)
                    {
                        rawCounts[row, column] += yearCells.Pitches[row, column];
                        pitches[row, column] += weight * yearCells.Pitches[row, column];
                        swings[row, column] += weight * yearCells.Swings[row, column];
                        valueSum[row, column] += weight * yearCells.ValueSum[row, column];
                    }
                }

                noZonePitches += yearCells.NoZonePitches;
                noZoneWeighted += weight * yearCells.NoZonePitches;
                noZoneValue += weight * yearCells.NoZoneValue;
            }

            return new WindowTotals(new CellTotals(pitches, swings, valueSum), rawCounts, noZonePitches, noZoneWeighted, noZoneValue);
        }
        private PitchZoneCompetitive CreateCompetitiveRow(ScenarioKey scenario, int year, int row, int column, CellTotals effective, double hbpRate)
        {
            double pitches = effective.Pitches[row, column];
            double swings = effective.Swings[row, column];

            return new PitchZoneCompetitive
            {
                Year = year,
                PitchClass = scenario.PitchClass,
                PitIsR = scenario.PitIsR,
                HitIsR = scenario.HitIsR,
                CountBalls = scenario.CountBalls,
                CountStrikes = scenario.CountStrike,
                ZoneRow = row,
                ZoneCol = column,
                PitchCount = (float)pitches,
                SwingCount = (float)swings,
                IsCompetitive = IsCompetitive(pitches, swings, hbpRate),
                HasEnoughSample = HasValidSampleSize(pitches),
                ExpectedValueHitter = pitches > 0
                    ? (float)(effective.ValueSum[row, column] / pitches)
                    : 0,
            };
        }
        private static PitchZoneNonCompetitive CreateNonCompetitiveRow(ScenarioKey scenario, int year, double count, double weightedCount, double weightedValue)
        {
            return new PitchZoneNonCompetitive
            {
                Year = year,
                PitchClass = scenario.PitchClass,
                PitIsR = scenario.PitIsR,
                HitIsR = scenario.HitIsR,
                CountBalls = scenario.CountBalls,
                CountStrikes = scenario.CountStrike,
                PitchCount = (int)count,
                ExpectedValueHitter = weightedCount > 0 ? (float)(weightedValue / weightedCount) : 0,
            };
        }

        // Buckets every pitch into its cell once, per (scenario, year). Independent of YearsOfData, YearDecay and the kernel.
        private Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> AggregateYears(IEnumerable<PitchData> pitches)
        {
            var groups = pitches
                .GroupBy(p => new ScenarioKey(p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike))
                .ToList();

            Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> scenarios = [];

            foreach (var group in groups)
            {
                // Row edges depend only on the batter's zone, which repeats heavily, so build each pair once
                Dictionary<(float Top, float Bot), double[]> rowEdgesByZone = [];
                SortedDictionary<int, YearCells> years = [];

                foreach (PitchData p in group)
                {
                    if (!years.TryGetValue(p.Year, out YearCells? cells))
                    {
                        cells = new YearCells(gridRows, gridColumns);
                        years[p.Year] = cells;
                    }

                    if (!rowEdgesByZone.TryGetValue((p.ZoneTop, p.ZoneBot), out double[]? rowEdges))
                    {
                        rowEdges = BuildRowEdges(p.ZoneTop, p.ZoneBot);
                        rowEdgesByZone[(p.ZoneTop, p.ZoneBot)] = rowEdges;
                    }

                    double value = p.RunValueSmoothedHitter;
                    if (GetZone(p.PlateX, p.PlateZ, rowEdges) is PitchZone zone)
                    {
                        cells.Pitches[zone.Row, zone.Column]++;
                        if (p.HadSwing)
                        {
                            cells.Swings[zone.Row, zone.Column]++;
                        }
                        if (p.Result == Db.DbEnums.PitchResult.HBP)
                        {
                            cells.Hbps[zone.Row, zone.Column]++;
                        }

                        cells.ValueSum[zone.Row, zone.Column] += value;
                        cells.ValueSumSq[zone.Row, zone.Column] += value * value;
                    }
                    else
                    {
                        cells.NoZonePitches++;
                        cells.NoZoneValue += value;
                    }
                }

                scenarios[group.Key] = years;
            }

            return scenarios;
        }

        // Returns the per-scenario, per-year cell totals for this bin's geometry, building them on first use.
        // Reused only while the same pitch collection instance is passed in, so a different (or rebuilt) list triggers a fresh build.
        private Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> GetScenarioData(IEnumerable<PitchData> pitches)
        {
            lock (AGGREGATION_LOCK)
            {
                if (AGGREGATION_CACHE.TryGetValue(Geometry, out AggregationCacheEntry? entry)
                    && entry.Source.TryGetTarget(out IEnumerable<PitchData>? source)
                    && ReferenceEquals(source, pitches))
                {
                    return entry.Scenarios;
                }

                Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> scenarios = AggregateYears(pitches);
                AGGREGATION_CACHE[Geometry] = new AggregationCacheEntry(new WeakReference<IEnumerable<PitchData>>(pitches), scenarios);
                return scenarios;
            }
        }

        // Unweighted moments of every non-empty scenario-cell over the YearsPerBin years ending at binYear.
        // Uses the same cache as BuildCompetitiveRows, so pass the same list instance.
        public List<CellNoise> GetCellNoise(IEnumerable<PitchData> pitches, int binYear)
        {
            List<CellNoise> rows = [];
            foreach ((ScenarioKey scenario, SortedDictionary<int, YearCells> years) in GetScenarioData(pitches))
            {
                double[,] counts = new double[gridRows, gridColumns];
                double[,] sums = new double[gridRows, gridColumns];
                double[,] sumsSq = new double[gridRows, gridColumns];
                for (int age = 0; age < args.YearsPerBin; age++)
                {
                    if (!years.TryGetValue(binYear - age, out YearCells? yearCells))
                    {
                        continue;
                    }
                    for (int row = 0; row < gridRows; row++)
                    {
                        for (int column = 0; column < gridColumns; column++)
                        {
                            counts[row, column] += yearCells.Pitches[row, column];
                            sums[row, column] += yearCells.ValueSum[row, column];
                            sumsSq[row, column] += yearCells.ValueSumSq[row, column];
                        }
                    }
                }
                for (int row = 0; row < gridRows; row++)
                {
                    for (int column = 0; column < gridColumns; column++)
                    {
                        if (counts[row, column] > 0)
                        {
                            rows.Add(new CellNoise(scenario.PitchClass, scenario.PitIsR, scenario.HitIsR, scenario.CountBalls,
                                scenario.CountStrike, row, column, counts[row, column], sums[row, column], sumsSq[row, column]));
                        }
                    }
                }
            }
            return rows;
        }

        // HBP rate per cell over the bin window, pooled across counts. Smoothed without the run-value caps or region rules,
        // since the chance of hitting the batter depends on location, not on the count or zone region.
        private double[,] PooledHbpRates(Dictionary<ScenarioKey, SortedDictionary<int, YearCells>> scenarios, ScenarioKey pooledKey, int targetYear)
        {
            double[,] pitches = new double[gridRows, gridColumns];
            double[,] hbps = new double[gridRows, gridColumns];
            foreach ((ScenarioKey scenario, SortedDictionary<int, YearCells> years) in scenarios)
            {
                if (scenario.PitchClass != pooledKey.PitchClass || scenario.PitIsR != pooledKey.PitIsR || scenario.HitIsR != pooledKey.HitIsR)
                {
                    continue;
                }

                for (int age = 0; age < args.YearsPerBin; age++)
                {
                    if (!years.TryGetValue(targetYear - age, out YearCells? yearCells))
                    {
                        continue;
                    }

                    double weight = YearWeight(targetYear, targetYear - age);
                    for (int row = 0; row < gridRows; row++)
                    {
                        for (int column = 0; column < gridColumns; column++)
                        {
                            pitches[row, column] += weight * yearCells.Pitches[row, column];
                            hbps[row, column] += weight * yearCells.Hbps[row, column];
                        }
                    }
                }
            }

            int radius = args.Kernel?.Radius ?? 0;
            double[,] rates = new double[gridRows, gridColumns];
            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    double pitchSum = 0, hbpSum = 0;
                    for (int sourceRow = Math.Max(0, row - radius); sourceRow <= Math.Min(gridRows - 1, row + radius); sourceRow++)
                    {
                        for (int sourceColumn = Math.Max(0, column - radius); sourceColumn <= Math.Min(gridColumns - 1, column + radius); sourceColumn++)
                        {
                            double weight = args.Kernel?.Weight(sourceRow - row, sourceColumn - column) ?? 1;
                            pitchSum += weight * pitches[sourceRow, sourceColumn];
                            hbpSum += weight * hbps[sourceRow, sourceColumn];
                        }
                    }

                    rates[row, column] = pitchSum > 0 ? hbpSum / pitchSum : 0;
                }
            }

            return rates;
        }
    }
}
