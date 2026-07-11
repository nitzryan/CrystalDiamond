using ModelEvaluation.PlayerGroupEvaluations;
using static ModelEvaluation.PlayerGroupEvaluations.DraftDemoTest;

namespace ModelEvaluation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunDraftPickBucketTest(1, 2,
            DraftPlotOptions.LogX | DraftPlotOptions.LogY);
        }
    }
}
