using MathNet.Numerics;

namespace ModelEvaluation.DraftPickBonus
{
    public class WarBestFit
    {
        // Y = aX1 + bX2 + c
        public double A { get; }
        public double B { get; }
        public double C { get; }
        public double R2 { get; }

        public WarBestFit(double a, double b, double c, double r2)
        {
            A = a;
            B = b;
            C = c;
            R2 = r2;
        }

        public double FitValues(double x1, double x2) =>
            (A * x1) + (B * x2) + C;

        public double GetWarPerMillion() => B * 1000000;
    }

    internal class GetRegression
    {
        public static WarBestFit GetBestFit(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus)
        {
            // Get data for configuration
            double[] ys = useActualWar ?
                data.Select(f => (double)f.ActualWar).ToArray() :
                data.Select(f => (double)f.PredictedWar).ToArray();

            double[][] xs = useSigningBonus ?
                data.Select(f => new[] {(double)f.DraftPick, f.SigningBonus }).ToArray() :
                data.Select(f => new[] {(double)f.DraftPick, f.SigningBonus - f.SlotValue}).ToArray();

            // Get fit
            double[] fit = Fit.MultiDim(xs, ys, intercept: true);

            // Predict values
            double[] predicted = xs.Select(x =>
                fit[0] + (fit[1] * x[0]) + (fit[2] * x[1])
            ).ToArray();

            // R2
            double r2 = GoodnessOfFit.RSquared(predicted, ys);

            return new WarBestFit(fit[1], fit[2], fit[0], r2);
        }
    }
}
