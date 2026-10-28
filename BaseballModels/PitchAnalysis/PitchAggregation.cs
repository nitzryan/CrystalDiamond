using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class PitchAggregation
    {
        private record PitchScenario
        {
            public int balls { get; init; }
            public int strikes { get; init; }
            public int year { get; init; }
        }
        private enum PitchResult
        {
            Strike,
            Ball,
            HBP,
            Foul
        }
        private record PitchScenarioResult(PitchScenario scenario, PitchResult result);

        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            // Get expected run values for each scenario each year
            Dictionary<PitchScenarioResult, float> psrDict = new();
            var pitchScenarioPitches = ptDb.PitchData.Where(f =>
                f.LevelId == 1 && f.Year >= 2017 // Non-comprehensive data before 2017
            )
            .Select(f => new { f.Year, f.CountBalls, f.CountStrike, f.Result, f.RunValueHitter, f.GameId, f.PitchId })
            .ToList();
            
            var scenarios = pitchScenarioPitches.Select(f => new PitchScenario
            {
                balls = f.CountBalls,
                strikes = f.CountStrike,
                year = f.Year
            }).Distinct();

            using (ProgressBar progressBar = new ProgressBar(scenarios.Count(), "Creating Year Scenario Pitch Expectancy Dict"))
            {
                foreach (var scenario in scenarios)
                {
                    float ballValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == Db.DbEnums.PitchResult.Ball)
                        .Average(f => f.RunValueHitter);

                    float strikeValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        (f.Result == Db.DbEnums.PitchResult.CalledStrike || f.Result == Db.DbEnums.PitchResult.SwingingStrike))
                        .Average(f => f.RunValueHitter);

                    float hbpValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == Db.DbEnums.PitchResult.HBP)
                        .Average(f => f.RunValueHitter);

                    psrDict[new PitchScenarioResult(scenario, PitchResult.Ball)] = ballValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.Strike)] = strikeValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.HBP)] = hbpValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.Foul)] = scenario.strikes == 2 ? 0 : strikeValue;

                    progressBar.Tick();
                }
            }

            // Load aggregated pitches (probabilities already averaged over eligible runs by Eval_Pitches;
            // StuffRuns/CombinedRuns are -1000 placeholders at this point)
            List<Output_PitchValueAggregation> pvaList = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .ToList();

            using (ProgressBar progressBar = new ProgressBar(pvaList.Count, "Calculating Pitch Run Values"))
            {
                foreach (var pva in pvaList)
                {
                    // Get value of different results in this scenario
                    PitchScenario pitchScenario = new PitchScenario
                    {
                        balls = pva.CountBalls,
                        strikes = pva.CountStrikes,
                        year = pva.Year
                    };
                    float valueBall = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Ball)];
                    float valueStrike = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Strike)];
                    float valueFoul = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Foul)];
                    float valueHBP = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.HBP)];

                    // Combine probabilities to expected values
                    pva.StuffRuns = ExpectedRuns(valueBall, valueStrike, valueFoul, valueHBP,
                        pva.StuffBall, pva.StuffCalledStrike, pva.StuffSwing, pva.StuffWhiff,
                        pva.StuffFoul, pva.StuffInPlay, pva.StuffInPlayExpected, pva.StuffHBP);
                    pva.CombinedRuns = ExpectedRuns(valueBall, valueStrike, valueFoul, valueHBP,
                        pva.CombinedBall, pva.CombinedCalledStrike, pva.CombinedSwing, pva.CombinedWhiff,
                        pva.CombinedFoul, pva.CombinedInPlay, pva.CombinedInPlayExpected, pva.CombinedHBP);

                    progressBar.Tick();
                }
            }

            // All-or-nothing write of the calculated run values
            pitchDb.BulkUpdate(pvaList, new BulkConfig
            {
                PropertiesToInclude = new List<string>
            {
                nameof(Output_PitchValueAggregation.StuffRuns),
                nameof(Output_PitchValueAggregation.CombinedRuns)
            }
            });
        }

        private static float ExpectedRuns(float valueBall, float valueStrike, float valueFoul, float valueHBP,
            float probBall, float probCS, float probSwing, float probWhiff,
            float probFoul, float probIP, float valueIP, float probHBP)
        {
            return (valueBall * probBall)
                + (valueStrike * (probCS + probSwing * probWhiff))
                + (valueFoul * (probSwing * probFoul))
                + (valueIP * (probSwing * probIP))
                + (valueHBP * probHBP);
        }
    }
}
