using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;
using Db;
using PitchAnalysis.PitchZoneBinNS;
using static Db.DbEnums;

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
        private enum PitchResult_Agg
        {
            Strike,
            Ball,
            HBP,
            Foul
        }

        private readonly record struct SituationModelKey(int Model, PitchClass PitchClass, int Balls, int Strikes);
        private record PitchScenarioResult(PitchScenario scenario, PitchResult_Agg result);
        private readonly record struct PzcKey(int Year, PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes, int Row, int Col);
        private readonly record struct PitchInfo(PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes, float PlateX, float PlateZ, float ZoneTop, float ZoneBot, float RunValueSmoothedHitter);
        private readonly record struct PzncKey(int Year, PitchClass PitchClass, bool PitIsR, bool HitIsR,
            int Balls, int Strikes);

        // Need to make sure that scenarios without enough pitches aren't graded.
        private readonly record struct ScenarioKey(PitchClass PitchClass, bool PitIsR, bool HitIsR, int Balls, int Strikes);
        private const int MIN_SCENARIO_YEAR = 2022;
        private const int MAX_SCENARIO_YEAR = 2026;
        private const int MIN_SCENARIO_PITCHES = 2000;

        // Scenarios with enough MLB pitches (MIN_SCENARIO_YEAR..MAX_SCENARIO_YEAR, inclusive) to grade against the bins.
        private static HashSet<ScenarioKey> LoadGradedScenarios(PitchTrackingDbContext ptDb)
        {
            return ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.LevelId == 1 && f.Year >= MIN_SCENARIO_YEAR && f.Year <= MAX_SCENARIO_YEAR
                    && f.PitchClass != PitchClass.Other)
                .GroupBy(f => new { f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike })
                .Where(g => g.Count() >= MIN_SCENARIO_PITCHES)
                .Select(g => new { g.Key.PitchClass, g.Key.PitIsR, g.Key.HitIsR, g.Key.CountBalls, g.Key.CountStrike })
                .ToList()
                .Select(g => new ScenarioKey(g.PitchClass, g.PitIsR, g.HitIsR, g.CountBalls, g.CountStrike))
                .ToHashSet();
        }

        public static void Update(PitchZoneBin bin)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            pitchDb.SituationValues.ExecuteDelete();

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

            HashSet<ScenarioKey> gradedScenarios = LoadGradedScenarios(ptDb);

            using (ProgressBar progressBar = new ProgressBar(scenarios.Count(), "Creating Year Scenario Pitch Expectancy Dict"))
            {
                foreach (var scenario in scenarios)
                {
                    float ballValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == PitchResult.Ball)
                        .Average(f => f.RunValueSmoothedHitter);

                    float strikeValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        (f.Result == PitchResult.CalledStrike || f.Result == PitchResult.SwingingStrike))
                        .Average(f => f.RunValueSmoothedHitter);

                    float hbpValue = pitchScenarioPitches.Where(
                        f => f.Year == scenario.year &&
                        f.CountBalls == scenario.balls &&
                        f.CountStrike == scenario.strikes &&
                        f.Result == PitchResult.HBP)
                        .Average(f => f.RunValueSmoothedHitter);

                    psrDict[new PitchScenarioResult(scenario, PitchResult_Agg.Ball)] = ballValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult_Agg.Strike)] = strikeValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult_Agg.HBP)] = hbpValue;
                    psrDict[new PitchScenarioResult(scenario, PitchResult_Agg.Foul)] = scenario.strikes == 2 ? 0 : strikeValue;

                    progressBar.Tick();
                }
            }

            // Load aggregated pitches, StuffRuns/CombinedRuns are -1000 placeholders at this point)
            List<Output_PitchValueAggregation> pvaList = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .ToList();

            List<SituationValues> situationValues = new();
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
                            f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot, f.RunValueSmoothedHitter
                        })
                        .ToDictionary(
                            f => (f.GameId, f.PitchId),
                            f => new PitchInfo(f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrike,
                                               f.PlateX, f.PlateZ, f.ZoneTop, f.ZoneBot, f.RunValueSmoothedHitter));

                    // Bin value depends only on the pitch, not the model, so compute once per pitch
                    var (pzcDict, pzncDict) = LoadBinYear(ptDb, bin, year - 1);
                    Dictionary<(int, int), float?> binCache = new();

                    // Need to track total MLB values for the year to adjust: stuff per model/class/count, location per class
                    Dictionary<SituationModelKey, (double Value, int Count)> stuffValueDict = new();
                    Dictionary<SituationModelKey, (double Value, int Count)> locValueDict = new();
                    Dictionary<SituationModelKey, (double Model, double Actual)> runValueDict = new();

                    // Iterate through pitches in the year
                    foreach (var pva in yearGroup)
                    {
                        var pitchKey = (pva.GameId, pva.PitchId);
                        PitchInfo pitch = pitchInfoDict[pitchKey];
                        pva.SitBinRuns = null;

                        // Get value of different results in this scenario
                        PitchScenario pitchScenario = new PitchScenario
                        {
                            balls = pitch.Balls,
                            strikes = pitch.Strikes,
                            year = pva.Year
                        };

                        float valueBall = psrDict[new PitchScenarioResult(pitchScenario, PitchResult_Agg.Ball)];
                        float valueStrike = psrDict[new PitchScenarioResult(pitchScenario, PitchResult_Agg.Strike)];
                        float valueFoul = psrDict[new PitchScenarioResult(pitchScenario, PitchResult_Agg.Foul)];
                        float valueHBP = psrDict[new PitchScenarioResult(pitchScenario, PitchResult_Agg.HBP)];

                        // Combine probabilities to expected values
                        pva.StuffRuns = ExpectedRuns(valueBall, valueStrike, valueFoul, valueHBP,
                            pva.StuffBall, pva.StuffCalledStrike, pva.StuffSwing, pva.StuffWhiff,
                            pva.StuffFoul, pva.StuffInPlay, pva.StuffInPlayExpected, pva.StuffHBP);
                        pva.CombinedRuns = ExpectedRuns(valueBall, valueStrike, valueFoul, valueHBP,
                            pva.CombinedBall, pva.CombinedCalledStrike, pva.CombinedSwing, pva.CombinedWhiff,
                            pva.CombinedFoul, pva.CombinedInPlay, pva.CombinedInPlayExpected, pva.CombinedHBP);

                        // Don't give a bin location/stuff grade if there aren't enough pitches for that to be viable
                        if (!gradedScenarios.Contains(new ScenarioKey(pitch.PitchClass, pitch.PitIsR, pitch.HitIsR, pitch.Balls, pitch.Strikes)))
                        {
                            pva.StuffBinRuns = null;
                            pva.LocBinRuns = null;
                            progressBar.Tick();
                            continue;
                        }

                        // Bin value from the PREVIOUS year's bins (no leakage)
                        if (!binCache.TryGetValue(pitchKey, out float? binValue))
                        {
                            binValue = GetBinValue(bin, pzcDict, year - 1, pitch);
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

                        // Track total value for MLB pitches
                        if (pva.LevelId == 1)
                        {
                            if (pva.StuffBinRuns != null)
                            {
                                SituationModelKey stuffKey = new(pva.Model, pitch.PitchClass, pitch.Balls, pitch.Strikes);
                                (double value, int count) = stuffValueDict.GetValueOrDefault(stuffKey);
                                stuffValueDict[stuffKey] = (value + pva.StuffBinRuns.Value, count + 1);
                            }
                            if (pva.LocBinRuns != null)
                            {
                                SituationModelKey locKey = new(pva.Model, pitch.PitchClass, pitch.Balls, pitch.Strikes);
                                (double value, int count) = locValueDict.GetValueOrDefault(locKey);
                                locValueDict[locKey] = (value + pva.LocBinRuns.Value, count + 1);

                                (double modelRuns, double actualRuns) = runValueDict.GetValueOrDefault(locKey);
                                runValueDict[locKey] = (modelRuns + pva.CombinedRuns, actualRuns + pitch.RunValueSmoothedHitter);
                            }
                        }

                        progressBar.Tick();
                    }

                    foreach (var (key, loc) in locValueDict)
                    {
                        var stuff = stuffValueDict.GetValueOrDefault(key);
                        var runs = runValueDict[key];
                        float locMean = (float)(loc.Value / loc.Count);
                        float stuffMean = stuff.Count > 0 ? (float)(stuff.Value / stuff.Count) : 0;

                        situationValues.Add(new SituationValues
                        {
                            ModelId = key.Model,
                            Year = year,
                            PitchClass = key.PitchClass,
                            Balls = key.Balls,
                            Strikes = key.Strikes,
                            NumPitches = loc.Count,
                            NumStuffPitches = stuff.Count,
                            LocMean = locMean,
                            StuffMean = stuffMean,
                            SitRuns = locMean + stuffMean,
                            ModelRuns = (float)(runs.Model / loc.Count),
                            ActualRuns = (float)(runs.Actual / loc.Count)
                        });
                    }

                    // Since bins are computed from previous years and used on a different year, the average
                    // values for the year will not be 0.  Adjust so that they are
                    foreach (var pva in yearGroup.Where(f => f.LocBinRuns != null))
                    {
                        var pitchKey = (pva.GameId, pva.PitchId);
                        PitchInfo pitch = pitchInfoDict[pitchKey];

                        SituationModelKey stuffKey = new(pva.Model, pitch.PitchClass, pitch.Balls, pitch.Strikes);

                        // With no MLB pitches in this group there is no baseline, so the pitch isn't graded
                        if (!locValueDict.TryGetValue(stuffKey, out var mlbLoc))
                        {
                            pva.LocBinRuns = null;
                            pva.StuffBinRuns = null;
                            continue;
                        }

                        float locMean = (float)(mlbLoc.Value / mlbLoc.Count);
                        bool hasStuffBaseline = stuffValueDict.TryGetValue(stuffKey, out var mlbStuff);
                        float stuffMean = hasStuffBaseline ? (float)(mlbStuff.Value / mlbStuff.Count) : 0;

                        pva.LocBinRuns -= locMean;
                        pva.SitBinRuns = locMean + stuffMean;
                        pva.StuffBinRuns = hasStuffBaseline ? pva.StuffBinRuns - stuffMean : null;
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
                    nameof(Output_PitchValueAggregation.SitBinRuns),
                }
            });
            pitchDb.BulkInsert(situationValues);
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
        private static float? GetBinValue(PitchZoneBin bin, Dictionary<PzcKey, float> pzcDict, int binYear, PitchInfo p)
        {
            if (bin.GetZone(p.PlateX, p.PlateZ, p.ZoneTop, p.ZoneBot) is not PitchZone zone)
                return null;

            if (!pzcDict.TryGetValue(new PzcKey(binYear, p.PitchClass, p.PitIsR, p.HitIsR, p.Balls, p.Strikes, zone.Row, zone.Column), out float value))
                return null;

            return value;
        }

        // Valid bins and the per-situation pool for binYear. A missing PzcKey means "no valid bin" (including PitchClass.Other).
        private static (Dictionary<PzcKey, float> Competitive, Dictionary<PzncKey, float> Pool) LoadBinYear(
            PitchTrackingDbContext ptDb, PitchZoneBin bin, int binYear)
        {
            List<PitchData> binPitches = [];
            binPitches = ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.Year == binYear && f.LevelId == 1 && f.PitchClass != PitchClass.Other)
                .ToList();

            var (competitiveRows, nonCompetitiveRows) = bin.BuildCompetitiveRows(binPitches);

            Dictionary<PzcKey, float> competitive = competitiveRows
                .Where(f => f.IsCompetitive && f.HasEnoughSample)
                .ToDictionary(
                    f => new PzcKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes, f.ZoneRow, f.ZoneCol),
                    f => f.ExpectedValueHitter);

            Dictionary<PzncKey, float> pool = nonCompetitiveRows
                .Where(f => f.PitchCount > 0)
                .ToDictionary(
                    f => new PzncKey(f.Year, f.PitchClass, f.PitIsR, f.HitIsR, f.CountBalls, f.CountStrikes),
                    f => f.ExpectedValueHitter);

            return (competitive, pool);
        }
    }
}
