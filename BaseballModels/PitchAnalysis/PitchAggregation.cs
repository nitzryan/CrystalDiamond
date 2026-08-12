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
        private record PitDictKey(int Model, int Year, int Run, int MlbId);

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

            // Create PIT dictionary
            Dictionary<PitDictKey, bool> pitDictionary = pitchDb.PlayersInTrainingData
                .AsNoTracking()
                .ToDictionary(
                    f => new PitDictKey(f.ModelId, f.Year, f.ModelRun, f.MlbId),
                    f => f.IsTrain
                );
            Console.WriteLine("Created PIT Dictionary");

            // Create pitch aggregations
            var pitchGroups = pitchDb.Output_PitchValue
                .AsNoTracking()
                .Where(f => !pitchDb.Output_PitchValueAggregation
                    .Any(a => a.GameId == f.GameId && a.PitchId == f.PitchId))
                .GroupBy(f => new { f.GameId, f.PitchId, f.Model });
            Console.WriteLine("Created Pitch Groups");

            List<Output_PitchValueAggregation> pvaList = new();
            Dictionary<(int, int), PitchScenario> pitchScenarioDict = ptDb.PitchData
                .Select(f => new
                {
                    f.GameId,
                    f.PitchId,
                    Balls = f.CountBalls,
                    Strikes = f.CountStrike,
                    f.Year
                })
                .ToDictionary(
                    f => (f.GameId, f.PitchId),
                    f => new PitchScenario
                    {
                        balls = f.Balls,
                        strikes = f.Strikes,
                        year = f.Year
                    }
                );
            Console.WriteLine("Created Pitch Scenario Dict");

            using (ProgressBar progressBar = new ProgressBar(pitchGroups.Count(), "Creating Pitch Aggregation"))
            {
                foreach (var pg in pitchGroups)
                {
                    // Filter out any in training data
                    List<Output_PitchValue> pitches = pg.ToList();
                    List<PitDictKey> pitKeys = pitches.Select(f => new PitDictKey(f.Model, f.Year, f.ModelRun, f.MlbId)).ToList();
                    for (int i = pitches.Count - 1; i >= 0; i--)
                    {
                        if (pitDictionary.ContainsKey(pitKeys[i]) && pitDictionary[pitKeys[i]])
                            pitches.RemoveAt(i);
                    }

                    var first = pg.First();
                    // Get value of different results in this scenario
                    PitchScenario pitchScenario = pitchScenarioDict[(first.GameId, first.PitchId)];

                    float valueBall = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Ball)];
                    float valueStrike = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Strike)];
                    float valueFoul = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.Foul)];
                    float valueHBP = psrDict[new PitchScenarioResult(pitchScenario, PitchResult.HBP)];

                    // Get probability of each event from individual model run pitches
                    float probBallStuff = pitches.Average(f => f.StuffBall);
                    float probBallCombined = pitches.Average(f => f.CombinedBall);

                    float probCSStuff = pitches.Average(f => f.StuffCalledStrike);
                    float probCSCombined = pitches.Average(f => f.CombinedCalledStrike);

                    float probSwingStuff = pitches.Average(f => f.StuffSwing);
                    float probSwingCombined = pitches.Average(f => f.CombinedSwing);

                    float probHBPStuff = pitches.Average(f => f.StuffHBP);
                    float probHBPCombined = pitches.Average(f => f.CombinedHBP);

                    float probWhiffStuff = pitches.Average(f => f.StuffWhiff);
                    float probWhiffCombined = pitches.Average(f => f.CombinedWhiff);

                    float probFoulStuff = pitches.Average(f => f.StuffFoul);
                    float probFoulCombined = pitches.Average(f => f.CombinedFoul);

                    float probIPStuff = pitches.Average(f => f.StuffInPlay);
                    float probIPCombined = pitches.Average(f => f.CombinedInPlay);

                    // Combine probabilities to expected values
                    float stuffExpectedValue = 0;
                    float combinedExpectedValue = 0;

                    float valueIPStuff = pitches.Average(f => f.StuffInPlayExpected);
                    float valueIPCombined = pitches.Average(f => f.CombinedInPlayExpected);

                    stuffExpectedValue += valueBall * probBallStuff;
                    combinedExpectedValue += valueBall * probBallCombined;

                    stuffExpectedValue += valueStrike * (probCSStuff + (probSwingStuff * probWhiffStuff));
                    combinedExpectedValue += valueStrike * (probCSCombined + (probSwingCombined * probWhiffCombined));

                    stuffExpectedValue += valueFoul * (probSwingStuff * probFoulStuff);
                    combinedExpectedValue += valueFoul * (probSwingCombined * probFoulCombined);

                    stuffExpectedValue += valueIPStuff * (probSwingStuff * probIPStuff);
                    combinedExpectedValue += valueIPCombined * (probSwingCombined * probIPCombined);

                    stuffExpectedValue += valueHBP * probHBPStuff;
                    combinedExpectedValue += valueHBP * probHBPCombined;

                    pvaList.Add(new Output_PitchValueAggregation
                    {
                        Model = first.Model,
                        GameId = first.GameId,
                        PitchId = first.PitchId,
                        Year = first.Year,
                        ModelYear = first.ModelYear,
                        MlbId = first.MlbId,
                        LevelId = first.LevelId,

                        CountBalls = pitchScenario.balls,
                        CountStrikes = pitchScenario.strikes,

                        StuffCalledStrike = probCSStuff,
                        StuffBall = probBallStuff,
                        StuffHBP = probHBPStuff,
                        StuffSwing = probSwingStuff,
                        StuffWhiff = probWhiffStuff,
                        StuffFoul = probFoulStuff,
                        StuffInPlay = probIPStuff,
                        StuffInPlayExpected = valueIPStuff,
                        CombinedCalledStrike = probCSCombined,
                        CombinedBall = probBallCombined,
                        CombinedHBP = probHBPCombined,
                        CombinedSwing = probSwingCombined,
                        CombinedWhiff = probWhiffCombined,
                        CombinedFoul = probFoulCombined,
                        CombinedInPlay = probIPCombined,
                        CombinedInPlayExpected = valueIPCombined,
                        StuffRuns = stuffExpectedValue,
                        CombinedRuns = combinedExpectedValue,
                    });

                    progressBar.Tick();
                }
            }

            if (pvaList.Any())
                pitchDb.BulkInsert(pvaList);
        }
    }
}
