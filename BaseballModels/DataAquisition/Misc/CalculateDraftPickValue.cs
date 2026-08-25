using Db;
using Microsoft.EntityFrameworkCore;

namespace DataAquisition.Misc
{
    internal class CalculateDraftPickValue
    {
        public static void Update()
        {
            ValidateSegments();

            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.DraftPickValues.ExecuteDelete();

            // Get Player Data
            var players = db.Model_Players
                .Where(p => p.SigningYear <= Constants.MODEL_CUTOFF_YEAR
                         && p.DraftPick != 2000
                         && !(p.IsHitter && p.IsPitcher))
                .Select(p => new
                {
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

            var hitters = players.Where(p => p.IsHitter).Select(p => (Pick: p.DraftPick, War: (double)p.WarHitter)).ToList();
            var pitchers = players.Where(p => p.IsPitcher).Select(p => (Pick: p.DraftPick, War: (double)p.WarPitcher)).ToList();

            int maxPick = players.Max(p => p.DraftPick);

            // Get Fit Data
            double[] hitterFit = FitPiecewise(hitters, maxPick, "Hitter");
            double[] pitcherFit = FitPiecewise(pitchers, maxPick, "Pitcher");

            // Write to DB
            var rows = new List<DraftPickValues>(maxPick);
            for (int pick = 1; pick <= maxPick; pick++)
            {
                rows.Add(new DraftPickValues
                {
                    Pick = pick,
                    WarHitter = (float)hitterFit[pick],
                    WarPitcher = (float)pitcherFit[pick],
                });
            }

            db.DraftPickValues.AddRange(rows);
            db.SaveChanges();
        }

        // Segment start/ends, will be validated in ValidateSegments
        private static readonly (int Start, int End)[] Segments =
        {
            (1, 5),     (6, 10),    (11, 20),
            (21, 35),   (36, 50),   (51, 75),   (76, 100),
            (101, 150), (151, 200), (201, 300), (301, 400), (401, 600),
            (601, 800), (801, 1000), (1001, int.MaxValue),
        };

        // Fits the data using piecewise regression of log-pick to war.
        private static double[] FitPiecewise(List<(int Pick, double War)> data, int maxPick, string label)
        {
            // Active segments, with the last one capped at maxPick.
            var segs = Segments
                .Where(s => s.Start <= maxPick)
                .Select(s => (s.Start, End: Math.Min(s.End, maxPick)))
                .ToList();

            // Get start and end point for each section
            // Y1 represents where it would be on the next point (so (1,3) would have X1 be 4)
            var fitPoints = new (double Y0, double Y1)[segs.Count];
            for (int i = 0; i < segs.Count; i++)
            {
                var (segStart, segEnd) = segs[i];
                var seg = data.Where(d => d.Pick >= segStart && d.Pick <= segEnd).ToList();

                if (seg.Count < 3)
                {
                    throw new Exception("3 points necessary for regression");
                }
                else
                {
                    // Get log-linear fit
                    var fit = LinearRegression(
                        seg.Select(d => Math.Log(d.Pick)).ToArray(),
                        seg.Select(d => d.War).ToArray());

                    //// Prevent negative slope
                    //if (fit.Slope > 0)
                    //{
                    //    double mean = seg.Average(d => d.War);
                    //    Console.WriteLine(
                    //        $"[{label}] picks {segStart}-{segEnd}: fit slope is POSITIVE ({fit.Slope:F4} WAR per ln(pick), " +
                    //        $"n={seg.Count}). Flattening to mean {mean:F3}.");
                    //    fit = (mean, 0);
                    //}

                    double Y0 = fit.Intercept + (Math.Log(segStart) * fit.Slope);
                    double Y1 = fit.Intercept + (Math.Log(segEnd + 1) * fit.Slope);

                    fitPoints[i] = (Y0, Y1);
                }
            }

            // Get the knot values by averaging the end of section N-1 with the start of section N
            double[] knotPicks = new double[segs.Count + 1];
            double[] knotVals = new double[segs.Count + 1];
            knotPicks[0] = 0;
            knotVals[0] = fitPoints[0].Y0;
            for (int i = 0; i < segs.Count; i++)
            {
                knotPicks[i + 1] = Math.Log((double)(segs[i].End + 1));
                knotVals[i + 1] = i < segs.Count - 1
                    ? (fitPoints[i].Y1 + fitPoints[i + 1].Y0) / 2
                    : fitPoints[i].Y1;
            }

            // TODO: There is a better way of doing this fit, this is a quick hack
            // For now, just sort the values in order so value always decreases.
            knotVals = knotVals.OrderDescending().ToArray();

            // Interpolate between knots
            double[] result = new double[maxPick + 1];
            int k = 0;
            for (int pick = 1; pick <= maxPick; pick++)
            {
                // Get right knot idx
                double knotPick = Math.Log(pick);
                while (knotPick > knotPicks[k + 1])
                    k++;

                // Interpolate
                double xFrac = (knotPick - knotPicks[k]) / (knotPicks[k + 1] - knotPicks[k]);
                result[pick] = (knotVals[k + 1] * xFrac) + (knotVals[k] * (1 - xFrac));
            }

            return result;
        }


        /// Gets slope/intercept for least squares
        private static (double Intercept, double Slope) LinearRegression(double[] x, double[] y)
        {
            int n = x.Length;
            double meanX = x.Average(), meanY = y.Average();
            double sxx = 0, sxy = 0;
            for (int i = 0; i < n; i++)
            {
                double dx = x[i] - meanX;
                sxx += dx * dx;
                sxy += dx * (y[i] - meanY);
            }

            if (sxx == 0) return (meanY, 0); // all x identical

            double slope = sxy / sxx;
            return (meanY - (slope * meanX), slope);
        }

        // Ensures that there are no gaps or dpilicates in Segments
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
    }
}
