using ScottPlot;

namespace ModelEvaluation.DraftPickBonus
{
    public static class DraftValueAnalysis
    {
        private static readonly int[] BucketCutoffs = { 5, 15, 30, 60, 100, 150, 200, 300, 450, 600 };

        private const int ActualWarCutoffYear = 2016;
        private const int PredictedWarCutoffYear = 2023;

        public static void RunAnalysis(bool useSigningBonus)
        {
            // Actual WAR: only data through 2016
            var actualData = AggregateData.Aggregate(ActualWarCutoffYear);
            GeneratePlots(actualData, useActualWar: true, useSigningBonus,
                fileSuffix: "actual");

            // Predicted WAR: data through 2023
            var predictedData = AggregateData.Aggregate(PredictedWarCutoffYear)
                .Where(d => d.SigningYear >= ActualWarCutoffYear)
                .ToList();
            GeneratePlots(predictedData, useActualWar: false, useSigningBonus,
                fileSuffix: "predicted");
        }

        private static void GeneratePlots(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus,
            string fileSuffix)
        {
            var hitterResults = FitBuckets(data.Where(d => d.IsHitter).ToList(),
                useActualWar, useSigningBonus);
            var pitcherResults = FitBuckets(data.Where(d => !d.IsHitter).ToList(),
                useActualWar, useSigningBonus);

            string warLabel = useActualWar ? "Actual WAR" : "Predicted WAR";
            string moneyLabel = useSigningBonus ? "Signing Bonus" : "Bonus vs Slot";

            PlotSeries(hitterResults, pitcherResults,
                r => r.WarPerMillion,
                title: $"WAR per $1M by Draft Pick ({warLabel} vs {moneyLabel})",
                yLabel: "WAR / $1M",
                fileName: $"../../../Output/DraftMoney/war_per_million_{fileSuffix}.png");
        }

        private record BucketResult(
            double MeanPick,
            double WarPerMillion,
            double R2,
            int SampleSize);

        private static List<BucketResult> FitBuckets(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus)
        {
            var results = new List<BucketResult>();
            int lowerBound = 0;

            foreach (int upperBound in BucketCutoffs)
            {
                var bucket = data
                    .Where(d => d.DraftPick > lowerBound && d.DraftPick <= upperBound)
                    .ToList();
                lowerBound = upperBound;

                if (bucket.Count < 10) // too few points for a meaningful 2-var fit
                    continue;

                var fit = GetRegression.GetBestFit(bucket, useActualWar, useSigningBonus);

                results.Add(new BucketResult(
                    MeanPick: bucket.Average(d => (double)d.DraftPick),
                    WarPerMillion: fit.GetWarPerMillion(),
                    R2: fit.R2,
                    SampleSize: bucket.Count));
            }

            return results;
        }

        private static void PlotSeries(
            List<BucketResult> hitters,
            List<BucketResult> pitchers,
            Func<BucketResult, double> ySelector,
            string title,
            string yLabel,
            string fileName)
        {
            var plt = new ScottPlot.Plot();

            var hitterScatter = plt.Add.Scatter(
                hitters.Select(r => r.MeanPick).ToArray(),
                hitters.Select(ySelector).ToArray());
            hitterScatter.LegendText = "Hitters";
            hitterScatter.Color = Colors.Blue;

            var pitcherScatter = plt.Add.Scatter(
                pitchers.Select(r => r.MeanPick).ToArray(),
                pitchers.Select(ySelector).ToArray());
            pitcherScatter.LegendText = "Pitchers";
            pitcherScatter.Color = Colors.Red;

            plt.Title(title);
            plt.XLabel("Mean Draft Pick in Bucket");
            plt.YLabel(yLabel);
            plt.ShowLegend();

            plt.SavePng(fileName, 900, 600);
        }
    }
}
