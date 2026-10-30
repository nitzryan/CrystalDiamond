using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;
using DataAquisition.PitchTracking;
using Db;

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
        private readonly record struct PzcKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes, int Row, int Col);
        private readonly record struct PitchInfo(Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes, float PlateX, float PlateZ, float ZoneTop, float ZoneBot);
        private readonly record struct PzncKey(int Year, Db.DbEnums.PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes);

        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            // Get expected run values for each scenario each year
            Dictionary<PitchScenarioResult, float> psrDict = new();
            var pitchScenarioPitches = ptDb.PitchData.Where(f =>
                f.LevelId == 1 && f.Year >= 2017 // Non-comprehensive data before 2017
            )
            .Select(f => new { f.Year, f.CountBalls, f.CountStrike, f.Result, f.RunValueSmoothedHitter, f.GameId, f.PitchId })
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
                        .Average(f => f.RunValueSmoothedHitter);

                    float strikeValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        (f.Result == Db.DbEnums.PitchResult.CalledStrike || f.Result == Db.DbEnums.PitchResult.SwingingStrike))
                        .Average(f => f.RunValueSmoothedHitter);

                    float hbpValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == Db.DbEnums.PitchResult.HBP)
                        .Average(f => f.RunValueSmoothedHitter);

                    psrDict[new PitchScenarioResult(scenario, PitchResult.Ball)] = ballValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.Strike)] = strikeValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.HBP)] = hbpValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult.Foul)] = scenario.strikes == 2 ? 0 : strikeValue;

                    progressBar.Tick();
                }
            }

            // Only valid bins are stored; a missing key means "no valid bin" (including PitchClass.Other)
            Dictionary<PzcKey, float> pzcDict = ptDb.PitchZoneCompetitive
                .AsNoTracking()
                .Where(f => f.IsCompetitive && f.HasEnoughSample)
                .ToDictionary(
                    f => new PzcKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes, f.ZoneRow, f.ZoneCol),
                    f => f.ExpectedValueHitter);

            // Pooled value of pitches that don't land in a valid bin, per situation
            Dictionary<PzncKey, float> pzncDict = ptDb.PitchZoneNonCompetitive
                .AsNoTracking()
                .Where(f => f.PitchCount > 0)
                .ToDictionary(
                    f => new PzncKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes),
                    f => f.ExpectedValueHitter);

            // Load aggregated pitches (probabilities already averaged over eligible runs by Eval_Pitches;
            // StuffRuns/CombinedRuns are -1000 placeholders at this point)
            List<Output_PitchValueAggregation> pvaList = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .ToList();

            using (ProgressBar progressBar = new ProgressBar(pvaList.Count, "Calculating Pitch Run Values"))
            {
                // One year of PitchData in memory at a time; only years that have OPVA rows are loaded
                foreach (var yearGroup in pvaList.GroupBy(f => f.Year))
                {
                    int year = yearGroup.Key;
                    Dictionary<(int, int), PitchInfo> pitchInfoDict = ptDb.PitchData
                        .AsNoTracking()
                        .Where(f => f.Year == year)
                        .Select(f => new {
                            f.GameId, f.PitchId, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                            f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot
                        })
                        .ToDictionary(
                            f => (f.GameId, f.PitchId),
                            f => new PitchInfo(f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                                               f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot));

                    // Bin value depends only on the pitch, not the model, so compute once per pitch
                    Dictionary<(int, int), float?> binCache = new();

                    // Iterate through pitches in the year
                    foreach (var pva in yearGroup)
                    {
                        var pitchKey = (pva.GameId, pva.PitchId);
                        PitchInfo pitch = pitchInfoDict[pitchKey];

                        // Get value of different results in this scenario
                        PitchScenario pitchScenario = new PitchScenario
                        {
                            balls = pitch.Balls,
                            strikes = pitch.Strikes,
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

                        // Bin value from the PREVIOUS year's bins (no leakage)
                        if (!binCache.TryGetValue(pitchKey, out float? binValue))
                        {
                            binValue = GetBinValue(pzcDict, year - 1, pitch);
                            binCache[pitchKey] = binValue;
                        }

                        if (binValue.HasValue)
                        {
                            pva.StuffBinRuns = pva.CombinedRuns - binValue.Value;
                            pva.LocBinRuns = binValue.Value;
                        }
                        else // No valid bin: location gets the situation's pooled non-competitive value
                        {
                            pva.StuffBinRuns = null;
                            pva.LocBinRuns = pzncDict.TryGetValue(
                                new PzncKey(year - 1, pitch.PitchClass, pitch.PitIsR, pitch.HitIsR, pitch.Balls, pitch.Strikes),
                                out float poolValue)
                                ? poolValue : null;
                        }

                        progressBar.Tick();
                    }
                }
            }

            pitchDb.BulkUpdate(pvaList, new BulkConfig
            {
                PropertiesToInclude = new List<string>
            {
                nameof(Output_PitchValueAggregation.StuffRuns),
                nameof(Output_PitchValueAggregation.CombinedRuns),
                nameof(Output_PitchValueAggregation.StuffBinRuns),
                nameof(Output_PitchValueAggregation.LocBinRuns),
            }
            });
        }

        private static float ExpectedRuns(float valueBall, float valueStrike, float valueFoul, float valueHBP,
            float probBall, float probCS, float probSwing, float probWhiff,
            float probFoul, float probIP, float valueIP, float probHBP)
        {
            return (valueBall * probBall)
                + (valueStrike * (probCS + (probSwing * probWhiff)))
                + (valueFoul * (probSwing * probFoul))
                + (valueIP * (probSwing * probIP))
                + (valueHBP * probHBP);
        }

        // Hitter expected value of the valid (competitive + enough sample) bin this pitch lands in,
        // using bins built from binYear. Null if there is no such bin.
        private static float? GetBinValue(Dictionary<PzcKey, float> pzcDict, int binYear, PitchInfo p)
        {
            if (PitchBinCompetitiveness.GetZone(p.PlateX, p.PlateZ, p.ZoneTop, p.ZoneBot) is not PitchBinCompetitiveness.PitchZone zone)
                return null;

            if (!pzcDict.TryGetValue(new PzcKey(binYear, p.PitchClass, p.PitIsR, p.HitIsR, p.Balls, p.Strikes, zone.Row, zone.Column), out float value))
                return null;

            return value;
        }
    }
}
