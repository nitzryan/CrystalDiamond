using Db;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using SiteDb;

namespace SitePrep.Draft
{
    internal class CalculateDraftPickValue
    {
        private const int ModelId = 1;
        private const int YearsAfter = 2;

        // Segment start/ends, will be validated in ValidateSegments
        private static readonly (int Start, int End)[] Segments =
        {
            (1, 10),    (11, 20), (21, 30), (31,50), (51, 75),
            (76, 100), (101, 200), (201, 400), (401, 600),
            (601, 800), (801, 1000), (1001, int.MaxValue),
        };

        // Ensures that there are no gaps or dupilicates in Segments
        private static void ValidateSegments()
        {
            if (Segments.Length == 0)
                throw new InvalidOperationException("Segments is empty.");
            if (Segments[0].Start != 1)
                throw new InvalidOperationException($"Segments must start at pick 1, starts at {Segments[0].Start}.");
            if (Segments[^1].End != int.MaxValue)
                throw new InvalidOperationException("Last segment must be open-ended (End == int.MaxValue).");

            for (int i = 0; i < Segments.Length; i++)
            {
                var (start, end) = Segments[i];
                if (end < start)
                    throw new InvalidOperationException($"Segment {i} ({start},{end}) has End < Start.");

                if (i == 0) continue;
                int prevEnd = Segments[i - 1].End;
                if (start <= prevEnd)
                    throw new InvalidOperationException(
                        $"Segment {i} ({start},{end}) overlaps previous segment ending at {prevEnd}.");
                if (start > prevEnd + 1)
                    throw new InvalidOperationException(
                        $"Gap: picks {prevEnd + 1}-{start - 1} are not covered by any segment.");
            }
        }

        // Writes DraftPickValues using a single continuous piecewise-linear least-squares fit
        public static void Update(DraftPickTarget target)
        {
            ValidateSegments();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

            // Get Player Data
            int maxYear = siteDb.HomeData.Max(g => g.Year);
            var players = (target == DraftPickTarget.Actual 
            ?   db.Model_Players
                .Where(p => p.SigningYear <= DataAquisition.Constants.MODEL_CUTOFF_YEAR
                         && p.DraftPick != 2000
                         && !(p.IsHitter && p.IsPitcher))
            :   
                db.Model_Players
                .Where(f => !f.IsEligible
                            && f.DraftPick != 2000
                            && !(f.IsHitter && f.IsPitcher)
                            && f.SigningYear <= maxYear - YearsAfter)
                )
                .Join(db.Player_CareerStatus, f => f.MlbId, f => f.MlbId, (mp, pcs) => new { mp, pcs })
                .Where(f => !(f.pcs.IgnorePlayer > 0))
                .Select(f => f.mp)
                .Select(p => new
                {
                    p.MlbId,
                    p.SigningYear,
                    p.DraftPick,
                    p.IsHitter,
                    p.IsPitcher,
                    p.WarHitter,
                    p.WarPitcher,
                })
                .ToList();

            if (players.Count == 0)
            {
                Console.WriteLine("DraftPickValues.Update: no eligible players found.");
                return;
            }

            // Get dictionary of model WAR value at specified timestep, or empty if calculating actual
            var mlbIds = players.Select(f => f.MlbId).ToHashSet();
            Dictionary<(int MlbId, bool IsHitter), List<(int Year, int Month, double War)>> warDict = target == DraftPickTarget.Model
                ? modelDb.Output_PlayerWarAggregation
                    .Where(f => f.ModelId == ModelId
                                && mlbIds.Contains(f.MlbId))
                    .Select(f => new { f.MlbId, f.IsHitter, f.War, f.Year, f.Month })
                    .AsEnumerable()
                    .GroupBy(f => (f.MlbId, f.IsHitter))
                    .ToDictionary(f => f.Key, f => f.Select(g => (g.Year, g.Month, War: (double)g.War))
                                                    .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
                                                    .ToList())
                : new();

            Dictionary<(int MlbId, bool IsHitter), double> modelWar = target == DraftPickTarget.Model
                ? players
                    .ToDictionary(f => (f.MlbId, f.IsHitter),
                        f => warDict[(f.MlbId, f.IsHitter)]
                                .Where(g => g.Year <= f.SigningYear + YearsAfter)
                                .First().War)
                : new Dictionary<(int MlbId, bool IsHitter), double>();

            var hitters = players
                .Where(p => p.IsHitter)
                .Select(p => (Pick: p.DraftPick, War: target == DraftPickTarget.Model
                    ? modelWar[(p.MlbId, true)]
                    : p.WarHitter))
                .ToList();

            var pitchers = players
                .Where(p => p.IsPitcher)
                .Select(p => (Pick: p.DraftPick, War: target == DraftPickTarget.Model
                    ? modelWar[(p.MlbId, false)]
                    : p.WarPitcher))
                .ToList();

            int maxPick = players.Max(p => p.DraftPick);

            // Get Fit Data
            double[] hitterFit = FitSplineLinear(hitters, maxPick);
            double[] pitcherFit = FitSplineLinear(pitchers, maxPick);

            // Write to DB
            var values = Enumerable.Range(1, maxPick)
                .Select(pick => (Pick: pick, WarHitter: (float)hitterFit[pick], WarPitcher: (float)pitcherFit[pick]))
                .ToList();

            if (target == DraftPickTarget.Actual)
            {
                db.DraftPickValues.ExecuteDelete();
                db.DraftPickValues.AddRange(values.Select(v => new DraftPickValues
                {
                    Pick = v.Pick,
                    WarHitter = v.WarHitter,
                    WarPitcher = v.WarPitcher,
                }));
                db.SaveChanges();
            }
            else
            {
                siteDb.ModelDraftPickValues.ExecuteDelete();
                siteDb.ModelDraftPickValues.AddRange(values.Select(v => new ModelDraftPickValues
                {
                    Pick = v.Pick,
                    WarHitter = v.WarHitter,
                    WarPitcher = v.WarPitcher,
                }));
                siteDb.SaveChanges();
            }
        }

