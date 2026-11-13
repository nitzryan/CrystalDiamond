namespace PitchAnalysis.BinEvaluationNS
{
    internal static partial class BinEvaluation
    {
        // One row per scenario (no size threshold) with MSE reduction vs the baseline for every method, so
        // scenarios can be sorted by size against how each method does.
        private static void WriteScenarioSummary(string classDir, string[] names, Dictionary<SituationKey, ScenarioTotals> scenarios)
        {
            static double Reduction(YearTotals totals, int method)
            {
                return totals.Count == 0 || totals.Sse[0] == 0
                    ? double.NaN
                    : 100 * (1 - (totals.Sse[method] / totals.Sse[0]));
            }

            List<string> lines = [];
            List<string> header = ["Year", "Balls", "Strikes", "Hands", "N", "InZoneN", "OutZoneN", "BaselineMse"];
            for (int m = 1; m < names.Length; m++)
            {
                header.Add($"{names[m]} All %");
                header.Add($"{names[m]} In %");
                header.Add($"{names[m]} Out %");
            }
            lines.Add(string.Join(",", header));

            foreach ((SituationKey key, ScenarioTotals scenario) in scenarios
                .OrderBy(kv => kv.Key.Year)
                .ThenBy(kv => kv.Key.Balls)
                .ThenBy(kv => kv.Key.Strikes)
                .ThenBy(kv => kv.Key.PitIsR)
                .ThenBy(kv => kv.Key.HitIsR))
            {
                if (scenario.All.Count == 0)
                {
                    continue;
                }
                List<string> row =
                [
                    key.Year.ToString(),
                    key.Balls.ToString(),
                    key.Strikes.ToString(),
                    $"{(key.PitIsR ? "RHP" : "LHP")}_{(key.HitIsR ? "RHH" : "LHH")}",
                    scenario.All.Count.ToString(),
                    scenario.InZone.Count.ToString(),
                    scenario.OutZone.Count.ToString(),
                    (scenario.All.Sse[0] / scenario.All.Count).ToString("G5"),
                ];
                for (int m = 1; m < names.Length; m++)
                {
                    row.Add(Reduction(scenario.InZone, m).ToString("F3"));
                    row.Add(Reduction(scenario.OutZone, m).ToString("F3"));
                }
                lines.Add(string.Join(",", row));
            }

            File.WriteAllLines(Path.Combine(classDir, "ScenarioSummary.csv"), lines);
        }

        // One row per (pitch class, count, handedness, region) with years pooled, listing the best methods by MSE reduction vs baseline.
        private static void WriteSituationSummary(string path, string[] names, Dictionary<SituationKey, ScenarioTotals> scenarios)
        {
            const int ELEMENTS_TO_SHOW = 3;

            List<string> lines = [];
            List<string> header = ["PitchClass", "Balls", "Strikes", "Hands", "Region", "N", "BaselineMse"];
            for (int rank = 1; rank <= ELEMENTS_TO_SHOW; rank++)
            {
                header.Add($"Best{rank} Method");
                header.Add($"Best{rank} MSE Reduction %");
            }
            lines.Add(string.Join(",", header));

            (string Region, Func<ScenarioTotals, YearTotals> Select)[] regions =
            [
                ("In-Zone", s => s.InZone),
                ("Out-Zone", s => s.OutZone),
            ];

            var situations = scenarios
                .GroupBy(pair => (pair.Key.PitchClass, pair.Key.PitIsR, pair.Key.HitIsR, pair.Key.Balls, pair.Key.Strikes))
                .OrderBy(g => g.Key.PitchClass)
                .ThenBy(g => g.Key.Balls)
                .ThenBy(g => g.Key.Strikes)
                .ThenBy(g => g.Key.PitIsR)
                .ThenBy(g => g.Key.HitIsR);

            foreach (var situation in situations)
            {
                string hands = $"{(situation.Key.PitIsR ? "RHP" : "LHP")}_{(situation.Key.HitIsR ? "RHH" : "LHH")}";

                foreach ((string region, Func<ScenarioTotals, YearTotals> select) in regions)
                {
                    // Pool every year's totals for this situation and region
                    double[] sse = new double[names.Length];
                    long count = 0;
                    foreach (KeyValuePair<SituationKey, ScenarioTotals> pair in situation)
                    {
                        YearTotals totals = select(pair.Value);
                        for (int m = 0; m < names.Length; m++)
                        {
                            sse[m] += totals.Sse[m];
                        }
                        count += totals.Count;
                    }

                    if (count == 0 || sse[0] == 0)
                    {
                        continue;
                    }

                    List<string> row =
                    [
                        situation.Key.PitchClass.ToString(),
                        situation.Key.Balls.ToString(),
                        situation.Key.Strikes.ToString(),
                        hands,
                        region,
                        count.ToString(),
                        (sse[0] / count).ToString("G5"),
                    ];

                    // names[0] is the baseline; rank the rest by lowest pooled SSE
                    foreach (int m in Enumerable.Range(1, names.Length - 1).OrderBy(m => sse[m]).Take(ELEMENTS_TO_SHOW))
                    {
                        row.Add(names[m]);
                        row.Add((100 * (1 - (sse[m] / sse[0]))).ToString("F3"));
                    }

                    lines.Add(string.Join(",", row));
                }
            }

            File.WriteAllLines(path, lines);
        }
    }
}
