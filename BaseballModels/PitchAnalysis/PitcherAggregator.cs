using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchAnalysis.PitchDecomp;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;
using System.Diagnostics.Metrics;
using static Db.DbEnums;

namespace PitchAnalysis
{
    internal class PitcherAggregator
    {
        private record YearLeagueDevationKey(int modelId, int year, int balls, int strikes);
        private record PitchValueKey(int gameId, int pitchId);
        private static Dictionary<YearLeagueDevationKey, YearLeagueDeviations> yldDict = new();

        private sealed class ScenarioTotals
        {
            public int NumPitches;
            public int NumStuffPitches;
            public double Dev;
            public double Actual;
            public double Stuff;
            public double Location;
            public double Situation;
            public double Vel;
            public double BreakHoriz;
            public double BreakVert;
        }

        private static PitchScenario GetScenario(PitchData p)
        {
            PitchScenario scenario = (p.PitIsR == p.HitIsR)
                ? PitchScenario.SameSide
                : PitchScenario.OppSide;

            scenario |= (p.CountStrike >= 2)
                ? PitchScenario.TwoStrikes
                : PitchScenario.NotTwoStrikes;

            if (p.CountStrike > p.CountBalls)
                scenario |= PitchScenario.AheadCount;
            else if (p.CountStrike == p.CountBalls)
                scenario |= PitchScenario.EvenCount;
            else
                scenario |= PitchScenario.BehindCount;

            return scenario;
        }

        private static List<PitcherStuff> GetPitcherYearMonthStuffByScenarios(
            IEnumerable<PitchData> pitches,
            Dictionary<PitchValueKey, Output_PitchValueAggregation> pvaDict,
            YearPitcherDeviations scale,
            int model,
            bool isFullYear,
            bool isSingleGame,
            IEnumerable<PitchScenario> scenarios)
        {
            List<PitcherStuff> results = new();

            foreach (var pitchGroup in pitches.GroupBy(f => f.PitchType))
            {
                var firstPitch = pitchGroup.First();
                Dictionary<PitchScenario, ScenarioTotals> totals = scenarios.ToDictionary(s => s, _ => new ScenarioTotals());

                foreach (var p in pitchGroup)
                {
                    Output_PitchValueAggregation pva = pvaDict[new PitchValueKey(p.GameId, p.PitchId)];
                    float stuffDev = yldDict[new YearLeagueDevationKey(model, p.Year, p.CountBalls, p.CountStrike)].StuffDev;
                    PitchScenario pitchScenario = GetScenario(p);

                    foreach (var (scenario, t) in totals)
                    {
                        // All is 0, so every pitch has that flag
                        if (!pitchScenario.HasFlag(scenario))
                            continue;

                        t.NumPitches++;
                        t.Actual += p.RunValueHitter;
                        t.Location += pva.LocBinRuns!.Value;
                        t.Situation += pva.SitBinRuns!.Value;

                        if (pva.StuffBinRuns.HasValue)
                        {
                            t.NumStuffPitches++;
                            t.Stuff += pva.StuffBinRuns.Value;
                        }

                        t.Dev += stuffDev;
                        t.Vel += p.Vel;
                        t.BreakHoriz += p.BreakHorizontal;
                        t.BreakVert += p.BreakInduced;
                    }
                }

                foreach (var (scenario, t) in totals)
                {
                    if (t.NumPitches == 0)
                        continue;

                    double countDev = t.Dev / t.NumPitches;
                    double avgDev = countDev * scale.ScoreDev;

                    results.Add(new PitcherStuff
                    {
                        MlbId = firstPitch.PitcherId,
                        Year = isSingleGame ? -1 : firstPitch.Year,
                        Month = isSingleGame ? -1 :
                            isFullYear ? 13 : firstPitch.Month,
                        GameId = isSingleGame ? firstPitch.GameId : -1,
                        ModelId = model,
                        PitchType = firstPitch.PitchType,
                        Scenario = scenario,
                        NumPitches = t.NumPitches,
                        NumStuffPitches = t.NumStuffPitches,
                        CountLeverage = (float)(countDev / scale.MeanCountDev),
                        ActualPlus = Utility.CalculatePitchPlus(t.Actual / t.NumPitches, avgDev),
                        PitchPlus = Utility.CalculatePitchPlus((t.Stuff + t.Location + t.Situation) / t.NumPitches, avgDev),
                        StuffPlus = t.NumStuffPitches > 0
                            ? Utility.CalculatePitchPlus(t.Stuff / t.NumStuffPitches, avgDev)
                            : Utility.CalculatePitchPlus(0, avgDev),
                        LocationPlus = Utility.CalculatePitchPlus(t.Location / t.NumPitches, avgDev),
                        SituationPlus = Utility.CalculatePitchPlus(t.Situation / t.NumPitches, avgDev),
                        Vel = (float)(t.Vel / t.NumPitches),
                        BreakHoriz = (float)(t.BreakHoriz / t.NumPitches),
                        BreakVert = (float)(t.BreakVert / t.NumPitches),
                    });
                }
            }

            return results;
        }

