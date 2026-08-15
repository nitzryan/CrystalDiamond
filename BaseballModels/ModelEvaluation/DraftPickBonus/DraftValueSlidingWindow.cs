using ScottPlot;

namespace ModelEvaluation.DraftPickBonus
{
    public static class DraftValueSlidingWindow
    {
        private const int ActualWarCutoffYear = 2016;
        private const int PredictedWarCutoffYear = 2023;
        private const int MinWindowSampleSize = 10;
        private const int MaxPick = 600;

        public static void RunAnalysis(
            bool useSigningBonus,
            int fixedWindowSize = 20,
            int fixedStepSize = 20,
            int smoothSpan = 3,
            int initialWindowSize = 5,
            double windowGrowthRate = 0.5)
        {
            var actualData = AggregateData.Aggregate(ActualWarCutoffYear);
            GeneratePlots(actualData, useActualWar: true, useSigningBonus,
                fixedWindowSize, fixedStepSize, smoothSpan,
                initialWindowSize, windowGrowthRate, fileSuffix: "actual");

            var predictedData = AggregateData.Aggregate(PredictedWarCutoffYear)
                .Where(d => d.SigningYear >= ActualWarCutoffYear)
                .ToList();
            GeneratePlots(predictedData, useActualWar: false, useSigningBonus,
                fixedWindowSize, fixedStepSize, smoothSpan,
                initialWindowSize, windowGrowthRate, fileSuffix: "predicted");
        }

        private static void GeneratePlots(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus,
            int fixedWindowSize,
            int fixedStepSize,
            int smoothSpan,
            int initialWindowSize,
            double windowGrowthRate,
            string fileSuffix)
        {
            var hitters = data.Where(d => d.IsHitter).ToList();
            var pitchers = data.Where(d => !d.IsHitter).ToList();

            string warLabel = useActualWar ? "Actual WAR" : "Predicted WAR";
            string moneyLabel = useSigningBonus ? "Signing Bonus" : "Bonus vs Slot";
            string yLabel = useSigningBonus ? "WAR / $1M" : "WAR / $1M Over Slot";

            // --- Plot 1: fixed window, raw ---
            var hitterFixed = FitFixedWindows(hitters, useActualWar, useSigningBonus,
                fixedWindowSize, fixedStepSize);
            var pitcherFixed = FitFixedWindows(pitchers, useActualWar, useSigningBonus,
                fixedWindowSize, fixedStepSize);

            PlotSeries(hitterFixed, pitcherFixed,
                title: $"WAR per $1M, Fixed Window ({warLabel} vs {moneyLabel}, " +
                       $"window={fixedWindowSize}, step={fixedStepSize})",
                yLabel: yLabel,
                fileName: $"../../../Output/DraftMoney/wpm_fixed_{fileSuffix}.png");

            // --- Plot 2: fixed window, smoothed ---
            var hitterSmooth = SmoothResults(hitterFixed, smoothSpan);
            var pitcherSmooth = SmoothResults(pitcherFixed, smoothSpan);

            PlotSeries(hitterSmooth, pitcherSmooth,
                title: $"WAR per $1M, Fixed Window Smoothed ({warLabel} vs {moneyLabel}, " +
                       $"window={fixedWindowSize}, step={fixedStepSize}, smooth=±{smoothSpan})",
                yLabel: yLabel,
                fileName: $"../../../Output/DraftMoney/wpm_smoothed_{fileSuffix}.png");

            // --- Plot 3: multiplicative (growing) window ---
            var hitterGrowing = FitGrowingWindows(hitters, useActualWar, useSigningBonus,
                initialWindowSize, windowGrowthRate);
            var pitcherGrowing = FitGrowingWindows(pitchers, useActualWar, useSigningBonus,
                initialWindowSize, windowGrowthRate);

            PlotSeries(hitterGrowing, pitcherGrowing,
                title: $"WAR per $1M, Growing Window ({warLabel} vs {moneyLabel}, " +
                       $"w0={initialWindowSize}, growth={windowGrowthRate})",
                yLabel: yLabel,
                fileName: $"../../../Output/DraftMoney/wpm_growing_{fileSuffix}.png");
        }