        // Least-squares solve over all data: WAR = b0 + b1*pick + sum(bk * max(0, pick - knotk))
        private static double[] FitSplineLinear(List<(int Pick, double War)> data, int maxPick)
        {
            // Knot sits between the last pick of one segment and the first pick of the next
            double[] knots = Segments
                .Select(s => s.Start)
                .Where(k => k > 1 && k <= maxPick)
                .Select(k => k - 0.5)
                .ToArray();

            int paramCount = 2 + knots.Length;

            if (data.Count < paramCount)
            {
                throw new InvalidOperationException(
                    $"Spline needs at least {paramCount} points, got {data.Count}.");
            }

            var x = Matrix<double>.Build.Dense(data.Count, paramCount);
            var y = Vector<double>.Build.Dense(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                double[] row = BuildSplineRow(data[i].Pick, knots);
                for (int j = 0; j < paramCount; j++)
                {
                    x[i, j] = row[j];
                }

                y[i] = data[i].War;
            }

            Vector<double> beta = x.QR().Solve(y);

            double[] result = new double[maxPick + 1];
            for (int pick = 1; pick <= maxPick; pick++)
            {
                result[pick] = BuildSplineRow(pick, knots).Zip(beta, (basis, coef) => basis * coef).Sum();
            }

            return result;
        }

        // Basis row for a single pick: [1, pick, max(0, pick - knot) for each knot]
        private static double[] BuildSplineRow(double pick, double[] knots)
        {
            var row = new double[2 + knots.Length];
            row[0] = 1;
            row[1] = pick;

            for (int k = 0; k < knots.Length; k++)
            {
                row[2 + k] = Math.Max(0, pick - knots[k]);
            }

            return row;
        }
    }
}
