using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
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
        private record PitDictKey(int model, int run, int mlbId);

        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            pitchDb.Output_PitchValueAggregation.ExecuteDelete();

            // Get expected run values for each scenario each year
            var scenarios = db.PitchStatcast.Select(f => new PitchScenario {
                balls = f.CountBalls,
                strikes = f.CountStrike,
                year = f.Year
            }).Distinct();

            Dictionary<PitchScenarioResult, float> psrDict = new();
            var pitchStatcastPitches = db.PitchStatcast.Where(f =>
                f.LevelId == 1
            )
            .Select(f => new { f.Year, f.CountBalls, f.CountStrike, f.Result, f.RunValueHitter })
            .ToList();
            using (ProgressBar progressBar = new ProgressBar(scenarios.Count(), "Creating Year Scenario Pitch Expectancy Dict"))
            {
                foreach (var scenario in scenarios)
                {
                    float ballValue = pitchStatcastPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == DbEnums.PitchResult.Ball)
                        .Average(f => f.RunValueHitter);

                    float strikeValue = pitchStatcastPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        (f.Result == DbEnums.PitchResult.CalledStrike || f.Result == DbEnums.PitchResult.SwingingStrike))
                        .Average(f => f.RunValueHitter);

                    float hbpValue = pitchStatcastPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == DbEnums.PitchResult.HBP)
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
                .ToDictionary(
                    f => new PitDictKey(f.ModelId, f.ModelRun, f.MlbId),
                    f => f.IsTrain
                );

            // Create pitch aggregations
            var pitchGroups = pitchDb.Output_PitchValue.GroupBy(f => new { f.GameId, f.PitchId, f.Model }).AsNoTracking();
            List<Output_PitchValueAggregation> pvaList = new(pitchGroups.Count());
            Dictionary<(int, int), PitchScenario> pitchScenarioDict = db.PitchStatcast
                .AsNoTracking()
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
            using (ProgressBar progressBar = new ProgressBar(pitchGroups.Count(), "Creating Pitch Aggregation"))
            {
                foreach (var pg in pitchGroups)
                {
                    // Filter out any in training data
                    List<Output_PitchValue> pitches = pg.ToList();
                    List<PitDictKey> pitKeys = pitches.Select(f => new PitDictKey(f.Model, f.ModelRun, f.MlbId)).ToList();
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
                    float probBallLocation = pitches.Average(f => f.LocationBall);
                    float probBallStuff = pitches.Average(f => f.StuffBall);
                    float probBallCombined = pitches.Average(f => f.CombinedBall);

                    float probCSLocation = pitches.Average(f => f.LocationCalledStrike);
                    float probCSStuff = pitches.Average(f => f.StuffCalledStrike);
                    float probCSCombined = pitches.Average(f => f.CombinedCalledStrike);

                    float probSwingLocation = pitches.Average(f => f.LocationSwing);
                    float probSwingStuff = pitches.Average(f => f.StuffSwing);
                    float probSwingCombined = pitches.Average(f => f.CombinedSwing);

                    float probHBPLocation = pitches.Average(f => f.LocationHBP);
                    float probHBPStuff = pitches.Average(f => f.StuffHBP);
                    float probHBPCombined = pitches.Average(f => f.CombinedHBP);

                    float probWhiffLocation = pitches.Average(f => f.LocationWhiff);
                    float probWhiffStuff = pitches.Average(f => f.StuffWhiff);
                    float probWhiffCombined = pitches.Average(f => f.CombinedWhiff);

                    float probFoulLocation = pitches.Average(f => f.LocationFoul);
                    float probFoulStuff = pitches.Average(f => f.StuffFoul);
                    float probFoulCombined = pitches.Average(f => f.CombinedFoul);

                    float probIPLocation = pitches.Average(f => f.LocationInPlay);
                    float probIPStuff = pitches.Average(f => f.StuffInPlay);
                    float probIPCombined = pitches.Average(f => f.CombinedInPlay);

                    // Combine probabilities to expected values
                    float locationExpectedValue = 0;
                    float stuffExpectedValue = 0;
                    float combinedExpectedValue = 0;

                    float valueIPLocation = pitches.Average(f => f.LocationInPlayExpected);
                    float valueIPStuff = pitches.Average(f => f.StuffInPlayExpected);
                    float valueIPCombined = pitches.Average(f => f.CombinedInPlayExpected);

                    locationExpectedValue += valueBall * probBallLocation;
                    stuffExpectedValue += valueBall * probBallStuff;
                    combinedExpectedValue += valueBall * probBallCombined;

                    locationExpectedValue += valueStrike * (probCSLocation + (probSwingLocation * probWhiffLocation));
                    stuffExpectedValue += valueStrike * (probCSStuff + (probSwingStuff * probWhiffStuff));
                    combinedExpectedValue += valueStrike * (probCSCombined + (probSwingCombined * probWhiffCombined));

                    locationExpectedValue += valueFoul * (probSwingLocation * probFoulLocation);
                    stuffExpectedValue += valueFoul * (probSwingStuff * probFoulStuff);
                    combinedExpectedValue += valueFoul * (probSwingCombined * probFoulCombined);

                    locationExpectedValue += valueIPLocation * (probSwingLocation * probIPLocation);
                    stuffExpectedValue += valueIPStuff * (probSwingStuff * probIPStuff);
                    combinedExpectedValue += valueIPCombined * (probSwingCombined * probIPCombined);

                    locationExpectedValue += valueHBP * probHBPLocation;
                    stuffExpectedValue += valueHBP * probHBPStuff;
                    combinedExpectedValue += valueHBP * probHBPCombined;

                    pvaList.Add(new Output_PitchValueAggregation
                    {
                        Model = first.Model,
                        GameId = first.GameId,
                        PitchId = first.PitchId,
                        Year = first.Year,
                        MlbId = first.MlbId,
                        LevelId = first.LevelId,

                        CountBalls = pitchScenario.balls,
                        CountStrikes = pitchScenario.strikes,

                        LocationCalledStrike = probCSLocation,
                        LocationBall = probBallLocation,
                        LocationHBP = probHBPLocation,
                        LocationSwing = probSwingLocation,
                        LocationWhiff = probWhiffLocation,
                        LocationFoul = probFoulLocation,
                        LocationInPlay = probIPLocation,
                        LocationInPlayExpected = valueIPLocation,
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
                        LocationRuns = locationExpectedValue,
                        StuffRuns = stuffExpectedValue,
                        CombinedRuns = combinedExpectedValue,
                    });

                    progressBar.Tick();
                }
            }
            pitchDb.BulkInsert(pvaList);
        }
    }
}