        public static void CreateStats(int endYear, bool fullUpdate)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext trackingDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            yldDict = pitchDb.YearLeagueDeviations
                .AsNoTracking()
                .ToDictionary(f => new YearLeagueDevationKey(f.ModelId, f.Year, f.Balls, f.Strikes));

            List<PitchScenario> scenarios = [
                PitchScenario.All,
                PitchScenario.SameSide, PitchScenario.OppSide,
                PitchScenario.NotTwoStrikes, PitchScenario.TwoStrikes,
                PitchScenario.AheadCount, PitchScenario.EvenCount, PitchScenario.BehindCount
            ];

            int startYear = pitchDb.YearLeagueDeviations.Min(f => f.Year);
            List<int> years = trackingDb.PitchData
                .Where(f => (fullUpdate || f.Year == endYear) && f.Year >= startYear)
                .Select(f => f.Year)
                .Distinct()
                .OrderBy(f => f)
                .ToList();
            List<int> modelIds = pitchDb.Output_PitchValueAggregation.Select(f => f.Model).Distinct().ToList();

            using (ProgressBar progressBar = new ProgressBar(years.Count * modelIds.Count, "Creating Pitcher Pitch Stats"))
            {
                foreach (int year in years)
                {
                    List<PitchData> yearPitches = trackingDb.PitchData
                        .AsNoTracking()
                        .Where(f => f.Year == year)
                        .ToList();

                    // Single-game rows have Year = -1, so they have to be found by game
                    List<int> gameIds = yearPitches.Select(f => f.GameId).Distinct().ToList();
                    pitchDb.PitcherStuff
                        .Where(f => f.Year == year || gameIds.Contains(f.GameId))
                        .ExecuteDelete();

                    foreach (int modelId in modelIds)
                    {
                        Dictionary<PitchValueKey, Output_PitchValueAggregation> pvaDict = pitchDb.Output_PitchValueAggregation
                            .AsNoTracking()
                            .Where(f => f.Year == year && f.Model == modelId && f.LocBinRuns != null)
                            .ToDictionary(f => new PitchValueKey(f.GameId, f.PitchId));

                        // A model may have no graded pitches in a year
                        if (pvaDict.Count == 0)
                        {
                            progressBar.Tick();
                            continue;
                        }


                        // Only graded pitches contribute
                        List<PitchData> gradedPitches = yearPitches
                            .Where(f => pvaDict.ContainsKey(new PitchValueKey(f.GameId, f.PitchId)))
                            .ToList();

                        YearPitcherDeviations scale = PitcherScale.CalculateAndSave(
                            pitchDb, year, modelId, gradedPitches,
                            p => pvaDict[new PitchValueKey(p.GameId, p.PitchId)].StuffBinRuns,
                            p => yldDict[new YearLeagueDevationKey(modelId, p.Year, p.CountBalls, p.CountStrike)].StuffDev);

                        List<PitcherStuff> stuffList = new();
                        foreach (var pitcher in gradedPitches.GroupBy(f => f.PitcherId))
                        {
                            // Year Stats
                            stuffList.AddRange(GetPitcherYearMonthStuffByScenarios(
                                pitcher, pvaDict, scale, modelId, true, false, scenarios));

                            // Month Stats
                            foreach (var mp in pitcher.GroupBy(f => f.Month))
                            {
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios(
                                    mp, pvaDict, scale, modelId, false, false, scenarios));
                            }

                            // Game stats
                            foreach (var gp in pitcher.GroupBy(f => f.GameId))
                            {
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios(
                                    gp, pvaDict, scale, modelId, false, true, scenarios));
                            }
                        }

                        pitchDb.BulkInsert(stuffList);
                        progressBar.Tick();
                    }
                }
            }
        }
    }
}