        private record WindowResult(
            double MeanPick,
            double WarPerMillion,
            int SampleSize);

        private static List<WindowResult> FitFixedWindows(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus,
            int windowSize,
            int stepSize)
        {
            var results = new List<WindowResult>();

            for (int start = 1; start + windowSize - 1 <= MaxPick; start += stepSize)
            {
                int end = start + windowSize - 1;
                TryFitWindow(data, useActualWar, useSigningBonus, start, end, results);
            }

            return results;
        }

        private static List<WindowResult> FitGrowingWindows(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus,
            int initialWindowSize,
            double windowGrowthRate)
        {
            var results = new List<WindowResult>();
            int start = 1;

            while (start <= MaxPick)
            {
                int width = Math.Max(initialWindowSize,
                    (int)Math.Round(start * windowGrowthRate));
                int end = Math.Min(MaxPick, start + width - 1);

                TryFitWindow(data, useActualWar, useSigningBonus, start, end, results);

                if (end >= MaxPick) break;
                start = end + 1;
            }

            return results;
        }

        private static void TryFitWindow(
            List<AggregatedDraftData> data,
            bool useActualWar,
            bool useSigningBonus,
            int start,
            int end,
            List<WindowResult> results)
        {
            var window = data
                .Where(d => d.DraftPick >= start && d.DraftPick <= end)
                .ToList();

            if (window.Count < MinWindowSampleSize)
                return;

            var fit = GetRegression.GetBestFit(window, useActualWar, useSigningBonus);
            results.Add(new WindowResult(
                MeanPick: window.Average(d => (double)d.DraftPick),
                WarPerMillion: fit.GetWarPerMillion(),
                SampleSize: window.Count));
        }

        private static List<WindowResult> SmoothResults(
            List<WindowResult> results,
            int smoothSpan)
        {
            if (smoothSpan <= 0) return results;

            var smoothed = new List<WindowResult>();

            for (int i = 0; i < results.Count; i++)
            {
                int lo = Math.Max(0, i - smoothSpan);
                int hi = Math.Min(results.Count - 1, i + smoothSpan);
                var slice = results.Skip(lo).Take(hi - lo + 1).ToList();

                double totalN = slice.Sum(r => r.SampleSize);

                smoothed.Add(new WindowResult(
                    MeanPick: slice.Sum(r => r.MeanPick * r.SampleSize) / totalN,
                    WarPerMillion: slice.Sum(r => r.WarPerMillion * r.SampleSize) / totalN,
                    SampleSize: slice.Max(r => r.SampleSize)));
            }

            return smoothed;
        }

        private static void PlotSeries(
            List<WindowResult> hitters,
            List<WindowResult> pitchers,
            string title,
            string yLabel,
            string fileName)
        {
            var plt = new ScottPlot.Plot();

            var hitterScatter = plt.Add.Scatter(
                hitters.Select(r => r.MeanPick).ToArray(),
                hitters.Select(r => r.WarPerMillion).ToArray());
            hitterScatter.LegendText = "Hitters";
            hitterScatter.Color = Colors.Blue;

            var pitcherScatter = plt.Add.Scatter(
                pitchers.Select(r => r.MeanPick).ToArray(),
                pitchers.Select(r => r.WarPerMillion).ToArray());
            pitcherScatter.LegendText = "Pitchers";
            pitcherScatter.Color = Colors.Red;

            plt.Title(title);
            plt.XLabel("Mean Draft Pick in Window");
            plt.YLabel(yLabel);
            plt.ShowLegend();

            plt.SavePng(fileName, 900, 600);
        }
    }
}
