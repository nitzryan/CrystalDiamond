using ModelEvaluation.DraftPickBonus;

namespace ModelEvaluation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //RunDraftPickBucketTest(1, 2,
            //DraftPlotOptions.LogX | DraftPlotOptions.LogY);
            //DraftInitialRatings.Calculate(1, 2023, "Base");
            //DraftPickBonus.PickBonusRegression.Calculate();

            //DraftValueAnalysis.RunAnalysis(false);

            DraftValueSlidingWindow.RunAnalysis(
                useSigningBonus: false);
        }
    }
}
