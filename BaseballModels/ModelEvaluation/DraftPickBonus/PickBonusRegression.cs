namespace ModelEvaluation.DraftPickBonus
{
    internal class PickBonusRegression
    {
        public static void Calculate()
        {
            List<AggregatedDraftData> draftData = AggregateData.Aggregate(2023);

            var hitterData = draftData.Where(f => f.IsHitter).ToList();
            List<double> warPerMillions = [];
            List<double> r2Values = [];
            for (int i = 0; i < 600; i+=10)
            {
                var partialData = hitterData.Where(f => f.DraftPick > i && f.DraftPick <= i + 10).ToList();
                WarBestFit bestFit = GetRegression.GetBestFit(partialData, false, true);
                warPerMillions.Add(bestFit.GetWarPerMillion());
                r2Values.Add(bestFit.R2);
            }

            hitterData = [];
        }
    }
}
